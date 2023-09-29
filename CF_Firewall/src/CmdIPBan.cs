using System;
using System.Collections.Generic;
using System.Net;
using static CF_Firewall.API;

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
                    if (checkedIPs.ContainsKey(_params[1]))
                    {
                        if (!checkedIPs[_params[1]].ipBan)
                        {
                            checkedIPs[_params[1]].ipBan = true;
                            SaveIPdata();
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"IP address banned.");
                        }
                        else SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"IP address already banned.");
                    }
                    else
                    {
                        checkedIPs.Add(_params[1], new CheckedIP());
                        SaveIPdata();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"IP address banned.");
                    }
                }
                else if (_params[0].Equals("del"))
                {
                    if (checkedIPs.ContainsKey(_params[1]))
                    {
                        if (checkedIPs[_params[1]].ipBan)
                        {
                            checkedIPs[_params[1]].ipBan = false;
                            SaveIPdata();
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"IP address unbanned.");
                        }
                        else SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"IP address already unbanned.");
                    }
                    else SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"IP address not found.");
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Invalid command usage.");
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
                }
            }
            catch (Exception e) 
            { 
                log.Error($"CmdIPban.Execute reported: {e}"); 
            }
        }
    }
}
