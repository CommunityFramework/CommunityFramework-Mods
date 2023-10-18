using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using CF_Core;
using HarmonyLib;
using System.Runtime.InteropServices;

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
        public static string cvCachedTeamSize;
        public static int teamUpdateInterval;
        public static TimeSpan teamSizeCacheDuration;

        public static int groupLimit;
        public static void OnConfigLoaded()
        {
            mod.AddSetting("TeamSize_Cvar", "CachedTeamSize", "", "Cvar name used to store the cached team size.", out cvCachedTeamSize);
            mod.AddSetting("TeamSize_Interval", 10, 1, 3600, "Interval in seconds for team updates.", out teamUpdateInterval);
            mod.AddSetting("TeamSize_CacheDuration", 1440, 1, int.MaxValue, "Duration in minutes for team size cache.", out int duration);

            mod.AddSetting("TeamSize_Max", 9, -1, 99, "Max team size", out groupLimit);
            teamSizeCacheDuration = TimeSpan.FromHours(duration);
        }
    }
}