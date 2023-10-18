using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static CF_Chat.API;

namespace CF_Chat
{
    public class RankConfigJson
    {
        public List<string> Players { get; set; } = new List<string>();
        public int PermissionLevel { get; set; } = 1000;
        public int Playtime { get; set; } = 0;
        public string ChatColor { get; set; } = "";
        public string NameColor { get; set; } = "";
        public string CustomName { get; set; } = "";
        public string TagPre { get; set; } = "";
        public string TagPost { get; set; } = "";
    }
    public class RankConfig : RankConfigJson
    {
        public RankConfig()
        {

        }
        public RankConfig(RankConfigJson json)
        {
            if (json == null)
            {
                log.Error("RankConfigJson is null.");
                return;
            }

            CopyFromJson(json);

            NameTagPostRaw = TagPost != null ? CF_Format.RemoveColorCodes(TagPost) : null;
            NameTagPreRaw = TagPre != null ? CF_Format.RemoveColorCodes(TagPre) : null;
        }
        public void CopyFromJson(RankConfigJson json)
        {
            Players = json.Players;
            PermissionLevel = json.PermissionLevel;
            Playtime = json.Playtime;
            ChatColor = json.ChatColor;
            NameColor = json.NameColor;
            CustomName = json.CustomName;
            TagPre = json.TagPre;
            TagPost = json.TagPost;
        }
        public string NameTagPreRaw;
        public string NameTagPostRaw;

        public string NamePost
        {
            get => TagPost;
            set
            {
                TagPost = value;
                NameTagPostRaw = CF_Format.RemoveColorCodes(TagPost);
            }
        }
        public string NamePre
        {
            get => TagPre;
            set
            {
                TagPre = value;
                NameTagPreRaw = CF_Format.RemoveColorCodes(TagPre);
            }
        }
        public bool IsSpecificPlayer(ClientInfo cInfo)
        {
            if (Players.Count < 1)
                return true;

            return Players.Contains(cInfo.InternalId.ReadablePlatformUserIdentifier)
                || Players.Contains(cInfo.PlatformId.ReadablePlatformUserIdentifier)
                || Players.Contains(cInfo.InternalId.CombinedString)
                || Players.Contains(cInfo.PlatformId.CombinedString);
        }
    }
}
