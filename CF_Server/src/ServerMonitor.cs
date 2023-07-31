using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemInformation;
using UnityEngine;

public class ServerMonitor
{
    public static void OnEverySec() // Main timer (async)
    {
        FillFpsLists();
        FillStats();
    }
    public static List<float> FPSlist_10s = new List<float>();
    public static List<float> FPSlist_30s = new List<float>();
    public static List<float> FPSlist_1m = new List<float>();
    public static List<float> FPSlist_3m = new List<float>();
    public static List<float> FPSlist_5m = new List<float>();
    public static List<float> FPSlist_10m = new List<float>();
    public static float FPS => FPSlist_10s.Count > 0 ? FPSlist_10s[0] : 1000;
    public static void FillFpsLists()
    {
        float fps = GameManager.Instance.fps.Counter;

        FPSlist_10s.Add(fps);
        if (FPSlist_10s.Count > 10) FPSlist_10s.RemoveAt(0);

        FPSlist_30s.Add(fps);
        if (FPSlist_30s.Count > 30) FPSlist_30s.RemoveAt(0);

        FPSlist_1m.Add(fps);
        if (FPSlist_1m.Count > 60) FPSlist_1m.RemoveAt(0);

        FPSlist_3m.Add(fps);
        if (FPSlist_3m.Count > 180) FPSlist_3m.RemoveAt(0);

        FPSlist_5m.Add(fps);
        if (FPSlist_5m.Count > 300) FPSlist_5m.RemoveAt(0);

        FPSlist_10m.Add(fps);
        if (FPSlist_10m.Count > 600) FPSlist_10m.RemoveAt(0);
    }
    public enum EnumSS
    {
        Uptime = 0,
        FPS = 1,
        Heap = 2,
        HeapMax = 3,
        RSS = 4,
        Chunks = 5,
        ChunkObservers = 6,
        CGO = 7,
        Players = 8,
        Zombies = 9,
        Entities = 10,
        Entities2 = 11,
        Items = 12,
        MAX = 13
    }
    public static string[] Stats = new string[(int)EnumSS.MAX];

    public static bool FillStats(bool _bDoGc = false)
    {
        World world = GameManager.Instance.World;
        if (world == null)
            return false;

        Stats[(int)EnumSS.Uptime] = (Time.timeSinceLevelLoad / 60f).ToCultureInvariantString("F2");
        Stats[(int)EnumSS.FPS] = GameManager.Instance.fps.Counter.ToCultureInvariantString("F2");
        Stats[(int)EnumSS.Heap] = ((float)GC.GetTotalMemory(_bDoGc) / 1048576f).ToCultureInvariantString("0.0");
        Stats[(int)EnumSS.HeapMax] = ((float)GameManager.MaxMemoryConsumption / 1048576f).ToCultureInvariantString("0.0");
        Stats[(int)EnumSS.ChunkObservers] = (world != null ? world.m_ChunkManager.m_ObservedEntities.Count : 0).ToString();
        Stats[(int)EnumSS.Chunks] = Chunk.InstanceCount.ToString();
        Stats[(int)EnumSS.CGO] = world.m_ChunkManager.GetDisplayedChunkGameObjectsCount().ToString();
        Stats[(int)EnumSS.Players] = world.Players.list.Count.ToString();
        Stats[(int)EnumSS.Zombies] = GameStats.GetInt(EnumGameStats.EnemyCount).ToString();
        Stats[(int)EnumSS.Entities] = world.Entities.Count.ToString();
        Stats[(int)EnumSS.Entities2] = Entity.InstanceCount.ToString();
        Stats[(int)EnumSS.Items] = EntityItem.ItemInstanceCount.ToString();
        Stats[(int)EnumSS.RSS] = ((float)((double)GetRSS.GetCurrentRSS() / 1024.0 / 1024.0)).ToCultureInvariantString("0.0");

        return true;
    }
}