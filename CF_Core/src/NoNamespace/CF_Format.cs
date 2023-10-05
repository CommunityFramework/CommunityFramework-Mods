using Epic.OnlineServices.Presence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class CF_Format
{
    public static string ListToString(List<string> stringList)
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
    public static string DictionaryToString(Dictionary<string, int> dict)
    {
        if (dict.Count == 0)
            return string.Empty;

        if (dict.Count == 1)
            return $"{dict.Values.First()}x {dict.Keys.First()}";

        string result = "";
        var entries = dict.Select(kvp => $"{kvp.Value}x {kvp.Key}").ToList();

        for (int i = 0; i < entries.Count; i++)
        {
            if (i == entries.Count - 1 && entries.Count > 1) // last entry and more than 1 entry
                result += " & " + entries[i]; // append "&" before the last entry
            else if (i == entries.Count - 2) // second last entry
                result += entries[i]; // append the second last entry
            else if (entries.Count > 1) // all other entries
                result += entries[i] + ", "; // append the entry followed by a comma
        }

        return result;
    }
    public static void ItemStack(ItemStack itemStack, out string output, bool quality = false, bool mods = false, bool seed = false)
    {
        output = ItemStack(itemStack, quality, mods, seed);
    }
    public static string ItemStack(ItemStack itemStack, bool quality = false, bool mods = false, bool seed = false)
    {
        ItemValue itemValue = itemStack.itemValue;
        if (itemStack.IsEmpty())
            return "";

        StringBuilder sb = new StringBuilder();

        sb.Append($"x{itemStack.count} ");
        sb.Append(Item(itemValue, quality, mods, seed));

        return sb.ToString();
    }
    public static void Item(ItemValue itemValue, out string output, bool quality = false, bool mods = false, bool seed = false)
    {
        output = Item(itemValue, quality, mods, seed);
    }
    public static string Item(ItemValue itemValue, bool quality = false, bool mods = false, bool seed = false)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append($"{itemValue.ItemClass.Name}");

        if (quality && itemValue.ItemClass.HasQuality)
        {
            sb.Append($" Q: {itemValue.Quality}");
            if (seed && itemValue.Seed > 0)
                sb.Append($" ({itemValue.Seed})");
        }

        if (mods && itemValue.Modifications != null && itemValue.Modifications.Length > 0 
            && CF_Array.ContainsNonNull(itemValue.Modifications))
        {
            sb.Append($" Mods (");
            int count = 0;
            ItemValue[] itemMods = itemValue.Modifications;
            for (int iMod = 0; iMod < itemMods.Length; iMod++)
            {
                if (itemMods[iMod] == null || itemMods[iMod].IsEmpty())
                    continue;

                count++;

                if (count > 1)
                    sb.Append(count == itemMods.Length - 1 ? " & " : ", ");

                sb.Append(itemMods[iMod].ItemClass.Name);
            }
            sb.Append($")");
        }

        return sb.ToString();
    }
    public static string Position(TileEntity _te)
    {
        if (_te.EntityId == -1)
        {
            Vector3i pos = _te.ToWorldPos();
            return $"{pos.x} {pos.y} {pos.z}";
        }

        Entity entity = CF_Entity.GetEntity(_te.EntityId);
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
    public static string TimeSpanCompact(TimeSpan timeSpan, bool _days = true, bool _hours = true, bool _minutes = true, bool _seconds = true)
    {
        return $"{(_days ? (int)timeSpan.TotalDays + "d " : "")}{(_hours ? timeSpan.Hours + "h " : "")}{(_minutes ? timeSpan.Minutes + "m " : "")}{(_seconds ? timeSpan.Seconds + "s" : "")}".Trim() ?? "0s";
    }
    public static string RemoveColorCodes(string input)
    {
        return Regex.Replace(input, @"\[\w{6}\]", "");
    }
}
