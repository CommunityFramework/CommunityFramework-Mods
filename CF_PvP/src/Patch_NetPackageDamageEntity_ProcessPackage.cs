using HarmonyLib;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking.Types;
using static CF_PvP.API;

namespace CF_PvP
{
    [HarmonyPatch(typeof(NetPackageDamageEntity), "ProcessPackage")]
    public class Patch_NetPackageDamageEntity_ProcessPackage
    {
        static bool Prefix(NetPackageDamageEntity __instance,
            int ___entityId, int ___attackerEntityId,
            ushort ___strength, float ___damageMultiplier, ItemValue ___attackingItem,
            bool ___bFatal, int ___ArmorDamage, byte ___bonusDamageType,
            int ___hitDirection, int ___hitBodyPart)
        {
            // Check if the attacker is invalid or attacking oneself, and ignore the package in such cases
            if (___attackerEntityId == -1 || ___attackerEntityId == ___entityId)
                return true;

            try
            {
                // Retrieve player and client information for the attacker and the victim
                EntityPlayer playerA = GetPlayerAndClientInfo(___attackerEntityId, out ClientInfo cInfoA);
                if (playerA == null || cInfoA == null)
                    return true;

                EntityPlayer playerV = GetPlayerAndClientInfo(___entityId, out ClientInfo cInfoV);
                if (playerV == null || cInfoV == null)
                    return true;

                // Build a log string with various information about the damage package
                string logString = BuildLogString(__instance.Sender, cInfoA, cInfoV, ___strength, ___damageMultiplier,
                    ___bonusDamageType, ___bFatal, ___ArmorDamage, playerA, playerV, ___attackingItem,
                    (int)___hitDirection, ___hitBodyPart);

                // Log the information for both the attacker and the victim
                log.Out(logString, cInfoA);
                log.Out(logString, cInfoV);

                // Add the damage details to the hit log manager for further processing
                float distance = Vector3.Distance(playerA.position, playerV.position);
                HitLog.AddEntry(__instance.Sender, cInfoA, cInfoV, playerA, playerV, ___strength, ___ArmorDamage, ___bFatal, ___attackingItem, (Utils.EnumHitDirection)___hitDirection, (EnumBodyPartHit)___hitBodyPart, ServerMonitor.FPS);

                // If the attacker is not the package sender or the distance is too great, return false to prevent processing
                if (___attackerEntityId != __instance.Sender.entityId || distance > 200)
                    return false;

                return true;
            }
            catch (Exception e)
            {
                // Log the error if an exception occurs during processing
                log.Error($"Patch_NetPackageDamageEntity_ProcessPackage reported error: {e.Message}\n{e.StackTrace}");
            }

            return true;
        }

        // Helper method to retrieve player and client information
        private static EntityPlayer GetPlayerAndClientInfo(int entityId, out ClientInfo clientInfo)
        {
            clientInfo = CF_Player.GetClient(entityId);
            return clientInfo != null ? CF_Player.GetPlayer(entityId) : null;
        }

        // Method to build the log string
        private static string BuildLogString(ClientInfo src, ClientInfo att, ClientInfo vic,
        ushort str, float dmgMult, byte bonusDmgType, bool isFatal, int armDmg,
        EntityPlayer attPlr, EntityPlayer vicPlr, ItemValue wpn, int hitDir, int hitPart)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Src: {CF_Format.PlayerNameAndPlatform(src)} ");
            sb.Append($"Att: {att.playerName} ({att.PlatformId.ReadablePlatformUserIdentifier}) ");
            sb.Append($"Vic: {vic.playerName} ({vic.PlatformId.ReadablePlatformUserIdentifier}) ");
            sb.Append($"Dmg: {str}{(dmgMult != 1f ? $" (x{dmgMult})" : "")}{((EnumDamageBonusType)bonusDmgType != EnumDamageBonusType.None ? $" +{(EnumDamageBonusType)bonusDmgType}" : "")}{(isFatal ? " (Fatal)" : "")} ");
            sb.Append($"Wpn: {(wpn != null ? wpn.ItemClass.GetLocalizedItemName() : "None")} ");
            float dist = Vector3.Distance(attPlr.position, vicPlr.position);
            sb.Append($"Dist: {dist} ");
            sb.Append($"{(Utils.EnumHitDirection)hitDir} {(EnumBodyPartHit)hitPart} ");
            sb.Append($"FPS: {(int)GameManager.Instance.fps.Counter} ");
            sb.Append($"Armor: {armDmg} Eff: {(armDmg > 0 ? (int)vicPlr.equipment.GetTotalPhysicalArmorRating(vicPlr, wpn) : 0)} ");
            sb.Append($"AttHP: {attPlr.Health} St: {(int)attPlr.Stamina:F1} ");
            sb.Append($"VicHP: {vicPlr.Health} St: {(int)vicPlr.Stamina:F1} ");
            sb.Append($"AttPos: {(int)attPlr.position.x} {(int)attPlr.position.y} {(int)attPlr.position.z} ");
            sb.Append($"VicPos: {(int)vicPlr.position.x} {(int)vicPlr.position.y} {(int)vicPlr.position.z} ");
            if (att.entityId != attPlr.entityId)
                sb.Append("*BAD_SOURCE");
            if (dist > 200)
                sb.Append("*BAD_DISTANCE");

            return sb.ToString();
        }
    }
}
