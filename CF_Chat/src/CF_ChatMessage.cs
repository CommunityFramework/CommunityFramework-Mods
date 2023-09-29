using System.Collections.Generic;
using System.Linq;

public class CF_ChatMessage
{
    public EChatType type;
    public int senderId;
    public string msg;
    public string name;
    public bool localizeMain;
    public List<int> recipientEntityIds;

    public bool isPrivateTrigger;
    public bool isPublicTrigger;

    public CF_ChatTrigger trigger;
    public string command;
    public List<string> args;

    public bool send;

    public string Name { get; set; }
    public string Message { get; set; }

    // List of name prefixes with sort order and type (add/reset)
    public List<NamePrefix> NamePrefixes { get; set; } = new List<NamePrefix>();

    // List of name colors with sort order
    public List<NameColor> NameColors { get; set; } = new List<NameColor>();

    // List of chat colors with sort order
    public List<ChatColor> ChatColors { get; set; } = new List<ChatColor>();

    public CF_ChatMessage(string msg)
    {
        type = EChatType.Global;
        senderId = -1;
        this.msg = msg;
        name = "";
        recipientEntityIds = null;

        send = true;
    }
    public CF_ChatMessage(string _msg, string _mainName)
    {
        type = EChatType.Global;
        senderId = -1;
        msg = _msg;
        name = _mainName;
        recipientEntityIds = null;

        send = true;
    }
    public CF_ChatMessage(EChatType _type, int _senderId, string _msg, string _mainName, bool _localizeMain, List<int> _recipientEntityIds)
    {
        type = _type;
        senderId = _senderId;
        msg = _msg;
        name = _mainName;
        localizeMain = _localizeMain;
        recipientEntityIds = _recipientEntityIds;

        isPrivateTrigger = msg.IndexOf('/') == 0;
        isPublicTrigger = msg.IndexOf('!') == 0;

        string[] Args = _msg.Substring(1).Trim().Split(' ');
        command = Args[0].ToLower();
        args = new List<string>(Args.Skip(1));

        if ((isPrivateTrigger || isPublicTrigger) && !string.IsNullOrEmpty(command) && CF_ChatManager.chatTriggers.TryGetValue(command, out CF_ChatTrigger chatTrigger))
            trigger = CF_ChatManager.chatTriggers.ContainsKey(command) ? CF_ChatManager.chatTriggers[command] : null;

        send = true;
    }
    public bool IsTrigger() => trigger != null;
    public void Send(int entityId)
    {
        SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(entityId)?.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(type, senderId, msg, name, localizeMain, null));
    }
    public void Send()
    {
        if (recipientEntityIds == null)
        {
            if (type == EChatType.Global)
                recipientEntityIds = (from cInfo in ConnectionManager.Instance.Clients.List.ToList() where cInfo != null select cInfo.entityId).ToList();
            else recipientEntityIds = new List<int>();
        }
            
        if (isPrivateTrigger)
        {
            recipientEntityIds.Clear();
            recipientEntityIds.Add(senderId);
        }

        if (!recipientEntityIds.Contains(senderId))
            recipientEntityIds.Add(senderId);

        recipientEntityIds = recipientEntityIds.Distinct().ToList();

        foreach (int recipientEntityId in recipientEntityIds)
        {
            if (recipientEntityId == -1)
                continue;

            SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(recipientEntityId)?.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(type, senderId, msg, name, localizeMain, null));
        }
    }

    public class NamePrefix
    {
        public string Prefix { get; set; }
        public int SortOrder { get; set; }
        public string Type { get; set; }  // "add" or "reset"
    }

    public class NameColor
    {
        public string ColorCode { get; set; }  // RGB code without #
        public int SortOrder { get; set; }
    }

    public class ChatColor
    {
        public string ColorCode { get; set; }  // RGB code without #
        public int SortOrder { get; set; }
    }
}