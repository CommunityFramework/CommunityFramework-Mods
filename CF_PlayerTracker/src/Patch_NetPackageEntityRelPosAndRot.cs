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
    [HarmonyPatch(typeof(NetPackageEntityRelPosAndRot), "ProcessPackage")]
    public class Patch_NetPackageEntityRelPosAndRot
    {
        static bool Prefix(NetPackageEntityRelPosAndRot __instance, Vector3i ___dPos, bool ___onGround, short ___updateSteps, int ___entityId)
        {
            try
            {
                if (__instance.Sender.entityId != ___entityId) // ???
                    return true;

                return true;
            }
            catch (Exception e)
            {
                log.Error($"Patch_NetPackageEntityRelPosAndRot_ProcessPackage reported error: {e.Message}\n{e.StackTrace}");
            }

            return true;
        }
    }
}
