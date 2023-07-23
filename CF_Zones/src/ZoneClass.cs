using System.Collections.Generic;
public class ZoneClass
{
    public string className;
    public Dictionary<string, string> Data { get; } = new Dictionary<string, string>();
    public ZoneClass(string _className)
    {
        className = _className;
    }
}