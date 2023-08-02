using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CF_PlayerAdminLog
{
    private List<CF_PlayerAdminLogEntry> entries = new List<CF_PlayerAdminLogEntry>();
    private int nextId = 1;
    private readonly object lockObj = new object();

    public int GetNextId()
    {
        lock (lockObj)
        {
            return nextId++;
        }
    }

    public List<CF_PlayerAdminLogEntry> GetAllEntries()
    {
        lock (lockObj)
        {
            return entries.ToList();
        }
    }

    public CF_PlayerAdminLogEntry GetEntryById(int id)
    {
        lock (lockObj)
        {
            return entries.FirstOrDefault(e => e.Id == id);
        }
    }

    public List<CF_PlayerAdminLogEntry> GetEntriesByAdmin(string admin)
    {
        lock (lockObj)
        {
            return entries.Where(e => e.Admin == admin).ToList();
        }
    }

    public List<CF_PlayerAdminLogEntry> GetActiveEntries()
    {
        lock (lockObj)
        {
            return entries.Where(e => e.ExpireDate == null || e.ExpireDate >= DateTime.UtcNow).ToList();
        }
    }

    public List<CF_PlayerAdminLogEntry> GetExpiredEntries()
    {
        lock (lockObj)
        {
            return entries.Where(e => e.ExpireDate != null && e.ExpireDate < DateTime.UtcNow).ToList();
        }
    }

    public int GetTotalScoreOfActiveEntries()
    {
        lock (lockObj)
        {
            return GetActiveEntries().Sum(e => e.Points);
        }
    }

    public int GetTotalScoreOfExpiredEntries()
    {
        lock (lockObj)
        {
            return GetExpiredEntries().Sum(e => e.Points);
        }
    }

    public int GetTotalScoreOfAllEntries()
    {
        lock (lockObj)
        {
            return entries.Sum(e => e.Points);
        }
    }

    public void RemoveExpiredEntries()
    {
        lock (lockObj)
        {
            var expiredEntries = GetExpiredEntries();
            entries.RemoveAll(e => expiredEntries.Contains(e));
        }
    }

    public CF_PlayerAdminLogEntry CreatePenaltyLogEntry(string reason, string admin, int points, DateTime? expireDate)
    {
        int nextId = GetNextId();
        var logEntry = new CF_PlayerAdminLogEntry(nextId, reason, admin, points, expireDate);
        lock (lockObj)
        {
            entries.Add(logEntry);
        }
        return logEntry;
    }
    public DateTime? CalculateExpirationTimeForJoin(int maxScore)
    {
        int totalActivePoints = GetTotalScoreOfActiveEntries();
        int remainingPoints = Math.Max(0, maxScore - totalActivePoints);
        DateTime? expirationTime = null;
        if (remainingPoints > 0)
            expirationTime = DateTime.UtcNow.AddHours(remainingPoints);

        return expirationTime;
    }
    public bool IsBannedByScore(int maxScore, out DateTime? banExpirationTime)
    {
        banExpirationTime = CalculateExpirationTimeForJoin(maxScore);
        return banExpirationTime > DateTime.UtcNow;
    }
}