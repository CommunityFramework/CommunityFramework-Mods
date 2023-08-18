using DynamicMusic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static CF_PvP.API;

public class CF_HitLogEntry
{
    public DateTime timestamp;
    public int sourceId;
    public int attackerId;
    public string attackerName;
    public int victimId;
    public string victimName;
    public int damage;
    public ItemValue itemValue;
    public string weaponName;
    public string weaponNameWithMods;
    public Vector3 attackerPos;
    public Vector3 victimPos;
    public Vector3 attackerRot;
    public Vector3 victimRot;
    public Utils.EnumHitDirection direction;
    public EnumBodyPartHit hitbox;
    public int armorDamage;
    public float armorRating;
    public float armorRatingEff;
    public int healthA;
    public int healthMaxA;
    public float staminaA;
    public List<string> buffsA;
    public int healthV;
    public int healthMaxV;
    public float staminaV;
    public List<string> buffsV;
    public bool fatal;
    public float fps;

    public CF_HitLogEntry(ClientInfo _cInfo, ClientInfo _cInfoAttacker, ClientInfo _cInfoVictim, EntityPlayer _playerAttacker, EntityPlayer _playerVictim, int _damage, int _armorDamage, bool _fatal, ItemValue _itemValue, Utils.EnumHitDirection _direction, EnumBodyPartHit _hitbox, float _fps)
    {
        timestamp = DateTime.UtcNow;
        sourceId = _cInfo.entityId;
        attackerId = _cInfoAttacker.entityId;
        attackerName = _cInfoAttacker.playerName;
        victimId = _cInfoVictim.entityId;
        victimName = _cInfoVictim.playerName;
        damage = _damage;
        armorDamage = _armorDamage;
        healthA = _playerAttacker.Health;
        healthMaxA = _playerAttacker.GetMaxHealth();
        staminaA = _playerAttacker.Stamina;
        healthV = _playerVictim.Health;
        healthMaxV = _playerVictim.GetMaxHealth();
        staminaV = _playerVictim.Stamina;
        fatal = _fatal;

        itemValue = _itemValue == null ? null : _itemValue.Clone();

        weaponName = _itemValue == null ? "???" : CF_Format.Item(_itemValue);
        weaponNameWithMods = _itemValue == null ? "???" : CF_Format.Item(_itemValue, true, true);
        direction = _direction;
        hitbox = _hitbox;
        attackerPos = _playerAttacker.position;
        victimPos = _playerVictim.position;
        attackerRot = _playerAttacker.rotation;
        victimRot = _playerVictim.rotation;
        fps = _fps;
        armorRating = _playerVictim.equipment.GetTotalPhysicalArmorRating(null, null);
        armorRatingEff = _playerVictim.equipment.GetTotalPhysicalArmorRating(_playerAttacker, _itemValue);

        buffsA = _playerAttacker.Buffs.ActiveBuffs.Select(b => b.BuffName ).Where(n => !IgnoreBuff(n)).ToList();
        buffsV = _playerVictim.Buffs.ActiveBuffs.Select(b => b.BuffName ).Where(n => !IgnoreBuff(n)).ToList();
    }
    public ClientInfo GetClientAttacker() => CF_Player.GetClient(attackerId);
    public ClientInfo GetClientVictim() => CF_Player.GetClient(victimId);
    public EntityPlayer GetPlayerAttacker() => CF_Player.GetPlayer(attackerId);
    public EntityPlayer GetPlayerVictim() => CF_Player.GetPlayer(victimId);
    public bool IsCriticalHit() => damage >= 100;
    public int TotalDamage() => damage + armorDamage;
    public bool IsHeadshot() =>  hitbox == EnumBodyPartHit.Head;
    public float Distance() => Vector3.Distance(attackerPos, victimPos);
    public float GetArmorPenetration() => armorRating > 0f ? armorRating - armorRatingEff / armorRating : 0f;
    public string FormatArmorRating() => $"{armorRating / 100f:F1}";
    public string FormatEffectiveArmorRating() => $"{armorRatingEff / 100f:F1}";
    public string FormatArmorPenetration() => $"{GetArmorPenetration() / 100f:F1}";
    public override string ToString() => $"{timestamp}: {attackerName} dealt {damage} damage to {victimName} with {weaponName}. {(fatal ? "Fatal hit." : "Non-fatal hit.")}";
    public string ToStringEx()
    {
        return $"Timestamp: {timestamp}\n" +
               $"Attacker ID: {attackerId}, Attacker Name: {attackerName}\n" +
               $"Victim ID: {victimId}, Victim Name: {victimName}\n" +
               $"Damage: {damage}, Armor: {armorDamage}, Health: {healthA}\n" +
               $"Fatal: {fatal}, Weapon: {weaponName}\n" +
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
               $"Wpn: {weaponName} " +
               $"AttHP: {healthA} St: {staminaA:F1} " +
               $"VicHP: {healthA} St: {staminaA:F1} " +
               $"Dist: {Distance()} Dir: {direction} Part: {hitbox} " +
               $"AttPos: {(int)attackerPos.x} {(int)attackerPos.y} {(int)attackerPos.z} ({(int)attackerRot.x} {(int)attackerRot.y} {(int)attackerRot.z}) " +
               $"VicPos: {(int)victimPos.x} {(int)victimPos.y} {(int)victimPos.z} ({(int)victimRot.x} {(int)victimRot.y} {(int)victimRot.z}) " +
               $"FPS: {fps:F1} " +
               $"{(sourceId != attackerId ? "*BAD_SOURCE" : "")} " +
               $"{(Distance() > 200 ? "*BAD_DISTANCE" : "")}";
    }
    public string ToStringLog()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append($"{timestamp.ToString("dd-MMM-yyyy HH:mm:ss")} ");

        if(sourceId != attackerId) 
            sb.Append($"Src: {sourceId} ");
        sb.Append($"{attackerName} ({attackerId}) ");
        sb.Append(fatal ? "kills " : "hits ");
        sb.Append($"{victimName} ({victimId}) ");
        sb.Append($"causing {damage} dmg ");
        sb.Append($"{hitbox.ToStringCached().Replace("Upper", "Up").Replace("Lower", "Lo")} from {direction} {Distance():F1}m away ");
        if(armorRating > 0)
            sb.Append($"Armor: {armorRatingEff:F1} ({armorRating:F1}) Pen: {GetArmorPenetration():P1} ({armorDamage} ad) ");
        sb.Append($"using {weaponNameWithMods} ");
        sb.Append($"# Server # {fps:F1} fps ");
        sb.Append($"# Victim # {healthV:F1} hp {staminaV:F1} stamina at {(int)attackerPos.x} {(int)attackerPos.y} {(int)attackerPos.z} ({(int)attackerRot.x} {(int)attackerRot.y} {(int)attackerRot.z}) Buffs: {CF_Format.ListToString(buffsA)}");
        sb.Append($"# Attacker # {healthA:F1} hp {staminaA:F1} stamina at {(int)victimPos.x} {(int)victimPos.y} {(int)victimPos.z} ({(int)victimRot.x} {(int)victimRot.y} {(int)victimRot.z}) Buffs: {CF_Format.ListToString(buffsV)}");

        if (Distance() > maxDistanceDrop)
            sb.Append($"*BAD_DIST ");
        else if (Distance() > maxDistanceReport)
            sb.Append($"*SUS_DIST ");

        if (sourceId != attackerId)
            sb.Append($"*BAD_SRC ");

        return sb.ToString();
    }
}
