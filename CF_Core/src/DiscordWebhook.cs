using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class DiscordWebhook : IDisposable
{
    private readonly WebClient client;
    public string webhook { get; set; }
    public string username { get; set; }

    public DiscordWebhook()
    {
        client = new WebClient();
    }

    public static void SendMessage(string message, string webhookURL, string userName = "Server")
    {
        if (string.IsNullOrEmpty(webhookURL))
            return;

        try
        {
            using (DiscordWebhook dcWeb = new DiscordWebhook())
            {
                dcWeb.username = string.IsNullOrEmpty(userName) ? "Server" : userName;
                dcWeb.webhook = webhookURL;
                dcWeb.Send(message);
            }
        }
        catch (Exception e)
        {
            Log.Error($"DiscordWebhook.SendMessage reported: {e}");
        }
    }

    public void Send(string msgSend)
    {
        try
        {
            NameValueCollection values = new NameValueCollection();
            values.Add("username", username);
            values.Add("content", msgSend);
            client.UploadValuesAsync(new Uri(webhook), values);
        }
        catch (Exception e)
        {
            Log.Error($"DiscordWebhook.SendMessage reported: {e}");
        }
    }

    public void Dispose()
    {
        client.Dispose();
    }
}

public class DiscordApp
{
    private PriorityQueue<Message> messageQueue;
    private RateLimiter rateLimiter;
    private LogX logger;

    public DiscordApp()
    {
        this.messageQueue = new PriorityQueue<Message>();
        this.rateLimiter = new RateLimiter();
        this.logger = new LogX("DiscordWebhooks");
    }

    public void SendMessage(Message message)
    {
        // Add message to queue
        this.messageQueue.Enqueue(message);

        // Process messages
        while (!this.messageQueue.IsEmpty())
        {
            Message nextMessage = this.messageQueue.Peek();

            // Check rate limit
            if (this.rateLimiter.CanMakeRequest())
            {
                try
                {
                    // Send message
                    DiscordWebhook.SendMessage(nextMessage.Content, nextMessage.webhookURL, nextMessage.userName);
                    // If successful, remove message from queue
                    this.messageQueue.Dequeue();
                }
                catch (RateLimitException ex)
                {
                    Console.WriteLine("Hit rate limit. Waiting for " + ex.RetryAfter + " milliseconds before retrying.");

                    // Get the current time
                    DateTime currentTime = DateTime.Now;

                    // Calculate the time to wait until
                    DateTime waitUntil = currentTime.AddMilliseconds(ex.RetryAfter.TotalMilliseconds);

                    // Wait until the specified time
                    while (DateTime.Now < waitUntil)
                    {
                        // Sleep for a short amount of time to avoid busy waiting
                        Thread.Sleep(100);
                    }

                    // Retry the request
                }

                catch (Exception ex)
                {
                    // Log error
                    this.logger.Log(ex.Message);
                }
            }
            else
            {
                // Wait for rate limit reset
                Thread.Sleep(this.rateLimiter.GetResetTime());
            }
        }
    }
    public async Task SendMessageAsync(string message, string webhookUrl, string username = "Server")
    {
        using (HttpClient client = new HttpClient())
        {
            var payload = new
            {
                content = message,
                username = username
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(webhookUrl, httpContent);

            if (response.StatusCode == (HttpStatusCode)429) // Too Many Requests
            {
                var rateLimitResponse = await response.Content.ReadAsStringAsync();
                var rateLimitData = JsonConvert.DeserializeObject<RateLimitData>(rateLimitResponse);

                // Log and handle rate limit
                Console.WriteLine($"Rate limit hit. Retry after: {rateLimitData.RetryAfter} ms. Global: {rateLimitData.Global}");

                // Wait for rate limit to reset
                await Task.Delay(rateLimitData.RetryAfter);

                // Retry sending the message
                await SendMessageAsync(message, webhookUrl, username);
            }
            else if (!response.IsSuccessStatusCode)
            {
                // Handle other possible HTTP errors
                Console.WriteLine($"Error sending message: {response.StatusCode}");
            }
        }
    }

    public class RateLimitData
    {
        public int RetryAfter { get; set; }
        public bool Global { get; set; }
    }

}

public class RateLimitException : Exception
{
    public TimeSpan RetryAfter { get; }

    public RateLimitException(string message, TimeSpan retryAfter)
        : base(message)
    {
        this.RetryAfter = retryAfter;
    }
}

public class Message : IComparable<Message>
{
    public string Content { get; set; }
    public int Priority { get; set; }
    public string webhookURL { get; set; }
    public string userName { get; set; }

    public Message(string content, int priority, string webhookURL, string userName)
    {
        this.Content = content;
        this.Priority = priority;
        this.webhookURL = webhookURL;
        this.userName = userName;
    }

    public int CompareTo(Message other)
    {
        // Messages with lower priority values are "greater" than messages with higher priority values
        return this.Priority.CompareTo(other.Priority);
    }
}


public class PriorityQueue<T> where T : IComparable<T>
{
    private List<T> elements;

    public PriorityQueue()
    {
        this.elements = new List<T>();
    }

    public void Enqueue(T element)
    {
        this.elements.Add(element);
        this.elements = this.elements.OrderBy(e => e).ToList();
    }

    public T Dequeue()
    {
        var highestPriorityElement = this.elements.First();
        this.elements.Remove(highestPriorityElement);
        return highestPriorityElement;
    }

    public bool IsEmpty()
    {
        return !this.elements.Any();
    }

    public T Peek()
    {
        return this.elements.First();
    }
}

public class RateLimiter
{
    private DateTime resetTime;
    private int remainingRequests;

    public RateLimiter()
    {
        this.resetTime = DateTime.Now;
        this.remainingRequests = 0;
    }

    public bool CanMakeRequest()
    {
        return DateTime.Now >= this.resetTime && this.remainingRequests > 0;
    }

    public TimeSpan GetResetTime()
    {
        return this.resetTime - DateTime.Now;
    }

    public void UpdateRateLimitInfo(int remaining, DateTime reset)
    {
        this.remainingRequests = remaining;
        this.resetTime = reset;
    }
}
