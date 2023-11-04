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
    [HarmonyPatch(typeof(NetPackageEntityVelocity), "ProcessPackage")]
    public class Patch_NetPackageEntityVelocity
    {
        private const float max = 8f;
        static bool Prefix(NetPackageEntityVelocity __instance, int ___entityId, Vector3 ___motion, bool ___bAdd)
        {
            try
            {
                if(__instance.Sender.entityId != ___entityId) // ???
                    return true;

                return true;
            }
            catch (Exception e)
            {
                log.Error($"Patch_NetPackageEntityVelocity_ProcessPackage reported error: {e.Message}\n{e.StackTrace}");
            }

            return true;
        }
    }
}
