using System;
using System.Collections.Generic;
using System.Linq;

public class CF_PlayerGroup
{
    public string groupName { get; set; } = "";
    public string groupTag { get; set; } = "";
    public string leader { get; set; }
    public string leaderName { get; set; } = "";
    public List<string> officers { get; set; } = new List<string>();
    public Dictionary<string, DateTime> members { get; set; } = new Dictionary<string, DateTime>();
    public CF_PlayerGroup() { } // Fix JSON
    public CF_PlayerGroup(ClientInfo _cInfoLeader, ClientInfo _cInfo2)
    {
        leader = _cInfoLeader.InternalId.ReadablePlatformUserIdentifier;
        members.Add(_cInfoLeader.InternalId.ReadablePlatformUserIdentifier, DateTime.UtcNow);
        members.Add(_cInfo2.InternalId.ReadablePlatformUserIdentifier, DateTime.UtcNow);
    }
    public void AddMember(PlatformUserIdentifierAbs user)
    {
        if (!members.ContainsKey(user.ReadablePlatformUserIdentifier))
            members.Add(user.ReadablePlatformUserIdentifier, DateTime.UtcNow);
    }
    public void RemoveMember(PlatformUserIdentifierAbs user)
    {
        if (officers.Contains(user.ReadablePlatformUserIdentifier))
            officers.Remove(user.ReadablePlatformUserIdentifier);
        members.Remove(user.ReadablePlatformUserIdentifier);
    }
    public bool IsLeader(ClientInfo _cInfo) => leader.Equals(_cInfo.InternalId.ReadablePlatformUserIdentifier);
    public bool IsLeader(PlatformUserIdentifierAbs _user) => leader.Equals(_user.ReadablePlatformUserIdentifier);
    public bool IsOfficer(ClientInfo _cInfo) => officers.Contains(_cInfo.InternalId.ReadablePlatformUserIdentifier);
    public bool IsOfficer(PlatformUserIdentifierAbs _user) => officers.Contains(_user.ReadablePlatformUserIdentifier);
    public bool IsMember(ClientInfo _cInfo) => members.Keys.Contains(_cInfo.InternalId.ReadablePlatformUserIdentifier);
    public bool IsMember(PlatformUserIdentifierAbs _user) => members.Keys.Contains(_user.ReadablePlatformUserIdentifier);
    public PlatformUserIdentifierAbs GetLeader() => PlatformUserIdentifierAbs.FromPlatformAndId("EOS", leader);
    public string GetLeaderName() => leaderName;
    public string GetOfficersNames()
    {
        List<string> names = new List<string>();
        foreach (string officer in officers)
        {
            PlatformUserIdentifierAbs user = PlatformUserIdentifierAbs.FromPlatformAndId("EOS", officer);

            if (CF_Player.TryGetClientInfo(user, out ClientInfo cInfo))
            {
                names.Add(cInfo.playerName);
                continue;
            }

            PersistentPlayerData pData = CF_Player.GetPersistentPlayerData(user);
            if (pData == null)
                continue;

            names.Add(pData.PlayerName);
        }
        return CF_Format.ListToString(names);
    }
}
