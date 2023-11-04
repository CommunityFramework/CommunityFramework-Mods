using HarmonyLib;
using static CF_Teams.API;

namespace CF_Teams
{
    [HarmonyPatch(typeof(NetPackagePartyActions), "ProcessPackage")]
    public class Patch_NetPackagePartyActions_ProcessPackage
    {
        public static CF_Log logParty = new CF_Log("CF_Teams", "PartySystem");
        static bool Prefix(NetPackagePartyActions __instance, World _world, int ___invitedByEntityID, int ___invitedEntityID, NetPackagePartyActions.PartyActions ___currentOperation)
        {
            ClientInfo cInfoS = CF_Player.GetClientInfo(___invitedByEntityID);
            ClientInfo cInfoR = CF_Player.GetClientInfo(___invitedEntityID);

            EntityPlayer playerS = _world.GetEntity(___invitedByEntityID) as EntityPlayer;
            EntityPlayer playerR = _world.GetEntity(___invitedEntityID) as EntityPlayer;

            switch (___currentOperation)
            {
                case NetPackagePartyActions.PartyActions.SendInvite:
                    if(playerS?.IsInParty() ?? false && playerS.Party.MemberList.Count >= partyLimit)
                    {
                        log.Out($"");
                        CF_Player.Message($"[ff0000] The party reached already the max size of {partyLimit} members.", cInfoS);
                        cInfoS.SendPackage(NetPackageManager.GetPackage<NetPackagePartyData>().Setup(playerS.Party, ___invitedEntityID, NetPackagePartyData.PartyActions.LeaveParty));

                        return false;
                    }
                    break;
                case NetPackagePartyActions.PartyActions.AcceptInvite:
                    if (playerR?.IsInParty() ?? false && playerR.Party.MemberList.Count >= partyLimit)
                    {
                        CF_Player.Message($"[ff0000] The party reached already the max size of {partyLimit} members.", cInfoS);
                        cInfoS.SendPackage(NetPackageManager.GetPackage<NetPackagePartyData>().Setup(playerR.Party, ___invitedEntityID, NetPackagePartyData.PartyActions.LeaveParty));

                        return false;
                    }
                    break;
            }

            logParty.Out($"{___currentOperation}: {cInfoS.playerName} => {cInfoR.playerName}");

            return true;
        }
    }
}
