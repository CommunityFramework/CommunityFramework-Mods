using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;
using Platform;

namespace CF_PlayerDatabase
{
    public class API : IModApi
    {
        // Create a CF_Mod instance for easy and unified Settings.xml and Phrases.xml file systems, those settings are relerased while the server is live
        public static CF_Mod mod = new CF_Mod("CF_PlayerDatabase", OnConfigLoaded, OnPhrasesLoaded);
        // Unified logging system with some extra log levels and features designed for this game
        public static CF_Log log = new CF_Log("CF_PlayerDatabase");
        // Harmony injections are an important tool for modding this game. Please visit https://harmony.pardeike.net/articles/intro.html for more infos
        public static Harmony harmony = new Harmony("CF_PlayerDatabase");
        // Simply helper we use from CF_Core to store data to a json file which contains some data about players
        public static CF_PlayerDB dbData = null;
        public static CF_JsonFile<CF_PlayerDB> db = new CF_JsonFile<CF_PlayerDB>(mod.modDatabasePath + "/Players.json", dbData, Formatting.None);

        // This is our entry point which is usually our Main() in C#
        // More info can be found here: https://7daystodie.fandom.com/wiki/ModAPI
        public void InitMod(Mod _modInstance)
        {
            mod.Activate(true); // Load settings & phrases

            if (!db.Load(out CF_PlayerDB data, out string err))
            {
                log.Error($"Could not load database: {err}");
                dbData = new CF_PlayerDB();
                db.Save();
            }
            else
            {
                dbData = data;
                log.Out($"Loaded {CF_PlayerDB.players.Count} players.");
            }

            harmony.PatchAll(); // Patch all harmony injections across all files of this project

            ModEvents.PlayerSpawning.RegisterHandler(OnPlayerSpawning); 
            ModEvents.PlayerSpawnedInWorld.RegisterHandler(OnPlayerSpawnedInWorld);
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
        public static string welcomeBackMessage;
        public static void OnPhrasesLoaded()
        {
            mod.AddPhrase("Welcome", "Welcome {PLAYERNAME}. ", "Displayed when a player joined the server first time.", out welcomeMessage);
            mod.AddPhrase("WelcomeBack", "Welcome back {PLAYERNAME}. {TIMESPAN} have passed since the last time you visited us", "Displayed when a player joined the server.", out welcomeBackMessage);
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
            if (db == null)
            {
                log.Out("db is null.");
                return;
            }

            if (db.data == null)
            {
                log.Out("db.data is null.");
                return;
            }

            if (!CF_PlayerDB.TryGetPlayer(_cInfo, out PlayerDBEntry _playerData, true))
                return;

            if (_cInfo == null)
            {
                log.Out("_cInfo is null.");
                return;
            }

            switch (_respawnReason)
            {
                case RespawnType.JoinMultiplayer:
                case RespawnType.EnterMultiplayer:
                    SendWelcomeMessage(_cInfo, _playerData);
                    break;
                default:
                    break;
            }
        }
        public static void SendWelcomeMessage(ClientInfo _cInfo, PlayerDBEntry _playerData)
        {
            if (_playerData == null)
            {
                log.Out("_playerData is null.");
                return;
            }

            TimeSpan passed = DateTime.UtcNow - _playerData.lastSeen;
            string timespan = passed.ToString();

            if (string.IsNullOrEmpty(welcomeMessage) || string.IsNullOrEmpty(welcomeBackMessage))
            {
                log.Out("welcomeMessage or welcomeBackMessage is null or empty.");
                return;
            }

            if (passed.Seconds == 0)
            {
                CF_Player.Message(welcomeMessage
                    .Replace("{PLAYERNAME}", _cInfo.playerName)
                    , _cInfo);
            }
            else
            {
                CF_Player.Message(welcomeBackMessage
                    .Replace("{PLAYERNAME}", _cInfo.playerName)
                    .Replace("{TIMESPAN}", timespan)
                    , _cInfo);
            }
        }
        public static void OnPlayerChat(ClientInfo _cInfo, string _message, List<string> _recipients, EChatType _type)
        {
            if (CF_PlayerDB.TryGetPlayer(_cInfo, out PlayerDBEntry playerData))
            {
                if (playerData.isMuted)
                {
                    CF_Player.Message("You are currently muted and cannot send messages.", _cInfo);
                    return;
                }
            }
        }
    }
}