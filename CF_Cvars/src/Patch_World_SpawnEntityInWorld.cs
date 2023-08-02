using HarmonyLib;
using System;
using System.Collections.Generic;
using static CF_Cvars.API;

namespace CF_Cvars
{
    [HarmonyPatch(typeof(World), "SpawnEntityInWorld")]
    public class Patch_World_SpawnEntityInWorld
    {
        static void Postfix(Entity _entity)
        {
            try
            {
                if (_entity is EntityPlayer)
                {
                    CF_CvarManager.UpdatePlayer(_entity as EntityPlayer);
                }
            }
            catch (Exception e)
            {
                x.Error($"Patch_World_SpawnEntityInWorld reported: {e}");
            }
        }
    }
}
