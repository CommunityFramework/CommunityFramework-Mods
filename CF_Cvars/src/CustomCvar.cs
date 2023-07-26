using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CustomCvar
{
    public class CustomCvarReturn
    {
        public float value { get; set; }
        public CustomCvarReturn(float value)
        {
            this.value = value;
        }
    }
    public string name;
    public int updateinterval;
    public Action<EntityPlayer, CustomCvarReturn> OnUpdate;
    public CustomCvar(string name, Action<EntityPlayer, CustomCvarReturn> callback = null, int interval = 30)
    {
        this.name = name;
        updateinterval = interval;
        OnUpdate = callback;

        CvarManager.cvars.Add(this);
    }
    public void UpdateAll()
    {
        foreach (EntityPlayer player in GameManager.Instance.World.Players.list)
            UpdatePlayer(player);
    }
    public void UpdateAll(float value)
    {
        foreach (EntityPlayer player in GameManager.Instance.World.Players.list)
            UpdatePlayer(player, value);
    }
    public void UpdatePlayer(EntityPlayer player)
    {
        if (OnUpdate == null)
            return;

        float oldValue = player.GetCVar(this.name);
        CustomCvarReturn customCvarReturn = new CustomCvarReturn(oldValue);

        OnUpdate(player, customCvarReturn);

        if (oldValue != customCvarReturn.value)
            player.SetCVar(this.name, customCvarReturn.value);
    }
    public void UpdatePlayer(EntityPlayer player, float value)
    {
        float oldValue = player.GetCVar(this.name);

        if (oldValue != value)
            player.SetCVar(this.name, value);
    }
}