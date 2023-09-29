using GameSparks.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CF_Blacklist
{
    public static void AddBan(PlatformUserIdentifierAbs _user, string reason = "")
    {
        AddBan(_user, DateTime.MaxValue, reason);
    }
    public static void AddBan(PlatformUserIdentifierAbs _user, DateTime unbanDate, string reason = "")
    {
        if(CF_Player.TryGetClientInfo(_user, out ClientInfo cInfo))
        {
            GameUtils.KickPlayerForClientInfo(cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.Banned, _customReason: reason));
        }

        string playername = "";

        if(cInfo != null)
        {
            playername = cInfo.playerName;
        }
        else
        {
            PersistentPlayerData playerdata = CF_Player.GetPersistentPlayerData(_user);
            playername = playerdata.PlayerName;
        }

        GameManager.Instance.adminTools.Blacklist.AddBan(playername, _user, DateTime.Now.AddYears(10), "Cheater");
    }
}
