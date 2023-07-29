using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CF_Math_Vector
{
    public static string Vec3ToTp(Vector3 p) => $"{(int)p.x} {(int)p.y} {(int)p.z}";
    public static string Vec3ToTele(Vector3i p) => $"{p.x} {p.y} {p.z}";
    public static bool MakeBox(string sp1, string sp2, out Rect box)
    {
        int[] ip1 = Array.ConvertAll(sp1.Split(' '), delegate (string s) { return int.Parse(s); });
        int[] ip2 = Array.ConvertAll(sp2.Split(' '), delegate (string s) { return int.Parse(s); });

        // 1: X1 Z1 2: X2 Z2
        if (ip1.Length == 2 && ip2.Length == 2)
        {
            box = new Rect(Math.Min(ip1[0], ip2[0]), Math.Min(ip1[1], ip2[1]), Math.Abs(ip1[0] - ip2[0]), Math.Abs(ip1[1] - ip2[1]));
            return true;
        }

        // 1: X1 Y1 Z1 2: X2 Y2 Z2
        if (ip1.Length == 3 && ip2.Length == 3)
        {
            box = new Rect(Math.Min(ip1[0], ip2[0]), Math.Min(ip1[2], ip2[2]), Math.Abs(ip1[0] - ip2[0]), Math.Abs(ip1[2] - ip2[2]));
            return true;
        }

        // 1: X Z 2: Radius
        if (ip1.Length == 2 && ip2.Length == 1)
        {
            box = new Rect(ip1[0] - Math.Abs(ip2[0]), ip1[1] - Math.Abs(ip2[0]), Math.Abs(ip2[0] * 2), Math.Abs(ip2[0] * 2));
            return true;
        }

        box = new Rect();
        return true;
    }
}