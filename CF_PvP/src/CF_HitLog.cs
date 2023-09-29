using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CF_PvP.API;

public class CF_HitLog
{
    // Concurrent data structures for thread-safety
    private static readonly object entriesLock = new object();
    private static readonly object entriesByPlayerIdLock = new object();
    private static readonly object totalDamageByPlayerLock = new object();
    private static readonly object latestHealthByPlayerLock = new object();

    public static HashSet<CF_HitLogEntry> entries = new HashSet<CF_HitLogEntry>();
    private static Dictionary<int, List<CF_HitLogEntry>> entriesByPlayerId = new Dictionary<int, List<CF_HitLogEntry>>();
    private static Dictionary<int, int> totalDamageByPlayer = new Dictionary<int, int>();
    private static Dictionary<int, int> latestHealthByPlayer = new Dictionary<int, int>();

    private static readonly List<Action<CF_HitLogEntry>> hitLogEntryCallbacks = new List<Action<CF_HitLogEntry>>();
    public static void RegisterHitLogEntryCallback(Action<CF_HitLogEntry> callback)
    {
        hitLogEntryCallbacks.Add(callback);
    }

    public static void AddEntry(ClientInfo source, ClientInfo attacker, ClientInfo victim, EntityPlayer playerA, EntityPlayer playerV, int damage, EnumDamageSource _damageSrc, EnumDamageTypes _damageTyp, int armorDamage, bool fatal, ItemValue weapon, Utils.EnumHitDirection direction, EnumBodyPartHit hitbox, float fps, bool blocked)
    {
        ThreadManager.AddSingleTaskMainThread("CF_AddHitLogEntry", (_taskInfo =>
        {
            var entry = new CF_HitLogEntry(source, attacker, victim, playerA, playerV, damage, _damageSrc, _damageTyp, armorDamage, fatal, weapon, direction, hitbox, fps);

            log.Out(entry.ToStringLog());

            // Invoke callbacks
            foreach (var callback in hitLogEntryCallbacks)
            {
                callback.Invoke(entry);
            }

            // Add the entry to the main list
            lock (entriesLock)
            {
                entries.Add(entry);
            }

            // Add the entry to the dictionary based on attacker and victim IDs
            lock (entriesByPlayerIdLock)
            {
                if (!entriesByPlayerId.ContainsKey(attacker.entityId))
                    entriesByPlayerId[attacker.entityId] = new List<CF_HitLogEntry>();
                entriesByPlayerId[attacker.entityId].Add(entry);

                if (!entriesByPlayerId.ContainsKey(victim.entityId))
                    entriesByPlayerId[victim.entityId] = new List<CF_HitLogEntry>();
                entriesByPlayerId[victim.entityId].Add(entry);
            }

            // Update cached data
            lock (totalDamageByPlayerLock)
            {
                if (!totalDamageByPlayer.ContainsKey(attacker.entityId))
                    totalDamageByPlayer[attacker.entityId] = 0;
                totalDamageByPlayer[attacker.entityId] += damage;

                if (!totalDamageByPlayer.ContainsKey(victim.entityId))
                    totalDamageByPlayer[victim.entityId] = 0;
                totalDamageByPlayer[victim.entityId] += damage;
            }

            lock (latestHealthByPlayerLock)
            {
                latestHealthByPlayer[victim.entityId] = playerV.Health;
            }
        }));
    }
    // All hit log entries for a specific player
    public static List<CF_HitLogEntry> GetEntriesForPlayer(int playerId)
    {
        return entriesByPlayerId.TryGetValue(playerId, out var playerEntries) ? playerEntries : new List<CF_HitLogEntry>();
    }
    public static List<CF_HitLogEntry> GetEntriesForPlayer(int playerId, DateTime? startTime = null, DateTime? endTime = null)
    {
        var query = GetEntriesForPlayer(playerId);

        if (startTime.HasValue)
            query = query.Where(entry => entry.timestamp >= startTime.Value).ToList();

        if (endTime.HasValue)
            query = query.Where(entry => entry.timestamp <= endTime.Value).ToList();

        return query;
    }
    // Within a specific time range
    public static List<CF_HitLogEntry> GetEntriesWithinTimeRange(DateTime startTime, DateTime endTime)
    {
        return entries.Where(entry => entry.timestamp >= startTime && entry.timestamp <= endTime).ToList();
    }
    // All fatal hit log entries
    public static List<CF_HitLogEntry> GetFatalOnly(List<CF_HitLogEntry> list = null)
    {
        if (list != null)
            return list.Where(entry => entry.fatal).ToList();
        return entries.Where(entry => entry.fatal).ToList();
    }
    public static List<CF_HitLogEntry> GetHitsOnly(List<CF_HitLogEntry> list = null)
    {
        return GetNonFatalOnly(list);
    }
    public static List<CF_HitLogEntry> GetNonFatalOnly(List<CF_HitLogEntry> list = null)
    {
        if (list != null)
            return list.Where(entry => !entry.fatal).ToList();
        return entries.Where(entry => !entry.fatal).ToList();
    }
    // All melee hits
    public static List<CF_HitLogEntry> GetMeleeOnly(List<CF_HitLogEntry> list = null)
    {
        if (list != null)
            return list.Where(entry => entry.fatal && entry.itemValue.ItemClass.IsDynamicMelee()).ToList();
        return entries.Where(entry => entry.fatal && entry.itemValue.ItemClass.IsDynamicMelee()).ToList();
    }
    // All gun hits
    public static List<CF_HitLogEntry> GetGunOnly(List<CF_HitLogEntry> list = null)
    {
        if (list != null)
            return list.Where(entry => entry.fatal && entry.itemValue.ItemClass.IsGun()).ToList();
        return entries.Where(entry => entry.fatal && entry.itemValue.ItemClass.IsGun()).ToList();
    }
    // Total damage dealt to a specific player
    public static int GetTotalDamageForPlayer(int playerId)
    {
        return totalDamageByPlayer.TryGetValue(playerId, out var totalDamage) ? totalDamage : 0;
    }
    public static int GetTotalDamageForPlayer(int playerId, DateTime? startTime = null, DateTime? endTime = null)
    {
        var query = GetEntriesForPlayer(playerId, startTime, endTime);
        return query.Sum(entry => entry.damage);
    }
    // Total damage dealt by a specific player
    public static int GetTotalDamageByPlayer(int playerId)
    {
        return totalDamageByPlayer.TryGetValue(playerId, out var totalDamage) ? totalDamage : 0;
    }
    public static int GetTotalDamageByPlayer(int playerId, DateTime? startTime = null, DateTime? endTime = null)
    {
        var query = GetEntriesForPlayer(playerId, startTime, endTime);
        return query.Sum(entry => entry.damage);
    }
    // High Damage
    public static List<CF_HitLogEntry> GetHighDamageEntries(int threshold)
    {
        return entries.Where(entry => entry.damage >= threshold).ToList();
    }
    // Low Health
    public static List<CF_HitLogEntry> GetLowHealthEntries(int threshold)
    {
        return entries.Where(entry => entry.healthA <= threshold).ToList();
    }
    // Specific Weapon Used
    public static List<CF_HitLogEntry> GetEntriesWithWeapon(string weaponName)
    {
        return entries.Where(entry => entry.weaponName.Equals(weaponName, StringComparison.OrdinalIgnoreCase)).ToList();
    }
    // Sorted by Damage
    public static List<CF_HitLogEntry> GetEntriesSortedByDamage(bool ascending = true)
    {
        return ascending
            ? entries.OrderBy(entry => entry.damage).ToList()
            : entries.OrderByDescending(entry => entry.damage).ToList();
    }
    // Players with Most Fatal Hits
    public static List<int> GetPlayersWithMostFatalHits(int count)
    {
        return entries
            .Where(entry => entry.fatal)
            .GroupBy(entry => entry.attackerId)
            .OrderByDescending(group => group.Count())
            .Take(count)
            .Select(group => group.Key)
            .ToList();
    }
    // Latest health of a player
    public static int GetLatestHealthForPlayer(int playerId)
    {
        return latestHealthByPlayer.TryGetValue(playerId, out var latestHealth) ? latestHealth : -1;
    }
    // Lowest health stage of a player
    public static int GetLowestHealthForPlayer(int playerId)
    {
        var playerEntries = GetEntriesForPlayer(playerId);
        return playerEntries.Count > 0 ? playerEntries.Min(entry => entry.healthA) : -1;
    }
    // Highest health stage of a player
    public static int GetHighestHealthForPlayer(int playerId)
    {
        var playerEntries = GetEntriesForPlayer(playerId);
        return playerEntries.Count > 0 ? playerEntries.Max(entry => entry.healthA) : -1;
    }
    // Average health stage of a player
    public static float GetAverageHealthForPlayer(int playerId)
    {
        var playerEntries = GetEntriesForPlayer(playerId);
        return playerEntries.Count > 0 ? (float)playerEntries.Average(entry => entry.healthA) : -1;
    }
    // Hit distances
    public static List<float> GetDistanceOfHits(int playerId, DateTime? startTime = null, DateTime? endTime = null)
    {
        var playerEntries = GetEntriesForPlayer(playerId, startTime, endTime);
        return playerEntries.Select(entry => Vector3.Distance(entry.attackerPos, entry.victimPos)).ToList();
    }
    // Kill distances
    public static List<float> GetDistanceOfKills(int playerId, DateTime? startTime = null, DateTime? endTime = null)
    {
        var playerEntries = GetEntriesForPlayer(playerId, startTime, endTime).Where(entry => entry.fatal);
        return playerEntries.Select(entry => Vector3.Distance(entry.attackerPos, entry.victimPos)).ToList();
    }
    // Cleanup
    public static void ClearOldEntries(DateTime expirationDate)
    {
        lock (entriesLock)
        {
            // Remove entries older than the specified expiration date from the main list
            entries.RemoveWhere(entry => entry.timestamp < expirationDate);
        }

        lock (entriesByPlayerIdLock)
        {
            // Remove entries older than the specified expiration date from the player ID dictionary
            foreach (var playerId in entriesByPlayerId.Keys.ToList())
            {
                entriesByPlayerId[playerId].RemoveAll(entry => entry.timestamp < expirationDate);
            }
        }

        lock (totalDamageByPlayerLock)
        {
            // Recalculate the total damage for players after removing old entries
            foreach (var playerId in totalDamageByPlayer.Keys.ToList())
            {
                totalDamageByPlayer[playerId] = GetTotalDamageForPlayer(playerId);
            }
        }

        lock (latestHealthByPlayerLock)
        {
            // Remove old health entries from the latest health dictionary
            var keysToRemove = latestHealthByPlayer.Keys.Where(playerId => !entriesByPlayerId.ContainsKey(playerId)).ToList();
            foreach (var playerId in keysToRemove)
            {
                latestHealthByPlayer.Remove(playerId);
            }
        }
    }
    // Ranking by damage dealt
    public static List<int> GetPlayerRankingByDamageDealt(DateTime? startTime = null, DateTime? endTime = null)
    {
        var query = totalDamageByPlayer;
        if (startTime.HasValue || endTime.HasValue)
        {
            query = query.Where(pair => GetEntriesForPlayer(pair.Key, startTime, endTime).Any()).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        return query.OrderByDescending(pair => pair.Value)
                    .Select(pair => pair.Key)
                    .ToList();
    }
    // Ranking by kills
    public static List<int> GetPlayerRankingByKills(DateTime? startTime = null, DateTime? endTime = null)
    {
        List<CF_HitLogEntry> query = (List<CF_HitLogEntry>)entries.Where(entry => entry.fatal);
        if (startTime.HasValue)
        {
            query = (List<CF_HitLogEntry>)query.Where(entry => entry.timestamp >= startTime.Value);
        }
        if (endTime.HasValue)
        {
            query = (List<CF_HitLogEntry>)query.Where(entry => entry.timestamp <= endTime.Value);
        }

        return query.GroupBy(entry => entry.attackerId)
                    .OrderByDescending(group => group.Count())
                    .Select(group => group.Key)
                    .ToList();
    }
    // Ranking by hit accuracy
    public static List<int> GetPlayerRankingByHitAccuracy(DateTime? startTime = null, DateTime? endTime = null)
    {
        List<CF_HitLogEntry> query = entries.ToList();
        if (startTime.HasValue)
        {
            query = (List<CF_HitLogEntry>)query.Where(entry => entry.timestamp >= startTime.Value);
        }
        if (endTime.HasValue)
        {
            query = (List<CF_HitLogEntry>)query.Where(entry => entry.timestamp <= endTime.Value);
        }

        return query.GroupBy(entry => entry.attackerId)
                    .Where(group => group.Count() > 0)
                    .OrderByDescending(group => group.Count(entry => entry.fatal) / (float)group.Count())
                    .Select(group => group.Key)
                    .ToList();
    }
    // Ranking by hit average amage per hit
    public static List<int> GetPlayerRankingByAverageDamagePerHit(DateTime? startTime = null, DateTime? endTime = null)
    {
        List<CF_HitLogEntry> query = entries.ToList();
        if (startTime.HasValue)
        {
            query = (List<CF_HitLogEntry>)query.Where(entry => entry.timestamp >= startTime.Value);
        }
        if (endTime.HasValue)
        {
            query = (List<CF_HitLogEntry>)query.Where(entry => entry.timestamp <= endTime.Value);
        }

        return query.GroupBy(entry => entry.attackerId)
                    .Where(group => group.Count() > 0)
                    .OrderByDescending(group => group.Sum(entry => entry.damage) / (float)group.Count())
                    .Select(group => group.Key)
                    .ToList();
    }
    // Ranking by any custom criteria
    public static List<int> GetPlayerRankingByCriteria(Func<List<CF_HitLogEntry>, int> rankingCriteria, DateTime? startTime = null, DateTime? endTime = null)
    {
        var query = entries.ToList();
        if (startTime.HasValue)
        {
            query = query.Where(entry => entry.timestamp >= startTime.Value).ToList();
        }
        if (endTime.HasValue)
        {
            query = query.Where(entry => entry.timestamp <= endTime.Value).ToList();
        }

        return query.GroupBy(entry => entry.attackerId)
                    .Where(group => group.Count() > 0)
                    .OrderByDescending(group => rankingCriteria(group.ToList()))
                    .Select(group => group.Key)
                    .ToList();
    }
    // Ranking by longest kill distance
    public static List<int> GetPlayerRankingByLongestKillDistance(DateTime? startTime = null, DateTime? endTime = null)
    {
        var query = entries.Where(entry => entry.fatal);
        if (startTime.HasValue)
        {
            query = query.Where(entry => entry.timestamp >= startTime.Value);
        }
        if (endTime.HasValue)
        {
            query = query.Where(entry => entry.timestamp <= endTime.Value);
        }

        return query.OrderByDescending(entry => Vector3.Distance(entry.attackerPos, entry.victimPos))
                    .Select(entry => entry.attackerId)
                    .ToList();
    }
    // Ranking by most melee kills
    public static List<int> GetPlayerRankingByMostMeleeKills(DateTime? startTime = null, DateTime? endTime = null)
    {
        var query = entries.Where(entry => entry.fatal && entry.itemValue.ItemClass.IsDynamicMelee());
        if (startTime.HasValue)
        {
            query = query.Where(entry => entry.timestamp >= startTime.Value);
        }
        if (endTime.HasValue)
        {
            query = query.Where(entry => entry.timestamp <= endTime.Value);
        }

        return query.GroupBy(entry => entry.attackerId)
                    .OrderByDescending(group => group.Count())
                    .Select(group => group.Key)
                    .ToList();
    }
}
