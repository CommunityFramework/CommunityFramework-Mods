using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CF_Firewall.API;

namespace CF_Firewall
{
    internal class Steam
    {
        public static void ProcesssVacBanWebResponse(ClientInfo _cInfo, SteamVacBanResponse response)
        {
            if (steamMaxDaysSinceLastBan != 0 && response.Players[0].DaysSinceLastBan > steamMaxDaysSinceLastBan)
            {
                //LogX.Date($"Detected old ban ({response.Players[0].DaysSinceLastBan} days ago) => {_cInfo.playerName} ({_cInfo.PlatformId.ReadablePlatformUserIdentifier}) IP: {_cInfo.ip}", false, "Bans");
                return;
            }

            if (steamVacBan && response.Players[0].VACBanned)
                Ban(_cInfo, banReasonVAC, $"Detected VAC ban");
            if (steamGameBan && response.Players[0].NumberOfGameBans > 0)
                Ban(_cInfo, banReasonGameBan, $"Detected game ban");
            if (steamCommunityBan && response.Players[0].CommunityBanned)
                Ban(_cInfo, banReasonCommunityBan, $"Detected community ban");
        }
    }
}
