namespace CF_Core
{
    public class API : IModApi
    {
        public static LogX x = new LogX("CF_Core", "Timers");
        public void InitMod(Mod _modInstance)
        {
            Timers.TimerStart();
        }
    }
}

