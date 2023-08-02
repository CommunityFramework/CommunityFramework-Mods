using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Platform;
using Platform.Steam;
using System.Collections.ObjectModel;
using LiteNetLib;
using HarmonyLib;

namespace CF_Economy
{
    public class API : IModApi
    {
        public static CF_Mod mod = new CF_Mod("CF_Economy", OnConfigLoaded, OnPhrasesLoaded);
        public static CF_Log log = new CF_Log("CF_Economy");
        public static Harmony harmony = new Harmony("CF_Economy");
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

