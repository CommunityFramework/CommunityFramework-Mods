using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Zone
{
    public string UniqueName { get; }
    public Rect Box { get; }
    public List<ZoneClass> classes { get; } = new List<ZoneClass>();
    public List<string> ExclusionZones { get; } = new List<string>();

    public Zone(string uniqueName, string pointA, string pointB)
    {
        UniqueName = uniqueName;
        Box = MakeBox(pointA, pointB);
    }
    public bool HasClass(string _className)
    {
        return classes.Where(c => c.className.Equals(_className)).Count() == 1;
    }
    void HasModule(ZoneClass _class)
    {
        classes.Contains(_class);
    }
    public void AddModule(ZoneClass _class)
    {
        classes.Add(_class);
    }
    public void RemoveModule(string _className)
    {
        classes.RemoveAll(c => c.Equals(_className));
    }
    public void RemoveModule(ZoneClass _class)
    {
        classes.Remove(_class);
    }
    public bool GetClass(string _className, out ZoneClass _class)
    {
        _class = null;
        var results = classes.Where(c => c.Equals(_className));
        if (results.Count() != 1)
            return false;

        _class = results.First();
        return true;
    }
    public void AddExclusionZone(Zone _zone)
    {
        ExclusionZones.Add(_zone.UniqueName);
    }
    public void AddExclusionZone(string _name)
    {
        ExclusionZones.Add(_name);
    }
    public void RemoveExclusionZone(Zone _zone)
    {
        ExclusionZones.Remove(_zone.UniqueName);
    }
    public void RemoveExclusionZone(string _name)
    {
        ExclusionZones.Remove(_name);
    }
    public bool Inside(Vector3 _pos)
    {
        return Inside(new Vector2(_pos.x, _pos.z));
    }
    public bool Inside(Vector2 _pos)
    {
        if (!Box.Contains(_pos)) 
            return false;

        foreach (string zoneName in ExclusionZones)
        {
            ZoneManager.GetZone(zoneName, out ZoneAlive _zone);
            if (_zone.Inside(_pos)) 
                return false;
        }
        return true;
    }
    public static Rect MakeBox(string sp1, string sp2)
    {
        int[] ip1 = Array.ConvertAll(sp1.Split(' '), delegate (string s) { return int.Parse(s); });
        int[] ip2 = Array.ConvertAll(sp2.Split(' '), delegate (string s) { return int.Parse(s); });

        // 1: X1 Z1 2: X2 Z2
        if (ip1.Length == 2 && ip2.Length == 2)
        {
            return new Rect(Math.Min(ip1[0], ip2[0]), Math.Min(ip1[1], ip2[1]), Math.Abs(ip1[0] - ip2[0]), Math.Abs(ip1[1] - ip2[1]));
        }

        // 1: X1 Y1 Z1 2: X2 Y2 Z2
        if (ip1.Length == 3 && ip2.Length == 3)
        {
            return new Rect(Math.Min(ip1[0], ip2[0]), Math.Min(ip1[2], ip2[2]), Math.Abs(ip1[0] - ip2[0]), Math.Abs(ip1[2] - ip2[2]));
        }

        // 1: X Z 2: Radius
        if (ip1.Length == 2 && ip2.Length == 1)
        {
            return new Rect(ip1[0] - Math.Abs(ip2[0]), ip1[1] - Math.Abs(ip2[0]), Math.Abs(ip2[0] * 2), Math.Abs(ip2[0] * 2));
        }

        return new Rect();
    }
}

