using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CF_Server
{
    internal class CmdRestart : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return $"Restart countdown in seconds, if no restart countdown is given it will abort any countdown.";
        }
        protected override string[] getCommands()
        {
            return new string[] { "restart" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count == 1)
                {
                    if (int.TryParse(_params[0], out int delay))
                    {
                        CF_RestartManager.Restart(delay, $"admin", true);
                    }
                }
                else if (_params.Count == 2)
                {
                    if (int.TryParse(_params[0], out int delay))
                    {
                        CF_RestartManager.Restart(delay, _params[1], true);
                    }
                }
                else CF_RestartManager.AbortRestart($"", true);
            }
            catch (Exception e) { Log.Out($"Error in ShutdownX.Execute: {e}"); }
        }
    }
}