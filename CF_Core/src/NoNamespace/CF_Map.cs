using System;
using System.Collections.Generic;
using UnityEngine;

public class CF_Map
{
    public static string FormatPosition(Vector3i position)
    {
        string eastWestPrefix = position.x >= 0 ? "E" : "W";
        string northSouthPrefix = position.z >= 0 ? "N" : "S";

        return $"{eastWestPrefix} {Math.Abs(position.x)} {northSouthPrefix} {Math.Abs(position.z)}";
    }
    public static string FormatPosition(Vector3 position)
    {
        string eastWestPrefix = position.x >= 0 ? "E" : "W";
        string northSouthPrefix = position.z >= 0 ? "N" : "S";

        return $"{eastWestPrefix} {Math.Abs(position.x):0} {northSouthPrefix} {Math.Abs(position.z):0}";
    }
    public static byte GetBiomeAt(Vector3i pos)
    {
        Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(pos);
        if (chunk == null)
            return 0;

        return chunk.DominantBiome;
    }
    public static PrefabInstance GetPrefabAt(int x, int z)
    {
        PrefabInstance final = null;
        using (List<PrefabInstance>.Enumerator enumerator = (GameManager.Instance).GetDynamicPrefabDecorator().GetDynamicPrefabs().GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                PrefabInstance current = enumerator.Current;
                Vector3i boundingBoxPosition = (Vector3i)current.boundingBoxPosition;
                Vector3i vector3i = new Vector3i((int)(current.boundingBoxPosition.x + current.boundingBoxSize.x), (int)(current.boundingBoxPosition.y + current.boundingBoxSize.y), (int)(current.boundingBoxPosition.z + current.boundingBoxSize.z));
                int num1 = boundingBoxPosition.x;
                int num2 = boundingBoxPosition.z;
                int num3 = vector3i.x;
                int num4 = vector3i.z;
                if (x > num1 && x < num3 && z > num2 && z < num4)
                {
                    final = current;
                    break;
                }
            }
        }
        return final;
    }
    public static bool WithinPrefabRange(int x, int z, int radius, int minTier = -1)
    {
        if (radius < 0)
            return false;

        bool found = false;

        using (List<PrefabInstance>.Enumerator enumerator = GameManager.Instance.GetDynamicPrefabDecorator().GetDynamicPrefabs().GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                PrefabInstance current = enumerator.Current;

                if (minTier != -1 && current.prefab.DifficultyTier <= minTier)
                    continue;

                Vector3i boundingBoxPosition = (Vector3i)current.boundingBoxPosition;
                Vector3i vector3i = new Vector3i((int)(current.boundingBoxPosition.x + current.boundingBoxSize.x), (int)(current.boundingBoxPosition.y + current.boundingBoxSize.y), (int)(current.boundingBoxPosition.z + current.boundingBoxSize.z)); ;
                int num1 = boundingBoxPosition.x - radius;
                int num2 = boundingBoxPosition.z - radius;
                int num3 = vector3i.x + radius;
                int num4 = vector3i.z + radius;
                if (x > num1 && x < num3 && z > num2 && z < num4)
                {
                    found = true;
                    break;
                }
            }
        }
        return found;
    }
    public static void GetChunkAndRegion(Vector3i _pos, out Vector2i cPos, out Vector2i rPos)
    {
        cPos = new Vector2i(_pos.x >> 4, _pos.z >> 4);
        rPos = new Vector2i(cPos.x >> 5, cPos.y >> 5);
    }
    public static string GetCompassDirectionVerticallyOnly(double x, double y)
    {
        string direction;
        if (y > 0)
            direction = "North";
        else direction = "South";

        return direction;
    }
    public static string GetCompassDirection(double x, double y)
    {
        string direction;
        if (y > 0)
        {
            if (x > 0)
                direction = "North-East";
            else direction = "North-West";
        }
        else
        {
            if (x > 0)
                direction = "South-East";
            else direction = "South-West";
        }

        return direction;
    }
    public static string GetCompassDirectionEx(double x, double y)
    {
        double angle = Math.Atan2(y, x) * 180 / Math.PI;
        if (angle < 0)
            angle += 360;

        string direction;
        if (angle >= 11.25 && angle < 33.75)
            direction = "NNE";
        else if (angle >= 33.75 && angle < 56.25)
            direction = "NE";
        else if (angle >= 56.25 && angle < 78.75)
            direction = "ENE";
        else if (angle >= 78.75 && angle < 101.25)
            direction = "E";
        else if (angle >= 101.25 && angle < 123.75)
            direction = "ESE";
        else if (angle >= 123.75 && angle < 146.25)
            direction = "SE";
        else if (angle >= 146.25 && angle < 168.75)
            direction = "SSE";
        else if (angle >= 168.75 && angle < 191.25)
            direction = "S";
        else if (angle >= 191.25 && angle < 213.75)
            direction = "SSW";
        else if (angle >= 213.75 && angle < 236.25)
            direction = "SW";
        else if (angle >= 236.25 && angle < 258.75)
            direction = "WSW";
        else if (angle >= 258.75 && angle < 281.25)
            direction = "W";
        else if (angle >= 281.25 && angle < 303.75)
            direction = "WNW";
        else if (angle >= 303.75 && angle < 326.25)
            direction = "NW";
        else if (angle >= 326.25 && angle < 348.75)
            direction = "NNW";
        else direction = "N";

        return direction;
    }
    public static string GetSectorName(int x, int y, int maxRadius)
    {
        // Calculate the sector row and column based on the given x, y, and maxRadius.
        int sectorRow = (y + maxRadius) / 512 + 1;
        int sectorColumn = (x + maxRadius) / 512 + 1;

        // Convert the sector row number to a letter.
        char sectorRowLetter = (char)('A' + sectorRow - 1);

        // Convert the sector column number to a string.
        string sectorColumnString = sectorColumn.ToString();

        // Combine the sector row letter and column string to form the sector name.
        string sectorName = sectorRowLetter.ToString() + sectorColumnString;

        return sectorName;
    }
}
