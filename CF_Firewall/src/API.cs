using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Platform;
using Platform.Steam;
using System.Collections.ObjectModel;
using LiteNetLib;
using HarmonyLib;

namespace CF_Firewall
{
    public class API : IModApi
    {
        public static ModX module = new ModX("CF_Firewall", OnConfigLoaded, OnPhrasesLoaded);
        public static LogX x = new LogX("CF_Firewall");
        public static Harmony harmony = new Harmony("CF_Firewall");
        public static string filePathIPdata;
        public static string filePathWhitelist;
        public void InitMod(Mod _modInstance)
        {
            filePathIPdata = module.modDatabasePath + "/IPdata.json";
            filePathWhitelist = module.modDatabasePath + "/Whitelist.json";
            module.Activate(true);
            harmony.PatchAll();
            ModEvents.PlayerSpawning.RegisterHandler(CheckPlayer);
        }

        public static bool banIP;
        public static bool banIP2;
        public static bool keepPermissions;
        public static int maxBanTime;

        public static string ipHubToken;
        public static string ipHubAsnListA;
        public static string ipHubAsnListB;
        public static List<AsnRange> IpHubAsnListA = new List<AsnRange>();
        public static List<AsnRange> IpHubAsnListB = new List<AsnRange>();
        public static int ipHubVpnMode;
        public static int ipHubVpnSensitivity;
        public static int ipHubCountryMode;
        public static string ipHubCountryFilter;
        public static string[] ipHubCountryList;
        public static int ipHubRecheck;
        public static int ipHubKeep;

        public static string steamToken;
        public static bool steamFamilyShare;
        public static bool steamFamilyShareBanPlayer;
        public static bool steamFamilyShareBanOwner;

        public static bool steamVacBan;
        public static bool steamGameBan;
        public static bool steamCommunityBan;
        public static int steamMaxDaysSinceLastBan;

        public static void OnConfigLoaded()
        {
            module.AddSetting("Ban_IP", true, "Ban IP when banned.", out banIP);
            module.AddSetting("Ban_IP2", true, "If enabled banning accounts joining with a banned IP.", out banIP2);
            module.AddSetting("KeepPerms_Enable", true, "Don't remove permissions when getting banned.", out keepPermissions);
            module.AddSetting("KeepPerms_MaxBanLength", 0, 0, 9999999, "Max banlength in minutes to prevent permission drop. 0: Unlimited", out maxBanTime);
            module.AddSetting("IPdata_Keep", 999999, 0, 999999, "Hours before deleting an IP data (including IP bans).", out ipHubKeep);
            module.AddSetting("IPhub_Token", "", "", "API token obtained from iphub.info", out ipHubToken);
            module.AddSetting("IPhub_Recheck", 72, 0, 99999, "Hours before rechecking an IP address.", out ipHubRecheck);
            module.AddSetting("IPhub_AsnAllow", "11414,20347,30781,38195,47104:51355,57246,396919,49902,56465,57248,57269,57536,57587,57608,57910,136557,137226", "", "ASN whitelist (separated by ',')", out ipHubAsnListA);
            module.AddSetting("IPhub_AsnBlock", "174,1299,4229,4804,8797,9335,13442,16509,21859,21880,24955,29076,29752,38195,38854,45090,47104:51355,55990,56320:58367,61952:62463,62610,63199,134557:139577,139625,141039,141167,141180,140602:141625,146834,147049,205119,206026,213250", "", "ASN blacklist (separated by ',')", out ipHubAsnListB);
            module.AddSetting("IPhub_VpnBanMode", 1, -1, 2, "-1: Disable, 0: Kick, 1: Ban", out ipHubVpnMode);
            module.AddSetting("IPhub_VpnSensitivity", 1, 0, 1, "0: Balanced, 1: Agressive (contains more false positives)", out ipHubVpnSensitivity);
            module.AddSetting("IPhub_CountryMode", 1, -1, 2, "-1: Disable, 0: Kick, 1: Ban", out ipHubCountryMode);
            module.AddSetting("IPhub_CountryFilter", "CN,HK,AF,AX,Al,DZ,AS,AD,AO,AI,AQ,AG,AM,AW,AZ,BS,BH,BD,BZ,BJ,BM,BT,BO,BQ,BW,BV,IO,BN,BF,BI,CV,KH,CM,KY,CF,TD,CX,CC,KM,CD,CG,CK,CR,CI,CU,CW,CY,DJ,CM,GQ,ER,SZ,ET,FK,FO,GF,TF,GA,GM,GH,GI,GL,GD,GP,GU,GT,GG,GN,GW,GY,HT,HM,VA,HM,VA,HN,IQ,IM,JM,JO,KE,KI,KP,KG,LA,LB,LR,LY,LI,LU,MO,MK,MG,MW,MV,ML,MT,MH,MQ,MR,MU,YT,FM,MD,MC,MN,ME,MS,MZ,MM,NA,NR,NP,NC,NI,NE,NG,NU,NF,MP,OM,PK,PW,PS,PA,PG,TY,PN,PR,QA,RE,RW,BL,SH,KN,LC,MF,PM,VC,WS,SM,ST,SN,SC,SL,SX,SB,SO,GS,SS,LK,SD,SR,SJ,SY,TJ,TZ,TL,TG,TK,TO,TT,TN,TM,TC,TV,UG,UM,VU,VG,VI,WF,EH,YE,ZM,ZW", "", "List of country codes to ban, separated by ','.", out ipHubCountryFilter);
            module.AddSetting("Steam_FamilyCheck", true, "Check for family sharing.", out steamFamilyShare);
            module.AddSetting("Steam_FamilyBan", true, "Ban players using family sharing. To to false to kick only", out steamFamilyShareBanPlayer);
            module.AddSetting("Steam_FamilyBanOwner", true, "Ban owner of family sharing account.", out steamFamilyShareBanOwner);
            module.AddSetting("Steam_Token", "", "", "API token obtained from steamcommunity.com/dev/apikey", out steamToken);
            module.AddSetting("Steam_VacBan", true, "Ban if VAC banned.", out steamVacBan);
            module.AddSetting("Steam_GameBan", true, "Ban if game banned.", out steamGameBan);
            module.AddSetting("Steam_CommunityBan", true, "Ban if community banned.", out steamCommunityBan);
            module.AddSetting("Steam_MaxBanDaysAgo", 500, 0, 999999, "Only ban player if last ban was less then set amount of days ago (0: Disable).", out steamMaxDaysSinceLastBan);

            string[] AsnListA = ipHubAsnListA.Split(',');
            IpHubAsnListA.Clear();
            foreach (string ans in AsnListA)
            {
                if (string.IsNullOrEmpty(ans))
                {
                    x.Error($"Empty ANS set");
                    return;
                }

                if (ans.Contains(":"))
                {
                    string[] ansRange = ans.Split(':');
                    if (!int.TryParse(ansRange[0], out int start))
                    {
                        x.Error($"Invalid ANS range start: {ans}");
                        return;
                    }
                    if (!int.TryParse(ansRange[1], out int end))
                    {
                        x.Error($"Invalid ANS range end: {ans}");
                        return;
                    }

                    IpHubAsnListA.Add(new AsnRange(start, end));
                    continue;
                }

                if (!int.TryParse(ans, out int Ans))
                {
                    x.Error($"Invalid ANS: {ans}");
                    return;
                }

                IpHubAsnListA.Add(new AsnRange(Ans));
            }
            string[] AsnListB = ipHubAsnListB.Split(',');
            IpHubAsnListB.Clear();
            foreach (string ans in AsnListB)
            {
                if (string.IsNullOrEmpty(ans))
                {
                    x.Error($"Empty ANS set");
                    return;
                }

                if (ans.Contains(":"))
                {
                    string[] ansRange = ans.Split(':');
                    if (!int.TryParse(ansRange[0], out int start))
                    {
                        x.Error($"Invalid ANS range start: {ans}");
                        return;
                    }
                    if (!int.TryParse(ansRange[1], out int end))
                    {
                        x.Error($"Invalid ANS range end: {ans}");
                        return;
                    }

                    IpHubAsnListB.Add(new AsnRange(start, end));
                    continue;
                }

                if (!int.TryParse(ans, out int Ans))
                {
                    x.Error($"Invalid ANS: {ans}");
                    return;
                }

                IpHubAsnListB.Add(new AsnRange(Ans));
            }

            ipHubCountryList = ipHubCountryFilter.Split(',');

            LoadIPdata();
            Whitelist.Load();
        }
        public static void OnPhrasesLoaded()
        {

        }
        public static Dictionary<string, CheckedIP> checkedIPs = new Dictionary<string, CheckedIP>();

        public static bool LoadIPdata()
        {
            if (!File.Exists(module.modDatabasePath + "/IPdata.json"))
                return false;

            try
            {
                checkedIPs = JsonConvert.DeserializeObject<Dictionary<string, CheckedIP>>(File.ReadAllText(filePathIPdata));

                foreach (KeyValuePair<string, CheckedIP> kv in new Dictionary<string, CheckedIP>(checkedIPs))
                {
                    if (kv.Value.last.AddHours(ipHubKeep) < DateTime.Now)
                        checkedIPs.Remove(kv.Key);
                }

                SaveIPdata();

                return true;
            }
            catch (Exception e)
            {
                x.Error($"Failed loading from {filePathIPdata}: {e}");
                return false;
            }
        }
        public static void SaveIPdata()
        {
            File.WriteAllText(filePathIPdata, JsonConvert.SerializeObject(checkedIPs, Newtonsoft.Json.Formatting.Indented));
        }
        public static void CheckPlayer(ClientInfo _cInfo, int _chunkViewDim, PlayerProfile _playerProfile)
        {
            if (checkedIPs.TryGetValue(_cInfo.ip, out CheckedIP checkedIP) && checkedIP.ipBan)
                Ban(_cInfo, "IP Ban", "Joined with already banned IP");

            if (!string.IsNullOrEmpty(ipHubToken) && (ipHubVpnMode >= 0 || ipHubCountryMode >= 0))
            {
                try
                {
                    if (checkedIPs.ContainsKey(_cInfo.ip) && (checkedIPs[_cInfo.ip].data == null || checkedIPs[_cInfo.ip].last.AddHours(ipHubRecheck) > DateTime.Now))
                        CheckIPhub.CheckIPdata(_cInfo);
                    else IPHubResponse.Check(_cInfo, CheckIPhub.ProcessIpHubResponse);
                }
                catch (Exception e) { x.Error($"CheckPlayer.IPHub reported: {e}"); }
            }

            if (_cInfo.PlatformId.PlatformIdentifier == EPlatformIdentifier.Steam 
                && steamFamilyShare 
                && Whitelist.FamilyShare(_cInfo))
            {
                try
                {
                    UserIdentifierSteam steam = _cInfo.PlatformId as UserIdentifierSteam;
                    if (!steam.OwnerId.ReadablePlatformUserIdentifier.Equals(steam.ReadablePlatformUserIdentifier))
                    {
                        /*
                        if (steamFamilyShareBanOwner)
                            Ban(PlatformUserIdentifierAbs.FromPlatformAndId("STEAM", steam.OwnerId.ReadablePlatformUserIdentifier), _cInfo.playerName, "FamilySharing", "Owner Account");
                        */

                        Ban(_cInfo, "FamilySharing", "User Account");
                    }
                }
                catch (Exception e) { x.Error($"CheckPlayer.FamilyShare reported: {e}"); }
            }

            if (_cInfo.PlatformId.PlatformIdentifier == EPlatformIdentifier.Steam 
                && !string.IsNullOrEmpty(steamToken) 
                && (steamVacBan || steamCommunityBan || steamGameBan) && Whitelist.Steam(_cInfo))
            {
                VacBanWebResponse.Check(_cInfo, CheckSteam.ProcesssVacBanWebResponse);
            }
        }
        public static bool Ban(ClientInfo _cInfo, string _reason, string _details)
        {
            x.Log($"Banned {_cInfo} for reason: {_reason} details: {_details}");
            GameManager.Instance.adminTools.Blacklist.AddBan(_cInfo.playerName, _cInfo.PlatformId, DateTime.Now.AddYears(10), _reason);
            GameUtils.KickPlayerForClientInfo(_cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, _customReason: _reason));
            Ban(_cInfo.ip);
            return true;
        }
        public static bool Ban(string _ip)
        {
            if (checkedIPs.ContainsKey(_ip))
            {
                checkedIPs[_ip].ipBan = true;
                checkedIPs[_ip].last = DateTime.Now;

                /*
                foreach (Database.Player dbPlayer in Database.GetPlayers(true))
                {
                    if (dbPlayer.IP.Equals(IP) || dbPlayer.IPs.Contains(IP))
                        BanManager.Ban(dbPlayer.GetUserPlatform(), dbPlayer.name, "Autoban 93", $"Matching banned IP: {IP}");
                }
                */

                x.Log($"IP: {_ip} was already banned");
            }
            else checkedIPs.Add(_ip, new CheckedIP());

            checkedIPs[_ip].ipBan = false;
            checkedIPs[_ip].last = DateTime.Now;

            x.Log($"IP: {_ip} banned.");

            SaveIPdata();

            return true;
        }
        public static bool Unban(string IP)
        {
            if (!checkedIPs.ContainsKey(IP))
                return false;

            checkedIPs[IP].ipBan = false;
            checkedIPs[IP].last = DateTime.Now;

            x.Log($"IP: {IP} unbanned by command.");

            SaveIPdata();

            return true;
        }
        public static bool AnsInWhitelist(int asn)
        {
            try
            {
                foreach (AsnRange range in IpHubAsnListA)
                {
                    if (range.rangeStart <= asn && range.rangeEnd >= asn)
                        return true;
                }
            }
            catch (Exception e) { x.Error($"AnsInWhitelist reported: {e.Message}"); }

            return false;
        }
        public static bool AnsInBlacklist(int asn)
        {
            try
            {
                foreach (AsnRange range in IpHubAsnListB)
                {
                    if (range.rangeStart <= asn && range.rangeEnd >= asn)
                        return true;
                }
            }
            catch (Exception e) { x.Error($"AnsInWhitelist reported: {e.Message}"); }

            return false;
        }
    }
}

