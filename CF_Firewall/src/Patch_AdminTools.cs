using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Platform;
using Platform.Steam;
using HarmonyLib;
using System.Collections.ObjectModel;
using LiteNetLib;

using static CF_Firewall.API;
using System.Security.Cryptography;

namespace CF_Firewall
{
    internal class Patch_AdminTools
    {
        [HarmonyPatch(typeof(AdminBlacklist), "AddBan")]
        public class AddBan
        {
            static void Prefix(AdminBlacklist __instance, string _name, PlatformUserIdentifierAbs _identifier, DateTime _banUntil, string _banReason)
            {
                if (!banIP)
                    return;

                ClientInfo cInfo = Players.GetClient(_identifier);
                if (cInfo == null)
                    return;

                Ban(cInfo.ip);
            }
        }
    }
}
