﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CF_Zone
{
    public string UniqueName { get; }
    public string pointA { get; }
    public string pointB { get; }
    public int heightFrom { get; set; } = -1;
    public int heightTo { get; set; } = -1;
    public List<CF_ZoneClass> classes { get; } = new List<CF_ZoneClass>();
    public List<string> ExclusionZones { get; } = new List<string>();
    public string group { get; } = "";

    public CF_Zone(string _uniqueName, string _pointA, string _pointB)
    {
        UniqueName = _uniqueName;
        pointA = _pointA;
        pointB = _pointB;
    }
    public bool HasClass(string _className)
    {
        return classes.Where(c => c.className.Equals(_className)).Count() == 1;
    }
    void HasModule(CF_ZoneClass _class)
    {
        classes.Contains(_class);
    }
    public void AddModule(CF_ZoneClass _class)
    {
        classes.Add(_class);
    }
    public void RemoveModule(string _className)
    {
        classes.RemoveAll(c => c.Equals(_className));
    }
    public void RemoveModule(CF_ZoneClass _class)
    {
        classes.Remove(_class);
    }
    public bool GetClass(string _className, out CF_ZoneClass _class)
    {
        _class = null;
        var results = classes.Where(c => c.Equals(_className));
        if (results.Count() != 1)
            return false;

        _class = results.First();
        return true;
    }
    public void AddExclusionZone(CF_Zone _zone)
    {
        ExclusionZones.Add(_zone.UniqueName);
    }
    public void AddExclusionZone(string _name)
    {
        ExclusionZones.Add(_name);
    }
    public void RemoveExclusionZone(CF_Zone _zone)
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
}

