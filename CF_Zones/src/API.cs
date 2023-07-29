using System.Collections.Generic;
using HarmonyLib;
using CF_Core;

namespace CF_Zones
{
    public class API : IModApi
    {
        public static CF_Mod mod = new CF_Mod("CF_Zones", OnConfigLoaded, OnPhrasesLoaded);
        public static CF_Log x = new CF_Log("CF_Zones");
        public static Harmony harmony = new Harmony("CF_Zones");
        public static string filePathZones;
        public void InitMod(Mod _modInstance)
        {
            filePathZones = mod.modDatabasePath + "/zones.json";
            mod.Activate();
            harmony.PatchAll();

            ModEvents.PlayerSpawnedInWorld.RegisterHandler(OnPlayerSpawnedInWorld);

            ChatManager.RegisterChatTrigger("zones", OnZonesCommand);
            ZoneManager.LoadZonesFromFile();
            CF_Timer.AddOneSecTimer(OnEvery1Sec, "CF_Zones", true);
        }
        public static void OnConfigLoaded()
        {

        }
        public static void OnPhrasesLoaded()
        {

        }
        public static void OnEvery1Sec()
        {
            CF_Player.GetPlayers().ForEach(player =>
            {
                ZoneManager.UpdateZonesForPlayer(player);
            });
        }
        public static void OnPlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)// Spawning player
        {
            if (_cInfo == null)
                return;

            ZoneManager.OnPlayerSpawnedInWorld(_cInfo, _respawnReason, _pos);
        }
        private void OnZonesCommand(ClientInfo _cInfo, string _command, List<string> _params)
        {
            switch (_params[0])
            {
                case "here":
                case "loc":
                case "location":
                    break;
            }
        }
    }
}