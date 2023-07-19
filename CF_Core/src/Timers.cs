using System;
using System.Collections.Generic;
using System.Timers;
using static CF_Core.API;

namespace CF_Core
{
    public class Timers
    {
        private static Timer aTimer = new Timer();
        public static void TimerStart()
        {
            if (aTimer.Enabled)
                return;

            x.Log($"Started main timer.");

            aTimer.Interval = 1000;
            aTimer.AutoReset = true;
            aTimer.Elapsed += OnTick;
            aTimer.Enabled = true;
            GC.KeepAlive(aTimer);
        }

        public static List<Action> callOneSec = new List<Action>();
        public static List<Action> callOneSecSync = new List<Action>();
        public static void AddOneSecTimer(Action action, string name, bool sync)
        {
            x.Log($"Added new timer listener: {name}");

            if(sync)
                callOneSecSync.Add(action);
            else callOneSec.Add(action);
        }
        private static void OnTick(object sender, ElapsedEventArgs e)
        {
            foreach (Action action in callOneSec)
                action();

            ThreadManager.AddSingleTaskMainThread("Zlib_1s_Timer", new ThreadManager.MainThreadTaskFunctionDelegate(mainThreadSync), null);
        }
        private static void mainThreadSync(object _parameter)
        {
            foreach (Action action in callOneSecSync)
                action();
        }
    }
}
