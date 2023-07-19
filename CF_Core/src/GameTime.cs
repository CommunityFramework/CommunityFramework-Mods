public class GameTime
{
    public static int WorldTimeToMinutes(ulong _worldTime) => (int)((double)_worldTime / 1000.0 * 60.0) % 60;
    public static int WorldTimeToMinutesTotal(ulong _worldTime) => (int)((double)_worldTime / 1000.0 * 60.0);
    public static int WorldTimeToSecondsTotal(ulong _worldTime) => (int)((double)_worldTime / 1000.0 * 60.0 * 60.0);
}