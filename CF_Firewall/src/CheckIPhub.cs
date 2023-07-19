using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CF_Firewall.API;

namespace CF_Firewall
{
    public class CheckIPhub
    {
        public static void ProcessIpHubResponse(ClientInfo _cInfo, IPHubResponse response)
        {
            try
            {
                if (checkedIPs.ContainsKey(_cInfo.ip))
                {
                    checkedIPs[_cInfo.ip].data = response;
                    checkedIPs[_cInfo.ip].last = DateTime.Now;
                }
                else checkedIPs.Add(_cInfo.ip, new CheckedIP(response));

                SaveIPdata();

                /*
                Database.Player dbPlayer = Database.GetPlayer(_cInfo.InternalId);
                if (dbPlayer == null)
                    return;

                dbPlayer.countryCode = response.countryCode;
                dbPlayer.countryName = response.countryName;
                dbPlayer.ASN = response.asn;
                dbPlayer.ISP = response.isp;
                dbPlayer.SetModified();
                */

                CheckIPdata(_cInfo);
            }
            catch (Exception e)
            {
                x.Error($"ProcessIpHubResponse reported: {e}");
            }
        }
        public static bool CheckIPdata(ClientInfo _cInfo)
        {
            try
            {
                if (!checkedIPs.ContainsKey(_cInfo.ip))
                {
                    x.Error($"CheckIPdata: IP not found {_cInfo.ip}");
                    return true;
                }

                IPHubResponse data = checkedIPs[_cInfo.ip].data;
                if (data == null)
                {
                    x.Error($"No data for checked IP: {_cInfo.ip}.");
                    return true;
                }

                if (Whitelist.VPN(_cInfo))
                    return true;

                if (AnsInWhitelist(data.asn))
                    return true;

                if (ipHubVpnMode == 1 && data.block > 0 && ipHubVpnSensitivity + 1 >= data.block)
                {
                    Ban(_cInfo, "VPN", $"Detected VPN => [{data.countryCode}] IP: {_cInfo.ip} ASN: {data.isp} ({data.asn})");
                    return false;
                }

                // Blacklisted ASN
                if (AnsInBlacklist(data.asn))
                {
                    Ban(_cInfo, "Region", $"Detected blocked ASN => [{data.countryCode}] IP: {_cInfo.ip} ASN: {data.isp} ({data.asn})");
                    return false;
                }

                // Blacklsited country
                if (ipHubCountryMode == 1 && ipHubCountryList.ContainsCaseInsensitive(data.countryCode) && !Whitelist.Country(_cInfo))
                {
                    Ban(_cInfo, "Country", $"Detected blocked country => [{data.countryCode}]");
                    return false;
                }
            }
            catch (Exception e) { x.Error($"CheckIPdata reported: {e.Message}"); }

            return true;
        }
    }
}
