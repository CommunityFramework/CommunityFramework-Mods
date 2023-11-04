using System;
using System.Collections.Generic;
using System.Linq;
using static CF_PlayerDatabase.API;

public class CF_PlayerDB
{
    public Dictionary<string, PlayerDBEntry> players = new Dictionary<string, PlayerDBEntry>();
    public PlayerDBEntry GetPlayer(string _eosId)
    {
        if (!players.ContainsKey(_eosId))
        {
            return null;
        }

        return players[_eosId];
    }
    public bool TryGetPlayer(string _eosId, out PlayerDBEntry _playerDbEntry)
    {
        if (!players.ContainsKey(_eosId))
        {
            _playerDbEntry = null;
            return false;
        }
        
        _playerDbEntry = players[_eosId];

        return true;
    }
    public bool TryGetPlayer(ClientInfo _cInfo, out PlayerDBEntry _playerDbEntry) => TryGetPlayer(_cInfo, out _playerDbEntry, false);
    public bool TryGetPlayer(ClientInfo _cInfo, out PlayerDBEntry _playerDbEntry, bool _allowCreate)
    {
        string eosId = _cInfo.InternalId.ReadablePlatformUserIdentifier;
        _playerDbEntry = null;

        if (!players.ContainsKey(eosId))
        {
            if (!_allowCreate)
            {
                _playerDbEntry = null;
                return false;
            }

            if(AddPlayer(_cInfo))
                _playerDbEntry = players[eosId];
        }
        else _playerDbEntry = players[eosId];

        return true;
    }
    public bool AddPlayer(ClientInfo _cInfo)
    {
        string playerId = _cInfo.InternalId.ReadablePlatformUserIdentifier;
        if (!players.ContainsKey(playerId))
        {
            log.Out($"Added {_cInfo}");
            players.Add(playerId, new PlayerDBEntry(_cInfo));
            return true;
        }

        return false;
    }
    public int TotalPlayers()
    {
        return players.Count;
    }
    public List<string> GetPlayerIds()
    {
        return new List<string>(players.Keys);
    }
    public static int ActivePlayersCount(IEnumerable<PlayerDBEntry> entries, TimeSpan timespan)
    {
        var currentTime = DateTime.UtcNow;
        return entries.Count(e => (currentTime - e.lastSeen) <= timespan);
    }
    public int TotalPlaytime()
    {
        return players.Values.Sum(player => player.playtime);
    }
    public static int TotalPlaytime(IEnumerable<PlayerDBEntry> entries)
    {
        return entries.Sum(e => e.playtime);
    }
    public static double AveragePlaytime(IEnumerable<PlayerDBEntry> entries)
    {
        return entries.Average(e => e.playtime);
    }
    public static TimeSpan AveragePlaytimeDuration(IEnumerable<PlayerDBEntry> entries)
    {
        return TimeSpan.FromMinutes(entries.Average(e => e.playtime));
    }
    public static int PlayersJoinedSince(IEnumerable<PlayerDBEntry> entries, TimeSpan timespan)
    {
        var ago = DateTime.UtcNow.Add(timespan);
        return entries.Count(e => e.firstSeen >= ago);
    }
    public int TotalMutedPlayers()
    {
        return players.Values.Count(player => player.isMuted);
    }
    public static int TotalMutedPlayers(IEnumerable<PlayerDBEntry> entries)
    {
        return entries.Count(e => e.isMuted);
    }
    public static int TotalPlayersInWatchlist(IEnumerable<PlayerDBEntry> entries)
    {
        return entries.Count(e => e.isInWatchlist);
    }
    public int TotalPlayersInWatchlist()
    {
        return players.Values.Count(player => player.isInWatchlist);
    }
    public void CleanupInactivePlayers(TimeSpan inactivityLimit)
    {
        var currentTime = DateTime.UtcNow;
        var inactivePlayers = players.Where(pair => currentTime - pair.Value.lastSeen > inactivityLimit)
                                     .Select(pair => pair.Key)
                                     .ToList();

        foreach (var playerId in inactivePlayers)
        {
            players.Remove(playerId);
        }
    }
    public void Wipe()
    {
        foreach (var entry in players)
        {
            entry.Value.playtime = 0;
            entry.Value.isMuted = false;
            entry.Value.isInWatchlist = false;
            entry.Value.cooldowns.Clear();
        }
    }
}