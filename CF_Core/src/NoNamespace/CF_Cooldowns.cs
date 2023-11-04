using System;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class CF_Cooldowns
{
    // Use ConcurrentDictionary for better thread-safety and performance
    private static ConcurrentDictionary<int, ConcurrentDictionary<string, DateTime>> cooldownData = new ConcurrentDictionary<int, ConcurrentDictionary<string, DateTime>>();
    private static string filePath;
    private static string fileName = "Cooldowns.json";
    private static FileSystemWatcher fileWatcher;
    private static bool savePending = false;
    public static void Init(CF_Mod mod)
    {
        filePath = Path.Combine(mod.modDatabasePath, fileName);
        Load();
        InitializeFileWatcher();
    }
    private static FileSystemWatcher InitializeFileWatcher()
    {
        var watcher = new FileSystemWatcher(filePath);
        watcher.Changed += OnFileChanged;
        watcher.EnableRaisingEvents = true;
        return watcher;
    }
    public static void Load()
    {
        if (!File.Exists(filePath))
            Save();

        string json = File.ReadAllText(filePath);
        cooldownData = JsonConvert.DeserializeObject<ConcurrentDictionary<int, ConcurrentDictionary<string, DateTime>>>(json);
    }
    public static async void Save()
    {
        if (savePending) return;
        savePending = true;

        await Task.Delay(1000);  // Debounce save operation

        string json = JsonConvert.SerializeObject(cooldownData);
        File.WriteAllText(filePath, json);

        savePending = false;
    }
    public static void Set(int entityId, string actionName, DateTime expiry)
    {
        cooldownData.AddOrUpdate(entityId, new ConcurrentDictionary<string, DateTime> { [actionName] = expiry }, (key, existing) => { existing[actionName] = expiry; return existing; });
        Save();
    }
    public static bool Get(int entityId, string actionName, out DateTime expiry)
    {
        expiry = DateTime.MinValue;
        return cooldownData.TryGetValue(entityId, out var actions) && actions.TryGetValue(actionName, out expiry);
    }
    public static int GetCooldownInSeconds(int entityId, string actionName)
    {
        if (Get(entityId, actionName, out DateTime expiry))
        {
            int remainingSeconds = (int)(expiry - DateTime.UtcNow).TotalSeconds;
            return Math.Max(remainingSeconds, 0);
        }
        return 0;
    }
    public static int GetCooldownInMinutes(int entityId, string actionName)
    {
        if (Get(entityId, actionName, out DateTime expiry))
        {
            int remainingMinutes = (int)(expiry - DateTime.UtcNow).TotalMinutes;
            return Math.Max(remainingMinutes, 0); 
        }
        return 0;
    }
    public static TimeSpan GetCooldownTimeSpan(int entityId, string actionName)
    {
        if (Get(entityId, actionName, out DateTime expiry))
        {
            TimeSpan remainingTime = expiry - DateTime.UtcNow;
            return remainingTime > TimeSpan.Zero ? remainingTime : TimeSpan.Zero;
        }
        return TimeSpan.Zero;
    }
    public static bool IsActive(int entityId, string actionName)
    {
        if (Get(entityId, actionName, out DateTime expiry))
        {
            return DateTime.UtcNow < expiry;
        }
        return false;
    }
    public static bool UsedOnce(int entityId, string actionName)
    {
        return cooldownData.ContainsKey(entityId) && cooldownData[entityId].ContainsKey(actionName);
    }
    private static void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        Load();
    }
    public static void RemoveCooldown(int entityId, string actionName)
    {
        if (cooldownData.TryGetValue(entityId, out var actions))
        {
            actions.TryRemove(actionName, out _);
            Save();
        }
    }
    public static void ClearCooldownsForEntity(int entityId)
    {
        if (cooldownData.TryRemove(entityId, out _))
        {
            Save();
        }
    }

}