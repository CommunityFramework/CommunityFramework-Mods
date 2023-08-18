using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CF_CustomCvar
{
    public class CustomCvarReturn
    {
        public float value { get; set; }
        public float defaultValue { get; }
        public CustomCvarReturn(float _value, float _defaultValue)
        {
            value = _value;
            defaultValue = _defaultValue;
        }
    }
    public string name;
    public int updateinterval;
    public float defaultValue { get; }
    public Action<EntityPlayer, CustomCvarReturn> OnUpdate;
    public CF_CustomCvar(string name, float _defaultValue, Action<EntityPlayer, CustomCvarReturn> callback = null, int interval = 30)
    {
        this.name = name;
        updateinterval = interval;
        OnUpdate = callback;
        defaultValue = _defaultValue;

        CF_CvarManager.cvars.Add(this);
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
        CustomCvarReturn customCvarReturn = new CustomCvarReturn(oldValue, defaultValue);

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