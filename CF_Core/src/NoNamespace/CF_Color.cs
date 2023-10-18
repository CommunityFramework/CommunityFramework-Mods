using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CF_Color
{
    [JsonProperty("color")]
    private string _colorHex;

    [JsonProperty("name")]
    public string Name { get; set; }

    public CF_Color() { }

    public CF_Color(byte red, byte green, byte blue, string name)
    {
        _colorHex = $"{red:X2}{green:X2}{blue:X2}";
        Name = name;
    }
    public CF_Color(string hexCode, string name)
    {
        if (hexCode.StartsWith("#"))
        {
            hexCode = hexCode.Substring(1);
        }
        _colorHex = hexCode;
        Name = name;
    }
    public string ToHex()
    {
        return _colorHex;
    }
    public string ToBB()
    {
        return $"[{_colorHex}]";
    }
    public string ToRGBString()
    {
        var rgb = ToRGBTuple();
        return $"{rgb.Item1},{rgb.Item2},{rgb.Item3}";
    }
    public void ApplyBrightness(float factor)
    {
        var rgb = ToRGBTuple();
        byte red = (byte)Mathf.Clamp(rgb.Item1 * factor, 0, 255);
        byte green = (byte)Mathf.Clamp(rgb.Item2 * factor, 0, 255);
        byte blue = (byte)Mathf.Clamp(rgb.Item3 * factor, 0, 255);
        _colorHex = $"{red:X2}{green:X2}{blue:X2}";
    }
    public bool Equals(CF_Color other)
    {
        return other != null && _colorHex.Equals(other._colorHex) && Name.Equals(other.Name);
    }
    public static CF_Color FromRGBString(string rgbString, string name)
    {
        var components = rgbString.Split(',');
        byte red = byte.Parse(components[0]);
        byte green = byte.Parse(components[1]);
        byte blue = byte.Parse(components[2]);
        return new CF_Color(red, green, blue, name);
    }
    public static CF_Color Lerp(CF_Color start, CF_Color end, float t, string name)
    {
        var startRGB = start.ToRGBTuple();
        var endRGB = end.ToRGBTuple();
        byte red = (byte)(startRGB.Item1 + (endRGB.Item1 - startRGB.Item1) * t);
        byte green = (byte)(startRGB.Item2 + (endRGB.Item2 - startRGB.Item2) * t);
        byte blue = (byte)(startRGB.Item3 + (endRGB.Item3 - startRGB.Item3) * t);
        return new CF_Color(red, green, blue, name);
    }
    public Tuple<byte, byte, byte> ToRGBTuple()
    {
        byte red = Convert.ToByte(_colorHex.Substring(0, 2), 16);
        byte green = Convert.ToByte(_colorHex.Substring(2, 2), 16);
        byte blue = Convert.ToByte(_colorHex.Substring(4, 2), 16);
        return new Tuple<byte, byte, byte>(red, green, blue);
    }
    public Color ToUnityColor()
    {
        var rgb = ToRGBTuple();
        return new Color(rgb.Item1 / 255f, rgb.Item2 / 255f, rgb.Item3 / 255f);
    }
    public static CF_Color FromUnityColor(Color color, string name)
    {
        byte red = (byte)(color.r * 255);
        byte green = (byte)(color.g * 255);
        byte blue = (byte)(color.b * 255);
        return new CF_Color(red, green, blue, name);
    }
    public void AdjustForReadability(CF_Color backgroundColor)
    {
        var fgRGB = ToRGBTuple();
        var bgRGB = backgroundColor.ToRGBTuple();

        // Calculate luminance for both foreground and background colors
        double fgLuminance = 0.2126 * fgRGB.Item1 + 0.7152 * fgRGB.Item2 + 0.0722 * fgRGB.Item3;
        double bgLuminance = 0.2126 * bgRGB.Item1 + 0.7152 * bgRGB.Item2 + 0.0722 * bgRGB.Item3;

        // Calculate contrast ratio
        double contrastRatio = (Math.Max(fgLuminance, bgLuminance) + 0.05) / (Math.Min(fgLuminance, bgLuminance) + 0.05);

        if (contrastRatio < 4.5)
        {
            byte red = (byte)(255 - fgRGB.Item1);
            byte green = (byte)(255 - fgRGB.Item2);
            byte blue = (byte)(255 - fgRGB.Item3);
            _colorHex = $"{red:X2}{green:X2}{blue:X2}";
        }
    }
    // Method to simulate Protanopia (red-green colorblindness)
    public CF_Color ToProtanopia()
    {
        var rgb = ToRGBTuple();
        float red = (float)(0.567 * rgb.Item1 + 0.433 * rgb.Item2 + 0.0 * rgb.Item3);
        float green = (float)(0.558 * rgb.Item1 + 0.442 * rgb.Item2 + 0.0 * rgb.Item3);
        float blue = (float)(0.0 * rgb.Item1 + 0.242 * rgb.Item2 + 0.758 * rgb.Item3);

        return new CF_Color((byte)Mathf.Clamp(red, 0, 255), (byte)Mathf.Clamp(green, 0, 255), (byte)Mathf.Clamp(blue, 0, 255), Name + "_protanopia");
    }

    // Method to simulate Deuteranopia (red-green colorblindness)
    public CF_Color ToDeuteranopia()
    {
        var rgb = ToRGBTuple();
        float red = (float)(0.625 * rgb.Item1 + 0.375 * rgb.Item2 + 0.0 * rgb.Item3);
        float green = (float)(0.7 * rgb.Item1 + 0.3 * rgb.Item2 + 0.0 * rgb.Item3);
        float blue = (float)(0.0 * rgb.Item1 + 0.3 * rgb.Item2 + 0.7 * rgb.Item3);

        return new CF_Color((byte)Mathf.Clamp(red, 0, 255), (byte)Mathf.Clamp(green, 0, 255), (byte)Mathf.Clamp(blue, 0, 255), Name + "_deuteranopia");
    }

    // Method to simulate Tritanopia (blue-yellow colorblindness)
    public CF_Color ToTritanopia()
    {
        var rgb = ToRGBTuple();
        float red = (float)(0.95 * rgb.Item1 + 0.05 * rgb.Item2 + 0.0 * rgb.Item3);
        float green = (float)(0.0 * rgb.Item1 + 0.433 * rgb.Item2 + 0.567 * rgb.Item3);
        float blue = (float)(0.0 * rgb.Item1 + 0.475 * rgb.Item2 + 0.525 * rgb.Item3);

        return new CF_Color((byte)Mathf.Clamp(red, 0, 255), (byte)Mathf.Clamp(green, 0, 255), (byte)Mathf.Clamp(blue, 0, 255), Name + "_tritanopia");
    }    
    // Get complementary color
    public CF_Color GetComplementary()
    {
        var rgb = ToRGBTuple();
        byte red = (byte)(255 - rgb.Item1);
        byte green = (byte)(255 - rgb.Item2);
        byte blue = (byte)(255 - rgb.Item3);
        return new CF_Color(red, green, blue, Name + "_complementary");
    }    
    // Generate a random color
    public static CF_Color RandomColor(string name)
    {
        byte red = (byte)CF_Random.Rnd(256);
        byte green = (byte)CF_Random.Rnd(256);
        byte blue = (byte)CF_Random.Rnd(256);
        return new CF_Color(red, green, blue, name);
    }
    public string FormatMessage(string message)
    {
        return $"[{_colorHex}]{message}[-]";
    }
    private string GenerateColorFromSeed(string seed)
    {
        // Hash the name to get a pseudo-random byte array
        byte[] hash;
        using (SHA1 sha1 = new SHA1Managed())
        {
            hash = sha1.ComputeHash(System.Text.Encoding.Default.GetBytes(seed));
        }

        // Use first three bytes of the hash to create a color
        return $"{hash[0]:X2}{hash[1]:X2}{hash[2]:X2}";
    }
    private static List<(string name, byte r, byte g, byte b)> specialColors = new List<(string, byte, byte, byte)>
    {
        ("Brown", 139, 69, 19),
        ("Light Brown", 181, 101, 29),
        ("Dark Brown", 101, 67, 33),
        ("Crimson", 220, 20, 60),
        ("Navy", 0, 0, 128),
        ("Maroon", 128, 0, 0),
        ("Pink", 255, 192, 203),
        ("Olive", 128, 128, 0),
        ("Tan", 210, 180, 140),
        ("Slate", 112, 128, 144),
        ("Ivory", 255, 255, 240),
        ("Burgundy", 128, 0, 32),
        ("Cyan", 0, 255, 255),
        ("Magenta", 255, 0, 255),
        ("Ochre", 204, 119, 34),
        ("Chartreuse", 127, 255, 0),
        ("Indigo", 75, 0, 130),
        ("Fuchsia", 255, 0, 255),
        ("Turquoise", 64, 224, 208),
        ("Coral", 255, 127, 80),
        ("Lavender", 230, 230, 250),
        ("Beige", 245, 245, 220),
        ("Mint", 189, 252, 201),
        ("Salmon", 250, 128, 114),
        ("Peach", 255, 218, 185),
        ("Teal", 0, 128, 128),
        ("Mustard", 255, 219, 88),
        ("Plum", 221, 160, 221),
        ("Periwinkle", 204, 204, 255),
        ("Gold", 255, 215, 0),
        ("Rose", 255, 0, 127),
        ("Aqua", 0, 255, 255),
        ("Lime", 0, 255, 0),
        ("Violet", 238, 130, 238),
        ("Apricot", 251, 206, 177),
        ("Emerald", 0, 128, 0),
        ("Amethyst", 153, 101, 21),
        ("Saffron", 244, 196, 48),
        ("Mauve", 224, 176, 255),
        ("Wheat", 245, 222, 179),
        ("Orchid", 218, 112, 214),
        ("Pumpkin", 255, 117, 24)
    };

    static readonly string[] shades = { "Deep", "Soft", "Pure", "Bright", "Vivid" };
    static readonly string[] redNames = { "Crimson", "Ruby", "Cherry" };
    static readonly string[] greenNames = { "Emerald", "Jade", "Lime" };
    static readonly string[] blueNames = { "Azure", "Cobalt", "Teal" };
    static readonly string[] grayNames = { "Charcoal", "Gray", "Silver" };
    static readonly string[] nuances = { "ish", "ful", "esque", "like", "oid" };
    private string GenerateNameFromRGB(byte red, byte green, byte blue)
    {
        // Declare variables outside the loop to avoid reinitialization
        int tempR, tempG, tempB, distance;

        foreach (var color in specialColors)
        {
            // Access the tuple's fields directly
            string name = color.name;

            // Use temporary variables for intermediate calculations
            tempR = Math.Abs(Math.Abs(red) - color.r);
            tempG = Math.Abs(Math.Abs(green) - color.g);
            tempB = Math.Abs(Math.Abs(blue) - color.b);

            // Distance
            distance = Math.Abs(Math.Abs(red) - color.r) + Math.Abs(Math.Abs(green) - color.g) + Math.Abs(Math.Abs(blue) - color.b);
            if (distance < 50)
                return name;
        }

        // Check if the color is a shade of gray
        if (red == green && green == blue)
        {
            string grayShade = grayNames[red / 86];
            string grayIntensity = shades[red / 51];
            return $"{grayIntensity}{grayShade}";
        }

        // Determine the dominant color for non-gray colors
        byte maxColor = Math.Max(red, Math.Max(green, blue));
        string dominantColor;
        if (maxColor == red) dominantColor = redNames[red / 86];
        else if (maxColor == green) dominantColor = greenNames[green / 86];
        else dominantColor = blueNames[blue / 86];

        // Determine shade based on the intensity of the dominant color
        string shade = shades[maxColor / 51];

        // Determine nuance based on the average color intensity
        byte avgColor = (byte)((red + green + blue) / 3);
        string nuance = nuances[avgColor / 51];

        return $"{shade}{dominantColor}{nuance}";
    }
}
