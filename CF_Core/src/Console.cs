using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Console
{
    public static void Out(string _msg)
    {
        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(_msg);
    }
}
