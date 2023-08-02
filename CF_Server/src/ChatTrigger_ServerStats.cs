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
            var stats = GetEntityStats();

            CF_Player.Message("== SERVER STATS ==", _cInfo);
            CF_Player.Message($"Bloodmoon: Day {bmDayRef(GameManager.Instance.World.aiDirector.BloodMoonComponent)}", _cInfo);
            CF_Player.Message($"Ent: {Entity.InstanceCount} Ply: {GameManager.Instance.World.Players.list.Count}", _cInfo);
            CF_Player.Message($"Zom: {stats.Zombies} Ani: {stats.Animals} Items: {EntityItem.ItemInstanceCount}", _cInfo);
            CF_Player.Message($"Veh: {stats.Vehicles + stats.Gyro} Gyro: {stats.Gyro} Air: {stats.SupplyCrates}", _cInfo);
            CF_Player.Message($"Uptime: {(DateTime.UtcNow - serverStarted).ToString(@"hh\:mm\:ss")}", _cInfo);
            CF_Player.Message($"Fps | Delta: {CF_ServerMonitor.CurrentFPS} | {Time.deltaTime:F2}ms", _cInfo);
            CF_Player.Message($"Avg FPS (1m|10m): {CF_ServerMonitor.GetAverageFPS(TimeSpan.FromMinutes(1)):F2} | {CF_ServerMonitor.GetAverageFPS(TimeSpan.FromMinutes(10)):F2}", _cInfo);
            CF_Player.Message($"95th percentile FPS (1m|10m): {CF_ServerMonitor.Get95thPercentileFPS(TimeSpan.FromMinutes(1)):F2} | {CF_ServerMonitor.Get95thPercentileFPS(TimeSpan.FromMinutes(10)):F2}", _cInfo);
            CF_Player.Message($"99th percentile FPS (1m|10m): {CF_ServerMonitor.Get99thPercentileFPS(TimeSpan.FromMinutes(1)):F2} | {CF_ServerMonitor.Get99thPercentileFPS(TimeSpan.FromMinutes(10)):F2}", _cInfo);
            CF_Player.Message($"Peak memory usage: {(float)System.Diagnostics.Process.GetCurrentProcess().PeakWorkingSet64 / (1024 * 1024):F2} MB", _cInfo);
        }

        private static EntityStats GetEntityStats()
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

            return new EntityStats
            {
                Zombies = zombies,
                Animals = animals,
                Vehicles = vehicles,
                Gyro = gyro,
                SupplyCrates = supplyCrates
            };
        }

        private struct EntityStats
        {
            public int Zombies;
            public int Animals;
            public int Vehicles;
            public int Gyro;
            public int SupplyCrates;
        }
    }
}
