using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

using static CF_Firewall.API;

namespace CF_Firewall
{
    public class Whitelist
    {
        private static WhitelistContainer whitelist = new WhitelistContainer();
        public class WhitelistContainer
        {
            public List<string> VPN = new List<string>();
            public List<string> Steam = new List<string>();
            public List<string> FamilyShare = new List<string>();
            public List<string> Country = new List<string>();
        }
        public static bool VPN(ClientInfo _cInfo) => whitelist.VPN.Contains(_cInfo.ip) || whitelist.VPN.Contains(_cInfo.InternalId.ReadablePlatformUserIdentifier) || whitelist.VPN.Contains(_cInfo.PlatformId.ReadablePlatformUserIdentifier);
        public static bool VPN(string auth) => whitelist.VPN.Contains(auth);
        public static void VPNAdd(string auth)
        {
            whitelist.VPN.Add(auth);
        }
        public static bool Steam(ClientInfo _cInfo) => whitelist.Steam.Contains(_cInfo.ip) || whitelist.Steam.Contains(_cInfo.InternalId.ReadablePlatformUserIdentifier) || whitelist.Steam.Contains(_cInfo.PlatformId.ReadablePlatformUserIdentifier);
        public static bool Steam(string auth) => whitelist.Steam.Contains(auth);
        public static void SteamAdd(string auth)
        {
            whitelist.Steam.Add(auth);
        }
        public static bool FamilyShare(ClientInfo _cInfo) => whitelist.FamilyShare.Contains(_cInfo.ip) || whitelist.FamilyShare.Contains(_cInfo.InternalId.ReadablePlatformUserIdentifier) || whitelist.FamilyShare.Contains(_cInfo.PlatformId.ReadablePlatformUserIdentifier);
        public static bool FamilyShare(string auth) => whitelist.FamilyShare.Contains(auth);
        public static void FamilyShareAdd(string auth)
        {
            whitelist.FamilyShare.Add(auth);
        }
        public static bool Country(ClientInfo _cInfo) => whitelist.Country.Contains(_cInfo.ip) || whitelist.Country.Contains(_cInfo.InternalId.ReadablePlatformUserIdentifier) || whitelist.Country.Contains(_cInfo.PlatformId.ReadablePlatformUserIdentifier);
        public static bool Country(string auth) => whitelist.Country.Contains(auth); 
        public static void CountryAdd(string auth)
        {
            whitelist.Country.Add(auth);
        }
        public static void Load()
        {
            try
            {
                whitelist = JsonConvert.DeserializeObject<WhitelistContainer>(File.ReadAllText(filePathWhitelist));
                return;
            }
            catch (FileNotFoundException e)
            {
                x.Error($"File not found: {e}");
            }
            catch (Exception e)
            {
                x.Error($"Failed loading from {filePathWhitelist}: {e}");
                return;
            }

            x.Log($"Create new whitelist file.");
            Save();
        }
        public static void Save()
        {
            File.WriteAllText(filePathWhitelist, JsonConvert.SerializeObject(whitelist, Newtonsoft.Json.Formatting.Indented));
            x.Debug($"Saved whitelist to {filePathWhitelist}.");
        }
    }
}
