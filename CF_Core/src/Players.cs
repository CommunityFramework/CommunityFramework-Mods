using System.Collections.Generic;
using System.Linq;

public class Players
{
    public static List<ClientInfo> GetClients() => ConnectionManager.Instance.Clients.List.ToList();
    public static ClientInfo GetClient(int _entityId) => ConnectionManager.Instance.Clients.ForEntityId(_entityId);
    public static ClientInfo GetClient(EntityPlayer _player) => ConnectionManager.Instance.Clients.ForEntityId(_player.entityId);
    public static ClientInfo GetClient(PlatformUserIdentifierAbs _user) => ConnectionManager.Instance.Clients.ForUserId(_user);
    public static ClientInfo GetClient(string _platform, string _platformAuth) => GetClient(PlatformUserIdentifierAbs.FromPlatformAndId(_platform, _platformAuth, false));
    public static ClientInfo GetClient(long _peer) => ConnectionManager.Instance.Clients.ForLiteNetPeer(_peer);
    public static ClientInfo GetClient(string _nameOrId) => ConnectionManager.Instance.Clients.GetForNameOrId(_nameOrId);

    public static EntityPlayer GetPlayer(ClientInfo _cInfo) => GetPlayer(_cInfo.entityId);
    public static EntityPlayer GetPlayer(int _entityId) => GameManager.Instance.World.Players.dict.ContainsKey(_entityId)? GameManager.Instance.World.Players.dict[_entityId] : null;

    public static PersistentPlayerData GetPersistent(EntityPlayer _player) => GetPersistent(_player.entityId);
    public static PersistentPlayerData GetPersistent(ClientInfo _cInfo) => GetPersistent(_cInfo.entityId);
    public static PersistentPlayerData GetPersistent(int _entityId) => GameManager.Instance.persistentPlayers?.GetPlayerDataFromEntityID(_entityId) ?? null;
    public static PersistentPlayerData GetPersistent(PlatformUserIdentifierAbs _user) => GameManager.Instance.persistentPlayers?.GetPlayerData(_user) ?? null;

    public static bool IsOnline(int _entityId) => GetClient(_entityId) != null;
    public static bool IsOnline(PlatformUserIdentifierAbs _user) => GetClient(_user) != null;
    public static bool IsOnline(string _platform, string _platformAuth) => GetClient(_platform, _platformAuth) != null;
    public static bool IsOnline(long _peer) => GetClient(_peer) != null;
    public static bool IsOnline(string _nameOrId) => GetClient(_nameOrId) != null;

    public static void SetCvar(ClientInfo _cInfo, string _cvarName, int amount) => SetCvar(_cInfo, _cvarName, (float)amount);
    public static void SetCvar(ClientInfo _cInfo, string _cvarName, float amount)
    {
        if (!string.IsNullOrEmpty(_cvarName))
            GetPlayer(_cInfo)?.SetCVar(_cvarName, amount);
    }
    public static bool GetNearestPlayer(Vector3i pos, out EntityPlayer nearestPlayer, out float distance)
    {
        nearestPlayer = null;
        distance = 0;

        // Get closest player
        List<EntityPlayer> players = GameManager.Instance.World.Players.list;

        if (players != null && players.Count > 0)
        {
            for (int i = 0; i < players.Count; i++)
            {
                // Skip dead players
                if (players[i].IsDead() || !players[i].Spawned)
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
    public static void GameEvent(ClientInfo _cInfo, string gameevent)
    {
        EntityPlayer player = GetPlayer(_cInfo.entityId);
        if (player == null || !player.IsAlive())
            return;

        GameEventManager.Current.HandleAction(gameevent, null, player, false, "");
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(gameevent, _cInfo.entityId, "", "", NetPackageGameEventResponse.ResponseTypes.Approved));
    }
}