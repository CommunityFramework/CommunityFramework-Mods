using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static CF_PlayerTracker.API;

namespace CF_PlayerTracker
{
    [HarmonyPatch(typeof(NetPackageEntityTeleport), "ProcessPackage")]
    public class Patch_NetPackageEntityTeleport
    {
        static bool Prefix(NetPackageEntityTeleport __instance, int ___entityId, Vector3 ___pos, Vector3 ___rot, bool ___onGround)
        {
            try
            {
                if (__instance.Sender.entityId != ___entityId) // ???
                    return true;

                return true;
            }
            catch (Exception e)
            {
                log.Error($"Patch_NetPackageEntityTeleport_ProcessPackage reported error: {e.Message}\n{e.StackTrace}");
            }

            return true;
        }
    }
}
