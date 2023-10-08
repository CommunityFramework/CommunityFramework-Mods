using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;
using static CF_Server.API;

public class CF_RestartManager
{
    static AccessTools.FieldRef<AIDirectorBloodMoonComponent, int> bmDayRef = AccessTools.FieldRefAccess<AIDirectorBloodMoonComponent, int>("bmDay");
    public static bool Restarting() => countdown != -1;
    public static void Restart(int seconds, string _reason, bool _manual = false)
    {
        if(seconds < 1)
        {
            AbortRestart(_reason, _manual);
            return;
        }

        countdown = seconds;

        if(!string.IsNullOrEmpty(_reason))
            CF_Player.Message(msgCountdownStarted.Replace("{REASON}", _reason));
        else 
            CF_Player.Message(msgCountdownStarted.Replace("{REASON}", msgCountdownByAdmin));

        log.Log($"CoundownExec => Seconds: {seconds} Manual: {_manual} Reason: {_reason}");
        Log.Out($"CoundownExec => Seconds: {seconds} Manual: {_manual} Reason: {_reason}");
    }
    public static void AbortRestart(string _reason, bool manual = false)
    {
        if (countdown == -1)
            return;

        countdown = -1;
        locked = false;

        if (!string.IsNullOrEmpty(_reason))
            CF_Player.Message(msgAborted.Replace("{REASON}", _reason));
        else
            CF_Player.Message(msgAborted.Replace("{REASON}", msgCountdownByAdmin));
    }
    public static void OnEverySec() // Main timer (async)
    {
        CheckConditions();
        OnCountdownTick();
    }
    public static void CheckConditions()
    {
        if (countdown != -1)
            return;

        TimeSpan uptime = DateTime.UtcNow - serverStarted;
        if (uptime.TotalMinutes < minUptime)
            return;

        log.Out($"CheckConditions => countdown: {countdown} uptime: {uptime.TotalMinutes}m");

        TimeSpan timespan = TimeSpan.FromSeconds(30);
        float averageFPS = CF_ServerMonitor.GetAverageFPS(timespan);
        float lowestFPS = CF_ServerMonitor.GetLowestFPS(timespan);
        bool isBloodMoonActive = GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive;

        // Set FPS thresholds based on Blood Moon status
        float veryLow = isBloodMoonActive ? (float)FPSveryLowBM : (float)FPSveryLow;
        int lowAverage = isBloodMoonActive ? FPSlowAverageBM : FPSlowAverage;
        int veryLowSamples = isBloodMoonActive ? FPSveryLowSamplesBM : FPSveryLowSamples;

        int lowFPS = CF_ServerMonitor.GetLowerFPSCount(veryLow, timespan);

        // Calculate additional stats

        /*
        // Prepare the log message
        StringBuilder logMessage = new StringBuilder("Server Stats: ");
        
        logMessage.AppendFormat(" == Status ==");
        logMessage.AppendFormat(" Countdown: {0}s, ", countdown);
        logMessage.AppendFormat(" Default: {0}s, ", restartCountdown);
        logMessage.AppendFormat(" == Stats ==");
        logMessage.AppendFormat(" Uptime: {0}s, ", uptime.ToString());
        logMessage.AppendFormat(" Timespan: {0}s, ", timespan.TotalSeconds);
        logMessage.AppendFormat(" Average FPS: {0}, ", CF_ServerMonitor.GetAverageFPS(timespan));
        logMessage.AppendFormat(" Lowest FPS: {0}, ", CF_ServerMonitor.GetLowestFPS(timespan));
        logMessage.AppendFormat(" Highest FPS: {0}, ", CF_ServerMonitor.GetMaximumFPS(timespan));
        logMessage.AppendFormat(" Median FPS: {0}, ", CF_ServerMonitor.GetMedianFPS(timespan));
        logMessage.AppendFormat(" == Triggers ==");
        logMessage.AppendFormat(" Low FPS Count: {0}, ", lowFPS);
        logMessage.AppendFormat(" Very Low FPS Threshold: {0}, ", veryLow);
        logMessage.AppendFormat(" Low Average FPS Threshold: {0}, ", lowAverage);
        logMessage.AppendFormat(" Very Low Samples Threshold: {0}, ", veryLowSamples);
        logMessage.AppendFormat(" Blood Moon Active: {0}", isBloodMoonActive ? "Yes" : "No");

        // Output the log
        log.Out(logMessage.ToString());
        */

        // Check for Blood Moon restart condition
        int day = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
        int hour = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
        int dayBM = bmDayRef(GameManager.Instance.World.aiDirector.BloodMoonComponent);

        if (restartBloodmoonHours != -1 && dayBM == day && hour == restartBloodmoonHours)
        {
            TriggerShutdown("[ff3333]Pre-Bloodmoon Restart!", restartCountdown ,"Pre BM restart");
            return;
        }

        // Check for low average FPS
        if (averageFPS > 0 && averageFPS < lowAverage)
        {
            TriggerShutdown("[ff0000]Bad server performance detected!", restartCountdown, $"Low FPS detected (average): {averageFPS:F1}. Restarting server.");
            return;
        }

        // Check for frequent low FPS
        if (veryLowSamples > 0 && lowFPS > veryLowSamples)
        {
            TriggerShutdown("[ff0000]Bad server performance detected!", restartCountdown, $"Low FPS detected: {lowFPS} of / {CF_ServerMonitor.GetLowerFPSCount(veryLow, timespan)}s below {veryLow}. Restarting server.");
            return;
        }
    }

    // Helper method to handle shutdown logic
    private static void TriggerShutdown(string playerMessage, int countdown, string logMessage = null)
    {
        if (!string.IsNullOrEmpty(logMessage))
        {
            log.Warn(logMessage);
        }
        CF_Player.Message(playerMessage);
        Shutdown(countdown);
    }

    public static DateTime lastWorldSave = DateTime.UtcNow;
    public static int countdown = -1;
    public static int restartAttempts = 0;
    public static bool locked = false;
    public static void Shutdown(int seconds = 0)
    {
        if (seconds == -1)
        {
            log.Log($"Aborted restart countdown");
            countdown = seconds;
            restartAttempts = 0;
            locked = false;
            CF_Player.Message(msgAborted);
            return;
        }
        else if (seconds > 0)
        {

            log.Log($"Restarting server in {seconds}s");
            countdown = seconds;
            lastS = -1;
            lastM = -1;
            return;
        }

        restartAttempts += 1;

        log.Log($"Shutdown attempt #{restartAttempts}");

        if (restartAttempts > 20)
        {
            KillServer();
        }

        ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
        if (list != null && list.Count > 0)
        {
            for (int index = 0; index < list.Count; ++index)
            {
                ClientInfo cInfo = list[index];

                // Save latest player data
                PlayerDataFile latestPlayerData = cInfo.latestPlayerData;
                if (latestPlayerData.bModifiedSinceLastSave)
                    latestPlayerData.Save(GameIO.GetPlayerDataDir(), cInfo.InternalId.CombinedString);

                // Kick
                log.Log($"ShutdownX(Kick)=> {cInfo.playerName} ({cInfo.PlatformId.ReadablePlatformUserIdentifier})");
                GameUtils.KickPlayerForClientInfo(cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, _customReason: "Server restarting..."));
            }

            return;
        }

        // Save Local Player
        log.Log($"ShutdownX=> Save Local Player Data");
        GameManager.Instance.SaveLocalPlayerData();

        // Save World
        log.Log($"ShutdownX=> Save World");
        GameManager.Instance.SaveWorld();

        KillServer();
    }
    public static void KillServer()
    {
        // Shutdown
        log.Log($"ShutdownX=> Shutting server down...");

        if (shutdownMode == 1)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = $"/C taskkill /IM 7DaysToDieServer{instanceName}.exe /F";
                process.StartInfo = startInfo;
                process.Start();
            }
            catch (Exception e)
            {
                Log.Error($"ShutdownA reported: {e.Message}");
            }

            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = $"/C wmic process where name='7DaysToDieServer{instanceName}.exe' deletegit add --all";
                process.StartInfo = startInfo;
                process.Start();
            }
            catch (Exception e)
            {
                Log.Error($"ShutdownB reported: {e}");
            }

            try
            {
                Application.Quit();
            }
            catch (Exception e) { Log.Error($"Shutdown reported: {e}"); }
        }
        else if (shutdownMode == 2)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.Arguments = "/home/sdtdserver/sdtdserver restart";
                process.StartInfo = startInfo;
                process.Start();

                countdown = -1;
            }
            catch (Exception e)
            {
                Log.Error($"ShutdownC reported: {e}");
                Application.Quit();
            }
        }
        else
        {
            try
            {
                Application.Quit();
            }
            catch (Exception e) { Log.Error($"Shutdown reported: {e}"); }
        }
    }
    public static int lastM = -1;
    public static int lastS = -1;
    public static void OnCountdownTick()
    {
        if (countdown < 0) return;
        if (countdown == 0) Shutdown();
        if (countdown < 60) TryLock();

        if (countdown >= 60 && countdown % 60 == 0)
        {
            int countdown = (int)((double)CF_RestartManager.countdown / 60.0);
            if (countdown != lastM)
            {
                lastM = countdown;

                switch (countdown)
                {
                    case 10:
                    case 5:
                    case 1:
                        // Save World
                        if (lastWorldSave.AddSeconds(10) > DateTime.UtcNow)
                        {
                            lastWorldSave = DateTime.UtcNow;
                            log.Log($"ShutdownX=> Save World");
                            Log.Out($"ShutdownX=> Save World"); ;
                            GameManager.Instance.SaveWorld();
                        }
                        break;
                }

                switch (countdown)
                {
                    case 60:
                    case 45:
                    case 30:
                    case 20:
                    case 15:
                    case 10:
                    case 5:
                    case 4:
                    case 3:
                    case 2:
                    case 1:

                        log.Log($"COUNTDOWN => {countdown}m");
                        Log.Out($"COUNTDOWN => {countdown}m");
                        CF_Player.Message(msgCountdownM.Replace("{COUNTDOWN}", countdown.ToString()));

                        if (!string.IsNullOrEmpty(eventS))
                        {
                            ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
                            if (list != null && list.Count > 0)
                            {
                                for (int index = 0; index < list.Count; ++index)
                                    CF_Player.FireGameEvent(list[index], eventS);
                            }
                        }
                        break;
                }
            }
        }
        else if (lastS != countdown)
        {
            lastS = countdown;

            switch (countdown)
            {
                case 30:
                case 10:
                case 5:
                case 1:
                case 0:
                    // Save World
                    log.Log($"ShutdownX=> Save World");
                    Log.Out($"ShutdownX=> Save World");
                    break;
            }

            switch (countdown)
            {
                case 50:
                case 40:
                case 30:
                case 25:
                case 20:
                case 15:
                case 10:
                case 5:
                case 4:
                case 3:
                case 2:
                case 1:
                    log.Log($"COUNTDOWN => {countdown}s");
                    Log.Out($"COUNTDOWN => {countdown}s");
                    CF_Player.Message(msgCountdown.Replace("{COUNTDOWN}", countdown.ToString()));
                    CF_Player.Message(msgLogout);

                    if (!string.IsNullOrEmpty(eventS))
                    {
                        ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
                        if (list != null && list.Count > 0)
                        {
                            for (int index = 0; index < list.Count; ++index)
                                CF_Player.FireGameEvent(list[index], eventS);
                        }
                    }
                    break;
            }
        }

        if (countdown > 0)
            countdown--;
    }
    public static void TryLock()
    {
        if (locked || countdown == -1 || countdown > 60) return;

        locked = true;

        List<ClientInfo> cList = CF_Player.GetClients();
        if (cList?.Count > 0)
        {
            cList.ForEach(client => {
                CF_Player.Message(msgLocked, client);
                CF_Player.CloseAllXui(client);
            });
        }
    }

}