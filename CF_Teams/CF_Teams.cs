using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

    private static float lastTeamUpdateTime = 0f;

    public static Dictionary<int, PlayerData> playerDataCache = new Dictionary<int, PlayerData>();
    private static readonly object lockObject = new object();

    public static void LoadData()
    {
        string filePath = Path.Combine(mod.modDatabasePath, "playerData.json");
        if (!File.Exists(filePath))
        {
            SaveData();
        }

        try
        {
            string json = File.ReadAllText(filePath);
            lock (lockObject)
            {
                playerDataCache = JsonConvert.DeserializeObject<Dictionary<int, PlayerData>>(json);
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
            File.WriteAllText(Path.Combine(mod.modDatabasePath, "playerData.json"), json);
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
        if (playerDataCache.TryGetValue(entityId, out PlayerData playerData))
        {
            members = ConvertToMinutes(playerData.TeamParty);
            return false;
        }

        members = null;
        return false;
    }
    public static bool GetAllyMembers(int entityId, out Dictionary<string, int> members)
    {
        if (playerDataCache.TryGetValue(entityId, out PlayerData playerData))
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
        if (!playerDataCache.TryGetValue(_cInfo.entityId, out PlayerData playerData))
            return;

        // Update Party
        EntityPlayer player = CF_Player.GetEntityPlayer(_cInfo);
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
    }
    public static int GetCachedTeamSize(int entityId)
    {
        if (playerDataCache.TryGetValue(entityId, out PlayerData playerData))
        {
            return playerData.CachedTeamSize;
        }
        return 0;
    }
    public static int GetTeamSize(int entityId, TimeSpan timeSpan)
    {
        if (playerDataCache.TryGetValue(entityId, out PlayerData playerData))
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
}
