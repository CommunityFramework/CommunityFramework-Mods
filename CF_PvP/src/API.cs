using System;
using HarmonyLib;
using System.Collections.Generic;
using CF_Core;

namespace CF_PvP
{
    public class API : IModApi
    {
        public static CF_Mod mod = new CF_Mod("CF_PvP", OnConfigLoaded, OnPhrasesLoaded);
        public static CF_Log log = new CF_Log("CF_PvP");
        Harmony harmony = new Harmony("CF_PvP");

        public void InitMod(Mod _modInstance)
        {
            mod.Activate();
            harmony.PatchAll();
        }

        public static void OnConfigLoaded()
        {

        }
        public static void OnPhrasesLoaded()
        {

        }
    }
}