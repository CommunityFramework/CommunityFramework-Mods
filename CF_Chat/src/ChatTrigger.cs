using System;
using System.Collections.Generic;

public class ChatTrigger
{
    public Action<ClientInfo, string, List<string>> callback;
    public string trigger;
    public List<string> aliases;
    public ChatTrigger(string _trigger, Action<ClientInfo, string, List<string>> _callback)
    {
        trigger = _trigger;
        callback = _callback;
        aliases = new List<string>();
    }
    public ChatTrigger(string _trigger, Action<ClientInfo, string, List<string>> _callback, List<string> _aliases)
    {
        trigger = _trigger;
        callback = _callback;
        aliases = _aliases;
    }
}
