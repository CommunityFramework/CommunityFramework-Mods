using CF_Chat;
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
            log.Error($"OnChatMessage reported: {e.Message}");
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

        if (!chatMessage.send)
            return;

        // Apply default chat colors, name prefixes, and name colors based on player's permission level and playtime

        ApplyRanking(cInfo, CF_Player.GetPermission(cInfo), cInfo.latestPlayerData.totalTimePlayed, chatMessage);

        chatMessage.ApplyNameAndChatColors();
        chatMessage.Send();

        if (chatMessage.IsTrigger() && chatMessage.trigger.CanExecute(cInfo))
            chatMessage.trigger.callback(cInfo, chatMessage.command, chatMessage.args);

        if (string.IsNullOrEmpty(discordWebhookURL) 
            || chatMessage.type != EChatType.Global 
            || discordFilterCmds && (chatMessage.isPublicTrigger || chatMessage.isPrivateTrigger))
            return;

        StringBuilder sb = new StringBuilder(discordMessageTemplate);
        sb.Replace("{NAME}", cInfo.playerName)
           .Replace("{MSG}", chatMessage.msg);

        if (discordFilterEveryone)
        {
            sb.Replace("@everyone", "@ everyone").
                Replace("@here", "@ here");
        }

        CF_DiscordWebhook.SendMessage(sb.ToString(), discordWebhookURL, GamePrefs.GetString(EnumGamePrefs.ServerName));
    }
    public static void ApplyRanking(ClientInfo cInfo, int permissionsLevel, float totalTimePlayed, CF_ChatMessage chatMessage)
    {
        // Sort the rankConfigs dictionary first by PermissionLevel in descending order, then by Playtime in descending order
        rankConfigs = rankConfigs.OrderBy(r => r.Value.PermissionLevel)
                                 .ThenByDescending(r => r.Value.Playtime)
                                 .ToDictionary(pair => pair.Key, pair => pair.Value);

        // Find the appropriate rank based on permissions and playtime
        RankConfig matchedRank = null;
        foreach (var rank in rankConfigs)
        {
            if (permissionsLevel > rank.Value.PermissionLevel)
                continue;

            if (totalTimePlayed < rank.Value.Playtime)
                continue;

            if (rank.Value.Players != null && !rank.Value.IsSpecificPlayer(cInfo))
                continue;

            matchedRank = rank.Value;
            break;
        }

        // If a rank is found, apply its properties to the chat message
        if (matchedRank != null)
        {
            // Apply the properties from the matched rank to the chat message
            if (!string.IsNullOrEmpty(matchedRank.CustomName))
                chatMessage.AddCustomName(matchedRank.CustomName);
            else if (!string.IsNullOrEmpty(matchedRank.NameColor))
                chatMessage.AddNameColor(matchedRank.NameColor);

            if (!string.IsNullOrEmpty(matchedRank.ChatColor))
                chatMessage.AddChatColor(matchedRank.ChatColor);

            if (!string.IsNullOrEmpty(matchedRank.NamePre))
                chatMessage.AddNameTagPrefix(matchedRank.NamePre);

            if (!string.IsNullOrEmpty(matchedRank.NamePost))
                chatMessage.AddNameTagPostfix(matchedRank.NamePost);
        }
    }
}