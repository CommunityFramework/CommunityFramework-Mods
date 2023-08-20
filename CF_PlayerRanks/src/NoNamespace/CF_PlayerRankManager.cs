using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using static CF_PlayerRanks.API;

public class CF_PlayerRankManager
{
    public static List<CF_PlayerRank> ranks = new List<CF_PlayerRank>();
    public static CF_PlayerRank rankDefault = new CF_PlayerRank("Default", "default");
    private static FileSystemWatcher FileWatcher = null;
    private static bool fileWatcher = false;
    public static void Init()
    {
        LoadFile();

        if (FileWatcher == null)
            FileWatcher = new FileSystemWatcher(filePathPlayerRanks, "PlayerRanks.json");

        if (!fileWatcher)
        {
            FileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.EnableRaisingEvents = true;
            fileWatcher = true;
        }

        CF_ChatManager.RegisterHandler(OnChatMessage);
    }
    public static void OnChatMessage(ClientInfo _cInfo, CF_ChatMessage _chatMsg)
    {
        if (!GetCurrentRank(out CF_PlayerRank _rank, (int)_cInfo.latestPlayerData.totalTimePlayed, CF_Player.GetPermission(_cInfo), "chat"))
            return;

        if (!string.IsNullOrEmpty(_rank.colorChat))
        {
            _chatMsg.msg = _rank.colorChat + _chatMsg.msg;
        }

        if (!string.IsNullOrEmpty(_rank.nameTag))
        {
            _chatMsg.name = _rank.nameTag + _chatMsg.msg;
        }
        else if (!string.IsNullOrEmpty(_rank.colorName))
        {
            _chatMsg.name = _rank.colorName + _chatMsg.name;
        }

    }
    public static bool LoadFile()
    {
        if (!Directory.Exists(mod.modDatabasePath))
            Directory.CreateDirectory(mod.modDatabasePath);

        if (!File.Exists(filePathPlayerRanks))
        {
            AddDefaultGroups();
            SaveToFile();
        }

        try
        {
            ranks = JsonConvert.DeserializeObject<List<CF_PlayerRank>>(File.ReadAllText(filePathPlayerRanks));
            return true;
        }
        catch (Exception e)
        {
            x.Error($"Failed loading from {filePathPlayerRanks}: {e}");
            return false;
        }
    }
    public static void SaveToFile()
    {
        File.WriteAllText(filePathPlayerRanks, JsonConvert.SerializeObject(ranks, Formatting.Indented));
    }
    private static void OnFileChanged(object source, FileSystemEventArgs e)
    {
        if (File.Exists(filePathPlayerRanks))
            LoadFile();
        else x.Error(string.Format("Lost {0}", filePathPlayerRanks));
    }
    public static void AddDefaultGroups()
    {
        if (ranks.Count != 0)
            return;

        ranks.Add(new CF_PlayerRank("Players", "player"));
        ranks.Add(new CF_PlayerRank("Admins", "admin", 0, 0, "", "", "[ff0000][[-]ADMIN[ff0000]] "));
    }
    public static bool GetRank(string name, out CF_PlayerRank _chatGroup)
    {
        _chatGroup = ranks.FirstOrDefault(group => group.name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return _chatGroup != null;
    }
    public static bool GetCurrentRank(out CF_PlayerRank _group, int _playtime = 0, int _permission = 1000, string _tag = "")
    {
        _group = ranks
            .OrderByDescending(group => group.playtime)
            .FirstOrDefault(group => group.playtime <= _playtime && group.permission <= _permission && group.HasTag(_tag));

        if (_group != null)
            return true;

        _group = rankDefault;
        return false;
    }
    public static bool GetNextGroup(out CF_PlayerRank _group, int _playtime = 0, int _permission = 1000, string _tag = "")
    {
        _group = ranks
            .OrderBy(group => group.playtime)
            .FirstOrDefault(group => group.playtime > _playtime && group.permission <= _permission && group.HasTag(_tag));

        if (_group != null)
            return true;

        _group = rankDefault;
        return false;
    }
}
