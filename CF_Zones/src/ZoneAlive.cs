using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ZoneAlive : Zone
{
    public Dictionary<int, ZonePlayerInfo> Players { get; } = new Dictionary<int, ZonePlayerInfo>();
    public ZoneAlive(string uniqueName, string pointA, string pointB) : base(uniqueName, pointA, pointB) { }
    public List<ZonePlayerInfo> GetPlayers() => new List<ZonePlayerInfo>(Players.Values.ToList());
    public void OnPlayerEnter(EntityPlayer _player, Vector3 _lastPosition)
    {
        Players[_player.entityId] = new ZonePlayerInfo(_player, _lastPosition);
    }
    public void OnPlayerLeave(EntityPlayer _player)
    {
        Players.Remove(_player.entityId);
    }
    public void OnPlayerUpdate(EntityPlayer _player)
    {
        Players[_player.entityId].UpdateInside(_player);
    }
}