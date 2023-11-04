using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using CF_Core;
using HarmonyLib;
using System.Runtime.InteropServices;

namespace CF_Teams
{
    public class API : IModApi
    {
        public static CF_Mod mod = new CF_Mod("CF_Teams", OnConfigLoaded);
        public static CF_Log log = new CF_Log("CF_Teams");
        public static Harmony harmony = new Harmony("CF_Teams");
        public void InitMod(Mod _modInstance)
        {
            mod.Activate(true);
            harmony.PatchAll();
            CF_Timer.AddOneSecTimer(CF_TeamManager.PeriodicUpdate, "CF_Teams", false);
            CF_TeamManager.LoadData();
            CF_ChatManager.RegisterChatTrigger("g,group,groups,clan,ally", CF_TeamManager.ExecCommand);

            CreateCvars();
        }
        public static string cvTeamSize;
        public static string cvCachedTeamSize;
        public static int teamUpdateInterval;
        public static TimeSpan teamSizeCacheDuration;
        public static bool allCanInvite;
        public static int groupLimit;

        public static int partyLimit;

        public static string cvarAllyCountName;
        public static string cvarPartyCountName;
        public static string cvarTeamCountName;

        public static void OnConfigLoaded()
        {
            mod.AddSetting("ClanMembers_CanInivte", true, "Are normal members if a clan allowed to invite others?", out allCanInvite);
            mod.AddSetting("TeamSize_Interval", 10, 1, 3600, "Interval in seconds for team updates.", out teamUpdateInterval);
            mod.AddSetting("TeamSize_CvarName", "cfTeamSize", "", "Cvar name used to store the cached team size.", out cvTeamSize);
            mod.AddSetting("TeamSize_Cache_CvarName", "cfTeamSize", "", "Cvar name used to store the cached team size.", out cvCachedTeamSize);
            mod.AddSetting("TeamSize_Cache_Duration", 1440, 1, int.MaxValue, "Duration in minutes for team size cache.", out int duration);

            mod.AddSetting("Cache_Ally_Cvar", "", "", "Cvar name to represent ally count", out cvarAllyCountName);
            mod.AddSetting("Cache_Party_Cvar", "", "", "Cvar name to represent party count", out cvarPartyCountName);
            mod.AddSetting("Cache_Team_Cvar", "TeamCount", "", "Cvar name to represent combined party and team count max count", out cvarTeamCountName);

            mod.AddSetting("TeamSize_Max", 8, -1, 99, "Max ally group size", out groupLimit);
            mod.AddSetting("PartySize_Max", 8, -1, 99, "Max party size", out partyLimit);
            teamSizeCacheDuration = TimeSpan.FromHours(duration);
        }
        // Team Size cVars
        public static CF_CustomCvar cvarAllyCount = null;
        public static CF_CustomCvar cvarPartyCount = null;
        public static CF_CustomCvar cvarTeamCount = null;
        public static void CreateCvars()
        {
            if (cvarAllyCount == null && !string.IsNullOrEmpty(cvarAllyCountName))
                cvarAllyCount = new CF_CustomCvar(cvarAllyCountName, 0f);
            if (cvarPartyCount == null && !string.IsNullOrEmpty(cvarPartyCountName))
                cvarPartyCount = new CF_CustomCvar(cvarPartyCountName, 0f);
            if (cvarTeamCount == null && !string.IsNullOrEmpty(cvarTeamCountName))
                cvarTeamCount = new CF_CustomCvar(cvarTeamCountName, 0f, OnUpdateTeamCount, 30);
        }
        public static void OnUpdateTeamCount(EntityPlayer player, CF_CustomCvar.CustomCvarReturn ret)
        {
            CF_TeamManager.GetAllyMembers(player.entityId, out Dictionary<string, int> allies);
            CF_TeamManager.GetPartyMembers(player.entityId, out Dictionary<string, int> party);

            int newTeamSize = Math.Max(1, Math.Max(allies.Count + 1, party.Count));

            log.Debug($"OnUpdateTeamCount: {player.entityId} T1: {allies.Count} T2: {party.Count} Old: {ret.value} New: {newTeamSize}");

            cvarAllyCount?.UpdatePlayer(player, Math.Max(1, allies.Count + 1));
            cvarPartyCount?.UpdatePlayer(player, Math.Max(1, party.Count));
            ret.value = newTeamSize;
        }
    }
}