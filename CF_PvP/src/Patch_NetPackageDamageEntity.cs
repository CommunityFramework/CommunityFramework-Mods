using HarmonyLib;
using System;
using System.Text;
using UnityEngine;
using static CF_PvP.API;

namespace CF_PvP
{
    [HarmonyPatch(typeof(NetPackageDamageEntity), "ProcessPackage")]
    public class Patch_NetPackageDamageEntity
    {
        static bool Prefix(NetPackageDamageEntity __instance,
            int ___entityId, int ___attackerEntityId,
            ushort ___strength, ItemValue ___attackingItem,
            bool ___bFatal, int ___ArmorDamage,
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

                CF_HitLog.AddEntry(__instance.Sender, cInfoA, cInfoV, playerA, playerV, ___strength, ___ArmorDamage, ___bFatal, ___attackingItem, (Utils.EnumHitDirection)___hitDirection, (EnumBodyPartHit)___hitBodyPart, CF_ServerMonitor.CurrentFPS);

                if (___attackerEntityId != __instance.Sender.entityId)
                {
                    log.Warn($"Invalid attacker: {___attackerEntityId} sender: {__instance.Sender.entityId} Target: {___entityId}. Blocked.");
                    return false;
                }

                float distance = Vector3.Distance(playerA.position, playerV.position);
                if (distance > maxDistanceDrop)
                {
                    return false;
                }

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
    }
}
