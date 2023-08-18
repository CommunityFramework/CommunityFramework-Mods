namespace CF_Core
{
    public class API : IModApi
    {
        public static CF_Log log = new CF_Log("CF_Core", "Timers");
        public void InitMod(Mod _modInstance)
        {
            CF_Timer.TimerStart();
        }
    }
}

