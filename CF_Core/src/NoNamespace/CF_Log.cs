using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;

public class CF_Log
{
    // Log Levels
    public enum LogLevel { Trace, Debug, Info, Warn, Error, Fatal, Corruption, Alert };

    public readonly string folder;
    public readonly string subfolder;
    public readonly bool playerFolders;
    public LogLevel thresholdLevel;
    public LogLevel minimumLevel;
    public LogLevel discordThreshold;
    public readonly string discordWebhookURL;
    private readonly ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();

    public CF_Log(string folder = "logs", string subfolder = "", LogLevel thresholdLevel = LogLevel.Error, LogLevel minimumLevel = LogLevel.Trace, string discordWebhookURL = "", LogLevel discordThreshold = LogLevel.Corruption, bool playerFolders = false)
    {
        this.folder = folder;
        this.subfolder = subfolder;
        this.thresholdLevel = thresholdLevel;
        this.minimumLevel = minimumLevel;
        this.discordWebhookURL = discordWebhookURL;
        this.discordThreshold = discordThreshold;
        this.playerFolders = playerFolders;
    }
    public CF_Log(bool playerFolders = false, string folder = "logs", string subfolder = "", LogLevel thresholdLevel = LogLevel.Error, LogLevel minimumLevel = LogLevel.Trace, string discordWebhookURL = "", LogLevel discordThreshold = LogLevel.Corruption)
    {
        this.folder = folder;
        this.subfolder = subfolder;
        this.thresholdLevel = thresholdLevel;
        this.minimumLevel = minimumLevel;
        this.discordWebhookURL = discordWebhookURL;
        this.discordThreshold = discordThreshold;
        this.playerFolders = playerFolders;
    }

    // Main Log method with timestamp and tags.
    public void Log(string message, ClientInfo cInfo = null, PersistentPlayerData pData = null, LogLevel level = LogLevel.Info, bool withTimestamp = true)
    {
        // Ignore log messages below the minimum level
        if (level < minimumLevel)
            return;
        try
        {
            var baseLogFileName = (cInfo == null && pData == null) || playerFolders 
                ? $"{DateTime.UtcNow:yyyy_MM_dd}"
                : (
                    cInfo != null 
                        ? $"{CF_Player.GetNameAndPlatformId(cInfo)}_{DateTime.UtcNow:yyyy_MM_dd}"
                        : $"{CF_Player.GetNameAndPlatformId(pData)}_{DateTime.UtcNow:yyyy_MM_dd}"
                    );

            var logDirectory = (cInfo == null && pData == null) || !playerFolders
                ? Path.Combine(Directory.GetCurrentDirectory(), "Mod_Logs", folder, subfolder)
                : (
                    cInfo != null
                    ? Path.Combine(Directory.GetCurrentDirectory(), "Mod_Logs", folder, subfolder, CF_Player.GetNameAndPlatformId(cInfo))
                    : Path.Combine(Directory.GetCurrentDirectory(), "Mod_Logs", folder, subfolder, CF_Player.GetNameAndPlatformId(pData))
                );

            Directory.CreateDirectory(logDirectory);

            var logFileName = (level >= thresholdLevel)
                ? $"{baseLogFileName}-{level}.log"
                : $"{baseLogFileName}.log";

            var logFilePath = Path.Combine(logDirectory, logFileName);
            var logMessage = withTimestamp ? $"{DateTime.UtcNow:HH:mm:ss} [{level}] {message}"
                                           : $"{level} {message}";

            File.AppendAllText(logFilePath, $"{logMessage}\n");

            // If level is above the discordThreshold, send message to Discord
            if (!string.IsNullOrEmpty(discordWebhookURL) && level >= discordThreshold)
            {
                CF_DiscordWebhook.SendMessage(logMessage, discordWebhookURL);
            }
        }
        catch (Exception){}
    }

    // Overloads for the Log method.

    // Log with no authName.
    public void Log(string message)
    {
        Log(message, null, null, LogLevel.Info, true);
    }

    // Log with authName.
    public void Log(string message, ClientInfo cInfo)
    {
        Log(message, cInfo, null, LogLevel.Info, true);
    }
    public void Log(string message, PersistentPlayerData pData)
    {
        Log(message, null, pData, LogLevel.Info, true);
    }

    // Log with authName and LogLevel.
    public void Log(string message, ClientInfo cInfo, LogLevel level)
    {
        Log(message, cInfo, null, level, true);
    }
    public void Log(string message, PersistentPlayerData pData, LogLevel level)
    {
        Log(message, null, pData, level, true);
    }

    // Log with LogLevel only.
    public void Log(string message, LogLevel level)
    {
        Log(message, null, null, level, true);
    }
    public void Out(string message)
    {
        Log(message, null, null, LogLevel.Info, true);
    }

    // Log with authName.
    public void Out(string message, ClientInfo cInfo)
    {
        Log(message, cInfo, null, LogLevel.Info, true);
    }
    public void Out(string message, PersistentPlayerData pData)
    {
        Log(message, null, pData, LogLevel.Info, true);
    }

    // Log with authName and LogLevel.
    public void Out(string message, ClientInfo cInfo, LogLevel level)
    {
        Log(message, cInfo, null, level, true);
    }
    public void Out(string message, PersistentPlayerData pData, LogLevel level)
    {
        Log(message, null, pData, level, true);
    }

    // Log with LogLevel only.
    public void Out(string message, LogLevel level)
    {
        Log(message, null, null, level, true);
    }
    public void Info(string message)
    {
        Log(message, null, null, LogLevel.Info, true);
    }

    // Log with authName.
    public void Info(string message, ClientInfo cInfo)
    {
        Log(message, cInfo, null, LogLevel.Info, true);
    }
    public void Info(string message, PersistentPlayerData pData)
    {
        Log(message, null, pData, LogLevel.Info, true);
    }

    // Log with authName and LogLevel.
    public void Info(string message, ClientInfo cInfo, LogLevel level)
    {
        Log(message, cInfo, null, level, true);
    }
    public void Info(string message, PersistentPlayerData pData, LogLevel level)
    {
        Log(message, null, pData, level, true);
    }

    // Log with LogLevel only.
    public void Info(string message, LogLevel level)
    {
        Log(message, null, null, level, true);
    }

    // ...
    public void Debug(string message)
    {
        Log(message, null, null, LogLevel.Debug, true);
    }
    public void Debug(string message, ClientInfo cInfo)
    {
        Log(message, cInfo, null, LogLevel.Debug, true);
    }
    public void Debug(string message, PersistentPlayerData pData)
    {
        Log(message, null, pData, LogLevel.Debug, true);
    }
    public void Warn(string message)
    {
        Log(message, null, null, LogLevel.Warn, true);
    }
    public void Warn(string message, ClientInfo cInfo)
    {
        Log(message, cInfo, null, LogLevel.Warn, true);
    }
    public void Warn(string message, PersistentPlayerData pData)
    {
        Log(message, null, pData, LogLevel.Warn, true);
    }
    public void Error(string message)
    {
        Log(message, null, null, LogLevel.Error, true);
    }
    public void Error(string message, ClientInfo cInfo)
    {
        Log(message, cInfo, null, LogLevel.Error, true);
    }
    public void Error(string message, PersistentPlayerData pData)
    {
        Log(message, null, pData, LogLevel.Error, true);
    }
    public void Fatal(string message)
    {
        Log(message, null, null, LogLevel.Fatal, true);
    }
    public void Fatal(string message, ClientInfo cInfo)
    {
        Log(message, cInfo, null, LogLevel.Fatal, true);
    }
    public void Fatal(string message, PersistentPlayerData pData)
    {
        Log(message, null, pData, LogLevel.Fatal, true);
    }
}
