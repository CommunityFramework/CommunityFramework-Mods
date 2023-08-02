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
    [HarmonyPatch(typeof(NetPackageEntityRotation), "ProcessPackage")]
    public class Patch_NetPackageEntityRotation
    {
        protected int entityId;
        protected Vector3i rot;
        protected Quaternion qrot;
        protected bool bUseQRotation;
        static bool Prefix(NetPackageEntityRotation __instance, int ___entityId, Vector3i ___rot, Quaternion ___qrot, bool ___bUseQRotation)
        {
            try
            {
                if (__instance.Sender.entityId != ___entityId) // ???
                    return true;

                return true;
            }
            catch (Exception e)
            {
                log.Error($"Patch_NetPackageEntityRotation_ProcessPackage reported error: {e.Message}\n{e.StackTrace}");
            }

            return true;
        }
    }
}
