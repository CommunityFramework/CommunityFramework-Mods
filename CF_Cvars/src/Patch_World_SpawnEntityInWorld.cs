using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using static CF_Cvars.API;
using static WorldGenerationEngineFinal.HighwayPlanner;

namespace CF_Cvars
{
    [HarmonyPatch(typeof(World), "SpawnEntityInWorld")]
    public class Patch_World_SpawnEntityInWorld
    {
        static void Postfix(Entity _entity)
        {
            try
            {
                if (_entity is EntityPlayer player)
                {
                    CF_CvarManager.UpdatePlayer(player);
                }
            }
            catch (Exception e)
            {
                x.Error($"Patch_World_SpawnEntityInWorld reported: {e}");
            }
        }
    }
}
