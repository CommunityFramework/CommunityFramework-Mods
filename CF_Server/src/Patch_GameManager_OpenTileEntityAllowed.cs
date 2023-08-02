using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CF_Server.API;

namespace CF_Server
{
    public class GameManager_OpenTileEntityAllowed
    {
        [HarmonyPatch(typeof(GameManager), "OpenTileEntityAllowed")]
        public class OnOpenTileEntityAllowed_Patch
        {
            static AccessTools.FieldRef<TileEntity, Chunk> chunkRef = AccessTools.FieldRefAccess<TileEntity, Chunk>("chunk");
            static bool Prefix(ref bool __result, int _entityIdThatOpenedIt, TileEntity _te)
            {
                ClientInfo cInfo = CF_Player.GetClient(_entityIdThatOpenedIt);
                if (cInfo == null)
                {
                    x.Error($"No ClientInfo for id {_entityIdThatOpenedIt}");
                    __result = false;
                    return false;
                }

                try
                {
                    if (!CF_RestartManager.CanOpenLootContainer(cInfo, _te))
                    {
                        x.Log($"ServerManager denied access for id {_entityIdThatOpenedIt} opening a {_te.GetTileEntityType()} at {CF_TileEntity.GetPosTele(_te)}");
                        __result = false;
                        return false;
                    }

                }
                catch (Exception e)
                {
                    x.Error($"OpenTileEntityAllowed: {e}");
                }

                return true;
            }
            static void Postfix(bool __result, int _entityIdThatOpenedIt, TileEntity _te)
            {
                try
                {
                    ClientInfo cInfo = CF_Player.GetClient(_entityIdThatOpenedIt);
                    if (cInfo == null)
                        return;

                    if (__result)
                        return;

                    // Antidupe
                    if (CF_RestartManager.locked)
                        CF_RestartManager.CloseAllXui(cInfo);
                }
                catch (Exception e)
                {
                    x.Error($"OnOpenTileEntityAllowed_Patch_Post reported: {e}");
                }
            }
        }
    }
}
