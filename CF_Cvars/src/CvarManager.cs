using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CvarManager
{
    public static List<CustomCvar> cvars = new List<CustomCvar>();
    private static int timer = 0;
    public static void OnEvery1Sec()
    {
        if (timer == int.MaxValue)
            timer = 0;
        else timer++;

        foreach (CustomCvar cvar in cvars)
        {
            if (timer % cvar.updateinterval == 0)
                cvar.UpdateAll();
        }
    }
    public static void UpdatePlayer(EntityPlayer _player)
    {
        foreach (CustomCvar cvar in cvars)
        {
            if (cvar.OnUpdate == null)
                continue;

            cvar.UpdatePlayer(_player);
        }
    }
}
