using Epic.OnlineServices.Presence;
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
    public class IPHubResponse
    {
        public static string ipHubUrl = "http://v2.api.iphub.info/ip/{IP}";

        public string ip { get; set; }
        public string countryCode { get; set; }
        public string countryName { get; set; }
        public int asn { get; set; }
        public string isp { get; set; }
        public int block { get; set; }
        public string hostname { get; set; }

        public static IPHubResponse Deserialize(string responseString) => JsonConvert.DeserializeObject<IPHubResponse>(responseString);
        public static void Check(ClientInfo _cInfo, Action<ClientInfo, IPHubResponse> callback)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("X-Key", ipHubToken);
                client.DownloadStringAsync(new Uri(ipHubUrl.Replace("{IP}", _cInfo.ip)));
                client.DownloadStringCompleted += (sender, args) => callback(_cInfo, Deserialize(args.Result));
            }
        }
    }
}
