using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static CF_Server.API;

namespace CF_Server
{
    [HarmonyPatch(typeof(GameManager), "OpenTileEntityAllowed")]
    public class Patch_GameManager_OnOpenTileEntityAllowed
    {
        static bool Prefix(ref bool __result, int _entityIdThatOpenedIt, TileEntity _te)
        {
            ClientInfo cInfo = CF_Player.GetClientInfo(_entityIdThatOpenedIt);
            if (cInfo == null)
            {
                log.Error($"Patch_GameManager_OnOpenTileEntityAllowed_Pre reported: No ClientInfo for id {_entityIdThatOpenedIt}");
                __result = false;
                return false;
            }

            try
            {
                if (!CanOpenLootContainer(cInfo, _te))
                {
                    //log.Log($"ServerManager denied access for id {_entityIdThatOpenedIt} opening a {_te.GetTileEntityType()} at {CF_TileEntity.GetPosTele(_te)}", cInfo);
                    __result = false;
                    return false;
                }

            }
            catch (Exception e)
            {
                log.Error($"Patch_GameManager_OnOpenTileEntityAllowed_Pre reported: : {e}");
            }

            return true;
        }
        static void Postfix(bool __result, int _entityIdThatOpenedIt, TileEntity _te)
        {
            try
            {
                ClientInfo cInfo = CF_Player.GetClientInfo(_entityIdThatOpenedIt);
                if (cInfo == null)
                    return;

                if (__result)
                    return;

                // Antidupe
                if (CF_RestartManager.locked)
                    CF_Player.CloseAllXui(cInfo);
            }
            catch (Exception e)
            {
                log.Error($"Patch_GameManager_OnOpenTileEntityAllowed_Post reported: {e}");
            }
        }
        public static bool CanOpenLootContainer(ClientInfo _cInfo, TileEntity _te)
        {
            if (!CF_RestartManager.locked)
                return true;

            if (_te is TileEntityLootContainer
                || _te is TileEntityWorkstation
                || _te is TileEntitySecureLootContainer
                || _te is TileEntityVendingMachine
                || _te is TileEntityLootContainer)
            {
                CF_Player.Message(msgDenied, _cInfo);
                return false;
            }

            return true;
        }
    }
}
