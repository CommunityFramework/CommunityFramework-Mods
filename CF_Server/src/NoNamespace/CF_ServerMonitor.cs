﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemInformation;
using UnityEngine;

public class CF_ServerMonitor
{
    private static readonly GameManager gameManager = GameManager.Instance;
    private static readonly List<(float, DateTime)> fpsDataPoints = new List<(float, DateTime)>();
    private static readonly TimeSpan maxTimeWindow = TimeSpan.FromMinutes(10);

    public static float CurrentFPS => gameManager.fps.Counter;

    public static void OnEverySec() // Main timer (async)
    {
        UpdateFpsList();
        UpdateStats();
    }

    public static float GetAverageFPS(TimeSpan timeWindow)
    {
        if (timeWindow > maxTimeWindow)
        {
            throw new ArgumentOutOfRangeException(nameof(timeWindow));
        }

        var minTimestamp = DateTime.UtcNow.Subtract(timeWindow);
        var relevantDataPoints = fpsDataPoints.FindAll(point => point.Item2 > minTimestamp);
        if (relevantDataPoints.Count == 0)
        {
            return 1000;
        }

        return relevantDataPoints.Average(point => point.Item1);
    }

    public static float GetLowestFPS(TimeSpan timeWindow)
    {
        if (timeWindow > maxTimeWindow)
        {
            throw new ArgumentOutOfRangeException(nameof(timeWindow));
        }

        var minTimestamp = DateTime.UtcNow.Subtract(timeWindow);
        var relevantDataPoints = fpsDataPoints.FindAll(point => point.Item2 > minTimestamp);
        if (relevantDataPoints.Count == 0)
        {
            return 1000;
        }

        return relevantDataPoints.Min(point => point.Item1);
    }
    public static float GetFPSPercentile(int percentile, TimeSpan? timeWindow = null)
    {
        if (percentile < 0 || percentile > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percentile));
        }

        if (!fpsDataPoints.Any())
        {
            return 1000; // Or other default value
        }

        var minTimestamp = DateTime.UtcNow.Subtract(timeWindow ?? maxTimeWindow);
        var relevantDataPoints = fpsDataPoints.FindAll(point => point.Item2 > minTimestamp);

        var orderedFPSValues = relevantDataPoints.Select(p => p.Item1).OrderBy(fps => fps).ToList();
        var index = (int)(percentile * orderedFPSValues.Count / 100.0 + 0.5); // rounding
        index = Math.Min(index, orderedFPSValues.Count - 1); // Make sure we don't go out of bounds
        return orderedFPSValues[index];
    }

    public static float Get95thPercentileFPS(TimeSpan? timeWindow = null) => GetFPSPercentile(95, timeWindow);

    public static float Get99thPercentileFPS(TimeSpan? timeWindow = null) => GetFPSPercentile(99, timeWindow);
    public static int CountLowerFPS(float threshold, TimeSpan? timeWindow = null)
    {
        var minTimestamp = DateTime.UtcNow.Subtract(timeWindow ?? maxTimeWindow);
        var relevantDataPoints = fpsDataPoints.FindAll(point => point.Item2 > minTimestamp);
        return relevantDataPoints.Count(point => point.Item1 < threshold);
    }

    public static int CountHigherFPS(float threshold, TimeSpan? timeWindow = null)
    {
        var minTimestamp = DateTime.UtcNow.Subtract(timeWindow ?? maxTimeWindow);
        var relevantDataPoints = fpsDataPoints.FindAll(point => point.Item2 > minTimestamp);
        return relevantDataPoints.Count(point => point.Item1 > threshold);
    }
    private static void UpdateFpsList()
    {
        float fps = gameManager.fps.Counter;
        fpsDataPoints.Add((fps, DateTime.UtcNow));

        var oldestAllowedTimestamp = DateTime.UtcNow.Subtract(maxTimeWindow);
        fpsDataPoints.RemoveAll(point => point.Item2 < oldestAllowedTimestamp);
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

    public static bool UpdateStats(bool _bDoGc = false)
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