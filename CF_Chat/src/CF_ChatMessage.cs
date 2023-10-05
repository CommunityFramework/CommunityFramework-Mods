using System.Collections.Generic;
using System.Linq;
using static CF_Chat.API;

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

    public bool send = true;

    // List of name prefixes with sort order and type (add/reset)
    public List<CustomName> customNames { get; } = new List<CustomName>();
    public List<NameTag> namePrefixes { get; } = new List<NameTag>();
    public List<NameTag> namePostfixes { get; } = new List<NameTag>();
    public List<NameColor> nameColors { get; } = new List<NameColor>();
    public List<ChatColor> chatColors { get; } = new List<ChatColor>();

    public CF_ChatMessage(string _msg)
    {
        type = EChatType.Global;
        senderId = -1;
        msg = _msg;
        name = "";
        recipientEntityIds = null;
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
    }
    public bool IsTrigger() => trigger != null;
    public void Send(ClientInfo _cInfo)
    {
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(type, senderId, msg, name, localizeMain, null));
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

        recipientEntityIds = recipientEntityIds.Distinct().ToList();  // Using Distinct to remove duplicate entity IDs

        foreach (int recipientEntityId in recipientEntityIds)
        {
            if (recipientEntityId == -1)
                continue;

            SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(recipientEntityId)?.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(type, senderId, msg, name, localizeMain, null));
        }
    }

    public void ApplyNameAndChatColors()
    {
        // Handle name color
        if (customNames.Count > 0)
        {
            var customName = customNames.OrderBy(c => c.SortOrder).FirstOrDefault().Tag;
            name = customName;
            log.Debug($"Applied custom name: {customName}");
        }
        else if (nameColors.Count > 0)
        {
            var nameColor = nameColors.OrderBy(c => c.SortOrder).FirstOrDefault().ColorCode;
            name = nameColor + name;
            log.Debug($"Applied name color: {nameColor}");
        }

        // Handle name prefixes
        if (namePrefixes.Count > 0)
        {
            var sortedNamePrefixes = namePrefixes.OrderBy(p => p.SortOrder).ToList();
            var resetPrefix = sortedNamePrefixes.FirstOrDefault(p => p.Reset);
            var resetSortOrder = !resetPrefix.IsEmpty ? resetPrefix.SortOrder : int.MaxValue;
            var finalNamePrefix = string.Join("", sortedNamePrefixes.Where(p => p.SortOrder <= resetSortOrder).Select(p => p.Tag));
            name = finalNamePrefix + name;
            log.Debug($"Applied name prefix: {finalNamePrefix}");
        }

        // Handle name postfixes
        if (namePostfixes.Count > 0)
        {
            var sortedNamePostfixes = namePostfixes.OrderBy(p => p.SortOrder).ToList();
            var resetPostfix = sortedNamePostfixes.FirstOrDefault(p => p.Reset);
            var resetSortOrder = !resetPostfix.IsEmpty ? resetPostfix.SortOrder : int.MaxValue;
            var finalNamePostfix = string.Join("", sortedNamePostfixes.Where(p => p.SortOrder <= resetSortOrder).Select(p => p.Tag));
            name = name + finalNamePostfix;
            log.Debug($"Applied name postfix: {finalNamePostfix}");
        }


        // Handle chat color
        if (chatColors.Count > 0)
        {
            var chatColor = chatColors.OrderBy(c => c.SortOrder).FirstOrDefault().ColorCode;
            msg = $"{chatColor}{msg}";
            log.Debug($"Applied chat color: {chatColor}");
        }

        log.Debug("Exiting ApplyNameAndChatColors");
    }

    public void AddNameTagPrefix(NameTag prefix)
    {
        namePrefixes.Add(prefix);
    }

    public void AddNameTagPostfix(NameTag postfix)
    {
        namePostfixes.Add(postfix);
    }

    public void AddNameColor(NameColor color)
    {
        nameColors.Add(color);
    }

    public void AddCustomName(CustomName customName)
    {
        customNames.Add(customName);
    }

    public void AddChatColor(ChatColor color)
    {
        chatColors.Add(color);
    }

    public struct NameTag
    {
        public string Tag { get; set; }
        public int SortOrder { get; set; }
        public bool Reset { get; set; }  // If true, clears or ignores entries with a higher sort order
        public bool IsEmpty
        {
            get { return SortOrder == 0 && string.IsNullOrEmpty(Tag) && !Reset; }
        }
    }

    public struct NameColor
    {
        public string ColorCode { get; set; }  // RGB code without #
        public int SortOrder { get; set; }
    }

    public struct ChatColor
    {
        public string ColorCode { get; set; }  // RGB code without #
        public int SortOrder { get; set; }
    }

    public struct CustomName
    {
        public string Tag { get; set; }  // RGB code without #
        public int SortOrder { get; set; }
    }
}