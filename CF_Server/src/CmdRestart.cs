using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroRestart
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
                        RestartManager.Restart(delay, $"admin", true);
                    }
                }
                else if (_params.Count == 2)
                {
                    if (int.TryParse(_params[0], out int delay))
                    {
                        RestartManager.Restart(delay, _params[1], true);
                    }
                }
                else RestartManager.AbortRestart($"", true);
            }
            catch (Exception e) { Log.Out($"Error in ShutdownX.Execute: {e}"); }
        }
    }
}