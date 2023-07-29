namespace CF_PlayerRanks
{
    public class API : IModApi
    {
        public static CF_Mod mod = new CF_Mod("CF_PlayerRanks", OnConfigLoaded, OnPhrasesLoaded);
        public static CF_Log x = new CF_Log("CF_PlayerRanks");
        public static string filePathPlayerRanks;
        public void InitMod(Mod _modInstance)
        {
            filePathPlayerRanks = mod.modDatabasePath + "/PlayerRanks.json";
            mod.Activate();
            PlayerRankManager.Init();
        }
        public static void OnConfigLoaded()
        {
        }
        public static void OnPhrasesLoaded()
        {
        }
    }
}