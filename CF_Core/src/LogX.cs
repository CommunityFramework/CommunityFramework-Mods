using System;
using System.IO;
using System.Collections.Concurrent;

public class LogX
{
    // Log Levels
    public enum LogLevel { Trace, Debug, Info, Warn, Error, Fatal, Corruption, Alert };

    private readonly string folder;
    private readonly string subfolder;
    private readonly LogLevel thresholdLevel;
    private readonly LogLevel minimumLevel;
    private readonly LogLevel discordThreshold;
    private readonly string discordWebhookURL;
    private readonly ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();

    public LogX(string folder = "logs", string subfolder = "", LogLevel thresholdLevel = LogLevel.Error, LogLevel minimumLevel = LogLevel.Trace, string discordWebhookURL = "", LogLevel discordThreshold = LogLevel.Corruption)
    {
        this.folder = folder;
        this.subfolder = subfolder;
        this.thresholdLevel = thresholdLevel;
        this.minimumLevel = minimumLevel;
        this.discordWebhookURL = discordWebhookURL;
        this.discordThreshold = discordThreshold;
        ProcessingThread();
    }

    // Main Log method with timestamp and tags.
    public void Log(string message, ClientInfo cInfo = null, LogLevel level = LogLevel.Info, bool withTimestamp = true)
    {
        // Ignore log messages below the minimum level
        if (level < minimumLevel)
            return;

        var baseLogFileName = cInfo == null ? $"{DateTime.UtcNow:yyyyMMdd}"
                                            : $"{XFormat.GetNameAndPlatform(cInfo)}_{DateTime.UtcNow:yyyyMMdd}";

        var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mod_Logs", folder, subfolder);
        Directory.CreateDirectory(logDirectory);

        var logFileName = (level >= thresholdLevel)
            ? $"{baseLogFileName}-{level}.log"
            : $"{baseLogFileName}.log";

        var logFilePath = Path.Combine(logDirectory, logFileName);
        var logMessage = withTimestamp ? $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff zzz} [{level}] {message}"
                                       : $"{level} {message}";

        logQueue.Enqueue($"{logFilePath}|{logMessage}");

        // If level is above the discordThreshold, send message to Discord
        if (!string.IsNullOrEmpty(discordWebhookURL) && level >= discordThreshold)
        {
            DiscordWebhook.SendMessage(logMessage, discordWebhookURL);
        }
    }

    private void ProcessingThread()
    {
        ThreadManager.AddSingleTaskMainThread("LogX", new ThreadManager.MainThreadTaskFunctionDelegate(ProcessQueue), null);
    }

    private void ProcessQueue(object _parameter)
    {
        int count = 0;
        while (logQueue.TryDequeue(out var logItem) && count < 20)
        {
            count++;
            var logParts = logItem.Split('|');
            File.AppendAllText(logParts[0], $"{logParts[1]}\n");
        }
    }

    // Overloads for the Log method.

    // Log with no authName.
    public void Log(string message)
    {
        Log(message, null, LogLevel.Info, true);
    }

    // Log with authName.
    public void Log(string message, ClientInfo cInfo)
    {
        Log(message, cInfo, LogLevel.Info, true);
    }

    // Log with authName and LogLevel.
    public void Log(string message, ClientInfo cInfo, LogLevel level)
    {
        Log(message, cInfo, level, true);
    }

    // Log with LogLevel only.
    public void Log(string message, LogLevel level)
    {
        Log(message, null, level, true);
    }
    public void Out(string message)
    {
        Log(message, null, LogLevel.Info, true);
    }

    // Log with authName.
    public void Out(string message, ClientInfo cInfo)
    {
        Log(message, cInfo, LogLevel.Info, true);
    }

    // Log with authName and LogLevel.
    public void Out(string message, ClientInfo cInfo, LogLevel level)
    {
        Log(message, cInfo, level, true);
    }

    // Log with LogLevel only.
    public void Out(string message, LogLevel level)
    {
        Log(message, null, level, true);
    }
    public void Info(string message)
    {
        Log(message, null, LogLevel.Info, true);
    }

    // Log with authName.
    public void Info(string message, ClientInfo cInfo)
    {
        Log(message, cInfo, LogLevel.Info, true);
    }

    // Log with authName and LogLevel.
    public void Info(string message, ClientInfo cInfo, LogLevel level)
    {
        Log(message, cInfo, level, true);
    }

    // Log with LogLevel only.
    public void Info(string message, LogLevel level)
    {
        Log(message, null, level, true);
    }

    // ...
    public void Debug(string message)
    {
        Log(message, null, LogLevel.Debug, true);
    }
    public void Debug(string message, ClientInfo cInfo)
    {
        Log(message, cInfo, LogLevel.Debug, true);
    }
    public void Warn(string message)
    {
        Log(message, null, LogLevel.Warn, true);
    }
    public void Warn(string message, ClientInfo cInfo)
    {
        Log(message, cInfo, LogLevel.Warn, true);
    }
    public void Error(string message)
    {
        Log(message, null, LogLevel.Error, true);
    }
    public void Error(string message, ClientInfo cInfo)
    {
        Log(message, cInfo, LogLevel.Error, true);
    }
    public void Fatal(string message)
    {
        Log(message, null, LogLevel.Fatal, true);
    }
    public void Fatal(string message, ClientInfo cInfo)
    {
        Log(message, cInfo, LogLevel.Fatal, true);
    }
}
