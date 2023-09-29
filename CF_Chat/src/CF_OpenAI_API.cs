using System;
using System.Net.Http;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using System.Xml.Linq;
using Newtonsoft.Json;
using static CF_Chat.API;
using CF_Core;

public class CF_OpenAI_API
{
    static readonly HttpClient client = new HttpClient();
    const string apiUrl = "https://api.openai.com/v1/engines/davinci/completions";

    static void Start()
    {
        string prompt = "You task is to analyse incomming chat messages for the level of toxcity from 0.0 to 1.0.\r\nIf a player is very toxic you can kick a player.\r\n\r\nYour output must be always using the following template:\r\n\r\nToxicity:{toxcity-level}\r\nKickPlayer:{name who to kick here if needs to kick and 0 if noone needs to be kicked}\r\nChatMessage:{chat message to respond to players if needed, try to be a good admin which only responds if needed}";
        HandleIncomingMessage(new CF_ChatMessage(prompt)).Wait();
    }

    static async Task HandleIncomingMessage(CF_ChatMessage _chatMessage)
    {
        // Check if the message is toxic
        string prompt = $"{DateTime.UtcNow} Player {_chatMessage.senderId} wrote: {_chatMessage.msg}.";
        string result = await CallGPT3(prompt);

        var parsed = ChatResponseTemplate.Parse(result);

        /*
        if(parsed.KickPlayer)

        // Parse the response here and decide whether the message is toxic
        float toxicityLevel = float.Parse(result);  // replace with actual parsing
        bool isToxic = toxicityLevel > 0.7;

        if (isToxic)
        {
            Console.WriteLine("This message contains toxic content.");
            return;
        }

        // Generate a response to the message
        prompt = $"{_chatMessage.msg}\nHow should I respond?";
        result = await CallGPT3(prompt);
        // Send the generated response
        OutgoingMessage(result);
        */
    }

    static async Task<string> CallGPT3(string prompt)
    {
        var content = new
        {
            engine = "text-davinci-003",
            prompt = prompt,
            temperature = 0.5,
            max_tokens = 60
        };

        var jsonContent = JsonConvert.SerializeObject(content);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {OpenAI_key}");
        var response = await client.PostAsync(apiUrl, httpContent);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        // Parse the response to extract the generated text
        // Replace this with actual parsing
        string generatedText = responseBody;
        return generatedText;
    }

    static void OutgoingMessage(string message)
    {
        // Implement this function to send the generated response
        Console.WriteLine($"Outgoing message: {message}");
    }

    public class ChatResponseTemplate
    {
        public float Toxicity { get; set; }
        public int KickPlayerID { get; set; }
        public string ChatMessage { get; set; }

        public static ChatResponseTemplate Parse(string template)
        {
            var lines = template.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var chatAdminTemplate = new ChatResponseTemplate();

            foreach (var line in lines)
            {
                if (line.StartsWith("Toxicity:"))
                {
                    var value = line.Substring("Toxicity:".Length);
                    chatAdminTemplate.Toxicity = float.Parse(value.Trim());
                }
                else if (line.StartsWith("KickPlayer:"))
                {
                    var value = line.Substring("KickPlayer:".Length);
                    if (int.TryParse(value.Trim(), out int KickPlayerID) && KickPlayerID >= 0)
                    {
                        chatAdminTemplate.KickPlayerID = KickPlayerID;
                    }
                    else log.Warn($"Expected player Id and got {KickPlayerID}");

                }
                else if (line.StartsWith("ChatMessage:"))
                {
                    var value = line.Substring("ChatMessage:".Length);
                    chatAdminTemplate.ChatMessage = value.Trim();
                }
            }

            return chatAdminTemplate;
        }
    }
}