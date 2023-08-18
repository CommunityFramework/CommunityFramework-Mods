using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace CF_PvP
{
    public class API : IModApi
    {
        public static CF_Mod mod = new CF_Mod("CF_PvP", OnConfigLoaded, OnPhrasesLoaded);
        public static CF_Log log = new CF_Log("CF_PvP", "Damage");
        Harmony harmony = new Harmony("CF_PvP");

        public void InitMod(Mod _modInstance)
        {
            mod.Activate();
            harmony.PatchAll();
        }
        public static int maxDistanceDrop;
        public static int maxDistanceReport;
        public static string buffsIgnore;
        public static List<string> buffsIgnoreList;
        public static void OnConfigLoaded()
        {
            mod.AddSetting("Damage_Dist_Drop", 200, 0, 9999, "Max distance before dropping pvp damage.", out maxDistanceDrop);
            mod.AddSetting("Damage_Dist_Report", 110, 0, 9999, "Max distance before reporting pvp damage.", out maxDistanceReport);
            mod.AddSetting("Buffs_Ignore", "buffstatuscheck01,buffstatuscheck02,buffperkabilityupdate,buffcampfireaoe", "", "List of buffs separated by',' which should not be logged.", out buffsIgnore);

            buffsIgnoreList = buffsIgnore.Trim().Split(',').ToList();
        }
        public static void OnPhrasesLoaded()
        {

        }
        public static bool IgnoreBuff(string _buffname) => buffsIgnoreList.Contains(_buffname);
    }
}