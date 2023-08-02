using System;
using UnityEngine;

public class CF_HitLogEntry
{
    public DateTime timestamp;
    public int sourceId;
    public int attackerId;
    public string attackerName;
    public int victim;
    public string victimName;
    public int damage;
    public int armorDamage;
    public int healthA;
    public float staminaA;
    public int healthV;
    public float staminaV;
    public bool fatal;
    public string weapon;
    public string weaponEx;
    public ItemValue itemValue;
    public Utils.EnumHitDirection direction;
    public EnumBodyPartHit hitbox;
    public Vector3 attackerPos;
    public Vector3 victimPos;
    public Vector3 attackerRot;
    public Vector3 victimRot;
    public float fps;

    public CF_HitLogEntry(ClientInfo source, ClientInfo attacker, ClientInfo victim, EntityPlayer playerA, EntityPlayer playerV, int damage, int armorDamage, bool fatal, ItemValue weapon, Utils.EnumHitDirection direction, EnumBodyPartHit hitbox, float fps)
    {
        this.timestamp = DateTime.UtcNow;
        this.sourceId = source.entityId;
        this.attackerId = attacker.entityId;
        this.attackerName = attacker.playerName;
        this.victim = victim.entityId;
        this.victimName = victim.playerName;
        this.damage = damage;
        this.armorDamage = armorDamage;
        this.healthA = playerA.Health;
        this.staminaA = playerA.Stamina;
        this.healthV = playerV.Health;
        this.staminaV = playerV.Stamina;
        this.fatal = fatal;
        this.itemValue = itemValue.Clone();
        this.weapon = CF_Format.Item(weapon);
        this.weaponEx = CF_Format.Item(weapon, true, true, true);
        this.direction = direction;
        this.hitbox = hitbox;
        this.attackerPos = playerA.position;
        this.victimPos = playerV.position;
        this.attackerRot = playerA.rotation;
        this.victimRot = playerV.rotation;
        this.fps = fps;
    }
    public float Distance() => Vector3.Distance(attackerPos, victimPos);
    public override string ToString()
    {
        return $"{timestamp}: {attackerName} dealt {damage} damage to {victimName} with {weapon}. {(fatal ? "Fatal hit." : "Non-fatal hit.")}";
    }
    public string ToStringEx()
    {
        return $"Timestamp: {timestamp}\n" +
               $"Attacker ID: {attackerId}, Attacker Name: {attackerName}\n" +
               $"Victim ID: {victim}, Victim Name: {victimName}\n" +
               $"Damage: {damage}, Armor: {armorDamage}, Health: {healthA}\n" +
               $"Fatal: {fatal}, Weapon: {weapon}\n" +
               $"Distance: {Distance()}, Direction: {direction}, Hitbox: {hitbox}\n";
    }    // ToStringEx method with the same info as the original ToString
    public string ToStringAll()
    {
        return $"{timestamp}: " +
               $"Src: {attackerName} " +
               $"Att: {attackerName} " +
               $"Vic: {victimName} " +
               $"Dmg: {damage}{(fatal ? " (Fatal)" : "")} " +
               $"Armor: {armorDamage} Eff: {(armorDamage > 0 ? healthA : 0)} " +
               $"Wpn: {weapon} " +
               $"AttHP: {healthA} St: {staminaA:F1} " +
               $"VicHP: {healthA} St: {staminaA:F1} " +
               $"Dist: {Distance()} Dir: {direction} Part: {hitbox} " +
               $"AttPos: {(int)attackerPos.x} {(int)attackerPos.y} {(int)attackerPos.z} ({(int)attackerRot.x} {(int)attackerRot.y} {(int)attackerRot.z}) " +
               $"VicPos: {(int)victimPos.x} {(int)victimPos.y} {(int)victimPos.z} ({(int)victimRot.x} {(int)victimRot.y} {(int)victimRot.z}) " +
               $"FPS: {fps:F1} " +
               $"{(sourceId != attackerId ? "*BAD_SOURCE" : "")} " +
               $"{(Distance() > 200 ? "*BAD_DISTANCE" : "")}";
    }
}
