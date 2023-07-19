using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CF_Firewall.API;

namespace CF_Firewall
{
    internal class CheckSteam
    {
        public static void ProcesssVacBanWebResponse(ClientInfo _cInfo, VacBanWebResponse response)
        {
            if (steamMaxDaysSinceLastBan != 0 && response.Players[0].DaysSinceLastBan > steamMaxDaysSinceLastBan)
            {
                //LogX.Date($"Detected old ban ({response.Players[0].DaysSinceLastBan} days ago) => {_cInfo.playerName} ({_cInfo.PlatformId.ReadablePlatformUserIdentifier}) IP: {_cInfo.ip}", false, "Bans");
                return;
            }

            if (steamVacBan && response.Players[0].VACBanned)
                Ban(_cInfo, "Profile Ban", $"Detected VAC ban");
            if (steamGameBan && response.Players[0].NumberOfGameBans > 0)
                Ban(_cInfo, "Profile Ban", $"Detected game ban");
            if (steamCommunityBan && response.Players[0].CommunityBanned)
                Ban(_cInfo, "Profile Ban", $"Detected community ban");
        }
    }
}
