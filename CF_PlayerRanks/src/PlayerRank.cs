using System.Linq;

public class PlayerRank
{
    public string name { get; set; } = "";
    public string tags { get; set; } = "";
    public int playtime { get; set; } = 0;
    public int permission { get; set; } = 1000;
    public string colorChat { get; set; } = "";
    public string colorName { get; set; } = "";
    public string nameTag { get; set; } = "";

    public bool HasTag(string _tag) => string.IsNullOrEmpty(_tag) || tags.Trim().ToLower().Split(',').Contains(_tag.Trim().ToLower());

    public PlayerRank(string _name)
    {
        name = _name;
    }
    public PlayerRank(string _name, string _tags)
    {
        name = _name;
        tags = _tags;
    }
    public PlayerRank(string _name, string _tags, int _playtime)
    {
        name = _name;
        tags = _tags;
        playtime = _playtime;
    }
    public PlayerRank(string _name, string _tags, int _playtime, int _permission)
    {
        name = _name;
        tags = _tags;
        playtime = _playtime;
        permission = _permission;
    }
    public PlayerRank(string _name, string _colorChat, string _colorName, string _nameTag)
    {
        name = _name;
        colorChat = _colorChat;
        colorName = _colorName;
        nameTag = _nameTag;
    }
    public PlayerRank(string _name, string _tags, string _colorChat, string _colorName, string _nameTag)
    {
        name = _name;
        tags = _tags;
        colorChat = _colorChat;
        colorName = _colorName;
        nameTag = _nameTag;
    }
    public PlayerRank(string _name, string _tags, int _playtime, string _colorChat, string _colorName, string _nameTag)
    {
        name = _name;
        tags = _tags;
        playtime = _playtime;
        colorChat = _colorChat;
        colorName = _colorName;
        nameTag = _nameTag;
    }
    public PlayerRank(string _name, string _tags, int _playtime, int _permission, string _colorChat, string _colorName, string _nameTag)
    {
        name = _name;
        tags = _tags;
        playtime = _playtime;
        permission = _permission;
        colorChat = _colorChat;
        colorName = _colorName;
        nameTag = _nameTag;
    }
}
