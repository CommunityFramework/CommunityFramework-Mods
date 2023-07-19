﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WorldGenerationEngineFinal;
using static CF_Server.API;

public class RestartManager
{
    static AccessTools.FieldRef<AIDirectorBloodMoonComponent, int> bmDayRef = AccessTools.FieldRefAccess<AIDirectorBloodMoonComponent, int>("bmDay");
    public static bool Restarting() => countdown != -1;
    public static void Restart(int seconds, string _reason, bool manual = false)
    {
        countdown = seconds;

        if(!string.IsNullOrEmpty(_reason))
            Chat.Message(msgCountdownStarted.Replace("{REASON}", _reason));
        else 
            Chat.Message(msgCountdownStarted.Replace("{REASON}", msgCountdownByAdmin));

        x.Log($"CoundownExec => Seconds: {seconds} Manual: {manual} Reason: {_reason}");
        Log.Out($"CoundownExec => Seconds: {seconds} Manual: {manual} Reason: {_reason}");
    }
    public static void AbortRestart(string _reason, bool manual = false)
    {
        if (countdown != -1)
            return;

        countdown = -1;
        Chat.Message(msgAborted);
    }
    public static void OnEverySec() // Main timer (async)
    {
        CheckConditions();
        OnCountdownTick();
    }
    public static void CheckConditions()
    {
        float averageFPS = ServerMonitor.FPSlist_30s.Average();
        float lowestFPS = ServerMonitor.FPSlist_30s.Min();

        float veryLow = (float)FPSveryLow;
        if (GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
            veryLow = (float)FPSveryLowBM;
        int lowFPS = ServerMonitor.FPSlist_3m.Where(x => x < veryLow).ToList().Count(); // TODO allow changing which FPS list to use


        // Server restarting already within timewindow
        if (countdown > -1 && countdown < restartCountdown)
            return;

        // Min uptime
        if ((DateTime.Now - serverStarted).TotalMinutes < minUptime)
            return;

        // Blood moon
        int day = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
        int hour = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
        int dayBM = bmDayRef(GameManager.Instance.World.aiDirector.BloodMoonComponent);
        if (restartBloodmoonHours != -1 && dayBM == day && hour == restartBloodmoonHours)
        {
            Chat.Message("[ff3333]Pre-Bloodmoon Restart!");
            Shutdown(restartCountdown);
            return;
        }

        // Low average performance
        int lowAverage = FPSlowAverage;
        if (GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
            lowAverage = FPSlowAverageBM;

        if (averageFPS > 0 && averageFPS < lowAverage)
        {
            x.Warn($"Low FPS detected (average): {averageFPS:F1}. Restarting server.");
            Chat.Message("[ff0000]Bad server performance detected!");
            Shutdown(restartCountdown);
            return;
        }

        // Much bad fps drops
        int veryLowSamples = FPSveryLowSamples;
        if (GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
            veryLowSamples = FPSveryLowSamplesBM;

        if (veryLowSamples > 0 && lowFPS > veryLowSamples)
        {
            x.Warn($"Low FPS detected: {lowFPS} of / {ServerMonitor.FPSlist_30s.Count}s below {FPSveryLow}. Restarting server.");
            Chat.Message("[ff0000]Bad server performance detected!");
            Shutdown(restartCountdown);
            return;
        }

        /*
        // Max Items
        if (GameManager.Instance.fps.Counter > maxItems)
        {
            Shutdown(restartCountdown);
            return;
        }

        long totalMemory = GC.GetTotalMemory(false); // Maybe req full = true?

        // RAM RSS
        float RSS = ((float)((double)GetRSS.GetCurrentRSS() / 1024.0 / 1024.0));
        if (lowFPS > ramRSS)
        {
            Shutdown(restartCountdown);
            return;
        }

        // RAM Heap
        if (((float)totalMemory / 1048576f) > ramHeap)
        {
            Shutdown(restartCountdown);
            return;
        }
        */
    }
    public static DateTime lastWorldSave = DateTime.Now;
    public static int countdown = -1;
    public static int restartAttempts = 0;
    public static bool locked = false;
    public static void Shutdown(int seconds = 0)
    {
        if (seconds == -1)
        {
            x.Log($"ShutdownX=> Aborted");
            countdown = seconds;
            restartAttempts = 0;
            locked = false;
            Chat.Message(msgAborted);
            return;
        }
        else if (seconds > 0)
        {

            x.Log($"ShutdownX=> {seconds}s");
            countdown = seconds;
            lastS = -1;
            lastM = -1;
            return;
        }

        restartAttempts += 1;

        x.Log($"ShutdownX=> Shutdown: {restartAttempts}");

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
                x.Log($"ShutdownX(Kick)=> {cInfo.playerName} ({cInfo.PlatformId.ReadablePlatformUserIdentifier})");
                GameUtils.KickPlayerForClientInfo(cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, _customReason: "Server restarting..."));
            }

            return;
        }

        // Save Local Player
        x.Log($"ShutdownX=> Save Local Player Data");
        GameManager.Instance.SaveLocalPlayerData();

        // Save World
        x.Log($"ShutdownX=> Save World");
        GameManager.Instance.SaveWorld();

        KillServer();
    }
    public static void KillServer()
    {
        // Shutdown
        x.Log($"ShutdownX=> Shutting server down...");

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
            int countdown = (int)((double)RestartManager.countdown / 60.0);
            if (countdown != lastM)
            {
                lastM = countdown;

                switch (countdown)
                {
                    case 10:
                    case 5:
                    case 1:
                        // Save World
                        if (lastWorldSave.AddSeconds(10) > DateTime.Now)
                        {
                            lastWorldSave = DateTime.Now;
                            x.Log($"ShutdownX=> Save World");
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

                        x.Log($"COUNTDOWN => {countdown}m");
                        Log.Out($"COUNTDOWN => {countdown}m");
                        Chat.Message(msgCountdownM.Replace("{COUNTDOWN}", countdown.ToString()));

                        if (!string.IsNullOrEmpty(eventS))
                        {
                            ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
                            if (list != null && list.Count > 0)
                            {
                                for (int index = 0; index < list.Count; ++index)
                                    Players.GameEvent(list[index], eventS);
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
                    x.Log($"ShutdownX=> Save World");
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
                    x.Log($"COUNTDOWN => {countdown}s");
                    Log.Out($"COUNTDOWN => {countdown}s");
                    Chat.Message(msgCountdown.Replace("{COUNTDOWN}", countdown.ToString()));
                    Chat.Message(msgLogout);

                    if (!string.IsNullOrEmpty(eventS))
                    {
                        ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
                        if (list != null && list.Count > 0)
                        {
                            for (int index = 0; index < list.Count; ++index)
                                Players.GameEvent(list[index], eventS);
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

        List<ClientInfo> cList = Players.GetClients();
        if (cList != null && cList.Count > 0)
        {
            for (int i = 0; i < cList.Count; i++)
            {
                Chat.Message(msgLocked, cList[i]);
                CloseAllXui(cList[i]);
            }
        }
    }
    public static void CloseAllXui(ClientInfo _cInfo, bool crafting = false, bool backpack = false)
    {
        if (crafting)
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close crafting", true));

        if (backpack)
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close backpack", true));

        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close looting", true));
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close trader", true));
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close workstation", true));
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close vehicleStorage", true));
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
            Chat.Message(msgDenied, _cInfo);
            return false;
        }

        return true;
    }
}