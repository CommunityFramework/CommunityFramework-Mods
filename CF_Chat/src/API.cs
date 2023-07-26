using static GameSparks.Api.Responses.ListTeamChatResponse;

namespace CF_Chat
{
    public class API : IModApi
    {
        public static ModX mod = new ModX("CF_Chat", OnConfigLoaded, OnPhrasesLoaded);
        public void InitMod(Mod _modInstance)
        {
            mod.Activate();

            ModEvents.ChatMessage.RegisterHandler(ChatManager.OnChatMessage);
        }
        public static string discordWebhookURL;
        public static bool discordFilterCmds;
        public static bool discordFilterEveryone;
        public static void OnConfigLoaded()
        {
            mod.AddSetting("Discord_WebhookURL", "", "", "URL to send the message to. You can obtain a webhook url inside the integration category of each channel settings. Leave empty to disable this feature.", out discordWebhookURL);
            mod.AddSetting("Discord_Filter_Cmds", true, "If enabled chat commands will not be send.", out discordFilterCmds);
            mod.AddSetting("Discord_Filter_Everyone", true, "Replace @ here and @ everyone to defuse it.", out discordFilterEveryone);
        }
        public static string discordMessageTemplate;
        public static void OnPhrasesLoaded()
        {
            mod.AddPhrase("Discord_MessageTemplate", "{NAME}: {MSG}", "Template used to format message for Discord.", out discordMessageTemplate);
        }
    }
}