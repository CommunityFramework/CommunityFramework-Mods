using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using static CF_Server.API;

public class CF_RestartManager
{
    static AccessTools.FieldRef<AIDirectorBloodMoonComponent, int> bmDayRef = AccessTools.FieldRefAccess<AIDirectorBloodMoonComponent, int>("bmDay");
    public static bool Restarting() => countdown != -1;
    public static void Restart(int seconds, string _reason, bool manual = false)
    {
        countdown = seconds;

        if(!string.IsNullOrEmpty(_reason))
            CF_Player.Message(msgCountdownStarted.Replace("{REASON}", _reason));
        else 
            CF_Player.Message(msgCountdownStarted.Replace("{REASON}", msgCountdownByAdmin));

        log.Log($"CoundownExec => Seconds: {seconds} Manual: {manual} Reason: {_reason}");
        Log.Out($"CoundownExec => Seconds: {seconds} Manual: {manual} Reason: {_reason}");
    }
    public static void AbortRestart(string _reason, bool manual = false)
    {
        if (countdown != -1)
            return;

        countdown = -1;
        CF_Player.Message(msgAborted);
    }
    public static void OnEverySec() // Main timer (async)
    {
        CheckConditions();
        OnCountdownTick();
    }
    public static void CheckConditions()
    {
        TimeSpan timespan = TimeSpan.FromSeconds(30);
        float averageFPS = CF_ServerMonitor.GetAverageFPS(timespan);
        float lowestFPS = CF_ServerMonitor.GetLowestFPS(timespan);

        float veryLow = (float)FPSveryLow;
        if (GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
            veryLow = (float)FPSveryLowBM;
        int lowFPS = CF_ServerMonitor.GetLowerFPSCount(FPSveryLow, TimeSpan.FromSeconds(30));

        // Server restarting already within timewindow
        if (countdown > -1 || countdown < restartCountdown)
            return;

        // Min uptime
        if ((DateTime.UtcNow - serverStarted).TotalMinutes < minUptime)
            return;

        // Blood moon
        int day = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
        int hour = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
        int dayBM = bmDayRef(GameManager.Instance.World.aiDirector.BloodMoonComponent);
        if (restartBloodmoonHours != -1 && dayBM == day && hour == restartBloodmoonHours)
        {
            CF_Player.Message("[ff3333]Pre-Bloodmoon Restart!");
            Shutdown(restartCountdown);
            return;
        }

        // Low average performance
        int lowAverage = FPSlowAverage;
        if (GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
            lowAverage = FPSlowAverageBM;

        if (averageFPS > 0 && averageFPS < lowAverage)
        {
            log.Warn($"Low FPS detected (average): {averageFPS:F1}. Restarting server.");
            CF_Player.Message("[ff0000]Bad server performance detected!");
            Shutdown(restartCountdown);
            return;
        }

        // Much bad fps drops
        int veryLowSamples = FPSveryLowSamples;
        if (GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
            veryLowSamples = FPSveryLowSamplesBM;

        if (veryLowSamples > 0 && lowFPS > veryLowSamples)
        {
            log.Warn($"Low FPS detected: {lowFPS} of / {CF_ServerMonitor.GetLowerFPSCount(FPSveryLow, TimeSpan.FromSeconds(30))}s below {FPSveryLow}. Restarting server.");
            CF_Player.Message("[ff0000]Bad server performance detected!");
            Shutdown(restartCountdown);
            return;
        }
    }
    public static DateTime lastWorldSave = DateTime.UtcNow;
    public static int countdown = -1;
    public static int restartAttempts = 0;
    public static bool locked = false;
    public static void Shutdown(int seconds = 0)
    {
        if (seconds == -1)
        {
            log.Log($"ShutdownX=> Aborted");
            countdown = seconds;
            restartAttempts = 0;
            locked = false;
            CF_Player.Message(msgAborted);
            return;
        }
        else if (seconds > 0)
        {

            log.Log($"ShutdownX=> {seconds}s");
            countdown = seconds;
            lastS = -1;
            lastM = -1;
            return;
        }

        restartAttempts += 1;

        log.Log($"ShutdownX=> Shutdown: {restartAttempts}");

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
        if (countdown < 0)
            return;

        if (countdown == 0)
            Shutdown();

        if (countdown < 60)
        {
            TryLock();

            /*
            if (ConnectionManagerEx.joinQueueList.Count > 0)
            {
                foreach (string EOS in ConnectionManagerEx.joinQueueList)
                {
                    ClientInfo cInfo = Utilz.GetClientByEOS(EOS);
                    if (cInfo == null)
                        continue;

                    GameUtils.KickPlayerForClientInfo(cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, _customReason: "Server restarting..."));
                }
            }
            */
        }

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
    // Lock tile entities with storage from being opened
    public static void TryLock()
    {
        if (locked || countdown == -1 || countdown > 60)
            return;

        locked = true;

        List<ClientInfo> cList = CF_Player.GetClients();
        if (cList != null && cList.Count > 0)
        {
            for (int i = 0; i < cList.Count; i++)
            {
                CF_Player.Message(msgLocked, cList[i]);
                CloseAllXui(cList[i]);
            }
        }
    }
    public static void CloseAllXui(ClientInfo _cInfo)
    {
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageCloseAllWindows>().Setup(_cInfo.entityId));
    }
    public static bool CanOpenLootContainer(ClientInfo _cInfo, TileEntity _te)
    {
        if (!locked)
            return true;

        if (_te is TileEntityLootContainer
            || _te is TileEntityWorkstation
            || _te is TileEntitySecureLootContainer
            || _te is TileEntityVendingMachine
            || _te is TileEntityLootContainer)
        {
            CF_Player.Message(msgDenied, _cInfo);
            return false;
        }

        return true;
    }
}