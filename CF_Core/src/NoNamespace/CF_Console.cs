using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class CF_Console
{
    public static CF_ConsoleMessageFilter messageFilter = new CF_ConsoleMessageFilter();

    public static void Out(string _msg)
    {
        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(_msg);
    }
    public static void OutFiltered(string _msg)
    {
        messageFilter.Out(_msg);
    }
}
