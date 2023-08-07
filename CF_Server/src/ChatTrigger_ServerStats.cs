using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static CF_Server.API;
using static CF_ServerMonitor;

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

            if (_adv)
                CF_Player.Message($"Entities: {Entity.InstanceCount} Players: {GameManager.Instance.World.Players.list.Count}", _cInfo);
            else CF_Player.Message($"Players: {GameManager.Instance.World.Players.list.Count}", _cInfo);

            if (_adv)
                CF_Player.Message($"Zombies: {stats.Zombies} Animals: {stats.Animals} Items: {EntityItem.ItemInstanceCount}", _cInfo);
            else CF_Player.Message($"Zombies: {stats.Zombies} Animals: {stats.Animals}", _cInfo);

            if (_adv)
                CF_Player.Message($"Veh: {stats.Vehicles + stats.Gyro} Gyro: {stats.Gyro} Air: {stats.SupplyCrates}", _cInfo);

            CF_Player.Message($"Uptime: {DateTime.UtcNow - serverStarted:hh\\:mm\\:ss}", _cInfo);
            CF_Player.Message($"Avg FPS (1m|10m): {GetAverageFPS(TimeSpan.FromMinutes(1)):F2} | {GetAverageFPS(TimeSpan.FromMinutes(10)):F2}", _cInfo);

            if (_adv)
            {
                CF_Player.Message($"95th perc. FPS (1m|10m): {Get95thPercentileFPS(TimeSpan.FromMinutes(1)):F2} | {Get95thPercentileFPS(TimeSpan.FromMinutes(10)):F2}", _cInfo);
                CF_Player.Message($"99th perc. FPS (1m|10m): {Get99thPercentileFPS(TimeSpan.FromMinutes(1)):F2} | {Get99thPercentileFPS(TimeSpan.FromMinutes(10)):F2}", _cInfo);
                CF_Player.Message($"Fps | Delta: {CurrentFPS:F2} | {(Time.deltaTime * 1000f):F2}ms Heap: {Stats[(int)EnumSS.Heap]}MB", _cInfo);
            }
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
