using System;
using System.Text;

public class Misc
{
    public static void OpenUrl(ClientInfo _cInfo, string websiteURL)
    {
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("createwebuser " + Convert.ToBase64String(Encoding.UTF8.GetBytes(websiteURL)), true));
    }
}
