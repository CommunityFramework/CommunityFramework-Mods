using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public class PlayerDBEntry
{
    readonly public string eosId;
    readonly public EPlatformIdentifier platform;
    readonly public string platformAuth;
    public string voteSteam;
    public string discordId;
    public string name { get; set; }
    public DateTime firstSeen { get; } = DateTime.UtcNow;
    public DateTime lastSeen { get; set; } = DateTime.UtcNow;
    public int playtime { get; set; } = 0;
    public bool isInWatchlist { get; set; } = false;
    public bool isMuted { get; set; } = false;
    public CF_PlayerAdminLog penaltyLog { get; set; } = new CF_PlayerAdminLog();
    private Dictionary<string, DateTime> cooldowns = new Dictionary<string, DateTime>();
    public PlayerDBEntry(ClientInfo _cInfo)
    {
        eosId = _cInfo.InternalId.ReadablePlatformUserIdentifier;
        name = _cInfo.playerName;

        switch (_cInfo.PlatformId.PlatformIdentifier)
        {
            case EPlatformIdentifier.Steam:
                voteSteam = _cInfo.PlatformId.ReadablePlatformUserIdentifier;
                break;
        }
    }
    public void UpdatePlaytime(int additionalPlaytime)
    {
        playtime += additionalPlaytime;
        lastSeen = DateTime.Now;
    }
    public void UpdateDiscordId(string newDiscordId)
    {
        discordId = newDiscordId;
    }
    public void Mute(bool _mute)
    {
        isMuted = _mute;
    }
    public void SetWatchlistStatus(bool _watch)
    {
        isInWatchlist = _watch;
    }
    public void SetCooldown(string key, TimeSpan duration)
    {
        var expiryTime = DateTime.UtcNow.Add(duration);
        if (cooldowns.ContainsKey(key))
        {
            cooldowns[key] = expiryTime;
        }
        else
        {
            cooldowns.Add(key, expiryTime);
        }
    }
    public bool IsOnCooldown(string key)
    {
        if (cooldowns.TryGetValue(key, out var expiryTime))
        {
            if (DateTime.UtcNow < expiryTime)
            {
                return true;
            }
            else
            {
                cooldowns.Remove(key);  // remove expired cooldowns
                return false;
            }
        }

        return false;
    }
}