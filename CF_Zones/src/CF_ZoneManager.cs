using Newtonsoft.Json;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using static CF_ZonesManager.API;

public class CF_Zones
{
    public static Dictionary<string, CF_ZoneClass> ZoneClasses = new Dictionary<string, CF_ZoneClass>();
    public static Dictionary<string, CF_ZoneAlive> Zones = new Dictionary<string, CF_ZoneAlive>();
    public static Dictionary<int, Vector3> lastPositions = new Dictionary<int, Vector3>();
    public static Dictionary<string, Action<EntityPlayer, CF_ZoneAlive>> EnterCallbacks = new Dictionary<string, Action<EntityPlayer, CF_ZoneAlive>>();
    public static Dictionary<string, Action<EntityPlayer, CF_ZoneAlive>> LeftCallbacks = new Dictionary<string, Action<EntityPlayer, CF_ZoneAlive>>();
    public static Dictionary<string, Action<EntityPlayer, CF_ZoneAlive>> TickCallbacks = new Dictionary<string, Action<EntityPlayer, CF_ZoneAlive>>();
    public static void RegisterZoneClass(string _className, CF_ZoneClass _class)
    {
        ZoneClasses.Add(_className, _class);
    }
    public static void RegisterZoneEnterCallback(string _className, Action<EntityPlayer, CF_ZoneAlive> _callback)
    {
        EnterCallbacks.Add(_className, _callback);
    }
    public static void RegisterZoneLeftCallback(string _className, Action<EntityPlayer, CF_ZoneAlive> _callback)
    {
        LeftCallbacks.Add(_className, _callback);
    }
    public static void RegisterZoneTickCallback(string _className, Action<EntityPlayer, CF_ZoneAlive> _callback)
    {
        TickCallbacks.Add(_className, _callback);
    }
    public static List<CF_ZoneAlive> GetZonesForPlayerPosition(EntityPlayer _player, string _className = "", string _group = "", bool _distinct = true) => GetZonesForPosition(_player.position, _className, _group, _distinct);
    public static List<CF_ZoneAlive> GetZonesForPosition(Vector3 _pos, string _className = "", string _group = "", bool _distinct = true)
    {
        List<CF_ZoneAlive> zones = new List<CF_ZoneAlive>();
        List<string> groups = new List<string>();
        foreach (CF_ZoneAlive zone in Zones.Values)
        {
            if (!string.IsNullOrEmpty(_group) && !zone.group.Equals(_group))
                continue;

            if (!string.IsNullOrEmpty(_className) && !zone.HasClass(_className))
                continue;

            if (!zone.Inside(_pos))
                continue;

            if (_distinct)
            {
                if (groups.Contains(zone.group))
                    continue;

                groups.Add(zone.group);
            }

            zones.Add(zone);
        }

        return zones;
    }
    public static List<CF_ZoneAlive> GetZonesFromPlayer(EntityPlayer _player, string _className = "", string _group = "", bool _distinct = true) => GetZonesFromPlayer(_player.entityId, _className, _group, _distinct);
    public static List<CF_ZoneAlive> GetZonesFromPlayer(int _playerid, string _className = "", string _group = "", bool _distinct = true)
    {
        List<CF_ZoneAlive> zones = new List<CF_ZoneAlive>();
        List<string> groups = new List<string>();
        foreach (CF_ZoneAlive zone in Zones.Values)
        {
            if (!string.IsNullOrEmpty(_group))
            { 
                if(!zone.group.Equals(_group))
                    continue;

                if (_distinct)
                {
                    if(groups.Contains(zone.group))
                        continue;

                    groups.Add(zone.group);
                }
            }

            if (!string.IsNullOrEmpty(_className) && !zone.HasClass(_className))
                continue;

            if (!zone.Players.ContainsKey(_playerid))
                continue;

            zones.Add(zone);
        }

        return zones;
    }
    public static void AddZone(string uniqueName, Vector3 pointA, Vector3 pointB)
    {
        Zones.Add(uniqueName, new CF_ZoneAlive(uniqueName, $"{pointA.x:F0} {pointA.z:F0}", $"{pointB.x:F0} {pointB.z:F0}"));
    }
    public static void AddZone(string uniqueName, string pointA, string pointB)
    {
        Zones.Add(uniqueName, new CF_ZoneAlive(uniqueName, pointA, pointB));
    }
    public static void RemoveZone(string uniqueName, bool _save = true)
    {
        Zones.Remove(uniqueName);
        SaveZonesToFile();
    }
    public static bool GetZone(string _uniqueName, out CF_ZoneAlive _zone)
    {
        return Zones.TryGetValue(_uniqueName, out _zone);
    }
    public static void OnPlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)
    {

    }
    public static void UpdateZonesForPlayer(EntityPlayer _player)
    {
        // We could use this to see if a player logged in inside a zone for example
        // or to teleport him outside if the player entered,
        // but where to teleport when the player joined inside the zone?

        if (!lastPositions.ContainsKey(_player.entityId))
            lastPositions.Add(_player.entityId, _player.position);

        List<CF_ZoneAlive> zonesOld = GetZonesFromPlayer(_player);
        List<CF_ZoneAlive> zonesNew = GetZonesForPlayerPosition(_player);

        // Loop zones player was inside
        foreach (CF_ZoneAlive zone in zonesOld)
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
        foreach (CF_ZoneAlive zone in zonesNew)
        {
            // Still inside zone
            if (zonesOld.Contains(zone))
                continue;

            // Enter zone
            OnPlayerEnter(_player, zone);
        }

        lastPositions[_player.entityId] = _player.position;
    }
    public static void OnPlayerEnter(EntityPlayer _player, CF_ZoneAlive _zone)
    {
        if (GetZonesFromPlayer(_player , "", _zone.group, false).Count() > 0)
            return;

        foreach(var kv in EnterCallbacks)
            kv.Value.Invoke(_player, _zone);

        _zone.OnPlayerEnter(_player, lastPositions[_player.entityId]);
    }
    public static void OnPlayerLeft(EntityPlayer _player, CF_ZoneAlive _zone)
    {
        if (GetZonesFromPlayer(_player, "", _zone.group, false).Count() > 1)
            return;

        foreach (var kv in LeftCallbacks)
            kv.Value.Invoke(_player, _zone);

        _zone.OnPlayerLeave(_player);
    }
    public static void OnPlayerTick(EntityPlayer _player, CF_ZoneAlive _zone)
    {
        foreach (var kv in TickCallbacks)
            kv.Value.Invoke(_player, _zone);

        _zone.OnPlayerUpdate(_player);
    }
    public static void LoadZonesFromFile()
    {
        if (!File.Exists(filePathZones))
        {
            //Zones.add
            SaveZonesToFile();
            return;
        }

        Zones = JsonConvert.DeserializeObject<Dictionary<string, CF_ZoneAlive>>(File.ReadAllText(filePathZones));
    }
    public static void SaveZonesToFile()
    {
        if (!Directory.Exists(mod.modConfigPath))
            Directory.CreateDirectory(mod.modConfigPath);

        Dictionary<string, CF_Zone> baseZones =
    Zones.ToDictionary(
        k => k.Key,
        v => (CF_Zone)v.Value);
        var json = JsonConvert.SerializeObject(baseZones);
        File.WriteAllText(filePathZones, json);
    }
}