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
    public class IPhub
    {
        public static void ProcessIpHubResponse(ClientInfo _cInfo, IPHubResponse response)
        {
            try
            {
                if (checkedIPs.ContainsKey(_cInfo.ip))
                {
                    checkedIPs[_cInfo.ip].data = response;
                    checkedIPs[_cInfo.ip].last = DateTime.UtcNow;
                }
                else checkedIPs.Add(_cInfo.ip, new CheckedIP(response));

                SaveIPdata();
                CheckIPdata(_cInfo);
            }
            catch (Exception e)
            {
                log.Error($"ProcessIpHubResponse reported: {e}");
            }
        }
        public static bool CheckIPdata(ClientInfo _cInfo)
        {
            try
            {
                if (!checkedIPs.ContainsKey(_cInfo.ip))
                {
                    log.Error($"CheckIPdata: IP not found {_cInfo.ip}");
                    return true;
                }

                IPHubResponse data = checkedIPs[_cInfo.ip].data;
                if (data == null)
                {
                    IPHubResponse.Check(_cInfo, ProcessIpHubResponse);
                    return true;
                }

                if (CF_Whitelist.VPN(_cInfo))
                    return true;

                if (AnsInWhitelist(data.asn))
                    return true;

                if (ipHubVpnMode == 1 && data.block > 0 
                    && 
                    ((ipHubVpnSensitivity == 0 && data.block == 1)
                    ||(ipHubVpnSensitivity == 1 && data.block >= 1))
                    )
                {
                    Ban(_cInfo, banReasonVPN, $"Detected VPN => [{data.countryCode}] IP: {_cInfo.ip} ASN: {data.isp} ({data.asn}) Block: {data.block}");
                    return false;
                }

                // Blacklisted ASN
                if (AnsInBlacklist(data.asn))
                {
                    Ban(_cInfo, banReasonASN, $"Detected blocked ASN => [{data.countryCode}] IP: {_cInfo.ip} ASN: {data.isp} ({data.asn})");
                    return false;
                }

                // Blacklisted country
                if (ipHubCountryMode == 1 && ipHubCountryList.ContainsCaseInsensitive(data.countryCode) && !CF_Whitelist.Country(_cInfo))
                {
                    Ban(_cInfo, banReasonCountry, $"Detected blocked country => [{data.countryCode}]");
                    return false;
                }
            }
            catch (Exception e) { log.Error($"CheckIPdata reported: {e.Message}"); }

            return true;
        }
    }
}
