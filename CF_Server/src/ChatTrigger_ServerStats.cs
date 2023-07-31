using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static CF_Server.API;

namespace CF_Server
{
    internal class ChatTrigger_ServerStats
    {
        static AccessTools.FieldRef<AIDirectorBloodMoonComponent, int> bmDayRef = AccessTools.FieldRefAccess<AIDirectorBloodMoonComponent, int>("bmDay");
        public static void ServerStats(ClientInfo _cInfo, bool _adv)
        {
            int zombies = 0, animals = 0, vehicles = 0, gyro = 0, supplyCrates = 0;
            List<Entity> _entities = GameManager.Instance.World.Entities.list;
            foreach (Entity _e in _entities)
            {
                if (_e.IsClientControlled())
                    continue;

                string tags = _e.EntityClass.Tags.ToString();
                if (tags.Contains("zombie") && _e.IsAlive())
                    zombies++;
                else if (tags.Contains("animal") && _e.IsAlive())
                    animals++;
                if (_e is EntityVGyroCopter)
                    gyro++;
                else if (_e is EntityVehicle)
                    vehicles++;
                if (EntityClass.list[_e.entityClass].entityClassName == "sc_General")
                    supplyCrates++;
            }

            CF_Player.Message($"== SERVER STATS ==", _cInfo);
            CF_Player.Message($"Bloodmoon: Day {bmDayRef(GameManager.Instance.World.aiDirector.BloodMoonComponent)}", _cInfo);
            CF_Player.Message($"Ent: {Entity.InstanceCount} Ply: {GameManager.Instance.World.Players.list.Count}", _cInfo);
            //Chat.Message($"Ent: {Entity.InstanceCount} Ply: {GameManager.Instance.World.Players.list.Count}+ Raids: {Raids.raidZones.Count}", _cInfo);
            CF_Player.Message($"Zom: {zombies} Ani: {animals} Items: {EntityItem.ItemInstanceCount}", _cInfo);
            CF_Player.Message($"Veh: {vehicles + gyro} Gyro: {gyro} Air: {supplyCrates}", _cInfo);
            CF_Player.Message($"Uptime: {(DateTime.UtcNow - serverStarted).ToString(@"hh\:mm\:ss")}", _cInfo);
            //Chat.Message($"Uptime: {(DateTime.UtcNow - serverStarted).ToString(@"hh\:mm\:ss")} Players Seen: {Database.GetPlayers(true).Count}", _cInfo);
            if (ServerMonitor.FPSlist_1m.Count > 5)
            {
                if (_adv)
                {
                    CF_Player.Message($"Delta Time: {Time.deltaTime} s", _cInfo);
                    CF_Player.Message($"== FPS STATS ==", _cInfo);
                    CF_Player.Message($"10s => Avg: {ServerMonitor.FPSlist_10s.Average()} Min: {ServerMonitor.FPSlist_10s.Min()}", _cInfo);
                    CF_Player.Message($"30s => Avg: {ServerMonitor.FPSlist_30s.Average()} Min: {ServerMonitor.FPSlist_30s.Min()}", _cInfo);
                    CF_Player.Message($"1m => Avg: {ServerMonitor.FPSlist_1m.Average()} Min: {ServerMonitor.FPSlist_1m.Min()}", _cInfo);
                    CF_Player.Message($"3m => Avg: {ServerMonitor.FPSlist_3m.Average()} Min: {ServerMonitor.FPSlist_3m.Min()}", _cInfo);
                    CF_Player.Message($"5m => Avg: {ServerMonitor.FPSlist_5m.Average()} Min: {ServerMonitor.FPSlist_5m.Min()}", _cInfo);
                    CF_Player.Message($"10m => Avg: {ServerMonitor.FPSlist_10m.Average()} Min: {ServerMonitor.FPSlist_10m.Min()}", _cInfo);
                    //Chat.Message($"FPS => Avg: {ServerMonitor.FPSlist_1m.Average()} Worst: {ServerMonitor.FPSlist_1m.Min()} Lags: {FPSlist.Where(x => x < (float)FPSveryLow).Count()}", _cInfo);
                    //Utilz.Message($"FPS => Cur: {FPSlist.GetRange(Math.Max(0, FPSlist.Count - 4), FPSlist.Count - 1).ToList<float>()}", _cInfo);
                }
                else
                {
                    CF_Player.Message($"FPS last minute => Avg: {ServerMonitor.FPSlist_1m.Average()} Min: {ServerMonitor.FPSlist_1m.Min()}", _cInfo);
                }
            }
        }
    }
}
