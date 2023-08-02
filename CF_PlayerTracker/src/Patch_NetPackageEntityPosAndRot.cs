using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static CF_PlayerMonitor.API;

namespace CF_PlayerMonitor
{
    [HarmonyPatch(typeof(NetPackageEntityPosAndRot), "ProcessPackage")]
    public class Patch_NetPackageEntityPosAndRot
    {
        static bool Prefix(NetPackageEntityPosAndRot __instance, int ___entityId, Vector3 ___pos, Vector3 ___rot, bool ___onGround)
        {
            try
            {
                if (__instance.Sender.entityId != ___entityId) // ???
                    return true;

                return true;
            }
            catch (Exception e)
            {
                log.Error($"Patch_NetPackageEntityPosAndRot_ProcessPackage reported error: {e.Message}\n{e.StackTrace}");
            }

            return true;
        }
    }
}
