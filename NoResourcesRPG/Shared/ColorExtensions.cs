using System.Drawing;

namespace NoResourcesRPG.Shared;

public static class ColorExtensions
{
    /// <summary>
    /// Convert a Color to HTML hex (#RRGGBB).
    /// </summary>
    public static string ToHtmlHex(this Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    /// <summary>
    /// Convert a Color to CSS rgba(r,g,b,a) string.
    /// </summary>
    public static string ToCssRgba(this Color color)
    {
        return $"rgba({color.R}, {color.G}, {color.B}, {color.A / 255.0:F2})";
    }

    /// <summary>
    /// Build HTML hex (#RRGGBB) from r,g,b.
    /// </summary>
    public static string ToHtmlHex(int r, int g, int b)
    {
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    /// <summary>
    /// Build CSS rgba(r,g,b,a) from a,r,g,b.
    /// </summary>
    public static string ToCssRgba(int a, int r, int g, int b)
    {
        return $"rgba({r}, {g}, {b}, {a / 255.0:F2})";
    }


    public static Color RandomColor => GetRandom();
    public static string RandomHexColor => GetRandomHtmlHexColor();
    public static Color GetRandom(int alpha = 255)
    {
        var rng = new Random();
        return Color.FromArgb(
              alpha,  // alpha always full
              rng.Next(0, 256), // R
              rng.Next(0, 256), // G
              rng.Next(0, 256)  // B
                               );
    }
    public static string GetRandomHtmlHexColor(int alpha = 255)
    {
        return GetRandom(alpha).ToHtmlHex();
    }
}