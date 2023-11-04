using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using HarmonyLib;

namespace CF_Economy
{
    public class API : IModApi
    {

        public static CF_Mod mod = new CF_Mod("CF_Economy", OnConfigLoaded, OnPhrasesLoaded);
        public static CF_Log log = new CF_Log("CF_Economy");
        public static Harmony harmony = new Harmony("CF_Economy");

        public static string fileName = "EconomyDB.json";
        public static string filePath;
        public static FileSystemWatcher fileWatcher;

        public void InitMod(Mod _modInstance)
        {
            mod.Activate();
            filePath = Path.Combine(mod.modDatabasePath, fileName);

            CF_EconomyManager.LoadDatabase();

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

