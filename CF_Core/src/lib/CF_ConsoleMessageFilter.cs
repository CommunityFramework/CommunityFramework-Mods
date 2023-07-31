using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UniLinq;

public class CF_ConsoleMessageFilter
{
    public class MessageInfo
    {
        public int Cooldown;
        public int Count;
        public DateTime LastSentTime = DateTime.MinValue;
        public DateTime lastStatsPrintTime = DateTime.MinValue;
    }

    public Dictionary<string, MessageInfo> messageCooldowns = new Dictionary<string, MessageInfo>();
    public List<Regex> filteredPatterns = new List<Regex>();
    public Dictionary<string, string> filteredMessages = new Dictionary<string, string>();

    public const int DefaultCooldown = 10; // Default cooldown in seconds
    public const int MaxCooldown = 60; // Maximum cooldown in seconds
    public const int MaxCountBeforeCooldownIncrease = 5; // Maximum number of messages before cooldown increases
    public int MinStatsPrintInterval { get; set; } = 30; // Min time interval between stat prints (in seconds)

    // Method to add a pattern (regex) to be filtered
    public void AddFilteredPattern(string pattern)
    {
        filteredPatterns.Add(new Regex(pattern, RegexOptions.Compiled));
    }
    // Method to add a simple message to be filtered (exact match)
    public void AddFilteredMessage(string message)
    {
        // Convert the message to a regex pattern with placeholders
        string regexPattern = Regex.Escape(message);
        regexPattern = regexPattern.Replace(@"\ ", @"\s+"); // Convert space escapes to whitespace matching
        regexPattern = regexPattern.Replace(@"\*", ".*"); // Convert * escapes to wildcard matching
        regexPattern = regexPattern.Replace(@"\?", "."); // Convert ? escapes to single character matching

        filteredMessages[message] = regexPattern;
    }

    // Method to write a console message after applying the filter and cooldown
    public void Out(string _msg, ClientInfo _cInfo = null)
    {
        // Convert the filteredPatterns list to IEnumerable<Regex> using AsEnumerable()
        IEnumerable<Regex> allRegexPatterns = System.Linq.Enumerable.AsEnumerable(filteredPatterns);

        // Check if the message matches any of the filtered patterns, including messages with placeholders
        foreach (var pattern in System.Linq.Enumerable.Concat(allRegexPatterns, System.Linq.Enumerable.Select(filteredMessages.Values, v => new Regex(v))))
        {
            if (pattern.IsMatch(_msg))
            {
                // The message matches a filtered pattern, don't output it
                return;
            }
        }

        // Find the matching pattern for grouping similar messages
        string matchingPattern = null;
        foreach (var pattern in messageCooldowns.Keys)
        {
            if (Regex.IsMatch(_msg, pattern))
            {
                matchingPattern = pattern;
                break;
            }
        }

        // Check if the message is on cooldown
        if (matchingPattern != null && messageCooldowns.TryGetValue(matchingPattern, out MessageInfo messageInfo))
        {
            var elapsedTime = (int)(DateTime.UtcNow - messageInfo.LastSentTime).TotalSeconds;

            // Print the stats if the elapsed time exceeds the MaxStatsPrintInterval
            if ((DateTime.UtcNow - messageInfo.lastStatsPrintTime).TotalSeconds >= MinStatsPrintInterval)
            {
                int remainingCooldown = messageInfo.Cooldown - elapsedTime;
                CF_Console.Out($"[Cooldown] {_msg} (x{messageInfo.Count}, Cooldown: {remainingCooldown}s)");

                // Update the last time stats were printed
                messageInfo.lastStatsPrintTime = DateTime.UtcNow;
            }

            // Continue with the regular cooldown logic
            if (elapsedTime < messageInfo.Cooldown)
            {
                return;
            }
            else
            {
                messageInfo.Count++;
                messageInfo.LastSentTime = DateTime.UtcNow;

                if (messageInfo.Count >= MaxCountBeforeCooldownIncrease)
                {
                    messageInfo.Cooldown = Math.Min(messageInfo.Cooldown * 2, MaxCooldown);
                }
                else
                {
                    messageInfo.Cooldown = DefaultCooldown;
                }
            }
        }
        else
        {
            // Message is not in the cooldown dictionary, add it with default cooldown
            messageCooldowns[_msg] = new MessageInfo
            {
                Cooldown = DefaultCooldown,
                Count = 1,
                LastSentTime = DateTime.UtcNow
            };
        }

        // Write the message to the console or send it to the client if provided
        if (_cInfo != null)
        {
            // Send the message to the client
            CF_Player.Console(_msg, _cInfo);
        }
        else
        {
            // Write the message to the console
            CF_Console.Out(_msg);
        }
    }
}
