using HarmonyLib;

namespace CF_PlayerMonitor
{
    public class API : IModApi
    {
        public static CF_Mod mod = new CF_Mod("CF_PlayerMonitor", OnConfigLoaded, OnPhrasesLoaded);
        public static CF_Log log = new CF_Log("CF_PlayerMonitor");
        Harmony harmony = new Harmony("CF_PlayerMonitor");

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