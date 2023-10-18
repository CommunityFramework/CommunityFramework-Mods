using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static CF_Teams.API;

public class CF_TeamManager
{
    public class PlayerData
    {
        public Dictionary<string, DateTime> TeamParty { get; set; } = new Dictionary<string, DateTime>();
        public Dictionary<string, DateTime> TeamAlly { get; set; } = new Dictionary<string, DateTime>();
        public float LastUpdate { get; set; }
        public int CachedTeamSize { get; set; }
    }
    public class TeamDB
    {
        public Dictionary<int, PlayerData> Players = new Dictionary<int, PlayerData>();
        public Dictionary<string, CF_PlayerGroup> Groups = new Dictionary<string, CF_PlayerGroup>();
    }
    public static string prefix = "[CLAN MANAGER]";
    private static string dbFileName = "playerData.json";
    private static float lastTeamUpdateTime = 0f;

    public static TeamDB playerDataCache = new TeamDB();
    private static readonly object lockObject = new object();

    public static void LoadData()
    {
        string filePath = Path.Combine(mod.modDatabasePath, dbFileName);
        if (!File.Exists(filePath))
        {
            SaveData();
        }

        try
        {
            string json = File.ReadAllText(filePath);
            lock (lockObject)
            {
                playerDataCache = JsonConvert.DeserializeObject<TeamDB>(json);
            }
        }
        catch (Exception e)
        {
            log.Error($"Failed to load player data: {e.Message}");
        }
    }
    public static void SaveData()
    {
        try
        {
            string json;
            lock (lockObject)
            {
                json = JsonConvert.SerializeObject(playerDataCache);
            }
            File.WriteAllText(Path.Combine(mod.modDatabasePath, dbFileName), json);
        }
        catch (Exception e)
        {
            log.Error($"Failed to save player data: {e.Message}");
        }
    }
    public static void PeriodicUpdate()
    {
        // Update team status
        if (Time.time - lastTeamUpdateTime >= teamUpdateInterval)
        {
            foreach (var cInfo in CF_Player.GetClients())  // Replace GetAllUsers with your method to get all users
            {
                UpdatePlayer(cInfo);
            }
            lastTeamUpdateTime = Time.time;
            SaveData();
        }
    }
    public static bool GetPartyMembers(int entityId, out Dictionary<string, int> members)
    {
        if (playerDataCache.Players.TryGetValue(entityId, out PlayerData playerData))
        {
            members = ConvertToMinutes(playerData.TeamParty);
            return false;
        }

        members = null;
        return false;
    }
    public static bool GetAllyMembers(int entityId, out Dictionary<string, int> members)
    {
        if (playerDataCache.Players.TryGetValue(entityId, out PlayerData playerData))
        {
            members = ConvertToMinutes(playerData.TeamAlly);
            return false;
        }

        members = null;
        return false;
    }
    public static Dictionary<string, int> ConvertToMinutes(Dictionary<string, DateTime> teamDict)
    {
        Dictionary<string, int> newDict = new Dictionary<string, int>();
        foreach (KeyValuePair<string, DateTime> kv in teamDict)
        {
            newDict.Add(kv.Key, (int)(kv.Value - DateTime.UtcNow).TotalMinutes);
        }
        return newDict;
    }
    public static void UpdatePlayer(ClientInfo _cInfo)
    {
        if (!playerDataCache.Players.TryGetValue(_cInfo.entityId, out PlayerData playerData))
            return;

        // Update Party
        if (!CF_Player.TryGetEntityPlayer(_cInfo, out EntityPlayer player))
            return;

        if (player?.Party != null)
        {
            foreach (EntityPlayer pplayer in player.Party.MemberList)
            {
                string identifier = CF_Player.GetClientInfo(pplayer.entityId).InternalId.ReadablePlatformUserIdentifier;
                playerData.TeamParty[identifier] = DateTime.UtcNow;
            }
        }

        // Update Allies
        PersistentPlayerData ppd = CF_Player.GetPersistentPlayerData(_cInfo);
        if (ppd?.ACL != null)
        {
            foreach (PlatformUserIdentifierAbs user2 in ppd.ACL)
            {
                playerData.TeamAlly[user2.ReadablePlatformUserIdentifier] = DateTime.UtcNow;
            }
        }

        playerData.CachedTeamSize = FilterByTimeSpan(playerData.TeamParty, teamSizeCacheDuration).Count + FilterByTimeSpan(playerData.TeamAlly, teamSizeCacheDuration).Count;

        if (!string.IsNullOrEmpty(cvCachedTeamSize))
            player.SetCVar(cvCachedTeamSize, playerData.CachedTeamSize);
    }
    public static int GetCachedTeamSize(int entityId)
    {
        if (playerDataCache.Players.TryGetValue(entityId, out PlayerData playerData))
        {
            return playerData.CachedTeamSize;
        }
        return 0;
    }
    public static int GetTeamSize(int entityId, TimeSpan timeSpan)
    {
        if (playerDataCache.Players.TryGetValue(entityId, out PlayerData playerData))
        {
            int partyCount = FilterByTimeSpan(playerData.TeamParty, timeSpan).Count;
            int allyCount = FilterByTimeSpan(playerData.TeamAlly, timeSpan).Count;
            return partyCount + allyCount;
        }
        return 0;
    }
    public static Dictionary<string, DateTime> FilterByTimeSpan(Dictionary<string, DateTime> teamDict, TimeSpan timeSpan)
    {
        Dictionary<string, DateTime> newDict = new Dictionary<string, DateTime>();
        DateTime now = DateTime.UtcNow;
        foreach (var kv in teamDict)
        {
            if (now - kv.Value <= timeSpan)
            {
                newDict.Add(kv.Key, kv.Value);
            }
        }
        return newDict;
    }
    // Groups
    public static bool CheckPersistentPlayerStateChangePre(
        ClientInfo cInfo,
        PlatformUserIdentifierAbs playerID,
        PlatformUserIdentifierAbs otherPlayerID,
        EnumPersistentPlayerDataReason reason)
    {
        GetGroup(playerID, out CF_PlayerGroup group);
        GetGroup(otherPlayerID, out CF_PlayerGroup group2);

        // Invite
        switch (reason)
        {
            case EnumPersistentPlayerDataReason.ACL_Invite:
                if (!OnInvitePre(playerID, otherPlayerID))
                    return false;
                break;
            case EnumPersistentPlayerDataReason.ACL_AcceptedInvite:
                if (!OnAcceptInvitePre(playerID, otherPlayerID))
                    return false;
                break;
        }

        return true;
    }
    public static void CheckPersistentPlayerStateChangePost(
        ClientInfo cInfo,
        PlatformUserIdentifierAbs playerID,
        PlatformUserIdentifierAbs otherPlayerID,
        EnumPersistentPlayerDataReason reason)
    {
        GetGroup(playerID, out CF_PlayerGroup group);
        GetGroup(otherPlayerID, out CF_PlayerGroup group2);

        // Invite
        switch (reason)
        {
            case EnumPersistentPlayerDataReason.ACL_Removed:
                OnRemovedPost(playerID, otherPlayerID);
                break;
        }
    }
    public static void OnRemovedPost(PlatformUserIdentifierAbs playerID, PlatformUserIdentifierAbs otherPlayerID)
    {
        ClientInfo cInfo = CF_Player.GetClientInfo(playerID);
        ClientInfo cInfoOther = CF_Player.GetClientInfo(otherPlayerID);

        PersistentPlayerData pData = CF_Player.GetPersistentPlayerData(otherPlayerID);
        string nameOther = cInfoOther != null ? cInfoOther.playerName : pData?.PlayerName ?? "";

        log.Out($"OnRemovedPost :: {cInfo.playerName} removed {nameOther}.");

        if (!GetGroup(playerID, out CF_PlayerGroup group))
        {
            log.Error($"OnRemovedPost :: {cInfo.playerName} removed {nameOther} but was not in a group.");
            return;
        }

        if (group.IsLeader(cInfo) || (group.IsOfficer(cInfo) && !group.IsLeader(otherPlayerID) && !group.IsOfficer(otherPlayerID)))
        {
            CF_Player.Message($"{prefix} You have successfully kicked {nameOther} from your group.", cInfo);
            if (cInfoOther != null)
                CF_Player.Message($"{prefix} You got kicked from your group.", cInfoOther);
            log.Out($"OnRemovedPost :: {cInfo.playerName} kicked {nameOther} from {group.GetLeaderName()}'s group.");
            LeaveGroup(playerID);
            return;
        }

        CF_Player.Message($"{prefix} You successfully left your group.", cInfo);
        log.Out($"OnRemovedPost :: {cInfo.playerName} left group of {group.GetLeaderName()}.");
        LeaveGroup(playerID);
    }
    public static bool OnAcceptInvitePre(PlatformUserIdentifierAbs playerID, PlatformUserIdentifierAbs otherPlayerID)
    {
        ClientInfo cInfo = CF_Player.GetClientInfo(playerID);
        ClientInfo cInfoOther = CF_Player.GetClientInfo(otherPlayerID);

        log.Out($"{cInfo.playerName} acceped {cInfoOther.playerName}'s invite.");

        GetGroup(playerID, out CF_PlayerGroup group);
        GetGroup(otherPlayerID, out CF_PlayerGroup groupOther);

        // Already in group
        if (group != groupOther && group != null && group.members.Count > 1)
        {
            CF_Player.Message($"{prefix} {cInfo.playerName} can not accept your invite while being in another group already.", cInfoOther);
            CF_Player.Message($"{prefix} You can't accept the invite from {cInfoOther.playerName}, you are already in another group. Type !group info for details.", cInfo);
            log.Out($"{cInfo.playerName} tried to accept an invite from  {cInfoOther.playerName} but is already in a group with more then 1 player (Leader: )");
            return false;
        }

        // Create new group
        if (groupOther == null)
        {
            CF_PlayerGroup groupNew = new CF_PlayerGroup(cInfoOther, cInfo);

            playerDataCache.Groups.Add(cInfoOther.InternalId.ReadablePlatformUserIdentifier, groupNew);

            log.Out($"{cInfoOther.playerName} created a new group.");
            CF_Player.Message($"{prefix} You successfully created a new group. Use !group to add a name and tag.", cInfoOther);
            CF_Player.Message($"{prefix} {cInfo.playerName} has accepted your invite, you are now leader of your group.", cInfoOther);
            CF_Player.Message($"{prefix} You are now in the group of {cInfoOther.playerName}, type !group for more info.", cInfo);

            SaveData();

            return true;
        }

        // Same group
        if (group == groupOther)
        {
            //log.Out($"Both players are already in the same group. Allow.", false, "GroupSystem");
            return true;
        }

        // Max team size
        if (groupOther.members.Count >= groupLimit)
        {
            CF_Player.Message($"{prefix} {cInfo.playerName} can not accept your invite, your group has reached the max team size already.", cInfoOther);
            CF_Player.Message($"{prefix} You can't accept the invite from {cInfoOther.playerName}, the group has reached the max team size already.", cInfo);
            log.Out($"{cInfo.playerName} tried to accept an invite from {cInfoOther.playerName} but target group reached already max team size.");
            return false;
        }

        // Add as member to group
        log.Out($"{cInfoOther.playerName} added {cInfo.playerName} to his group.");
        CF_Player.MessageFriends(otherPlayerID, $"{prefix} {cInfo.playerName} joined your group.");
        CF_Player.Message($"{prefix} You are now in the group of {cInfoOther.playerName}, is is your group leader now.", cInfo);

        groupOther.AddMember(cInfo.InternalId);
        SaveData();

        return true;
    }
    public static bool OnInvitePre(PlatformUserIdentifierAbs playerID, PlatformUserIdentifierAbs otherPlayerID)
    {
        ClientInfo cInfo = CF_Player.GetClientInfo(playerID);
        ClientInfo cInfoOther = CF_Player.GetClientInfo(otherPlayerID);

        log.Out($"{cInfo.playerName} ({cInfo.PlatformId.ReadablePlatformUserIdentifier}) invited {cInfoOther.playerName} ({cInfoOther.PlatformId.ReadablePlatformUserIdentifier})");

        GetGroup(playerID, out CF_PlayerGroup group);
        GetGroup(otherPlayerID, out CF_PlayerGroup groupOther);

        // Both players are not in a group yet
        if (group == null && groupOther == null)
        {
            //log.Out($"Both players are not in a group yet. Allow.", false, "GroupSystem");
            CF_Player.Message($"{prefix} You invited {cInfoOther.playerName}.", cInfo);
            CF_Player.Message($"{prefix} {cInfo.playerName} invited you. (new group)", cInfoOther);
            return true;
        }

        // Same group
        if (group == groupOther)
        {
            //log.Out($"OnInvitePre :: Both players are already in the same group. Allow.", false, "GroupSystem");
            CF_Player.Message($"{prefix} You invited {cInfoOther.playerName}.", cInfo);
            CF_Player.Message($"{prefix} {cInfo.playerName} invited you, you are aleady n the sam group.", cInfoOther);
            return true;
        }

        // Only target player is in a group
        if (group == null && groupOther != null)
        {
            if (groupOther.members.Count == 1)
            {
                //log.Out($"OnInvitePre :: {cInfoOther.playerName} is the only member in his group. Lets remove it! Allow.", false, "GroupSystem");
                CF_Player.Message($"{prefix} You invited {cInfoOther.playerName}.", cInfo);
                CF_Player.Message($"{prefix} {cInfo.playerName} invited you. (new group)", cInfoOther);
                playerDataCache.Groups.Remove(groupOther.leader);
                SaveData();
                return true;
            }

            log.Out($"{cInfoOther.playerName} is already in a group with members. Declined.");
            CF_Player.Message($"{prefix} {cInfoOther.playerName} is already in a group, their group leader or an officer has to invite you first.", cInfo);
            CF_Player.Message($"{prefix} {cInfo.playerName} tried to invite you, but you are already in a group.", cInfoOther);
            return false;
        }

        // Only inviting player is in a group
        if (group != null && (groupOther == null || groupOther.members.Count <= 1))
        {
            bool leader = group.IsLeader(playerID);
            bool officer = group.IsOfficer(playerID);

            // Check Leader or Officer
            if (!leader && !officer)
            {
                log.Out($"{cInfo.playerName} has no permission to invite. Decline.");
                CF_Player.Message($"{prefix} Only the leader or an officer of your group can invite new players.", cInfo);
                CF_Player.Message($"{prefix} {cInfo.playerName} tried to invite you, but has no leader or officer permission.", cInfoOther);
                return false;
            }

            // Check team size
            if (group.members.Count >= groupLimit)
            {
                log.Out($"Max group size reached. Decline.");
                CF_Player.Message($"{prefix} Your group reached already the full member size.", cInfo);
                CF_Player.Message($"{prefix} {cInfo.playerName} tried to invite you, but the group reached already the limit of {groupLimit} members.", cInfoOther);
                return false;
            }

            CF_Player.Message($"{prefix} You invited {cInfoOther.playerName}.", cInfo);
            CF_Player.Message($"{prefix} {cInfo.playerName} invited you to join {group.groupName} (Tag: {group.groupTag} Membercount: {group.members.Count})", cInfoOther);
            //log.Out($"Allow.", false, "GroupSystem");
            return true;
        }

        // WTF?
        log.Out($"Decline. Already in a group with at least 2 members");
        CF_Player.Message($"{prefix} Invite failed. {cInfoOther.playerName} is already in a different group. he needs to type '!group leave' before he can become invited by a different group.", cInfo);
        return false;
    }
    public static void KickMember(ClientInfo _cInfo, List<string> _argList)
    {
        log.Out($"KickMember :: {_cInfo.playerName} Args: {_argList.ToArray()}.");
        if (_argList.Count != 2)
        {
            CF_Player.Message($"{prefix} Usage: !group kick <ID>", _cInfo);
            CF_Player.Message($"{prefix} You get the ID from a player by listing the group members using !group list.", _cInfo);
            return;
        }

        if (!GetGroup(_cInfo.InternalId, out CF_PlayerGroup group))
        {
            CF_Player.Message($"{prefix} You are currently not in a group. Invite another player to start a group.", _cInfo);
            return;
        }

        if (!int.TryParse(_argList[1], out int ID))
        {
            CF_Player.Message($"{_argList[1]} is not a number.", _cInfo);
            return;
        }

        if (ID < 1 || ID > group.members.Count)
        {
            CF_Player.Message($"{prefix} The ID must be between 0 and {group.members.Count}.", _cInfo);
            return;
        }

        if (!group.IsLeader(_cInfo) && !group.IsOfficer(_cInfo))
        {
            CF_Player.Message($"{prefix} Only the leader and officers of your group can kick members.", _cInfo);
            return;
        }

        string targetMember = null;
        int count = 0;
        foreach (KeyValuePair<string, DateTime> kv in group.members)
        {
            count++;
            if (ID == count)
                targetMember = kv.Key;
        }

        if (targetMember == null)
            return;

        if (targetMember.Equals(_cInfo.InternalId.ReadablePlatformUserIdentifier))
        {
            CF_Player.Message($"{prefix} You can't kick yourself.", _cInfo);
            return;
        }

        if (group.leader.Equals(targetMember))
        {
            CF_Player.Message($"{prefix} You can't kick the leader.", _cInfo);
            return;
        }

        if (!group.IsLeader(_cInfo) && group.officers.Contains(targetMember))
        {
            CF_Player.Message($"{prefix} You can't kick officers when you are not the leader.", _cInfo);
            return;
        }

        PlatformUserIdentifierAbs targetUser = PlatformUserIdentifierAbs.FromPlatformAndId("EOS", targetMember);

        if (CF_Player.TryGetClientInfo(targetUser, out ClientInfo cInfoTarget))
            CF_Player.Message($"{prefix} You got kicked from your group.", cInfoTarget);

        PersistentPlayerData pData = CF_Player.GetPersistentPlayerData(targetUser);

        log.Out($"KickMember :: {_cInfo.playerName} kicked {pData.PlayerName} from group {group.groupName} lead by {group.GetLeaderName()}");
        CF_Player.Message($"{prefix} You have successfully kicked {pData.PlayerName} from your group.", _cInfo);
        LeaveGroup(targetUser);

        SaveData();
    }
    public static void SetName(ClientInfo _cInfo, List<string> _argList)
    {
        log.Out($"SetName :: {_cInfo.playerName} Args: {_argList}.");
        if (_argList.Count < 2)
        {
            CF_Player.Message($"{prefix} Usage: !group setname <name>", _cInfo);
            CF_Player.Message($"{prefix} You get the ID from a player by listing the group members using !group list.", _cInfo);
            return;
        }

        if (!GetGroup(_cInfo.InternalId, out CF_PlayerGroup group))
        {
            CF_Player.Message($"{prefix} You are currently not in a group. Invite another player to start a group.", _cInfo);
            return;
        }

        string name = String.Join(" ", _argList.Skip(1).ToArray());

        if (name.Length < 3 || name.Length > 32)
        {
            CF_Player.Message($"{prefix} A group name must be between 3 and 32 chars long.", _cInfo);
            return;
        }

        if (!group.IsLeader(_cInfo))
        {
            CF_Player.Message($"{prefix} Only the leader can change the group name.", _cInfo);
            return;
        }

        CF_Player.Message($"{prefix} Your group has been renamed from {group.groupName} to {_argList[1]}.", _cInfo);

        group.groupName = name;

        SaveData();
    }
    public static void SetTag(ClientInfo _cInfo, List<string> _argList)
    {
        log.Out($"SetTag :: {_cInfo.playerName} Args: {_argList}.");
        if (_argList.Count != 2)
        {
            CF_Player.Message($"{prefix} Usage: !group settag <tag>", _cInfo);
            CF_Player.Message($"{prefix} You get the ID from a player by listing the group members using !group list.", _cInfo);
            return;
        }

        if (!GetGroup(_cInfo.InternalId, out CF_PlayerGroup group))
        {
            CF_Player.Message($"{prefix} You are currently not in a group. Invite another player to start a group.", _cInfo);
            return;
        }

        if (_argList[1].Length < 2 || _argList[1].Length > 5)
        {
            CF_Player.Message($"{prefix} A group tag must be between 2 and 5 chars long, no spaces.", _cInfo);
            return;
        }

        if (!group.IsLeader(_cInfo))
        {
            CF_Player.Message($"{prefix} Only the leader can change the group tag.", _cInfo);
            return;
        }

        CF_Player.Message($"{prefix} Your group tag has been renamed from {group.groupTag} to {_argList[1]}.", _cInfo);

        group.groupTag = _argList[1];

        SaveData();
    }
    public static void Leave(ClientInfo _cInfo, List<string> _argList)
    {
        log.Out($"Leave :: {_cInfo.playerName} Args: {_argList}.");

        if (!GetGroup(_cInfo.InternalId, out CF_PlayerGroup group))
        {
            CF_Player.Message($"{prefix} You are currently not in a group.", _cInfo);
            return;
        }

        CF_Player.Message($"{prefix} You left your group.", _cInfo);
        LeaveGroup(_cInfo.InternalId);
    }
    public static void LeaveGroup(PlatformUserIdentifierAbs user)
    {
        if (!GetGroup(user, out CF_PlayerGroup group))
            return;

        if (group.IsLeader(user))
        {
            RemoveGroup(group);
            return;
        }

        group.RemoveMember(user);
        SaveData();
        ClearAllies(user);
    }
    public static void RemoveGroup(CF_PlayerGroup _group)
    {
        playerDataCache.Groups.Remove(_group.leader);

        foreach (string eos in _group.members.Keys)
        {
            PlatformUserIdentifierAbs member = PlatformUserIdentifierAbs.FromPlatformAndId("EOS", eos);
            ClearAllies(member);
        }
    }
    public static void ClearAllies(PlatformUserIdentifierAbs user)
    {
        Dictionary<PlatformUserIdentifierAbs, PersistentPlayerData> persistentPlayers = GameManager.Instance.GetPersistentPlayerList().Players;
        if (!persistentPlayers.TryGetValue(user, out PersistentPlayerData pData))
            return;

        ClientInfo cInfo = CF_Player.GetClientInfo(user);

        // Check user ACL
        if (pData.ACL == null)
            return;

        foreach (PlatformUserIdentifierAbs userOther in pData.ACL.ToList())
        {
            // Remove from own list on server
            pData.RemovePlayerFromACL(userOther);

            // Get pData from the other player
            if (!persistentPlayers.TryGetValue(userOther, out PersistentPlayerData pDataOther))
                continue;

            // Remove from other list on server
            pDataOther.RemovePlayerFromACL(user);

            // Fake the removed by friends netpackage
            if (cInfo != null)
            {
                log.Out($"fake removed by member");
                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePersistentPlayerState>().Setup(pDataOther, user, EnumPersistentPlayerDataReason.ACL_Removed));
            }

            if (!pDataOther.ACL.Contains(user))
            {
                log.Out($"other not on ACL");
                continue;
            }

            // Fake target player left on his own netpackage if online
            ClientInfo cInfoOther = CF_Player.GetClientInfo(userOther);
            if (cInfoOther != null)
            {
                log.Out($"fake removed by kicked player");
                cInfoOther.SendPackage(NetPackageManager.GetPackage<NetPackagePersistentPlayerState>().Setup(pData, userOther, EnumPersistentPlayerDataReason.ACL_Removed));
            }
        }
    }
    public static void GroupInfo(ClientInfo _cInfo)
    {
        if (!GetGroup(_cInfo.InternalId, out CF_PlayerGroup group))
        {
            CF_Player.Message($"{prefix} You are currently not in a group. Invite another player to start a group.", _cInfo);
            return;
        }

        CF_Player.Message($"{prefix} == GROUP INFO ==", _cInfo);
        CF_Player.Message($"Tag: {group.groupTag} Name: {group.groupName}", _cInfo);

        PersistentPlayerData ppd = CF_Player.GetPersistentPlayerData(group.GetLeader());
        CF_Player.Message($"Leader: {ppd.PlayerName} Members: {group.members.Count}", _cInfo);
        if (group.officers.Count > 0)
            CF_Player.Message($"Officers: {group.GetOfficersNames()}", _cInfo);

        CF_Player.Message($"{prefix} == GROUP MEMBERS ==", _cInfo);
        int count = 0;
        foreach (KeyValuePair<string, DateTime> kv in group.members)
        {
            count++;
            PersistentPlayerData ppd2 = CF_Player.GetPersistentPlayerData(PlatformUserIdentifierAbs.FromPlatformAndId("EOS", kv.Key));
            CF_Player.Message($"#{count} - {ppd2.PlayerName} (joined {TimePassed(kv.Value)} ago)", _cInfo);
        }
    }
    public static string TimePassed(DateTime when)
    {
        StringBuilder sb = new StringBuilder();
        TimeSpan ts = DateTime.UtcNow.Subtract(when);

        if (ts.TotalDays > 4)
            sb.AppendFormat("{0}d", (int)ts.TotalDays);
        else if (ts.TotalDays > 0)
            sb.AppendFormat("{0}d {1}h", (int)ts.TotalDays, (int)ts.TotalHours);
        else if (ts.TotalHours > 0)
            sb.AppendFormat("{0}h {1}m", (int)ts.TotalHours, (int)ts.TotalMinutes);
        else sb.AppendFormat(" {0}m", (int)ts.TotalMinutes);

        return sb.ToString();
    }
    public static bool GetGroup(PlatformUserIdentifierAbs user, out CF_PlayerGroup group)
    {
        group = null;

        foreach (CF_PlayerGroup g in playerDataCache.Groups.Values)
        {
            if (!g.IsMember(user))
                continue;

            group = g;
            return true;
        }

        return false;
    }
    public static void StartupChecks()
    {
        foreach (CF_PlayerGroup g in playerDataCache.Groups.Values)
        {
            PlatformUserIdentifierAbs leader = g.GetLeader();
            PersistentPlayerData ppdL = CF_Player.GetPersistentPlayerData(leader);

            foreach (string m in g.members.Keys)
            {
                PlatformUserIdentifierAbs member = PlatformUserIdentifierAbs.FromPlatformAndId("EOS", m);
                PersistentPlayerData ppd = CF_Player.GetPersistentPlayerData(member);

                foreach (string m2 in g.members.Keys)
                {
                    PlatformUserIdentifierAbs member2 = PlatformUserIdentifierAbs.FromPlatformAndId("EOS", m2);
                    PersistentPlayerData ppd2 = CF_Player.GetPersistentPlayerData(member2);

                    // Are allies?
                    if (!m.Equals(m2) && ppd2 != null && ppd != null && !CF_PersistentPlayer.IsFriendOf(ppd, ppd2))
                    {
                        log.Out($"Startup :: {ppd.PlayerName} was not allied with {ppd2.PlayerName}");
                        ppd.AddPlayerToACL(member2);
                        ppd2.AddPlayerToACL(member);
                    }
                }
            }
        }
    }
    public static void ExecCommand(ClientInfo _cInfo, List<string> _argList)
    {
        if (_argList.Count > 0)
        {
            switch (_argList[0])
            {
                case "list":
                case "info":
                    GroupInfo(_cInfo);
                    return;
                case "kick":
                    KickMember(_cInfo, _argList);
                    return;
                case "setname":
                    SetName(_cInfo, _argList);
                    return;
                case "settag":
                    SetTag(_cInfo, _argList);
                    return;
                case "leave":
                    Leave(_cInfo, _argList);
                    return;
                case "promote":
                    return;
                case "demote":
                    return;
                case "makeleader":
                    return;
                case "delete":
                    return;
            }
        }

        CF_Player.Message($"{prefix} == GROUP SYSTEM ==", _cInfo);
        CF_Player.Message($"Usage:", _cInfo);
        CF_Player.Message($"!group info - Group info & member list", _cInfo);
        CF_Player.Message($"!group leave - Leave your group", _cInfo);
        CF_Player.Message($"!group kick <ID> - Kick member (only needed if you are not allied)", _cInfo);
        CF_Player.Message($"!group setname <name> - Set group name", _cInfo);
        CF_Player.Message($"!group settag <name> - Set group chat tag", _cInfo);
    }
    public static List<string> GetAllPlayerGroupNames()
    {
        List<string> names = new List<string>();
        foreach (CF_PlayerGroup group in playerDataCache.Groups.Values)
            names.Add(group.groupName);
        return names;
    }
    public static List<string> GetAllPlayerGroupTags()
    {
        List<string> names = new List<string>();
        foreach (CF_PlayerGroup group in playerDataCache.Groups.Values)
            names.Add(group.groupTag);
        return names;
    }
}
