using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CF_PlayerTracker
{
    private CF_JsonFile<List<CF_PlayerTrackerEntry>> playerUpdateFile;

    public CF_PlayerTracker(string playerUpdateFilePath)
    {
        playerUpdateFile = new CF_JsonFile<List<CF_PlayerTrackerEntry>>(playerUpdateFilePath, new List<CF_PlayerTrackerEntry>());
    }

    public void SavePlayerUpdate(CF_PlayerTrackerEntry entry)
    {
        lock (playerUpdateFile)
        {
            playerUpdateFile.data.Add(entry);
            playerUpdateFile.Save();
        }
    }

    public List<CF_PlayerTrackerEntry> GetPlayerUpdates()
    {
        lock (playerUpdateFile)
        {
            return playerUpdateFile.data;
        }
    }
}