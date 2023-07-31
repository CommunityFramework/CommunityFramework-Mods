using System;
using System.Collections.Generic;
using System.Linq;
using static Prefab;

public class HitLog
{
    public static List<HitLogEntry> entries = new List<HitLogEntry>();

    // Method to add a new hit log entry to the list
    public static void AddEntry(HitLogEntry entry)
    {
        entries.Add(entry);
    }
    public static void AddEntry(DateTime timestamp, ClientInfo attacker, ClientInfo victim, EntityPlayer playerA, EntityPlayer playerV, int damage, int armor, int health, float stamina, bool fatal, string weapon, int distance, Utils.EnumHitDirection direction, EnumBodyPartHit hitbox)
    {
        HitLogEntry entry = new HitLogEntry(DateTime timestamp, ClientInfo attacker, ClientInfo victim, EntityPlayer playerA, EntityPlayer playerV, int damage, int armor, int health, float stamina, bool fatal, string weapon, int distance, Utils.EnumHitDirection direction, EnumBodyPartHit hitbox);
        entries.Add(entry);
    }
    // Method to retrieve all hit log entries for a specific player
    public static List<HitLogEntry> GetEntriesForPlayer(int playerId)
    {
        return entries.Where(entry => entry.attackerId == playerId || entry.victim == playerId).ToList();
    }
    // Method to retrieve hit log entries within a specific time range
    public static List<HitLogEntry> GetEntriesWithinTimeRange(DateTime startTime, DateTime endTime)
    {
        return entries.Where(entry => entry.timestamp >= startTime && entry.timestamp <= endTime).ToList();
    }
    // Method to retrieve all fatal hit log entries
    public static List<HitLogEntry> GetFatalEntries()
    {
        return entries.Where(entry => entry.fatal).ToList();
    }
    // Method to retrieve the total damage dealt to a specific player
    public static int GetTotalDamageForPlayer(int playerId)
    {
        return entries.Where(entry => entry.victim == playerId).Sum(entry => entry.damage);
    }
    // Method to retrieve the total damage dealt by a specific player
    public static int GetTotalDamageByPlayer(int playerId)
    {
        return entries.Where(entry => entry.attackerId == playerId).Sum(entry => entry.damage);
    }
    // Method to Retrieve Entries with High Damage
    public static List<HitLogEntry> GetHighDamageEntries(int threshold)
    {
        return entries.Where(entry => entry.damage >= threshold).ToList();
    }
    // Method to Retrieve Entries with Low Health
    public static List<HitLogEntry> GetLowHealthEntries(int threshold)
    {
        return entries.Where(entry => entry.health <= threshold).ToList();
    }
    // Method to Retrieve Entries with Specific Weapon Used
    public static List<HitLogEntry> GetEntriesWithWeapon(string weaponName)
    {
        return entries.Where(entry => entry.weapon.Equals(weaponName, StringComparison.OrdinalIgnoreCase)).ToList();
    }
    // Method to Retrieve Entries Sorted by Damage
    public static List<HitLogEntry> GetEntriesSortedByDamage(bool ascending = true)
    {
        return ascending
            ? entries.OrderBy(entry => entry.damage).ToList()
            : entries.OrderByDescending(entry => entry.damage).ToList();
    }
    // Method to Retrieve Players with Most Fatal Hits
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
    // Method to detect potential aimbot usage by a player
    public static bool DetectAimbotUsage(int playerId)
    {
        // Get all hit log entries for the specified player as the attacker
        var playerEntries = entries.Where(entry => entry.attackerId == playerId).ToList();

        // If the player has only a few entries, aimbot detection is not meaningful
        if (playerEntries.Count < 10)
            return false;

        // Calculate average hit distance and hitbox distribution
        float averageDistance = (float)playerEntries.Average(entry => entry.distance);
        Dictionary<EnumBodyPartHit, int> hitboxCounts = new Dictionary<EnumBodyPartHit, int>();

        foreach (var entry in playerEntries)
        {
            if (hitboxCounts.ContainsKey(entry.hitbox))
                hitboxCounts[entry.hitbox]++;
            else
                hitboxCounts[entry.hitbox] = 1;
        }

        // Check for suspiciously high accuracy (e.g., hitting only headshots at long distances)
        int headshotCount;
        hitboxCounts.TryGetValue(EnumBodyPartHit.Head, out headshotCount);
        float headshotRatio = (float)headshotCount / playerEntries.Count;
        if (headshotRatio > 0.8 && averageDistance > 50)
        {
            // The player might be using an aimbot
            return true;
        }

        // No indication of aimbot usage
        return false;
    }
}
