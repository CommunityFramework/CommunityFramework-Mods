using System.Collections.Generic;
using CF_Core;

namespace CF_Zones
{
    public class API : IModApi
    {
        public static ModX module = new ModX("CF_Zones", OnConfigLoaded, OnPhrasesLoaded);
        public static string filePathZones;
        public void InitMod(Mod _modInstance)
        {
            filePathZones = module.modDatabasePath + "/zones.json";
            module.Activate();
            ChatManager.RegisterChatTrigger("zones", OnZonesCommand);
            ZoneManager.LoadZonesFromFile();
            Timers.AddOneSecTimer(OnEvery1Sec, "CF_Zones", true);
        }
        public static void OnConfigLoaded()
        {

        }
        public static void OnPhrasesLoaded()
        {

        }
        public static void OnEvery1Sec()
        {
            Players.GetPlayers().ForEach(player =>
            {
                ZoneManager.UpdateZonesForPlayer(player);
            });
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