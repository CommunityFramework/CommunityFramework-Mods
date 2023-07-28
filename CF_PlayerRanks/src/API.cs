namespace CF_PlayerRanks
{
    public class API : IModApi
    {
        public static ModX mod = new ModX("CF_PlayerRanks", OnConfigLoaded, OnPhrasesLoaded);
        public static LogX x = new LogX("CF_PlayerRanks");
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