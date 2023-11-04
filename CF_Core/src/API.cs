namespace CF_Core
{
    public class API : IModApi
    {
        public static CF_Log log = new CF_Log("CF_Core", "Timers");
        public static CF_Mod mod = new CF_Mod("CF_Core");
        public void InitMod(Mod _modInstance)
        {
            CF_Cooldowns.Init(mod);
            CF_Timer.TimerStart();
        }
    }
}

