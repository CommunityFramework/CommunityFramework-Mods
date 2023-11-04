using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static CF_PlayerTracker.API;

public class CF_PlayerTrackerManager
{
    private static string fileName = "PlayerTrackerDB.bin";
    private static string filePath;
    private static CF_PlayerTrackerData database = new CF_PlayerTrackerData();
    private static CF_Log log = new CF_Log("CF_PlayerTracker");
    public void Init()
    {
        filePath = Path.Combine(mod.modDatabasePath, fileName);
        LoadPlayerUpdates();
    }
    private void LoadPlayerUpdates()
    {
        if (File.Exists(filePath))
        {
            if (TryLoadFromFile(filePath))
            {
                return; // Successfully loaded, no need to proceed further
            }
        }

        // Attempt to load from backup if main file loading failed
        string backupFilePath = filePath + ".bak";
        if (File.Exists(backupFilePath))
        {
            log.Error("Loading from main file failed, attempting to load from backup file.");
            if (TryLoadFromFile(backupFilePath))
            {
                // If loading from backup succeeds, replace the corrupted main file with the backup
                File.Copy(backupFilePath, filePath, overwrite: true);
                log.Out("Successfully restored player updates from backup file.");
            }
            else
            {
                // If loading from backup also fails, initialize an empty list to avoid null references
                database = new CF_PlayerTrackerData();
                log.Error("Loading from backup file also failed. Initialized with an empty list.");
            }
        }
        else
        {
            // No backup file exists, initialize an empty list
            database = new CF_PlayerTrackerData();
            log.Out("No player updates file or backup to load from. Initialized with an empty list.");
        }
    }
    private bool TryLoadFromFile(string filePath)
    {
        try
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                database = (CF_PlayerTrackerData)formatter.Deserialize(stream);
                return true; // Successfully loaded
            }
        }
        catch (Exception ex)
        {
            log.Error($"Failed to deserialize player updates from {filePath}: {ex.Message}");
            return false; // Failed to load
        }
    }
    public void Save()
    {
        lock (database)
        {
            SavePlayerUpdatesToFile();
        }
    }
    public void AddPosRecord(CF_PlayerTrackerPosRecord entry, bool saveToFile = false)
    {
        lock (database)
        {
            database.positionData.Add(entry);
            if (saveToFile)
            {
                SavePlayerUpdatesToFile();
            }
        }
    }
    public void AddPosRecord(CF_PlayerTrackerInvRecord entry, bool saveToFile = false)
    {
        lock (database)
        {
            database.inventoryData.Add(entry);
            if (saveToFile)
            {
                SavePlayerUpdatesToFile();
            }
        }
    }
    private void SavePlayerUpdatesToFile()
    {
        string tempFilePath = filePath + ".tmp";
        string backupFilePath = filePath + ".bak";

        // Serialize to a temporary file first to ensure atomic writes
        using (MemoryStream memoryStream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(memoryStream, database);
            File.WriteAllBytes(tempFilePath, memoryStream.ToArray()); // Atomic operation
        }

        // Replace the old backup with the previous data file
        if (File.Exists(backupFilePath))
        {
            File.Delete(backupFilePath);
        }
        if (File.Exists(filePath))
        {
            File.Move(filePath, backupFilePath);
        }

        // Move the temporary file to the main data file location
        File.Move(tempFilePath, filePath);
    }

    public List<CF_PlayerTrackerPosRecord> GetPosData()
    {
        lock (database)
        {
            return new List<CF_PlayerTrackerPosRecord>(database.positionData);
        }
    }
}