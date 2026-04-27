using System.Globalization;

namespace NestCore.Model;

/// <summary>Parsování váhy nebo intervalu „min;max“ z textového vstupu (čárka i tečka).</summary>
public static class WeightInputParser
{
    /// <summary>Jedna hodnota nebo interval oddělený středníkem; při jedné hodnotě jsou min=max.</summary>
    public static bool TryParseWeightOrInterval(string? text, out double minW, out double maxW)
    {
        minW = maxW = 0;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var trimmed = text.Trim();
        var semi = trimmed.IndexOf(';');
        if (semi >= 0)
        {
            var left = NormalizeNumberPart(trimmed[..semi]);
            var right = NormalizeNumberPart(trimmed[(semi + 1)..]);
            if (!double.TryParse(left, NumberStyles.Any, CultureInfo.InvariantCulture, out minW))
                return false;
            if (!double.TryParse(right, NumberStyles.Any, CultureInfo.InvariantCulture, out maxW))
                return false;
            if (minW > maxW)
                (minW, maxW) = (maxW, minW);
            return true;
        }

        var single = NormalizeNumberPart(trimmed);
        if (!double.TryParse(single, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
            return false;
        minW = maxW = v;
        return true;
    }

    static string NormalizeNumberPart(string s)
    {
        s = s.Trim();
        if (string.IsNullOrEmpty(s))
            return s;
        return s.Replace(',', '.');
    }
}
