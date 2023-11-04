using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Platform;
using Platform.Steam;
using HarmonyLib;
using InControl.NativeDeviceProfiles;

namespace CF_Firewall
{
    public class API : IModApi
    {
        public static CF_Mod mod = new CF_Mod("CF_Firewall", OnConfigLoaded, OnPhrasesLoaded);
        public static CF_Log log = new CF_Log("CF_Firewall");
        public static Harmony harmony = new Harmony("CF_Firewall");
        public static string filePathIPdata;
        public static string filePathWhitelist;
        public void InitMod(Mod _modInstance)
        {
            filePathIPdata = mod.modDatabasePath + "/IPdata.json";
            filePathWhitelist = mod.modDatabasePath + "/Whitelist.json";
            mod.Activate(true);
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

        public static string banReasonASN;
        public static string banReasonVPN;
        public static string banReasonCountry;
        public static string banReasonIP;
        public static string banReasonVAC;
        public static string banReasonGameBan;
        public static string banReasonCommunityBan;
        public static string banReasonFamilySharing;
        public static void OnConfigLoaded()
        {
            mod.AddSetting("Ban_IP", false, "Ban IP when banned.", out banIP);
            mod.AddSetting("Ban_IP2", false, "If enabled banning accounts joining with a banned IP.", out banIP2);
            mod.AddSetting("KeepPerms_Enable", true, "Don't remove permissions when getting banned.", out keepPermissions);
            mod.AddSetting("KeepPerms_MaxBanLength", 0, 0, 9999999, "Max banlength in minutes to prevent permission drop. 0: Unlimited", out maxBanTime);
            mod.AddSetting("IPdata_Keep", 999999, 0, 999999, "Hours before deleting an IP data (including IP bans).", out ipHubKeep);
            mod.AddSetting("IPhub_Token", "", "", "API token obtained from iphub.info", out ipHubToken);
            mod.AddSetting("IPhub_Recheck", 72, 0, 99999, "Hours before rechecking an IP address.", out ipHubRecheck);
            mod.AddSetting("IPhub_AsnAllow", "11414,20347,30781,38195,47104:51355,57246,396919,49902,56465,57248,57269,57536,57587,57608,57910,136557,137226", "", "ASN whitelist (separated by ',')", out ipHubAsnListA);
            mod.AddSetting("IPhub_AsnBlock", "174,1299,4229,9335,13442,16509,21859,21880,29752,38195,38854,45090,55990,62610,139625,141039,141167,141180,146834,147049,205119", "", "ASN blacklist (separated by ',')", out ipHubAsnListB);
            mod.AddSetting("IPhub_VpnBanMode", 1, -1, 2, "-1: Disable, 0: Kick, 1: Ban", out ipHubVpnMode);
            mod.AddSetting("IPhub_VpnSensitivity", 1, 0, 1, "0: Balanced, 1: Agressive (contains more false positives)", out ipHubVpnSensitivity);
            mod.AddSetting("IPhub_CountryMode", 1, -1, 2, "-1: Disable, 0: Kick, 1: Ban", out ipHubCountryMode);
            mod.AddSetting("IPhub_CountryFilter", "CN,HK,AF,AX,Al,DZ,AS,AD,AO,AI,AQ,AG,AM,AW,AZ,BS,BH,BD,BZ,BJ,BM,BT,BO,BQ,BW,BV,IO,BN,BF,BI,CV,KH,CM,KY,CF,TD,CX,CC,KM,CD,CG,CK,CR,CI,CU,CW,CY,DJ,CM,GQ,ER,SZ,ET,FK,FO,GF,TF,GA,GM,GH,GI,GL,GD,GP,GU,GT,GG,GN,GW,GY,HT,HM,VA,HM,VA,HN,IQ,IM,JM,JO,KE,KI,KP,KG,LA,LB,LR,LY,LI,LU,MO,MK,MG,MW,MV,ML,MT,MH,MQ,MR,MU,YT,FM,MD,MC,MN,ME,MS,MZ,MM,NA,NR,NP,NC,NI,NE,NG,NU,NF,MP,OM,PK,PW,PS,PA,PG,TY,PN,PR,QA,RE,RW,BL,SH,KN,LC,MF,PM,VC,WS,SM,ST,SN,SC,SL,SX,SB,SO,GS,SS,LK,SD,SR,SJ,SY,TJ,TZ,TL,TG,TK,TO,TT,TN,TM,TC,TV,UG,UM,VU,VG,VI,WF,EH,YE,ZM,ZW", "", "List of country codes to ban, separated by ','.", out ipHubCountryFilter);
            mod.AddSetting("Steam_FamilyCheck", true, "Check for family sharing.", out steamFamilyShare);
            mod.AddSetting("Steam_FamilyBan", true, "Ban players using family sharing. To to false to kick only", out steamFamilyShareBanPlayer);
            mod.AddSetting("Steam_FamilyBanOwner", true, "Ban owner of family sharing account.", out steamFamilyShareBanOwner);
            mod.AddSetting("Steam_Token", "", "", "API token obtained from steamcommunity.com/dev/apikey", out steamToken);
            mod.AddSetting("Steam_VacBan", true, "Ban if VAC banned.", out steamVacBan);
            mod.AddSetting("Steam_GameBan", true, "Ban if game banned.", out steamGameBan);
            mod.AddSetting("Steam_CommunityBan", true, "Ban if community banned.", out steamCommunityBan);
            mod.AddSetting("Steam_MaxBanDaysAgo", 500, 0, 999999, "Only ban player if last ban was less then set amount of days ago (0: Disable).", out steamMaxDaysSinceLastBan);


            mod.AddSetting("BanReason_Region", "Region", "", "Ban region when banned by ASN", out banReasonASN);
            mod.AddSetting("BanReason_VPN", "Region", "", "Ban region when banned by ASN", out banReasonVPN);
            mod.AddSetting("BanReason_Country", "Country", "", "Ban region when banned by Country", out banReasonCountry);

            mod.AddSetting("BanReason_IP", "IP", "", "Ban region when banned by IP", out banReasonIP);
            mod.AddSetting("BanReason_VAC", "VAC", "", "Ban region when banned by VAC", out banReasonVAC);
            mod.AddSetting("BanReason_GameBan", "Game Ban", "", "Ban region when banned by Steam game ban", out banReasonGameBan);
            mod.AddSetting("BanReason_CommunityBan", "Community Ban", "", "Ban region when banned by Steam community ban", out banReasonCommunityBan);
            mod.AddSetting("BanReason_FamilySharing", "Family Sharing", "", "Ban region when banned by Steam family sharing", out banReasonFamilySharing);

            string[] AsnListA = ipHubAsnListA.Split(',');
            IpHubAsnListA.Clear();
            foreach (string ans in AsnListA)
            {
                if (string.IsNullOrEmpty(ans))
                {
                    log.Error($"Empty ANS set");
                    return;
                }

                if (ans.Contains(":"))
                {
                    string[] ansRange = ans.Split(':');
                    if (!int.TryParse(ansRange[0], out int start))
                    {
                        log.Error($"Invalid ANS range start: {ans}");
                        return;
                    }
                    if (!int.TryParse(ansRange[1], out int end))
                    {
                        log.Error($"Invalid ANS range end: {ans}");
                        return;
                    }

                    IpHubAsnListA.Add(new AsnRange(start, end));
                    continue;
                }

                if (!int.TryParse(ans, out int Ans))
                {
                    log.Error($"Invalid ANS: {ans}");
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
                    log.Error($"Empty ANS set");
                    return;
                }

                if (ans.Contains(":"))
                {
                    string[] ansRange = ans.Split(':');
                    if (!int.TryParse(ansRange[0], out int start))
                    {
                        log.Error($"Invalid ANS range start: {ans}");
                        return;
                    }
                    if (!int.TryParse(ansRange[1], out int end))
                    {
                        log.Error($"Invalid ANS range end: {ans}");
                        return;
                    }

                    IpHubAsnListB.Add(new AsnRange(start, end));
                    continue;
                }

                if (!int.TryParse(ans, out int Ans))
                {
                    log.Error($"Invalid ANS: {ans}");
                    return;
                }

                IpHubAsnListB.Add(new AsnRange(Ans));
            }

            ipHubCountryList = ipHubCountryFilter.Split(',');

            LoadIPdata();
            CF_Whitelist.Load();
        }
        public static void OnPhrasesLoaded()
        {

        }
        public static Dictionary<string, CheckedIP> checkedIPs = new Dictionary<string, CheckedIP>();

        public static bool LoadIPdata()
        {
            if (!File.Exists(mod.modDatabasePath + "/IPdata.json"))
                return false;

            try
            {
                checkedIPs = JsonConvert.DeserializeObject<Dictionary<string, CheckedIP>>(File.ReadAllText(filePathIPdata));

                foreach (KeyValuePair<string, CheckedIP> kv in new Dictionary<string, CheckedIP>(checkedIPs))
                {
                    if (kv.Value.last.AddHours(ipHubKeep) < DateTime.UtcNow)
                        checkedIPs.Remove(kv.Key);
                }

                SaveIPdata();

                return true;
            }
            catch (Exception e)
            {
                log.Error($"Failed loading from {filePathIPdata}: {e}");
                return false;
            }
        }
        public static void SaveIPdata()
        {
            File.WriteAllText(filePathIPdata, JsonConvert.SerializeObject(checkedIPs, Formatting.Indented));
        }
        public static void CheckPlayer(ClientInfo _cInfo, int _chunkViewDim, PlayerProfile _playerProfile)
        {
            if (checkedIPs.TryGetValue(_cInfo.ip, out CheckedIP checkedIP) && checkedIP.ipBan)
                Ban(_cInfo, banReasonIP, "Joined with already banned IP");

            if (!string.IsNullOrEmpty(ipHubToken) && (ipHubVpnMode >= 0 || ipHubCountryMode >= 0))
            {
                try
                {
                    if (checkedIPs.ContainsKey(_cInfo.ip) && (checkedIPs[_cInfo.ip].data == null || checkedIPs[_cInfo.ip].last.AddHours(ipHubRecheck) > DateTime.UtcNow))
                        IPhub.CheckIPdata(_cInfo);
                    else IPHubResponse.Check(_cInfo, IPhub.ProcessIpHubResponse);
                }
                catch (Exception e) { log.Error($"CheckPlayer.IPHub reported: {e}"); }
            }

            if (_cInfo.PlatformId.PlatformIdentifier == EPlatformIdentifier.Steam 
                && steamFamilyShare 
                && CF_Whitelist.FamilyShare(_cInfo))
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

                        Ban(_cInfo, banReasonFamilySharing, "User Account");
                    }
                }
                catch (Exception e) { log.Error($"CheckPlayer.FamilyShare reported: {e}"); }
            }

            if (_cInfo.PlatformId.PlatformIdentifier == EPlatformIdentifier.Steam 
                && !string.IsNullOrEmpty(steamToken) 
                && (steamVacBan || steamCommunityBan || steamGameBan) && CF_Whitelist.Steam(_cInfo))
            {
                SteamVacBanResponse.Check(_cInfo, Steam.ProcesssVacBanWebResponse);
            }
        }
        public static bool isBanning = false;
        public static bool Ban(ClientInfo _cInfo, string _reason, string _details)
        {
            if(isBanning) 
                return false;

            log.Log($"Banned {CF_Player.GetNameAndPlatformId(_cInfo)} for reason: {_reason} details: {_details} IP: {_cInfo.ip}");

            isBanning = true;
            GameManager.Instance.adminTools.Blacklist.AddBan(_cInfo.playerName, _cInfo.PlatformId, DateTime.UtcNow.AddYears(10), _reason);
            isBanning = false; 

            GameUtils.KickPlayerForClientInfo(_cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, _customReason: _reason));
            return true;
        }
        public static bool BanIP(string _ip)
        {
            if (checkedIPs.ContainsKey(_ip))
            {
                checkedIPs[_ip].ipBan = true;
                checkedIPs[_ip].last = DateTime.UtcNow;

                log.Log($"IP: {_ip} was already banned");
                return true;
            }
            else checkedIPs.Add(_ip, new CheckedIP());

            checkedIPs[_ip].ipBan = false;
            checkedIPs[_ip].last = DateTime.UtcNow;

            log.Log($"IP: {_ip} banned.");
                                                                               
            SaveIPdata();                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           

            return true;
        }
        public static bool BanIP(ClientInfo _cInfo)
        {
            if (checkedIPs.ContainsKey(_cInfo.ip))
            {
                checkedIPs[_cInfo.ip].ipBan = true;
                checkedIPs[_cInfo.ip].last = DateTime.UtcNow;

                log.Log($"IP: {_cInfo.ip} was already banned");
                return true;
            }                                                                                                                                                                                                                      
            else checkedIPs.Add(_cInfo.ip, new CheckedIP());                                                                                                                                                                              

            checkedIPs[_cInfo.ip].ipBan = false;
            checkedIPs[_cInfo.ip].last = DateTime.UtcNow;

            log.Log($"IP: {_cInfo.ip} banned. From Player: {_cInfo}");

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
            catch (Exception e) { log.Error($"AnsInWhitelist reported: {e.Message}"); }

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
            catch (Exception e) { log.Error($"AnsInWhitelist reported: {e.Message}"); }

            return false;
        }
    }
}

