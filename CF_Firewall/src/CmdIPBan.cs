using System;
using System.Collections.Generic;
using System.Net;

namespace CF_Firewall
{
    internal class CmdIPban : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return string.Format("IP ban commands.");
        }
        protected override string[] getCommands()
        {
            return new string[] { "ipban" };
        }
        protected override string getDescription()
        {
            return "Usage:\nipban add <IP> - Ban IP\nipban del <IP> - Unban IP";
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Wrong number of arguments, expected 2, found: {_params.Count}");
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
                    return;
                }

                if (_params[0].Equals("add"))
                {
                    if (API.checkedIPs.ContainsKey(_params[0]))
                    {
                        if (!API.checkedIPs[_params[0]].ipBan)
                        {
                            API.checkedIPs[_params[0]].ipBan = true;
                            API.SaveIPdata();
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"IP address banned.");
                        }
                        else SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"IP address already banned.");
                    }
                    else
                    {
                        API.checkedIPs.Add(_params[0], new CheckedIP());
                        API.SaveIPdata();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"IP address banned.");
                    }
                }
                else if (_params[0].Equals("del"))
                {
                    if (API.checkedIPs.ContainsKey(_params[0]))
                    {
                        if (API.checkedIPs[_params[0]].ipBan)
                        {
                            API.checkedIPs[_params[0]].ipBan = false;
                            API.SaveIPdata();
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"IP address unbanned.");
                        }
                        else SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"IP address already unbanned.");
                    }
                    else SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"IP address already unbanned.");
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Invalid command usage.");
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
                }
            }
            catch (Exception e) { Log.Out($"Error in CmdIPban.Execute: {e}"); }
        }
    }
}
