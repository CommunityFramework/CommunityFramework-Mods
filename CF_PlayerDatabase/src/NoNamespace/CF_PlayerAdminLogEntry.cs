using System;
using System.Collections.Generic;
using System.Linq;

public class CF_PlayerAdminLogEntry
{
    public int Id { get; }
    public DateTime Timestamp { get; }
    public string Admin { get; }
    public string Reason { get; }
    public int Points { get; }
    public DateTime? ExpireDate { get; }

    public CF_PlayerAdminLogEntry(int id, string reason, string admin, int points, DateTime? expireDate)
    {
        Id = id;
        Reason = reason;
        Timestamp = DateTime.UtcNow;
        Points = points;
        ExpireDate = expireDate;
        Admin = admin;
    }
}