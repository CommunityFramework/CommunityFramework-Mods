namespace CF_Chat
{
    public class API : IModApi
    {
        public static ModX module = new ModX("ChatManager", OnConfigLoaded, OnPhrasesLoaded);
        public void InitMod(Mod _modInstance)
        {
            module.Activate();

            ModEvents.ChatMessage.RegisterHandler(ChatManager.OnChatMessage);
        }
        public static void OnConfigLoaded()
        {

        }
        public static void OnPhrasesLoaded()
        {

        }
    }
}