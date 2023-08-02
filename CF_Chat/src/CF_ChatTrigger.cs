using System;
using System.Collections.Generic;

public class CF_ChatTrigger
{
    public Action<ClientInfo, string, List<string>> callback;
    public string trigger;
    public List<string> aliases = new List<string>();
    public int permission;
    public CF_ChatTrigger(string _trigger, Action<ClientInfo, string, List<string>> _callback, int _permission = 1000)
    {
        trigger = _trigger;
        callback = _callback;
        permission = _permission;
    }
    public CF_ChatTrigger(string _trigger, Action<ClientInfo, string, List<string>> _callback, List<string> _aliases, int _permission = 1000)
    {
        trigger = _trigger;
        callback = _callback;
        aliases = _aliases;
        permission = _permission;
    }
    public bool CanExecute(ClientInfo _cInfo) => GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo) <= permission;
}
