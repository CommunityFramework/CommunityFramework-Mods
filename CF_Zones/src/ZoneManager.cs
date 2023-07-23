using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using static CF_Zones.API;
public class ZoneManager
{
    public static Dictionary<string, ZoneClass> ZoneClasses = new Dictionary<string, ZoneClass>();
    public static Dictionary<string, ZoneAlive> Zones = new Dictionary<string, ZoneAlive>();
    public static Dictionary<int, Vector3> lastPositions = new Dictionary<int, Vector3>();
    public static Dictionary<string, Action<EntityPlayer, ZoneAlive>> EnterCallbacks = new Dictionary<string, Action<EntityPlayer, ZoneAlive>>();
    public static Dictionary<string, Action<EntityPlayer, ZoneAlive>> LeftCallbacks = new Dictionary<string, Action<EntityPlayer, ZoneAlive>>();
    public static Dictionary<string, Action<EntityPlayer, ZoneAlive>> TickCallbacks = new Dictionary<string, Action<EntityPlayer, ZoneAlive>>();
    public static void RegisterZoneClass(string _className, ZoneClass _class)
    {
        ZoneClasses.Add(_className, _class);
    }
    public static void RegisterZoneEnterCallback(string _className, Action<EntityPlayer, ZoneAlive> _callback)
    {
        EnterCallbacks.Add(_className, _callback);
    }
    public static void RegisterZoneLeftCallback(string _className, Action<EntityPlayer, ZoneAlive> _callback)
    {
        LeftCallbacks.Add(_className, _callback);
    }
    public static void RegisterZoneTickCallback(string _className, Action<EntityPlayer, ZoneAlive> _callback)
    {
        TickCallbacks.Add(_className, _callback);
    }
    public static List<ZoneAlive> GetZonesForPlayer(EntityPlayer _player, string _className = "") => GetZonesForPlayer(_player.entityId, _className);
    public static List<ZoneAlive> GetZonesForPlayer(int _playerId, string _className = "")
    {
        List<ZoneAlive> zones = new List<ZoneAlive>();
        foreach (ZoneAlive zone in Zones.Values)
        {
            if (!string.IsNullOrEmpty(_className) && !zone.HasClass(_className))
                continue;

            if (!zone.Players.ContainsKey(_playerId))
                continue;

            zones.Add(zone);
        }

        return zones;
    }
    public static List<ZoneAlive> GetZonesForPlayerPosition(EntityPlayer _player, string _className = "") => GetZonesForPosition(_player.position, _className);
    public static List<ZoneAlive> GetZonesForPosition(Vector3 _pos, string _className = "")
    {
        List<ZoneAlive> zones = new List<ZoneAlive>();
        foreach (ZoneAlive zone in Zones.Values)
        {
            if (!string.IsNullOrEmpty(_className) && !zone.HasClass(_className))
                continue;

            if (zone.Inside(_pos))
                zones.Add(zone);
        }

        return zones;
    }
    public static void AddZone(string uniqueName, string pointA, string pointB)
    {
        Zones.Add(uniqueName, new ZoneAlive(uniqueName, pointA, pointB));
    }
    public static void RemoveZone(string uniqueName, bool _save = true)
    {
        Zones.Remove(uniqueName);
        SaveZonesToFile();
    }
    public static bool GetZone(string _uniqueName, out ZoneAlive _zone)
    {
        return Zones.TryGetValue(_uniqueName, out _zone);
    }
    public static void UpdateZonesForPlayer(EntityPlayer _player)
    {
        // We could use this to see if a player logged in inside a zone for example
        // or to teleport him outside if the player entered,
        // but where to teleport when the player joined inside the zone?

        if (!lastPositions.ContainsKey(_player.entityId))
            lastPositions.Add(_player.entityId, _player.position);

        List<ZoneAlive> zonesOld = GetZonesForPlayer(_player);
        List<ZoneAlive> zonesNew = GetZonesForPlayerPosition(_player);

        // Loop zones player was inside
        foreach (ZoneAlive zone in zonesOld)
        {
            // Still inside zone
            if (zonesNew.Contains(zone))
            {
                // Tick Zone
                OnPlayerTick(_player, zone);
                continue;
            }

            // Left zone
            OnPlayerLeft(_player, zone);
        }
        // Loop zones player is inside
        foreach (ZoneAlive zone in zonesNew)
        {
            // Still inside zone
            if (zonesOld.Contains(zone))
                continue;

            // Enter zone
            OnPlayerEnter(_player, zone);
        }

        lastPositions[_player.entityId] = _player.position;
    }
    public static void OnPlayerEnter(EntityPlayer _player, ZoneAlive _zone)
    {
        foreach(var kv in EnterCallbacks)
            kv.Value.Invoke(_player, _zone);

        _zone.OnPlayerEnter(_player, lastPositions[_player.entityId]);
    }
    public static void OnPlayerLeft(EntityPlayer _player, ZoneAlive _zone)
    {
        foreach (var kv in LeftCallbacks)
            kv.Value.Invoke(_player, _zone);

        _zone.OnPlayerLeave(_player);
    }
    public static void OnPlayerTick(EntityPlayer _player, ZoneAlive _zone)
    {
        foreach (var kv in TickCallbacks)
            kv.Value.Invoke(_player, _zone);

        _zone.OnPlayerUpdate(_player);
    }
    public static void LoadZonesFromFile()
    {
        if (!File.Exists(filePathZones)) 
            return;

        Zones = JsonConvert.DeserializeObject<Dictionary<string, ZoneAlive>>(File.ReadAllText(filePathZones));
    }
    public static void SaveZonesToFile()
    {
        var json = JsonConvert.SerializeObject(Zones);
        File.WriteAllText(filePathZones, json);
    }
}