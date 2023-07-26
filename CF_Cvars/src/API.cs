using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using CF_Core;
using HarmonyLib;

namespace CF_Cvars
{
    public class API : IModApi
    {
        public static ModX mod = new ModX("CF_Cvars", OnConfigLoaded, OnPhrasesLoaded);
        public static LogX x = new LogX("CF_Cvars");
        public static Harmony harmony = new Harmony("CF_Cvars");
        public void InitMod(Mod _modInstance)
        {
            mod.Activate();
            harmony.PatchAll();
            Timers.AddOneSecTimer(CvarManager.OnEvery1Sec, "CF_Cvars", false);
        }
        public static void OnConfigLoaded()
        {

        }
        public static void OnPhrasesLoaded()
        {

        }
    }
}