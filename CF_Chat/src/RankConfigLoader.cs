using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static CF_Chat.API;

namespace CF_Chat
{
    public class RankConfigLoader
    {
        private static string fileName { get; } = "ChatRanks.json";
        private static string filePath { get; } = Path.Combine(mod.modConfigPath, fileName);
        private static FileSystemWatcher fileWatcher;

        public static void InitializeFileWatcher()
        {
            Directory.CreateDirectory(mod.modConfigPath);

            fileWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(filePath),
                Filter = Path.GetFileName(filePath),
                NotifyFilter = NotifyFilters.LastWrite
            };

            fileWatcher.Changed += OnChanged;
            fileWatcher.EnableRaisingEvents = true;
        }
        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            log.Out("Config file changed, reloading...");
            LoadRankConfig();
        }
        public static void LoadRankConfig()
        {
            log.Out($"Loading ranks from {filePath}");

            if (string.IsNullOrEmpty(filePath))
            {
                log.Error("File path is null or empty.");
                return;
            }

            if (!File.Exists(filePath))
            {
                log.Warn("Config file not found. Creating default config.");

                // Create a default config with example ranks
                rankConfigs = new Dictionary<string, RankConfig>
                {
                    {"Zipcore", new RankConfig(new RankConfigJson {
                        TagPre = "",
                        CustomName = "[000000]Zipcore",
                        PermissionLevel = 0,
                        Players = new List<string> { "Steam_76561198035410392" }
                    })},
                    {"Admin", new RankConfig(new RankConfigJson { TagPre = "[ffffff][[ff0000]A[ffffff]d[ff0000]m[ffffff]i[ff0000]n][ffffff] ", NameColor = "ff0000", PermissionLevel = 1 })},
                    {"Moderator", new RankConfig(new RankConfigJson { TagPre = "[ffffff][[0000ff]M[ffffff]o[0000ff]d[ffffff]e[0000ff]r[ffffff]a[0000ff]t[ffffff]o[0000ff]r][ffffff] ", NameColor = "[0000ff]", PermissionLevel = 2 })},
                    {"VIP_Fresh", new RankConfig(new RankConfigJson { TagPre = "[d4af37][[cd7f32]F[c0c0c0]r[ffd700]e[e5e4e2]s[d4af37]h][d4af37] ", NameColor = "[cd7f32]", PermissionLevel = 500, Playtime = 200 })},
                    {"VIP_Rookie", new RankConfig(new RankConfigJson { TagPre = "[d4af37][[cd7f32]R[c0c0c0]o[ffd700]o[e5e4e2]k[d4af37]i[cd7f32]e][d4af37] ", NameColor = "[cd7f32]", PermissionLevel = 500, Playtime = 200 })},
                    {"VIP_Veteran", new RankConfig(new RankConfigJson { TagPre = "[d4af37][[cd7f32]V[c0c0c0]e[ffd700]t[e5e4e2]e[d4af37]r[cd7f32]a[c0c0c0]n][d4af37] ", NameColor = "[c0c0c0]", PermissionLevel = 500, Playtime = 400 })},
                    {"VIP_Elite", new RankConfig(new RankConfigJson { TagPre = "[d4af37][[cd7f32]E[c0c0c0]l[ffd700]i[e5e4e2]t[d4af37]e][d4af37] ", NameColor = "[ffd700]", PermissionLevel = 500, Playtime = 600 })},
                    {"VIP_Master", new RankConfig(new RankConfigJson { TagPre = "[d4af37][[cd7f32]M[c0c0c0]a[ffd700]s[e5e4e2]t[d4af37]e[cd7f32]r][d4af37] ", NameColor = "[e5e4e2]", PermissionLevel = 500, Playtime = 800 })},
                    {"VIP_Legend", new RankConfig(new RankConfigJson { TagPre = "[d4af37][[cd7f32]L[c0c0c0]e[ffd700]g[e5e4e2]e[d4af37]n[cd7f32]d][d4af37] ", NameColor = "[d4af37]", PermissionLevel = 500, Playtime = 1000 })},
                    {"Fresh", new RankConfig(new RankConfigJson { TagPre = "[d17f8f][[ea799a]Fresh][d17f8f] ", NameColor = "[ea899a]", Playtime = 0 })},
                    {"Rookie", new RankConfig(new RankConfigJson { TagPre = "[9cad2e][[9dff2e]Rookie][9cad2e] ", NameColor = "[adff2f]", Playtime = 50 })},
                    {"Veteran", new RankConfig(new RankConfigJson { TagPre = "[2c9d30][[2ecd31]Veteran][2c9d30] ", NameColor = "[32cd32]", Playtime = 100 })},
                    {"Elite", new RankConfig(new RankConfigJson { TagPre = "[1e6f20][[1e8b21]Elite][1e6f20] ", NameColor = "[228b22]", Playtime = 200 })},
                    {"Master", new RankConfig(new RankConfigJson { TagPre = "[00522e][[005f3f]Master][00522e] ", NameColor = "[006400]", Playtime = 300 })},
                    {"Legend", new RankConfig(new RankConfigJson { TagPre = "[b38f34][[c3a036]Legend][b38f34] ", NameColor = "[d4af37]", Playtime = 500 })}
                };

                SaveRankConfig();
            }

            try
            {
                string json = File.ReadAllText(filePath);

                if (string.IsNullOrEmpty(json))
                {
                    log.Error("JSON data is null or empty.");
                    return;
                }

                var tempDict = JsonConvert.DeserializeObject<Dictionary<string, RankConfigJson>>(json);

                if (tempDict == null)
                {
                    log.Error("Deserialized dictionary is null.");
                    return;
                }

                // Filter out any key-value pairs where the value is null before creating new RankConfig objects
                rankConfigs = tempDict
                    .Where(kvp => kvp.Value != null)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => new RankConfig(kvp.Value)
                    );

                if (rankConfigs.Count == 0)
                {
                    log.Warn("No valid RankConfigs were loaded.");
                }
                else
                {
                    log.Out($"Config loaded successfully. Ranks loaded: {rankConfigs.Count}");
                }
            }
            catch (Exception e)
            {
                log.Error($"Failed to load config: {e.Message}");
            }
        }
        public static void SaveRankConfig()
        {
            try
            {
                // Convert RankConfig objects back to RankConfigJson objects
                var rankConfigsJson = rankConfigs.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new RankConfigJson
                    {
                        Players = kvp.Value.Players,
                        PermissionLevel = kvp.Value.PermissionLevel,
                        Playtime = kvp.Value.Playtime,
                        ChatColor = kvp.Value.ChatColor,
                        NameColor = kvp.Value.NameColor,
                        CustomName = kvp.Value.CustomName,
                        TagPre = kvp.Value.TagPre,
                        TagPost = kvp.Value.TagPost
                    }
                );

                string json = JsonConvert.SerializeObject(rankConfigsJson, Formatting.Indented);
                File.WriteAllText(filePath, json);
                log.Out("Config saved successfully.");
            }
            catch (Exception e)
            {
                log.Error($"Failed to save config: {e.Message}");
            }
        }
    }
}
