using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

namespace Example_Mod
{
    public class API : IModApi
    {
        // Create a CF_Mod instance for easy and unified Settings.xml and Phrases.xml file systems, those settings are relerased while the server is live
        public static CF_Mod mod = new CF_Mod("Example_Mod", OnConfigLoaded, OnPhrasesLoaded);
        // Unified logging system with some extra log levels and features designed for this game
        public static CF_Log log = new CF_Log("Example_Mod");
        // Harmony injections are an important tool for modding this game. Please visit https://harmony.pardeike.net/articles/intro.html for more infos
        public static Harmony harmony = new Harmony("Example_Mod");
        // Simply helper we use from CF_Core to store data to a json file which contains some data about players
        public static CF_JsonFile<PlayerDb> db = new CF_JsonFile<PlayerDb>(mod.modDatabasePath + "/Players.json", new PlayerDb(), Formatting.None);

        public class PlayerDb
        {
            public Dictionary<string, PlayerDbEntry> players = new Dictionary<string, PlayerDbEntry>();
            public bool GetPlayer(ClientInfo _cInfo, out PlayerDbEntry _playerDbEntry, bool _allowCreate = true)
            {
                _playerDbEntry = null;
                if (!players.ContainsKey(_cInfo.InternalId.ReadablePlatformUserIdentifier))
                {
                    if (!_allowCreate)
                    {
                        return false;
                    }
                    
                    players.Add(_cInfo.InternalId.ReadablePlatformUserIdentifier, new PlayerDbEntry(_cInfo));
                }


                return true;
            }
        }
        public class PlayerDbEntry
        {
            readonly public string eosId;
            public string name;
            public DateTime firstSeen = DateTime.Now;
            public DateTime lastSeen = DateTime.Now;
            public int playtime = 0;
            public PlayerDbEntry(ClientInfo _cInfo)
            {
                eosId = _cInfo.InternalId.ReadablePlatformUserIdentifier;
                name = _cInfo.playerName;
            }
        }
        // This is our entry point which is usually our Main() in C#
        // More info can be found here: https://7daystodie.fandom.com/wiki/ModAPI
        public void InitMod(Mod _modInstance)
        {
            mod.Activate(true); // Load settings & phrases

            harmony.PatchAll(); // Patch all harmony injections across all files of this project

            ModEvents.PlayerSpawning.RegisterHandler(OnPlayerSpawning); 
            ModEvents.PlayerSpawnedInWorld.RegisterHandler(OnPlayerSpawnedInWorld);

            if(!db.Load<PlayerDb>(out PlayerDb data, out string err))
            {
                log.Error($"Could not load database: {err}");
            }
        }
        // Phrases
        // All phrases created in this callback can be used after
        // a Phrases.xml is created which can be edited while the server is running
        public static bool enabled;
        public static void OnConfigLoaded()
        {
            mod.AddSetting("Enable", true, "Set to true to enable this mod and false to disable.", out enabled);
        }
        // Phrases
        // All phrases created in this callback can be used after
        // Since we can't get a players game language a multi-language phrase system is obsolete but will be added in case they add that feature
        // a Phrases.xml is created which can be edited while the server is running
        public static string welcomeMessage;
        public static void OnPhrasesLoaded()
        {
            mod.AddPhrase("WelcomeMessage", "Welcome back {PLAYERNAME}. {TIMESPAN} have passed since the last time you visited us", "Displayed when a player joined the server.", out welcomeMessage);
        }
        // PlayerSpawning
        // This is called right after the player sends his character profile and before initializing the chunk observer and creating + sending the player entity to everyone
        public static void OnPlayerSpawning(ClientInfo _cInfo, int _chunkViewDim, PlayerProfile _playerProfile)
        {
            // implementation omitted for brevity
        }
        // PlayerSpawnedInWorld
        // Called when the player is spawning, a teleport by a closed trader is also a spawn, check the respawn reason depending on what you want to do
        public static void OnPlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)
        {
            switch(_respawnReason) 
            {
                case RespawnType.JoinMultiplayer:
                case RespawnType.EnterMultiplayer:
                    SendWelcomeMessage(_cInfo);
                    break;
                default:
                    break;
            }
        }
        public static void SendWelcomeMessage(ClientInfo _cInfo)
        {
            if (!db.data.GetPlayer(_cInfo, out PlayerDbEntry _playerData))
                return;

            TimeSpan passed = DateTime.Now - _playerData.lastSeen;
            string timespan = passed.ToString();
            CF_Player.Message(welcomeMessage
                .Replace("{PLAYERNAME}", _cInfo.playerName)
                .Replace("{TIMESPAN}", timespan)
                , _cInfo);
        }
    }
}