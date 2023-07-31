using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CF_Server.API;

internal class RestartVoting
{
    public static Dictionary<int, DateTime> votes = new Dictionary<int, DateTime>();
    public static void ClearVotes()
    {
        votes.Clear();
    }
    public static void CleanupVotes()
    {
        foreach (KeyValuePair<int, DateTime> kv in new Dictionary<int , DateTime>(votes))
        {
            int entityId = kv.Key;
            ClientInfo cInfo = CF_Player.GetClient(entityId);
            if(cInfo == null)
            {
                RemoveVote(entityId);
                continue; // Disconnected
            }

            if(restartVoteExpire < 1)
                continue; // Infinite 

            if(kv.Value.AddMinutes(restartVoteExpire) > DateTime.UtcNow)
                continue; // Not expired yet

            // Expired
            RemoveVote(entityId);
            CF_Player.Message($"Your restart vote expired.", cInfo);
        }
    }
    public static void OnVoteRestart(ClientInfo _cInfo, string _trigger, List<string> args)
    {
        AddVote(_cInfo);
    }
    public static void AddVote(ClientInfo _cInfo)
    {
        if (HasVoted(_cInfo))
        {
            CF_Player.Message($"You voted already, you can vote again when your vote expired.", _cInfo);
            return;
        }

        if((DateTime.UtcNow-serverStarted).TotalMinutes < restartVoteMinUptime)
        {
            CF_Player.Message($"Server need to be up for at least {restartVoteMinUptime} minutes to be able to vote for a server restart.", _cInfo);
            return;
        }

        if (RestartManager.Restarting())
        {
            CF_Player.Message($"Server is already restarting.", _cInfo);
            return;
        }

        votes.Add(_cInfo.entityId,DateTime.UtcNow);

        int votesLeft = GetVotesRequired();

        CF_Player.Message($"{_cInfo.playerName} has voted to restart the server. {votesLeft} votes requied. Type !rr if you like to vote for a server restart too.");

        if (restartVoteExpire > 0 && votesLeft > 0)
        {
            CF_Player.Message($"Your restart vote will expire in {restartVoteExpire} minutes.", _cInfo);
            return;
        }

        ClearVotes();
        CF_Player.Message($"{votes.Count} players voted for a server restart. Starting countdown...");
        RestartManager.Restart(restartVoteCountdown, "Restart Voted");
    }
    public static bool HasVoted(ClientInfo _cInfo) => HasVoted(_cInfo.entityId);
    public static bool HasVoted(int _entityId) => votes.ContainsKey(_entityId);
    public static void RemoveVote(int _entityId)
    {
        votes.Remove(_entityId);
    }
    public static int GetTotalRequired()
    {
        return Math.Max((int)((float)ConnectionManager.Instance.Clients.Count * ((float)restartVoteMinVotesPerc / 100f)), restartVoteMinVotes);
    }
    public static int GetVotesRequired()
    {
        CleanupVotes();
        return Math.Max(GetTotalRequired() - votes.Count, 0);
    }
}