using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CF_PlayerTrackerEntry
{
    public DateTime timestamp;
    public int playerId;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;

    public CF_PlayerTrackerEntry(int playerId, Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        this.timestamp = DateTime.UtcNow;
        this.playerId = playerId;
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
    }
}
