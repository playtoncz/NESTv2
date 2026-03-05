using System.Globalization;
using System.Xml;

namespace NestFormat;

internal static class XmlHelper
{
    private static readonly CultureInfo CommaCulture = new("cs-CZ");

    /// <summary>Odstraní DOCTYPE z XML řetězce (kvůli kompatibilitě bez DTD). Odstraní i úvodní BOM a jakékoli znaky před prvním '&lt;'.</summary>
    public static string StripDoctype(string xml)
    {
        if (string.IsNullOrEmpty(xml)) return xml;
        var s = xml.TrimStart('\uFEFF'); // BOM
        var firstLt = s.IndexOf('<');
        if (firstLt > 0)
            s = s.Substring(firstLt);
        if (string.IsNullOrWhiteSpace(s))
            return xml;
        var i = s.IndexOf("<!DOCTYPE", StringComparison.OrdinalIgnoreCase);
        if (i >= 0)
        {
            var end = s.IndexOf('>', i);
            if (end >= 0)
                s = s.Remove(i, end - i + 1);
        }
        return s.Replace("xmlns=\"http://www.vse.cz/NEST\"", "");
    }

    public static double ParseDouble(string? value, double defaultValue = 0)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        var normalized = value.Trim().Replace(',', '.');
        return double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : defaultValue;
    }

    public static string? GetElementText(XmlElement? parent, string localName)
    {
        var el = parent?.SelectSingleNode(localName) as XmlElement;
        return el?.InnerText?.Trim();
    }
}
