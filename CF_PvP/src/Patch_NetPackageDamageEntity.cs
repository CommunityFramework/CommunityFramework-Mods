using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CF_PvP.API;

namespace CF_PvP
{
    [HarmonyPatch(typeof(NetPackageDamageEntity), "ProcessPackage")]
    public class Patch_NetPackageDamageEntity
    {
        private static Dictionary<int, DateTime> fatalHitTime = new Dictionary<int, DateTime>();
        private static TimeSpan fatalHitThreshold = TimeSpan.FromSeconds(10);
        static bool Prefix(NetPackageDamageEntity __instance,
            int ___entityId, int ___attackerEntityId,
            ushort ___strength, ItemValue ___attackingItem,
            bool ___bFatal, int ___ArmorDamage,
            int ___hitDirection, int ___hitBodyPart,
            EnumDamageSource ___damageSrc, EnumDamageTypes ___damageTyp)
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

                bool allow = true;

                if (!playerV.Spawned)
                {
                    log.Warn($"Victim not spawned. Attacker: {___attackerEntityId} sender: {__instance.Sender.entityId} Target: {___entityId}. Blocked.");
                    allow = false;
                }

                if (!playerA.IsAlive())
                {
                    log.Warn($"Attacker not alive. Attacker: {___attackerEntityId} sender: {__instance.Sender.entityId} Target: {___entityId}. Blocked.");
                    allow = false;
                }

                // If the player died decently then he may be shown where he died as tpose and can get damage while respawned somewher else already
                if (fatalHitTime.TryGetValue(___entityId, out DateTime lastHit) && (DateTime.UtcNow - lastHit) < fatalHitThreshold)
                {
                    log.Warn($"Victim already dead. Attacker: {___attackerEntityId} sender: {__instance.Sender.entityId} Target: {___entityId}. Blocked.");
                    allow = false;
                }

                if (___bFatal)
                    fatalHitTime[___entityId] = DateTime.UtcNow;

                if (___attackerEntityId != __instance.Sender.entityId)
                {
                    log.Warn($"Invalid attacker. Attacker: {___attackerEntityId} sender: {__instance.Sender.entityId} Target: {___entityId}. Blocked.");
                    allow = false;
                }

                float distance = Vector3.Distance(playerA.position, playerV.position);
                if (distance > maxDistanceDrop)
                {
                    log.Warn($"Bad distance ({distance:F1}m). Attacker: {___attackerEntityId} ({CF_Map.FormatPosition(playerA.position)}) sender: {__instance.Sender.entityId} Target: {___entityId} ({CF_Map.FormatPosition(playerV.position)}). Blocked.");
                    allow = false;
                }

                CF_HitLog.AddEntry(__instance.Sender, cInfoA, cInfoV, playerA, playerV, ___strength, ___damageSrc, ___damageTyp, ___ArmorDamage, ___bFatal, ___attackingItem, (Utils.EnumHitDirection)___hitDirection, (EnumBodyPartHit)___hitBodyPart, GameManager.Instance.fps.Counter, allow);

                return allow;
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
            clientInfo = CF_Player.GetClientInfo(entityId);
            return clientInfo != null ? CF_Player.GetEntityPlayer(entityId) : null;
        }
    }
}
