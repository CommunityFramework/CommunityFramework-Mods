using System;
using System.Collections.Generic;
public class ZoneClass
{
    public string className;
    public Dictionary<string, string> DataString { get; } = new Dictionary<string, string>();
    public Dictionary<string, int> DataInt { get; } = new Dictionary<string, int>();
    public Dictionary<string, float> DataFloat { get; } = new Dictionary<string, float>();
    public Dictionary<string, DateTime> DataDate { get; } = new Dictionary<string, DateTime>();
    public Dictionary<string, ulong> DataUlong { get; } = new Dictionary<string, ulong>();
    public Dictionary<float, Tuple<(float x, float y, float z)>> DataVec3 = new Dictionary<float, Tuple<(float x, float y, float z)>>();
    public ZoneClass(string _className)
    {
        className = _className;
    }
}