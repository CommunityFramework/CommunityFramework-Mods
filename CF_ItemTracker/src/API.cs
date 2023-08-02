using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Platform;
using Platform.Steam;
using HarmonyLib;
using System.Linq;

namespace CF_ItemTracker
{
    public class API : IModApi
    {
        public static CF_Mod mod = new CF_Mod("CF_ItemTracker", OnConfigLoaded, OnPhrasesLoaded);
        public static CF_Log log = new CF_Log("CF_ItemTracker");
        public static Harmony harmony = new Harmony("CF_ItemTracker");
        public static string pathItemTrackerDb;
        public static string filePathWhitelist;
        public void InitMod(Mod _modInstance)
        {
            pathItemTrackerDb = mod.modDatabasePath + "/Tracked_Players/";
            mod.Activate(true);
            harmony.PatchAll();
            //ModEvents.PlayerSpawning.RegisterHandler(CheckPlayer);
        }
        public static int adminIgnore;
        public static bool antiDupeCheck;
        public static string ignoreDupeItems;
        public static HashSet<string> IgnoreDupeItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public static bool illigalItemsCheck;
        public static string invalidItems;
        public static List<string> InvalidItems = new List<string>();
        public static void OnConfigLoaded()
        {
            mod.AddSetting("AdminLevelBypass", 0, -1, 1000, "Admin level required to bypass anti cheat.", out adminIgnore);
            mod.AddSetting("AntiDupe_InventoryChecks", false, "Checks inventory changes and logs them.", out antiDupeCheck);
            mod.AddSetting("AntiDupe_IgnoreItems", "", "", "Items which should be ignored, selerated by ','.", out ignoreDupeItems);
            mod.AddSetting("InvalidItems__Enable", false, "Enable invalid item checks", out illigalItemsCheck);
            mod.AddSetting("InvalidItems__Enable", false, "Enable invalid item checks", out illigalItemsCheck);
            mod.AddSetting("InvalidItems_List", "rScrapIronMaster,concreteMaster,steelShapes,steelMaster,stainlessSteelShapes,stainlessSteelMaster,h7sb_adminBlock,h7sb_adminBlockMaster,h7sb_adminGlassMaster,concreteNoUpgradeMaster,corrugatedMetalNoUpgradeMaster,concreteNoUpgradeMaster", "", "Checks inventory changes for invalid items.", out invalidItems);
            InvalidItems = invalidItems.Split(',').ToList();
            IgnoreDupeItems = ignoreDupeItems.Split(',').ToHashSet<string>();
        }
        public static void OnPhrasesLoaded()
        {

        }
    }
}

