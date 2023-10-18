using HarmonyLib;
using System.Collections.Generic;
using static CF_Teams.API;
using static DroneWeapons;

namespace CF_Teams
{
    [HarmonyPatch(typeof(NetPackagePersistentPlayerState), "ProcessPackage")]
    public class PersistentPlayerState_Patch
    {
        public static CF_Log logAllies = new CF_Log("CF_Teams", "AllySystem");
        static bool Prefix(NetPackagePersistentPlayerState __instance,
        PlatformUserIdentifierAbs ___m_playerID, PlatformUserIdentifierAbs ___m_otherPlayerID,
        ref EnumPersistentPlayerDataReason ___m_reason,
        out bool __state)
        {
            __state = false;

            ClientInfo cInfoS = __instance.Sender;
            ClientInfo cInfoA = CF_Player.GetClientInfo(___m_playerID);
            ClientInfo cInfoB = CF_Player.GetClientInfo(___m_otherPlayerID);
            
            string playerS = cInfoS != null ? cInfoS.playerName + $"({cInfoS.InternalId.ReadablePlatformUserIdentifier})" : cInfoS.InternalId.ReadablePlatformUserIdentifier;
            string playerA = cInfoA != null ? cInfoA.playerName + $"({___m_playerID.ReadablePlatformUserIdentifier})" : ___m_playerID.ReadablePlatformUserIdentifier;
            string playerB = cInfoB != null ? cInfoB.playerName + $"({___m_otherPlayerID.ReadablePlatformUserIdentifier})" : ___m_otherPlayerID.ReadablePlatformUserIdentifier;

            if (!CF_TeamManager.CheckPersistentPlayerStateChangePre(cInfoA, ___m_playerID, ___m_otherPlayerID, ___m_reason))
            {
                if (cInfoS.entityId != cInfoA.entityId)
                    logAllies.Out($"Denied {___m_reason}: {playerA} => {playerB} (Sender: {playerS})");
                else logAllies.Out($"Denied {___m_reason}: {playerA} => {playerB}");

                __state = true;
                return false;
            }

            if (cInfoS.entityId != cInfoA.entityId)
                logAllies.Out($"Accepted {___m_reason}: {playerA} => {playerB} (Sender: {playerS})");
            else logAllies.Out($"Accepted {___m_reason}: {playerA} => {playerB}");

            return true;
        }
        static void Postfix(NetPackagePersistentPlayerState __instance,
        PlatformUserIdentifierAbs ___m_playerID, PlatformUserIdentifierAbs ___m_otherPlayerID,
        EnumPersistentPlayerDataReason ___m_reason, PersistentPlayerData ___m_ppData,
        bool __state)
        {
            ClientInfo cInfo = CF_Player.GetClientInfo(___m_playerID);
            ClientInfo cInfoOther = CF_Player.GetClientInfo(___m_otherPlayerID);

            string player = cInfo != null ? cInfo.playerName + $"({___m_playerID.ReadablePlatformUserIdentifier})" : ___m_playerID.ReadablePlatformUserIdentifier;
            string playerOther = cInfoOther != null ? cInfoOther.playerName + $"({___m_otherPlayerID.ReadablePlatformUserIdentifier})" : ___m_otherPlayerID.ReadablePlatformUserIdentifier;

            CF_TeamManager.CheckPersistentPlayerStateChangePost(cInfo, ___m_playerID, ___m_otherPlayerID, ___m_reason);

            if (!__state)
                return;

            // Denied

            Dictionary<PlatformUserIdentifierAbs, PersistentPlayerData> persistentPlayers = GameManager.Instance.GetPersistentPlayerList().Players;
            if (!persistentPlayers.TryGetValue(___m_playerID, out PersistentPlayerData pData))
            {
                logAllies.Error($"Error getting pData");
                return;
            }
            if (!persistentPlayers.TryGetValue(___m_otherPlayerID, out PersistentPlayerData pDataOther))
            {
                logAllies.Error($"Error getting pDataOther");
                return;
            }

            // TODO fixed pending invites when an invite or accept invite got declined

            switch (___m_reason)
            {
                case EnumPersistentPlayerDataReason.ACL_Invite:
                    logAllies.Out($"{___m_reason}: {playerOther} => {player} (Invite Declined by Mod)");
                    cInfo.SendPackage(new NetPackagePersistentPlayerState().Setup(pDataOther, ___m_playerID, EnumPersistentPlayerDataReason.ACL_DeclinedInvite));
                    break;
                case EnumPersistentPlayerDataReason.ACL_AcceptedInvite:
                    logAllies.Out($"{___m_reason}: {playerOther} => {player} (AcceptedInvite Declined by Mod)");
                    cInfo.SendPackage(new NetPackagePersistentPlayerState().Setup(pData, ___m_otherPlayerID, EnumPersistentPlayerDataReason.ACL_Removed));
                    cInfo.SendPackage(new NetPackagePersistentPlayerState().Setup(pDataOther, ___m_playerID, EnumPersistentPlayerDataReason.ACL_DeclinedInvite));
                    break;
            }
        }
    }
}
