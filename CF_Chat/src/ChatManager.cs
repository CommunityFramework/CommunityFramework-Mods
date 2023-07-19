using System;
using System.Collections.Generic;
using System.Linq;

public class ChatManager
{
    private static List<Action<ClientInfo, ChatMessage>> chatHandlers = new List<Action<ClientInfo, ChatMessage>>();
    public static Dictionary<string, ChatTrigger> chatTriggers = new Dictionary<string, ChatTrigger>();
    public static void RegisterHandler(Action<ClientInfo, ChatMessage> _callback)
    {
        chatHandlers.Add(_callback);
    }
    public static void RegisterChatTrigger(string trigger, Action<ClientInfo, string, List<string>> _callback)
    {
        foreach (string alias in trigger.Split(',').ToList())
            chatTriggers.Add(alias, new ChatTrigger(alias, _callback));
    }
    public static bool OnChatMessage(ClientInfo cInfo, EChatType type, int senderId, string msg, string mainName, bool localizeMain, List<int> recipientEntityIds)
    {
        try
        {
            HandleChatMessage(cInfo, new ChatMessage(type, senderId, msg, mainName, localizeMain, recipientEntityIds));
            return false;
        }
        catch (Exception e)
        {
            Log.Error($"OnChatMessage reported: {e}");
        }

        return true;
    }
    public static void HandleChatMessage(ClientInfo cInfo, ChatMessage chatMessage)
    {
        PersistentPlayerData ppd = GameManager.Instance.persistentPlayers?.GetPlayerDataFromEntityID(cInfo.entityId) ?? null;
        if (ppd == null)
            return;

        EntityPlayer entPlayer = GameManager.Instance.World.Players.dict.ContainsKey(cInfo.entityId) ? GameManager.Instance.World.Players.dict[cInfo.entityId] : null;
        if (entPlayer == null)
            return;

        foreach (Action<ClientInfo, ChatMessage> chatListener in chatHandlers)
            chatListener(cInfo, chatMessage);

        if (!chatMessage.send)
            return;

        chatMessage.Send();

        chatMessage.trigger?.callback(cInfo, chatMessage.command, chatMessage.args);
    }
}