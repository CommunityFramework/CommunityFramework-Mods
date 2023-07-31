using System;
using UnityEngine;

public class ZonePlayerInfo
{
    public int entityId;
    public DateTime entered;
    public Vector3 outPos;
    public Vector3 enterPos;
    public Vector3 lastPos;

    public ZonePlayerInfo(EntityPlayer _player, Vector3 _outPos)
    {
        entityId = _player.entityId;
        entered = DateTime.UtcNow;
        outPos = _outPos;
        enterPos = _player.position;
        lastPos = _player.position;
    }

    public void UpdateInside(EntityPlayer _player)
    {
        lastPos = _player.position;
    }
}
