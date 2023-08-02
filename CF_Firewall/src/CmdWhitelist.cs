using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CF_Firewall
{
    internal class CmdWhitelist : ConsoleCmdAbstract
    {
        protected override string[] getCommands()
        {
            return new string[] { "dk-whitelist", "wl" };
        }
        protected override string getDescription()
        {
            return "Usage:\n"
                + "wl <whitelist type> <auth> <true|false>\n"
                + "\n"
                + "Auth can be: STEAM64, IP-Address, XBL-ID & EOS-ID\n"
                + "Whitelist Types: steam, vpn, fam & country\n"
                + "\n"
                + "Example: wl vpn 765611988123123 true\n"
                ;
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 3)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Wrong number of arguments, expected 3, found: {_params.Count}");
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
                    return;
                }

                if (!bool.TryParse(_params[2], out bool allow))
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
                    return;
                }

                if (allow)
                {
                    switch (_params[0])
                    {
                        case "vpn":
                        case "v":
                            if (!CF_Whitelist.VPN(_params[1]))
                            {
                                CF_Whitelist.VPNAdd(_params[1]);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Successfully granted VPN immunity.");
                                CF_Whitelist.Save();
                            }
                            else SingletonMonoBehaviour<SdtdConsole>.Instance.Output("VAC immunity already set.");
                            break;
                        case "vac":
                        case "steam":
                        case "s":
                            if (!CF_Whitelist.Steam(_params[1]))
                            {
                                CF_Whitelist.SteamAdd(_params[1]);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Successfully granted VAC immunity.");
                                CF_Whitelist.Save();
                            }
                            else SingletonMonoBehaviour<SdtdConsole>.Instance.Output("VAC immunity already set.");
                            break;
                        case "country":
                        case "count":
                        case "c":
                            if (!CF_Whitelist.Country(_params[1]))
                            {
                                CF_Whitelist.CountryAdd(_params[1]);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Successfully granted country filter immunity.");
                                CF_Whitelist.Save();
                            }
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Country filter immunity already set.");
                            break;
                        case "family":
                        case "fam":
                        case "f":
                            if (!CF_Whitelist.FamilyShare(_params[1]))
                            {
                                CF_Whitelist.FamilyShareAdd(_params[1]);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Successfully granted family sharing immunity.");
                                CF_Whitelist.Save();
                            }
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Family sharing immunity already set.");
                            break;
                    }
                }
                else
                {
                    switch (_params[0])
                    {
                        case "vpn":
                            break;
                        case "vac":
                            break;
                        case "ping":
                            break;
                        case "country":
                            break;
                        case "fam":
                            break;
                    }
                }
            }
            catch (Exception e) { Log.Out($"Error in CmdWhitelist.Execute: {e.Message}"); }
        }
    }
}
