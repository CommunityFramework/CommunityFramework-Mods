using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CF_Format
{
    public static string PlayerNameAndPlatform(EntityPlayer _player) => PlayerNameAndPlatform(_player.entityId);
    public static string PlayerNameAndPlatform(int _entityId) => PlayerNameAndPlatform(CF_Player.GetPlayer(_entityId));
    public static string PlayerNameAndPlatform(ClientInfo _cInfo) => _cInfo == null ? "null" : $"{_cInfo.playerName} ({_cInfo.PlatformId.ReadablePlatformUserIdentifier})";
    public static string StringList(List<string> stringList)
    {
        if (stringList.Count == 1)
            return stringList[0];

        string result = "";
        if (stringList != null && stringList.Count > 0)
        {
            for (int i = 0; i < stringList.Count; i++)
            {
                if (i == stringList.Count - 1 && stringList.Count > 1) // last entry and more then 1 entry
                    result += " & " + stringList[i]; // append "&" before the last entry
                else if (i == stringList.Count - 2) // second last entry
                    result += stringList[i]; // append the second last entry
                else if (stringList.Count > 1)// all other entries
                    result += stringList[i] + ", "; // append the entry followed by a comma
            }
        }

        return result;
    }
    public static bool ItemStack(ItemStack itemStack, bool quality, bool mods, out string output)
    {
        output = null;

        if (itemStack.IsEmpty())
            return false;

        StringBuilder sb = new StringBuilder();

        sb.Append($"x{itemStack.count} {itemStack.itemValue.ItemClass.Name}");

        if (quality && itemStack.itemValue.ItemClass.HasQuality)
        {
            sb.Append($" Q: {itemStack.itemValue.Quality}");
            if (itemStack.itemValue.Seed > 0)
                sb.Append($" ({itemStack.itemValue.Seed})");
        }

        if (mods && itemStack.itemValue.Modifications != null && itemStack.itemValue.Modifications.Length > 0)
        {
            sb.Append($" Mods (");
            int count = 0;
            for (int iMod = 0; iMod < itemStack.itemValue.Modifications.Length; iMod++)
            {
                if (itemStack.itemValue.Modifications[iMod] == null || itemStack.itemValue.Modifications[iMod].IsEmpty())
                    continue;

                count++;

                if (count > 1)
                    sb.Append(count == itemStack.itemValue.Modifications.Length - 1 ? " & " : ", ");

                sb.Append(itemStack.itemValue.Modifications[iMod].ItemClass.Name);
            }
            sb.Append($")");
        }

        output = sb.ToString();

        return true;
    }
    public static string Position(TileEntity _te)
    {
        if (_te.EntityId == -1)
        {
            Vector3i pos = _te.ToWorldPos();
            return $"{pos.x} {pos.y} {pos.z}";
        }

        Entity entity = Entities.GetEntity(_te.EntityId);
        if (entity == null)
            return "null";

        return $"{entity.position.x} {entity.position.y} {entity.position.z}";
    }
    public const string RomanLetters = "MDCLXVI";
    public static readonly int[] RomanValues = { 1000, 500, 100, 50, 10, 5, 1 };
    public static string Roman(int num)
    {
        if (num < 1 || num > 3999)
        {
            throw new ArgumentOutOfRangeException(nameof(num), "Input must be between 1 and 3999.");
        }

        StringBuilder result = new StringBuilder();

        for (int i = 0; i < RomanValues.Length; i += 2)
        {
            int value = RomanValues[i];
            char letter = RomanLetters[i];

            while (num >= value)
            {
                result.Append(letter);
                num -= value;
            }
        }

        for (int i = 1; i < RomanValues.Length; i += 2)
        {
            int value = RomanValues[i];
            char letter = RomanLetters[i];

            while (num >= value)
            {
                result.Append(letter);
                num -= value;
            }
        }

        return result.ToString();
    }
    static string TimeSpanCompact(TimeSpan timeSpan, bool _days = true, bool _hours = true, bool _minutes = true, bool _seconds = true)
    {
        return $"{(_days ? (int)timeSpan.TotalDays + "d " : "")}{(_hours ? timeSpan.Hours + "h " : "")}{(_minutes ? timeSpan.Minutes + "m " : "")}{(_seconds ? timeSpan.Seconds + "s" : "")}".Trim() ?? "0s";
    }
}
