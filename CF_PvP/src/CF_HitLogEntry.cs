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
    public EnumDamageSource damageSrc;
    public EnumDamageTypes damageTyp;
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
    public int pingA;
    public int pingV;

    public CF_HitLogEntry(ClientInfo _cInfo, ClientInfo _cInfoAttacker, ClientInfo _cInfoVictim, EntityPlayer _playerAttacker, EntityPlayer _playerVictim, int _damage, EnumDamageSource _damageSrc, EnumDamageTypes _damageTyp, int _armorDamage, bool _fatal, ItemValue _itemValue, Utils.EnumHitDirection _direction, EnumBodyPartHit _hitbox, float _fps)
    {
        timestamp = DateTime.UtcNow;
        sourceId = _cInfo.entityId;
        attackerId = _cInfoAttacker.entityId;
        attackerName = _cInfoAttacker.playerName;
        victimId = _cInfoVictim.entityId;
        victimName = _cInfoVictim.playerName;
        damage = _damage;
        damageSrc = _damageSrc;
        damageTyp = _damageTyp;
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
        pingV = _cInfo.ping;
        pingA = _cInfoAttacker.ping;
        armorRating = _playerVictim.equipment.GetTotalPhysicalArmorRating(null, null);
        armorRatingEff = _playerVictim.equipment.GetTotalPhysicalArmorRating(_playerAttacker, _itemValue);

        buffsA = _playerAttacker.Buffs.ActiveBuffs.Select(b => b.BuffName ).Where(n => !IgnoreBuff(n)).ToList();
        buffsV = _playerVictim.Buffs.ActiveBuffs.Select(b => b.BuffName ).Where(n => !IgnoreBuff(n)).ToList();
    }
    public ClientInfo GetClientAttacker() => CF_Player.GetClientInfo(attackerId);
    public ClientInfo GetClientVictim() => CF_Player.GetClientInfo(victimId);
    public EntityPlayer GetPlayerAttacker() => CF_Player.GetEntityPlayer(attackerId);
    public EntityPlayer GetPlayerVictim() => CF_Player.GetEntityPlayer(victimId);
    public bool IsCriticalHit() => damage >= 100;
    public int TotalDamage() => damage + armorDamage;
    public bool IsHeadshot() =>  hitbox == EnumBodyPartHit.Head;
    public float Distance() => Vector3.Distance(attackerPos, victimPos);
    public float GetArmorPenetration() => armorRating > 0f ? (armorRating - armorRatingEff) / armorRating : 0f;
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
        sb.Append($"Type: {damageSrc.ToStringCached()} - {damageTyp.ToStringCached()}");
        if (armorRating > 0)
            sb.Append($"Armor: {armorRatingEff:F1} ({armorRating:F1}) Pen: {GetArmorPenetration():P1} ({armorDamage} ad) ");
        sb.Append($"using {weaponNameWithMods} ");
        sb.Append($"# Server # {fps:F1} fps ");
        sb.Append($"# Victim # {healthV:F1} hp {staminaV:F1} stamina at {(int)attackerPos.x} {(int)attackerPos.y} {(int)attackerPos.z} ({(int)attackerRot.x} {(int)attackerRot.y} {(int)attackerRot.z}) Ping: {pingA} Buffs: {CF_Format.ListToString(buffsA)}");
        sb.Append($"# Attacker # {healthA:F1} hp {staminaA:F1} stamina at {(int)victimPos.x} {(int)victimPos.y} {(int)victimPos.z} ({(int)victimRot.x} {(int)victimRot.y} {(int)victimRot.z}) Ping: {pingV} Buffs: {CF_Format.ListToString(buffsV)}");

        if (Distance() > maxDistanceDrop)
            sb.Append($"*BAD_DIST ");
        else if (Distance() > maxDistanceReport)
            sb.Append($"*SUS_DIST ");

        if (sourceId != attackerId)
            sb.Append($"*BAD_SRC ");

        return sb.ToString();
    }
    public string GenerateKillMessage()
    {
        if (!fatal)
        {
            return ""; // If it's not a kill, return an empty string (no kill message).
        }
        string[] headshotMessages = {
            $"{attackerName} drew fast and scored a clean headshot on {victimName} with {weaponName}!",
            $"{attackerName} delivered a deadly headshot to {victimName} using {weaponName} like a true gunslinger!",
            $"{victimName} fell down like a sack of potatoes after {attackerName}'s accurate headshot with {weaponName}!",
            $"Headshot! {attackerName} made a name for themselves by taking down {victimName} with {weaponName}!",
            $"{attackerName} took down {victimName} in a blink of an eye with a precise headshot from {weaponName}!",
            $"{victimName} met their fate with a fatal headshot from {attackerName}'s {weaponName}!",
            $"{attackerName} showed no mercy, landing a lethal headshot on {victimName} using {weaponName}!",
            $"A clean headshot by {attackerName} sent {victimName} to the afterlife with {weaponName}!",
            $"{attackerName} proved to be an expert marksman with a headshot on {victimName} from {weaponName}!",
            $"{victimName} won't forget the pain of {attackerName}'s perfect headshot using {weaponName}!",
            $"{attackerName} displayed unrivaled precision, hitting {victimName} in the head with {weaponName}!",
            $"{victimName} went down with a shattered skull after {attackerName}'s headshot with {weaponName}!",
            $"{attackerName} took a moment to aim and landed a deadly headshot on {victimName} using {weaponName}!",
            $"Headshot! {attackerName} skillfully took down {victimName} with a shot from {weaponName}!",
            $"{attackerName} proved themselves as the fastest draw in the west with a headshot on {victimName} using {weaponName}!"
        };
        string[] criticalHitMessages = {
            $"{attackerName} hit {victimName} right in the heart with a powerful shot from {weaponName}!",
            $"{victimName} ain't gonna forget the sting of {attackerName}'s critical hit using {weaponName}!",
            $"{attackerName}'s shot found its mark, leaving {victimName} in a heap with {weaponName}!",
            $"Critical Hit! {attackerName} put an end to {victimName} using {weaponName}!",
            $"{attackerName} struck {victimName} with a deadly critical hit from {weaponName}!",
            $"{victimName} felt the wrath of {attackerName}'s critical hit using {weaponName}!",
            $"{attackerName} delivered a crushing blow with a critical hit on {victimName} using {weaponName}!",
            $"Critical Hit! {attackerName} left {victimName} reeling with a shot from {weaponName}!",
            $"{attackerName} dealt a devastating critical hit to {victimName} using {weaponName}!",
            $"{victimName} won't forget the pain of {attackerName}'s critical hit from {weaponName}!",
            $"{attackerName} displayed deadly accuracy, landing a critical hit on {victimName} with {weaponName}!",
            $"{victimName} couldn't withstand the impact of {attackerName}'s critical hit using {weaponName}!",
            $"{attackerName} made a critical shot, leaving {victimName} in agony with {weaponName}!",
            $"Critical Hit! {attackerName} skillfully took down {victimName} with a powerful shot from {weaponName}!",
            $"{attackerName} unleashed a brutal critical hit, ending {victimName}'s life with {weaponName}!"
        };
        string[] eliminationMessages = {
            $"{attackerName} eliminated {victimName} with {weaponName} like a true gunslinger!",
            $"{attackerName} put an end to {victimName} using {weaponName}!",
            $"{victimName} met their demise at the hands of {attackerName} and {weaponName}!",
            $"{attackerName} claimed victory by taking down {victimName} with {weaponName}!",
            $"{attackerName} delivered a fatal shot, sending {victimName} to meet their maker with {weaponName}!",
            $"{victimName} was no match for {attackerName}'s quick draw and fell to {weaponName}!",
            $"{attackerName} emerged victorious in the showdown, defeating {victimName} with {weaponName}!",
            $"{victimName} learned the hard way not to cross paths with {attackerName} and {weaponName}!",
            $"{attackerName} proved their mettle by eliminating {victimName} with {weaponName}!",
            $"{victimName} met their end with a bullet from {attackerName}'s {weaponName}!",
            $"{attackerName} stood tall as they defeated {victimName} using {weaponName}!",
            $"{victimName} underestimated {attackerName}'s skill and paid the price with {weaponName}!",
            $"{attackerName} put {victimName} in the ground with a precise shot from {weaponName}!",
            $"{victimName} won't be causing any trouble anymore after {attackerName}'s shot with {weaponName}!",
            $"{attackerName} proved to be the deadliest shooter, taking down {victimName} with {weaponName}!"
        };
        string[] longDistanceMessages = {
            $"{attackerName} shot down {victimName} from a distance that'd make a hawk jealous using {weaponName}!",
            $"{victimName} never saw it comin' when {attackerName} picked 'em off from a mile away with {weaponName}!",
            $"Unbelievable! {attackerName} pulled off a kill on {victimName} from a range that'd make legends proud with {weaponName}!",
            $"{attackerName} displayed incredible sharpshooting and scored a kill on {victimName} from a jaw-dropping distance with {weaponName}!",
            $"{attackerName} made an astonishing kill on {victimName} from an incredible distance using {weaponName}!",
            $"{victimName} was taken down by a precise shot from {attackerName} at an immense distance with {weaponName}!",
            $"{attackerName} proved their sniper skills by eliminating {victimName} from an extreme distance using {weaponName}!",
            $"{attackerName} delivered a death shot to {victimName} with {weaponName} from a staggering distance!",
            $"{attackerName} shot {victimName} with deadly accuracy from a distance that seemed impossible with {weaponName}!",
            $"{victimName} never stood a chance against {attackerName}'s deadly shot from {weaponName} at an incredible distance!",
            $"{attackerName} displayed unparalleled marksmanship by taking down {victimName} from an astonishing distance with {weaponName}!",
            $"{victimName} couldn't believe their eyes when they fell to {attackerName}'s shot from {weaponName} at such a range!",
            $"{attackerName} proved to be the long-range ace, making a lethal shot on {victimName} using {weaponName}!",
            $"{victimName} was surprised by a deadly shot from {attackerName} at a distance that seemed impossible with {weaponName}!",
            $"{attackerName} demonstrated expert precision, eliminating {victimName} from an astounding distance with {weaponName}!"
        };
        string[] superMessages = {
            $"{attackerName} proved to be a master marksman, landing an astonishing headshot on {victimName} from an incredible distance with {weaponName}!",
            $"{attackerName} displayed unparalleled precision, eliminating {victimName} with a jaw-dropping long-range headshot from {weaponName}!",
            $"{victimName} never saw it coming when {attackerName} picked them off with a precise headshot from a mile away using {weaponName}!",
            $"Unbelievable! {attackerName} pulled off an incredible feat, scoring a headshot on {victimName} from a range that'd make legends proud with {weaponName}!",
            $"{attackerName} took down {victimName} with a masterful shot, hitting the bullseye on a long-range headshot using {weaponName}!",
            $"{victimName} fell to the incredible accuracy of {attackerName}, who scored a headshot from an astonishing distance with {weaponName}!",
            $"{attackerName} showcased their deadly sniping skills, delivering a fatal headshot to {victimName} from an immense distance with {weaponName}!",
            $"{attackerName} proved themselves as a long-range ace, making a lethal headshot on {victimName} using {weaponName}!",
            $"{victimName} was left in disbelief after being struck by {attackerName}'s perfectly aimed headshot from an incredible distance with {weaponName}!",
            $"{attackerName} unleashed an astounding shot, hitting a headshot on {victimName} from a staggering distance using {weaponName}!",
            $"{attackerName} demonstrated expert precision, taking down {victimName} with a headshot from an astonishing distance using {weaponName}!",
            $"{victimName} couldn't believe their eyes when they fell to {attackerName}'s headshot from a seemingly impossible range with {weaponName}!",
            $"{attackerName} proved to be the ultimate sharpshooter, scoring a headshot on {victimName} at an extreme distance with {weaponName}!",
            $"{victimName} was taken out by {attackerName} with a jaw-dropping headshot from an incredible range using {weaponName}!",
            $"{attackerName} displayed unmatched marksmanship, delivering a deadly headshot to {victimName} from a distance that seemed impossible with {weaponName}!"
        };

        string killMessage = "";
        if (Distance() > 200)
        {
            if (IsHeadshot())
            {
                killMessage = superMessages[CF_Random.Rnd(headshotMessages.Length)];
            }
            else
            {
                killMessage += longDistanceMessages[CF_Random.Rnd(longDistanceMessages.Length)];
            }
        }
        else if (IsHeadshot())
        {
            killMessage = headshotMessages[CF_Random.Rnd(headshotMessages.Length)];
        }
        else if (IsCriticalHit())
        {
            killMessage = criticalHitMessages[CF_Random.Rnd(criticalHitMessages.Length)];
        }
        else
        {
            killMessage = eliminationMessages[CF_Random.Rnd(eliminationMessages.Length)];
        }

        // Apply colors to all possible placeholders using the AddColorToPlaceholder method
        killMessage = AddStringBeforeSubstring(killMessage, "{attackerName}", ColorAttacker);
        killMessage = AddStringBeforeSubstring(killMessage, "{victimName}", ColorVictim);
        killMessage = AddStringBeforeSubstring(killMessage, "{damage}", ColorDamage);
        killMessage = AddStringBeforeSubstring(killMessage, "{weapon}", ColorWeapon);
        killMessage = AddStringBeforeSubstring(killMessage, "{headshot}", ColorHeadshot);
        killMessage = AddStringBeforeSubstring(killMessage, "{criticalHit}", ColorCriticalHit);
        killMessage = AddStringBeforeSubstring(killMessage, "{general}", ColorGeneral);

        return killMessage;
    }

    // Define color variables
    private string ColorAttacker = "[4F77FF]"; // Sky bluean
    private string ColorVictim = "[B4B4B4]";   // Cloud gray
    private string ColorDamage = "[61FF00]";  // Grass green
    private string ColorWeapon = "[FFFF00]";  // Yellow
    private string ColorHeadshot = "[FF8C00]"; // Dark orange
    private string ColorCriticalHit = "[FF0000]"; // Red
    private string ColorGeneral = "[B4B4B4]"; // Cloud gray
    private string ColorReset = "[-]"; // Reset to default color (white)

    public static string AddStringBeforeSubstring(string source, string substringToFind, string stringToAdd)
    {
        StringBuilder result = new StringBuilder();
        int previousIndex = 0;
        int index = source.IndexOf(substringToFind);

        while (index != -1)
        {
            result.Append(source.Substring(previousIndex, index - previousIndex));
            result.Append(stringToAdd);

            previousIndex = index;
            index = source.IndexOf(substringToFind, index + substringToFind.Length);
        }

        result.Append(source.Substring(previousIndex));

        return result.ToString();
    }
}
