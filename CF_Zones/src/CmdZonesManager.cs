using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CF_ZonesManager
{
    internal class CmdZonesManager : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "This commands provides various tools to manage zones on your server.";
        }
        protected override string[] getCommands()
        {
            return new string[] { "zonemanager", "zm" };
        }
        protected override string getDescription()
        {
            return "Usage:\n "; // TODO
        }
        public static Vector3 point1 = Vector3.zero;
        public static Vector3 point2 = Vector3.zero;
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if(_senderInfo.RemoteClientInfo == null)
                {
                    CF_Console.Out($"Can only be executed by a player.");
                    return;
                }
                EntityPlayer player = CF_Player.GetEntityPlayer(_senderInfo.RemoteClientInfo);
                if (player == null || player.IsDead())
                {
                    CF_Console.Out($"You have to be alive to use this.");
                    return;
                }

                string lower = _params[0].ToLower();
                switch (lower)
                {
                    case "p1":
                        point1 = player.position;
                        CF_Console.Out($"Position for point 1 set.");
                        break;
                    case "p2":
                        point2 = player.position;
                        CF_Console.Out($"Position for point 2 set.");
                        break;
                    case "create":
                        if (point1.Equals(Vector3.zero))
                        {
                            CF_Console.Out($"Point 1 is not set.");
                            break;
                        }
                        if (point2.Equals(Vector3.zero))
                        {
                            CF_Console.Out($"Point 2 is not set.");
                            break;
                        }

                        //ZoneManager.AddZone("");


                        CF_Console.Out($"Position for point 2 set.");
                        break;
                }






                /*
                if (_params.Count != 2)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Wrong number of arguments, expected 2, found: {_params.Count}");
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
                    return;
                }

                EntityPlayer player = null;
                ClientInfo cInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.GetForNameOrId(_params[0]);
                if (cInfo == null)
                    return;

                player = GameManager.Instance.World.Players.dict[cInfo.entityId];

                if (player == null)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Playername or entity id not found.");
                    return;
                }

                if (_params[1].Equals("clear"))
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"clear {_params[0]}");
                    foreach (OwnedEntityData ownedEntity in player.GetOwnedEntities())
                    {
                        if (ownedEntity.ClassId != -1 && EntityClass.list[ownedEntity.ClassId].entityClassName == "entityJunkDrone")
                        {
                            player.RemoveOwnedEntity(ownedEntity);
                            PlayerDataFile latestPlayerData = cInfo.latestPlayerData;
                            if (latestPlayerData.bModifiedSinceLastSave)
                                latestPlayerData.Save(GameIO.GetPlayerDataDir(), cInfo.InternalId.CombinedString);
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"found a entityJunkDrone and removed it, player kicked.");
                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerDenied>().Setup(new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick)));
                            return;
                        }
                    }
                }
                */
            }
            catch (Exception e) { Log.Out($"Error in CmdIPban.Execute: {e.Message}"); }
        }
    }
}
