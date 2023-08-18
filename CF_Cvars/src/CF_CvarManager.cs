using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CF_CvarManager
{
    public static List<CF_CustomCvar> cvars = new List<CF_CustomCvar>();
    private static int timer = 0;
    public static void OnEvery1Sec()
    {
        if (timer == int.MaxValue)
            timer = 0;
        else timer++;

        foreach (CF_CustomCvar cvar in cvars)
        {
            if (timer % cvar.updateinterval == 0)
                cvar.UpdateAll();
        }
    }
    public static void CheckPlayer(EntityPlayer _player)
    {
        foreach (CF_CustomCvar cvar in cvars)
        {
            if (_player.Buffs.CVars.ContainsKey(cvar.name))
                continue;

            cvar.UpdatePlayer(_player, cvar.defaultValue);
        }
    }
    public static void UpdatePlayer(EntityPlayer _player)
    {
        foreach (CF_CustomCvar cvar in cvars)
        {
            if (cvar.OnUpdate == null)
                continue;

            cvar.UpdatePlayer(_player);
        }
    }
}
