using System;
using HarmonyLib;
using System.Collections.Generic;
using CF_Core;

namespace CF_Server
{
    public class API : IModApi
    {
        public static DateTime serverStarted;
        public static CF_Mod mod = new CF_Mod("CF_Server", OnConfigLoaded, OnPhrasesLoaded);
        public static CF_Log x = new CF_Log("CF_Server");
        Harmony harmony = new Harmony("CF_Server");

        public void InitMod(Mod _modInstance)
        {
            serverStarted = DateTime.UtcNow;

            mod.Activate();

            harmony.PatchAll();

            ChatManager.RegisterChatTrigger("serverstats,ss,server", OnServerStatsCommand);

            CF_Timer.AddOneSecTimer(ServerMonitor.OnEverySec, "ServerMonitor", false);
            CF_Timer.AddOneSecTimer(RestartManager.OnEverySec, "RestartManager", false);
        }
        public static string eventS;
        public static int restartBloodmoonHours;
        public static int tickerInterval;
        public static int FPSsamples;
        public static int FPSveryLow;
        public static int FPSveryLowBM;
        public static int FPSveryLowSamples;
        public static int FPSveryLowSamplesBM;
        public static int FPSlowAverage;
        public static int FPSlowAverageBM;
        public static int minUptime;
        public static int restartCountdown;
        public static int shutdownMode;
        public static int saveInterval;

        public static int restartVoteMinUptime;
        public static int restartVoteCountdown;
        public static int restartVoteMinVotes;
        public static int restartVoteMinVotesPerc;
        public static int restartVoteExpire;
        public static string restartVoteCommands;
        public static string instanceName;
        public static int adminPermIgnore;

        public static void OnConfigLoaded()
        {
            mod.AddSetting("EventS", "restart_countdown", "", "", out eventS);
            mod.AddSetting("BM_Hour", 17, -1, 23, "At which day hour to start restart countdown before BM (-1: Disabled)", out restartBloodmoonHours);
            mod.AddSetting("Ticker_Interval", 600, 0, 9999, "Discord report stats interval", out tickerInterval);
            mod.AddSetting("FPS_Samples", 120, 0, 9999, "How much fps samples to take (every 1s)", out FPSsamples);
            mod.AddSetting("FPS_VeryLow", 10, 0, 9999, "When FPS is below this limit it's counted as very low", out FPSveryLow);
            mod.AddSetting("FPS_VeryLowBM", 5, 0, 9999, "When FPS is below this limit it's counted as very low (during bloodmoon)", out FPSveryLowBM);
            mod.AddSetting("FPS_VeryLow_Samples", 60, 0, 999999, "How much very low fps samples to trigger restart countdown", out FPSveryLowSamples);
            mod.AddSetting("FPS_VeryLow_SamplesBM", 120, 0, 999999, "How much very low fps samples to trigger restart countdown (during bloodmoon)", out FPSveryLowSamplesBM);
            mod.AddSetting("FPS_LowAvg", 18, 0, 9999, "If fps is below this limit on average trigger a restart countdown", out FPSlowAverage);
            mod.AddSetting("FPS_LowAvgBM", 7, 0, 9999, "If fps is below this limit on average trigger a restart countdown (during bloodmoon)", out FPSlowAverageBM);
            mod.AddSetting("Restart_MinUptime", 90, 0, 999999, "Min. uptime in minutes before an auto. restart countdown can be triggered", out minUptime);
            mod.AddSetting("Restart_Countdown", 600, 0, 999999, "Restart countdown length", out restartCountdown);
            mod.AddSetting("Shutdown_Mode", 0, 0, 2, "0: Normal, 1: Windows 2: Linux", out shutdownMode);
            mod.AddSetting("SaveWorld_Interval", 300, 0, 999999, "Interval in seconds to trigger a world save.", out saveInterval);
            mod.AddSetting("Vote_MinUptime", 30, 0, 999999, "Min uptime before allowing votes.", out restartVoteMinUptime);
            mod.AddSetting("Vote_Countdown", 180, 0, 999999, "How long should the countdown be.", out restartVoteCountdown);
            mod.AddSetting("Vote_MinVotes", 1, 0, 999999, "Min amount of votes.", out restartVoteMinVotes);
            mod.AddSetting("Vote_MinVotesPerc", 35, 0, 100, "Min amount of votes as percentage of connected players.", out restartVoteMinVotesPerc);
            mod.AddSetting("Vote_Expire", 5, 0, 999999, "How long a vote counts in minutes.", out restartVoteExpire);
            mod.AddSetting("Vote_Commands", "rr,restart,reset", "", "Chat commands which can be used to vote for server restart separated by ','", out restartVoteCommands);
            mod.AddSetting("Shutdown_Instance", "1", "", ".", out instanceName);
            mod.AddSetting("AdminIgnore", 0, -1, 1000, "", out adminPermIgnore);

            ChatManager.RegisterChatTrigger(restartVoteCommands, RestartVoting.OnVoteRestart);
        }
        public static string msgCountdown;
        public static string msgCountdownM;
        public static string msgDenied;
        public static string msgLocked;
        public static string msgAborted;
        public static string msgLogout;

        public static string msgCountdownStarted;
        public static string msgCountdownByAdmin;
        public static string msgCountdownStartedDefault;
        public static void OnPhrasesLoaded()
        {
            mod.AddPhrase("Countdown", "Server will restart in {COUNTDOWN} s.", "", out msgCountdown);
            mod.AddPhrase("CountdownM", "Server will restart in {COUNTDOWN} m.", "", out msgCountdownM);
            mod.AddPhrase("Denied", "You are not allowed to interact with this until the server restarted.", "", out msgDenied);
            mod.AddPhrase("Locked", "[76eec6]Loot, storage, trader, vendor and workstations are now locked until the server restarted, [ec1414]items lost when not disconnected normal will not be restored!.", "", out msgLocked);
            mod.AddPhrase("Aborted", "[76eec6]Shutdown [ec1414]aborted[76eec6].", "", out msgAborted);
            mod.AddPhrase("Logout", "[76eec6]!!! [ec1414]DISCONNECT NOW [76eec6]!!!", "", out msgLogout);

            mod.AddPhrase("Countdown_Started", "[76eec6]Restart countdown started. Reason: [ec1414]{REASON}.", "", out msgCountdownStarted);
            mod.AddPhrase("Countdown_ByAdmin", "[76eec6]An [ec1414]admin [76eec6]triggered a restart countdown. ", "", out msgCountdownByAdmin);
            mod.AddPhrase("Countdown_StartedDefault", "[76eec6]Restart countdown started.", "", out msgCountdownStartedDefault);
        }
        public static void OnServerStatsCommand(ClientInfo _cInfo, string _trigger, List<string> args)
        {
            ChatTrigger_ServerStats.ServerStats(_cInfo, args.Count == 1 && args[0].Equals("all"));
        }
    }
}