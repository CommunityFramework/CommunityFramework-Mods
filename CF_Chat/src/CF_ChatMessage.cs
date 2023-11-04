using Epic.OnlineServices.Presence;
using Epic.OnlineServices.RTCAudio;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using UnityEngine;
using static CF_Chat.API;
using static CF_ChatMessage;

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

        /*
        bSending = true;

        Mod mod = ModEvents.ChatMessage.Invoke(_cInfo, type, _senderEntityId, _msg, _mainName, _localizeMain, _recipientEntityIds);
        string str = string.Format("Chat (from '{0}', entity id '{1}', to '{2}'): '{3}': {4}", (object)(_cInfo?.PlatformId != null ? _cInfo.PlatformId.CombinedString : "-non-player-"), (object)_senderEntityId, (object)_chatType.ToStringCached<EChatType>(), _localizeMain ? (object)Localization.Get(_mainName) : (object)_mainName, (object)Utils.FilterBbCode(_msg));
        if (mod != null)
            Log.Out("Chat handled by mod '{0}': {1}", new object[2]
            {
                (object) mod.Name,
          (object) str
            });
        else Log.Out(str);

        bSending = false;
        */

        foreach (int recipientEntityId in recipientEntityIds)
        {
            if (recipientEntityId == -1)
                continue;

            SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(recipientEntityId)?.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(type, senderId, msg, name, localizeMain, null));
        }
    }
    public void ApplyNameAndChatColors()
    {
        if (isPrivateTrigger)
        {
            return;
        }

        switch (type)
        {
            case EChatType.Friends:
                name = "[33ff33](Friends) [-]" + name;
                break;
            case EChatType.Party:
                name = "[eed202](Party) [-]" + name;
                break;
            case EChatType.Global:
                ApplyNameColors();
                ApplyPreTags();
                ApplyPostTags();
                ApplyChatColors();
                break;
        }
    }
    private void ApplyNameColors()
    {
        if (customNames.Count > 0)
        {
            var customName = customNames.OrderBy(c => c.SortOrder).FirstOrDefault().Tag;
            log.Out($"{name} => {customName}");
            name = customName;
        }
        else if (nameColors.Count > 0)
        {
            var nameColor = nameColors.OrderBy(c => c.SortOrder).FirstOrDefault().ColorCode;
            log.Out($"{nameColor + name} = {nameColor} + {name}");
            name = nameColor + name;
        }
    }
    private void ApplyChatColors()
    {
        if (chatColors.Count > 0)
        {
            string chatColor = chatColors.OrderBy(c => c.SortOrder)?.FirstOrDefault().ColorCode ?? "[-]";
            msg = chatColor + msg;
        }
    }
    private void ApplyPreTags()
    {
        if (namePrefixes.Count > 0)
        {
            var sortedNamePrefixes = namePrefixes.OrderBy(p => p.SortOrder).ToList();
            var resetPrefix = sortedNamePrefixes.FirstOrDefault(p => p.Reset);
            var resetSortOrder = !resetPrefix.IsEmpty ? resetPrefix.SortOrder : int.MaxValue;
            var finalNamePrefix = string.Join("", sortedNamePrefixes.Where(p => p.SortOrder <= resetSortOrder).Select(p => p.Tag));
            name = finalNamePrefix + name;
        }
    }
    private void ApplyPostTags()
    {
        if (namePostfixes.Count > 0)
        {
            var sortedNamePostfixes = namePostfixes.OrderBy(p => p.SortOrder).ToList();
            var resetPostfix = sortedNamePostfixes.FirstOrDefault(p => p.Reset);
            var resetSortOrder = !resetPostfix.IsEmpty ? resetPostfix.SortOrder : int.MaxValue;
            var finalNamePostfix = string.Join("", sortedNamePostfixes.Where(p => p.SortOrder <= resetSortOrder).Select(p => p.Tag));
            name = name + finalNamePostfix;
        }
    }
    public void AddNameTagPrefix(string tag, int sortOrder = 0, bool reset = false)
    {
        namePrefixes.Add(new NameTag { Tag = tag, SortOrder = sortOrder, Reset = reset });
    }
    public void AddNameTagPostfix(string tag, int sortOrder = 0, bool reset = false)
    {
        namePostfixes.Add(new NameTag { Tag = tag, SortOrder = sortOrder, Reset = reset });
    }
    public void AddNameColor(string color, int sortOrder = 0)
    {
        nameColors.Add(new NameColor { ColorCode = color, SortOrder = sortOrder });
    }
    public void AddCustomName(string tag, int sortOrder = 0)
    {
        customNames.Add(new CustomName { Tag = tag, SortOrder = sortOrder });
    }
    public void AddChatColor(string color, int sortOrder = 0)
    {
        chatColors.Add(new ChatColor { ColorCode = color, SortOrder = sortOrder });
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