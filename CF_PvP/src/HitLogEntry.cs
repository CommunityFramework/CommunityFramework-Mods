using System;
using UnityEngine;

public class HitLogEntry
{
    public DateTime timestamp;
    public int attackerId;
    public string attackerName;
    public int victim;
    public string victimName;
    public int damage;
    public int armor;
    public int health;
    public float stamina;
    public bool fatal;
    public string weapon;
    public int distance;
    public Utils.EnumHitDirection direction;
    public EnumBodyPartHit hitbox;
    public Vector3 attackerPos;
    public Vector3 victimPos;

    public HitLogEntry(ClientInfo attacker, ClientInfo victim, EntityPlayer playerA, EntityPlayer playerV, int damage, int armor, int health, float stamina, bool fatal, string weapon, int distance, Utils.EnumHitDirection direction, EnumBodyPartHit hitbox)
    {
        this.timestamp = DateTime.UtcNow;
        this.attackerId = attacker.entityId;
        this.attackerName = attacker.playerName;
        this.victim = victim.entityId;
        this.victimName = victim.playerName;
        this.damage = damage;
        this.armor = armor;
        this.health = health;
        this.stamina = stamina;
        this.fatal = fatal;
        this.weapon = weapon;
        this.distance = distance;
        this.direction = direction;
        this.hitbox = hitbox;
        this.attackerPos = playerA.position;
        this.victimPos = playerV.position;
    }
    public override string ToString()
    {
        return $"{timestamp}: {attackerName} dealt {damage} damage to {victimName} with {weapon}. {(fatal ? "Fatal hit." : "Non-fatal hit.")}";
    }
    public string ToStringEx()
    {
        return $"Timestamp: {timestamp}\n" +
               $"Attacker ID: {attackerId}, Attacker Name: {attackerName}\n" +
               $"Victim ID: {victim}, Victim Name: {victimName}\n" +
               $"Damage: {damage}, Armor: {armor}, Health: {health}\n" +
               $"Fatal: {fatal}, Weapon: {weapon}\n" +
               $"Distance: {distance}, Direction: {direction}, Hitbox: {hitbox}\n";
    }    // ToStringEx method with the same info as the original ToString
    public string ToStringAll()
    {
        return $"{timestamp}: " +
               $"Src: {attackerName} " +
               $"Att: {attackerName} " +
               $"Vic: {victimName} " +
               $"Dmg: {damage}{(fatal ? " (Fatal)" : "")} " +
               $"Armor: {armor} Eff: {(armor > 0 ? health : 0)} " +
               $"Wpn: {weapon} " +
               $"AttHP: {health} St: {stamina:F1} " +
               $"VicHP: {health} St: {stamina:F1} " +
               $"Dist: {distance} Dir: {direction} Part: {hitbox} " +
               $"AttPos: {(int)attackerId.position.x} {(int)attackerId.position.y} {(int)attackerId.position.z} " +
               $"VicPos: {(int)victim.position.x} {(int)victim.position.y} {(int)victim.position.z} " +
               $"FPS: {fpsCounter} " +
               $"{(attackerId != attackerEntityId ? "*BAD_SOURCE" : "")} " +
               $"{(distance > 200 ? "*BAD_DISTANCE" : "")}";
    }
}
