using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static CF_Chat.API;

public class CF_ChatManager
{
    private static List<Action<ClientInfo, CF_ChatMessage>> chatHandlers = new List<Action<ClientInfo, CF_ChatMessage>>();
    public static Dictionary<string, CF_ChatTrigger> chatTriggers = new Dictionary<string, CF_ChatTrigger>();
    public static void RegisterHandler(Action<ClientInfo, CF_ChatMessage> _callback)
    {
        chatHandlers.Add(_callback);
    }
    public static void RegisterChatTrigger(string trigger, Action<ClientInfo, string, List<string>> _callback, int _permission = 1000)
    {
        foreach (string alias in trigger.Split(','))
        {
            if (chatTriggers.ContainsKey(alias.ToLower()))
                continue;

            chatTriggers.Add(alias.ToLower(), new CF_ChatTrigger(alias, _callback, _permission));
        }
    }
    public static bool OnChatMessage(ClientInfo cInfo, EChatType type, int senderId, string msg, string mainName, bool localizeMain, List<int> recipientEntityIds)
    {
        try
        {
            if (cInfo == null)
                return true;

            HandleChatMessage(cInfo, new CF_ChatMessage(type, senderId, msg, mainName, localizeMain, recipientEntityIds));
            return false;
        }
        catch (Exception e)
        {
            Log.Error($"OnChatMessage reported: {e}");
        }

        return true;
    }
    public static void HandleChatMessage(ClientInfo cInfo, CF_ChatMessage chatMessage)
    {
        PersistentPlayerData ppd = GameManager.Instance.persistentPlayers?.GetPlayerDataFromEntityID(cInfo.entityId) ?? null;
        if (ppd == null)
            return; 

        EntityPlayer entPlayer = GameManager.Instance.World.Players.dict.ContainsKey(cInfo.entityId) ? GameManager.Instance.World.Players.dict[cInfo.entityId] : null;
        if (entPlayer == null)
            return;

        // All other mods to handle chat first
        foreach (Action<ClientInfo, CF_ChatMessage> chatListener in chatHandlers)
            chatListener(cInfo, chatMessage);

        // Apply default chat colors, name prefixes, and name colors based on player's permission level and playtime
        ApplyDefaultChatFormatting(cInfo, chatMessage);

        if (!chatMessage.send)
            return;

        chatMessage.Send();

        if(chatMessage.trigger != null && chatMessage.trigger.CanExecute(cInfo))
            chatMessage.trigger.callback(cInfo, chatMessage.command, chatMessage.args);

        if (string.IsNullOrEmpty(discordWebhookURL))
            return;

        if (chatMessage.type != EChatType.Global)
            return;

        if (discordFilterCmds && (chatMessage.isPublicTrigger || chatMessage.isPrivateTrigger))
            return;

        StringBuilder msg = new StringBuilder(discordMessageTemplate);
        msg.Replace("{NAME}", cInfo.playerName)
           .Replace("{MSG}", chatMessage.msg);

        if (discordFilterEveryone)
        {
            msg.Replace("@everyone", "@ everyone").
                Replace("@here", "@ here");
        }

        CF_DiscordWebhook.SendMessage(msg.ToString(), discordWebhookURL);
    }
    public static void ApplyDefaultChatFormatting(ClientInfo cInfo, CF_ChatMessage chatMessage)
    {
        // TODO: Implement logic to apply default chat formatting based on player's permission level and playtime
    }
}