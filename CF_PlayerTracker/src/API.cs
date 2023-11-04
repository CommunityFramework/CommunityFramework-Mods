using HarmonyLib;

namespace CF_PlayerTracker
{
    public class API : IModApi
    {
        public static CF_Mod mod = new CF_Mod("CF_PlayerTracker", OnConfigLoaded, OnPhrasesLoaded);
        public static CF_Log log = new CF_Log("CF_PlayerTracker");
        Harmony harmony = new Harmony("CF_PlayerTracker");

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