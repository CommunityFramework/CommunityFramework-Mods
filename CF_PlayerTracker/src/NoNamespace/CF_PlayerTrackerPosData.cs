using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CF_PlayerTrackerData
{
    public List<CF_PlayerTrackerPosRecord> positionData = new List<CF_PlayerTrackerPosRecord>();
    public List<CF_PlayerTrackerInvRecord> inventoryData = new List<CF_PlayerTrackerInvRecord>();
}

[Serializable]
public class CF_PlayerTrackerPosRecord
{
    public DateTime timestamp;
    public int playerId;
    public Vector3 position;
    public Vector3 rotation;
    public float speedForward;
    public float speedStrafe;
    public float speedVertical;
    public float Stamina;
    public float Health;

    public CF_PlayerTrackerPosRecord(EntityPlayer player)
    {
        this.timestamp = DateTime.UtcNow;
        this.playerId = player.entityId;
        this.position = player.position;
        this.rotation = player.rotation;
        this.speedForward = player.speedForward;
        this.speedStrafe = player.speedStrafe;
        this.speedVertical = player.speedVertical;
        this.Stamina = player.Stamina;
        this.Health = player.Health;
    }
}

[Serializable]
public class CF_PlayerTrackerInvRecord
{
    public Vector3 PlayerLocation { get; set; } // Assuming Vector3 is a struct with X, Y, Z coordinates
    public DateTime Timestamp { get; set; }
    public List<CF_PlayerTrackerInvRecordStack> ItemStacksData { get; set; }

    // Constructor to initialize from an array of ItemStacks
    public CF_PlayerTrackerInvRecord(ItemStack[] itemStacks, Vector3 playerLocation, DateTime timestamp)
    {
        PlayerLocation = playerLocation;
        Timestamp = timestamp;
        ItemStacksData = new List<CF_PlayerTrackerInvRecordStack>();

        foreach (var itemStack in itemStacks)
        {
            ItemStacksData.Add(new CF_PlayerTrackerInvRecordStack(itemStack));
        }
    }
}

[Serializable]
public class CF_PlayerTrackerInvRecordStack
{
    public string ItemClassName { get; set; }
    public int Count { get; set; }

    public CF_PlayerTrackerInvRecordStack(ItemStack itemStack)
    {
        ItemClassName = itemStack.itemValue.ItemClass.Name; 
        Count = itemStack.count;
    }
}