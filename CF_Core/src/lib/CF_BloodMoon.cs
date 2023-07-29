public class CF_BloodMoon
{
    public static int GetBloodMoonLength()
    {
        (int duskHour, int dawnHour) = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
        return (24000 - duskHour * 1000) + (dawnHour * 1000);
    }
    public static int GetBloodMoonTicks()
    {
        (int duskHour, int dawnHour) = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));

        int days = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
        int dayTicks = (int)(GameManager.Instance.World.worldTime % (ulong)days);

        int hours = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);

        // after midnight
        if (hours < dawnHour)
            return dayTicks + ((24 - duskHour) * 1000);

        // before midnight
        return (dawnHour * 1000) - dayTicks;
    }
}
