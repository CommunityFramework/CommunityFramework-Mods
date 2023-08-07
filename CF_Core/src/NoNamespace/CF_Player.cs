using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CF_Player
{
    // ClientInfo 
    public static List<ClientInfo> GetClients() => ConnectionManager.Instance.Clients.List.ToList();
    public static ClientInfo GetClient(int _entityId) => ConnectionManager.Instance.Clients.ForEntityId(_entityId);
    public static bool GetClient(int _entityI, out ClientInfo _cInfo)
    {
        _cInfo = GetClient(_entityI);
        return _cInfo != null;
    }
    public static ClientInfo GetClient(EntityPlayer _player) => ConnectionManager.Instance.Clients.ForEntityId(_player.entityId);
    public static bool GetClient(EntityPlayer _player, out ClientInfo _cInfo) 
    {
        _cInfo = GetClient(_player);
        return _cInfo  != null; 
    }
    public static ClientInfo GetClient(PlatformUserIdentifierAbs _user) => ConnectionManager.Instance.Clients.ForUserId(_user);
    public static bool GetClient(PlatformUserIdentifierAbs _user, out ClientInfo _cInfo)
    {
        _cInfo = GetClient(_user);
        return _cInfo != null;
    }
    public static ClientInfo GetClient(string _platform, string _userId) => GetClient(PlatformUserIdentifierAbs.FromPlatformAndId(_platform, _userId, false));
    public static bool GetClient(string _platform, string _userId, out ClientInfo _cInfo)
    {
        _cInfo = GetClient(_platform, _userId);
        return _cInfo != null;
    }
    public static ClientInfo GetClient(long _peer) => ConnectionManager.Instance.Clients.ForLiteNetPeer(_peer);
    public static ClientInfo GetClient(string _nameOrId) => ConnectionManager.Instance.Clients.GetForNameOrId(_nameOrId);

    // EntityPlayer
    public static List<EntityPlayer> GetPlayers() => new List<EntityPlayer>(GameManager.Instance.World.Players.dict.Values.ToList());
    public static EntityPlayer GetPlayer(ClientInfo _cInfo) => GetPlayer(_cInfo.entityId);
    public static EntityPlayer GetPlayer(int _entityId) => GameManager.Instance.World.Players.dict.ContainsKey(_entityId)? GameManager.Instance.World.Players.dict[_entityId] : null;
    public static bool GetPlayer(ClientInfo _cInfo, out EntityPlayer _player)
    {
        _player = GetPlayer(_cInfo.entityId);
        return _player != null;
    }
    public static bool GetPlayer(int _entityId, out EntityPlayer _player)
    {
        _player = GetPlayer(_entityId);
        return _player != null;
    }

    // PersistentPlayerData
    public static PersistentPlayerData GetPersistent(EntityPlayer _player) => GetPersistent(_player.entityId);
    public static PersistentPlayerData GetPersistent(ClientInfo _cInfo) => GetPersistent(_cInfo.entityId);
    public static PersistentPlayerData GetPersistent(int _entityId) => GameManager.Instance.persistentPlayers?.GetPlayerDataFromEntityID(_entityId) ?? null;
    public static PersistentPlayerData GetPersistent(PlatformUserIdentifierAbs _user) => GameManager.Instance.persistentPlayers?.GetPlayerData(_user) ?? null;
    
    // Permission
    public static int GetPermission(EntityPlayer _player) => GetPermission(GetClient(_player));
    public static int GetPermission(ClientInfo _cInfo) => GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);
    public static int GetPermission(PlatformUserIdentifierAbs _user) => GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_user);

    // Is player online
    public static bool IsOnline(int _entityId) => GetClient(_entityId) != null;
    public static bool IsOnline(PlatformUserIdentifierAbs _user) => GetClient(_user) != null;
    public static bool IsOnline(string _platform, string _platformAuth) => GetClient(_platform, _platformAuth) != null;
    public static bool IsOnline(long _peer) => GetClient(_peer) != null;
    public static bool IsOnline(string _nameOrId) => GetClient(_nameOrId) != null;

    // Get nearest player
    public static bool GetNearestPlayer(Vector3i pos, out EntityPlayer nearestPlayer, out float distance, float maxDistance = 0, bool aliveOnly = true)
    {
        nearestPlayer = null;
        distance = maxDistance;

        List<EntityPlayer> players = GameManager.Instance.World.Players.list;

        if (players != null && players.Count > 0)
        {
            for (int i = 0; i < players.Count; i++)
            {
                // Skip dead players
                if (aliveOnly && players[i].IsDead() || !players[i].Spawned)
                    continue;

                // Get distance
                float distance2 = (pos.x - players[i].position.x) *
                    (pos.x - players[i].position.x) +
                    (pos.z - players[i].position.z) *
                    (pos.z - players[i].position.z);

                // Is nearer?
                if (nearestPlayer == null || distance > distance2)
                {
                    nearestPlayer = players[i];
                    distance = distance2;
                }
            }

            return nearestPlayer != null;
        }

        return false;
    }
    // Game Events
    public static bool FireGameEvent(ClientInfo _cInfo, string _sequenceName)
    {
        if (!GetPlayer(_cInfo, out EntityPlayer _player) || !_player.IsAlive() || !_player.IsSpawned())
        {
            throw new Exception($"GameEvent can't be triggered only on alive players: {_cInfo}");
        }

        GameEventManager.Current.HandleAction(_sequenceName, null, _player, false, "");
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(_sequenceName, _cInfo.entityId, "", "", NetPackageGameEventResponse.ResponseTypes.Approved));
        return true;
    }
    // Close open windows (including the crafting)
    public static void CloseAllOpenWindows(ClientInfo _cInfo)
    {
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageCloseAllWindows>().Setup(_cInfo.entityId));
    }
    // Write text to client console
    public static void Console(ClientInfo _cInfo, string _msg, bool _bExecute = false)
    {
        if (_cInfo == null)
            throw new Exception($"ConsoleMessageFilter.Out reported: Can't send to client=null.");

        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(_msg, _bExecute));
    }
    // Write multiple text lines to client console (can be usefull instead of using line breaks)
    public static void Console(ClientInfo _cInfo, List<string> _msgs, bool _bExecute = false)
    {
        if (_cInfo == null)
            throw new Exception($"ConsoleMessageFilter.Out reported: Can't send to client=null.");

        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(_msgs, _bExecute));
    }
    // Message all or specific player in chat
    public static void Message(string msg)
    {
        List<ClientInfo> _clientList = CF_Player.GetClients();
        if (_clientList == null || _clientList.Count < 1)
            return;

        for (int i = 0; i < _clientList.Count; i++)
        {
            ClientInfo _cInfo2 = _clientList[i];
            if (_cInfo2 == null)
                continue;
            Message(msg, _cInfo2);
        }
    }
    public static void Message(string msg, ClientInfo cInfo)
    {
        if (cInfo != null)
            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, msg + "[-]", "", false, null));
    }
    // Message online persistent allies of a player in chat
    public static void MessageFriends(PlatformUserIdentifierAbs _UserId, string msg)
    {
        PersistentPlayerData ppd = GameManager.Instance.persistentPlayers.GetPlayerData(_UserId);

        if (ppd == null)
            return;

        if (ppd.ACL != null)
        {
            foreach (PlatformUserIdentifierAbs friend in ppd.ACL)
            {
                ClientInfo _cInfo = CF_Player.GetClient(friend);
                if (_cInfo == null)
                    continue;
                Message(msg, _cInfo);
            }
        }
    }
    // Show toolbelt message
    public static void ShowToolbeltMessage(ClientInfo _cInfo, string _toolbeltMessage)
    {
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageShowToolbeltMessage>().Setup(_toolbeltMessage, ""));
    }
    // Show toolbelt message with sound
    public static void ShowToolbeltMessageWithSound(ClientInfo _cInfo, string _toolbeltMessage, string _sound)
    {
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageShowToolbeltMessage>().Setup(_toolbeltMessage, _sound));
    }
    // Play sound for player
    public static void PlaySound(ClientInfo _cInfo, string _sound)
    {
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageShowToolbeltMessage>().Setup("", _sound));
    }
    // Open website in system browser (needs confirmation by the client)
    public static void OpenUrl(ClientInfo _cInfo, string websiteURL)
    {
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("createwebuser " + Convert.ToBase64String(Encoding.UTF8.GetBytes(websiteURL)), true));
    }
    // Teleport
    public static void Teleport(ClientInfo _cInfo, Vector3 _pos)
    {
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(_pos));
    }
    public static void Teleport(ClientInfo _cInfo, Vector3 _pos, Vector3 _viewDirection)
    {
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(_pos, _viewDirection));
    }
    public static void Teleport(ClientInfo _cInfo, Vector3 _pos, Vector3 _viewDirection, bool _onlyIfNotflying)
    {
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(_pos, _viewDirection, _onlyIfNotflying));
    }
}