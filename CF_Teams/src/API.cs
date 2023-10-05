using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using CF_Core;
using HarmonyLib;

namespace CF_Teams
{
    public class API : IModApi
    {
        public static CF_Mod mod = new CF_Mod("CF_Teams", OnConfigLoaded);
        public static CF_Log log = new CF_Log("CF_Teams");
        public static Harmony harmony = new Harmony("CF_Teams");
        public void InitMod(Mod _modInstance)
        {
            mod.Activate(true);
            harmony.PatchAll();
            CF_Timer.AddOneSecTimer(CF_TeamManager.PeriodicUpdate, "CF_Teams", false);
            CF_TeamManager.LoadData();
        }
        public static int teamUpdateInterval;
        public static TimeSpan teamSizeCacheDuration;
        public static void OnConfigLoaded()
        {
            mod.AddSetting("TeamUpdate_Interval", 10, 1, 3600, "Interval in seconds for team updates.", out teamUpdateInterval);
            mod.AddSetting("TeamSize_CacheDuration", 360, 1, int.MaxValue, "Duration in minutes for team size cache.", out int duration);
            teamSizeCacheDuration = TimeSpan.FromHours(duration);
        }
    }
}