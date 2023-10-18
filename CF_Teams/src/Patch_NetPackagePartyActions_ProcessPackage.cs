using HarmonyLib;
using static CF_Teams.API;

namespace CF_Teams
{
    [HarmonyPatch(typeof(NetPackagePartyActions), "ProcessPackage")]
    public class Patch_NetPackagePartyActions_ProcessPackage
    {
        public static CF_Log logParty = new CF_Log("CF_Teams", "PartySystem");
        static bool Prefix(NetPackagePartyActions __instance, int ___invitedByEntityID, int ___invitedEntityID, NetPackagePartyActions.PartyActions ___currentOperation)
        {
            ClientInfo cInfoS = CF_Player.GetClientInfo(___invitedByEntityID);
            ClientInfo cInfoR = CF_Player.GetClientInfo(___invitedEntityID);

            logParty.Out($"{___currentOperation}: {cInfoS.playerName} => {cInfoR.playerName}");

            return true;
        }
    }
}
