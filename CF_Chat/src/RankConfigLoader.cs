using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using static CF_Chat.API;

namespace CF_Chat
{
    public class RankConfigLoader
    {
        public static List<RankConfig> LoadRankConfig(string filePath)
        {
            // Check if the file exists
            if (!File.Exists(filePath))
            {
                log.Out("Config file not found. Creating default config.");

                // Create a default config with example ranks
                List<RankConfig> defaultConfigs = new List<RankConfig>
            {
                new RankConfig { Role = "Owner", PermissionLevel = 0, Playtime = 0, Color = "#FF0000" },
                new RankConfig { Role = "Admin", PermissionLevel = 1, Playtime = 0, Color = "#00FF00" },
                new RankConfig { Role = "Moderator", PermissionLevel = 2, Playtime = 0, Color = "#0000FF" },
                new RankConfig { Role = "VIP", PermissionLevel = 10, Playtime = 100, Color = "#FFFF00" },
                new RankConfig { Role = "Player", PermissionLevel = 1000, Playtime = 0, Color = "#FFFFFF" }
            };

                // Write the default config to the file
                SaveRankConfig(defaultConfigs, filePath);

                return defaultConfigs;
            }

            try
            {
                // Read the existing config from the file
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<RankConfig>>(json);
            }
            catch (Exception e)
            {
                log.Error($"Failed to load config: {e}");
                return new List<RankConfig>();
            }
        }

        public static void SaveRankConfig(List<RankConfig> rankConfigs, string filePath)
        {
            try
            {
                string json = JsonConvert.SerializeObject(rankConfigs, Formatting.Indented);
                File.WriteAllText(filePath, json);
                log.Out("Config saved successfully.");
            }
            catch (Exception e)
            {
                log.Error($"Failed to save config: {e}");
            }
        }
    }
}
