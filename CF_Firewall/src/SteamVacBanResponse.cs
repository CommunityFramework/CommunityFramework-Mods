﻿using Epic.OnlineServices.Presence;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static CF_Firewall.API;

namespace CF_Firewall
{
    public class SteamVacBanResponse
    {
        public static string steamVacBanUrl = "http://api.steampowered.com/ISteamUser/GetPlayerBans/v1/?key={TOKEN}&steamids={STEAMID}";
        [JsonProperty("Players")]
        public Player[] Players { get; set; }

        public class Player
        {
            [JsonProperty("SteamId")]
            public string SteamId { get; set; }

            [JsonProperty("CommunityBanned")]
            public bool CommunityBanned { get; set; }

            [JsonProperty("VACBanned")]
            public bool VACBanned { get; set; }

            [JsonProperty("NumberOfVACBans")]
            public int NumberOfVACBans { get; set; }

            [JsonProperty("DaysSinceLastBan")]
            public int DaysSinceLastBan { get; set; }

            [JsonProperty("NumberOfGameBans")]
            public int NumberOfGameBans { get; set; }

            [JsonProperty("EconomyBan")]
            public string EconomyBan { get; set; }
        }
        public static SteamVacBanResponse Deserialize(string responseString) => JsonConvert.DeserializeObject<SteamVacBanResponse>(responseString);
        public static void Check(ClientInfo _cInfo, Action<ClientInfo, SteamVacBanResponse> callback)
        {
            using (WebClient client = new WebClient())
            {
                string url = steamVacBanUrl.Replace("{TOKEN}", steamToken).Replace("{STEAMID}", _cInfo.PlatformId.ReadablePlatformUserIdentifier);
                client.DownloadStringAsync(new Uri(url));
                client.DownloadStringCompleted += (sender, args) => callback(_cInfo, Deserialize(args.Result));
            }
        }
    }
}
