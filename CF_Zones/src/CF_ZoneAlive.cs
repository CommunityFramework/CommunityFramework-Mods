using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CF_ZoneAlive : CF_Zone
{
    public Rect box;
    public Dictionary<int, CF_ZonePlayerInfo> Players { get; } = new Dictionary<int, CF_ZonePlayerInfo>();
    public CF_ZoneAlive(string uniqueName, string pointA, string pointB) : base(uniqueName, pointA, pointB)
    {
        box = MakeBox();
    }
    public List<CF_ZonePlayerInfo> GetPlayerList() => new List<CF_ZonePlayerInfo>(Players.Values.ToList());
    public void OnPlayerEnter(EntityPlayer _player, Vector3 _lastPosition)
    {
        Players[_player.entityId] = new CF_ZonePlayerInfo(_player, _lastPosition);
    }
    public void OnPlayerLeave(EntityPlayer _player)
    {
        Players.Remove(_player.entityId);
    }
    public void OnPlayerUpdate(EntityPlayer _player)
    {
        Players[_player.entityId].UpdateInside(_player);
    }
    public bool Inside(Vector2 _pos)
    {
        if (!box.Contains(_pos))
            return false;

        foreach (string zoneName in ExclusionZones)
        {
            CF_Zones.GetZone(zoneName, out CF_ZoneAlive _zone);
            if (_zone.Inside(_pos))
                return false;
        }

        if (heightFrom != -1 && heightFrom > _pos.x)
            return false;

        if (heightTo != -1 && heightTo < _pos.x)
            return false;

        return true;
    }
    public Rect MakeBox()
    {
        int[] ip1 = Array.ConvertAll(pointA.Split(' '), delegate (string s) { return int.Parse(s); });
        int[] ip2 = Array.ConvertAll(pointB.Split(' '), delegate (string s) { return int.Parse(s); });

        // 1: X1 Z1 2: X2 Z2
        if (ip1.Length == 2 && ip2.Length == 2)
        {
            return new Rect(Math.Min(ip1[0], ip2[0]), Math.Min(ip1[1], ip2[1]), Math.Abs(ip1[0] - ip2[0]), Math.Abs(ip1[1] - ip2[1]));
        }

        // 1: X1 Y1 Z1 2: X2 Y2 Z2
        if (ip1.Length == 3 && ip2.Length == 3)
        {
            return new Rect(Math.Min(ip1[0], ip2[0]), Math.Min(ip1[2], ip2[2]), Math.Abs(ip1[0] - ip2[0]), Math.Abs(ip1[2] - ip2[2]));
        }

        // 1: X Z 2: Radius
        if (ip1.Length == 2 && ip2.Length == 1)
        {
            return new Rect(ip1[0] - Math.Abs(ip2[0]), ip1[1] - Math.Abs(ip2[0]), Math.Abs(ip2[0] * 2), Math.Abs(ip2[0] * 2));
        }

        return new Rect();
    }
}