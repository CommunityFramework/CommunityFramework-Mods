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
        public static CF_Mod mod = new CF_Mod("CF_Cvars", OnConfigLoaded, OnPhrasesLoaded);
        public static CF_Log x = new CF_Log("CF_Cvars");
        public static Harmony harmony = new Harmony("CF_Cvars");
        public void InitMod(Mod _modInstance)
        {
            mod.Activate();
            harmony.PatchAll();
            CF_Timer.AddOneSecTimer(CF_CvarManager.OnEvery1Sec, "CF_Cvars", false);
        }
        public static void OnConfigLoaded()
        {

        }
        public static void OnPhrasesLoaded()
        {

        }
    }
}