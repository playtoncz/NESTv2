using System.Text;
using System.Text.RegularExpressions;

namespace NestFormat;

/// <summary>Načtení XML souboru s respektováním deklarace encoding (včetně windows-1250) a BOM.</summary>
public static class XmlFileEncoding
{
    private static readonly Regex EncodingDecl = new(
        @"encoding\s*=\s*[""']([^""']+)[""']",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    static XmlFileEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>Načte celý text souboru podle BOM, XML deklarace encoding, nebo heuristiky (UTF-8 vs Windows-1250).</summary>
    public static string ReadAllText(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length == 0)
            throw new InvalidOperationException("Soubor je prázdný.");

        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Encoding.Unicode.GetString(bytes);
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return Encoding.BigEndianUnicode.GetString(bytes);
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return Encoding.UTF8.GetString(bytes);

        var headerLength = Math.Min(bytes.Length, 1024);
        var asciiHeader = Encoding.ASCII.GetString(bytes, 0, headerLength);
        if (asciiHeader.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
        {
            var encMatch = EncodingDecl.Match(asciiHeader);
            if (encMatch.Success)
            {
                var encName = encMatch.Groups[1].Value.Trim();
                try
                {
                    var enc = Encoding.GetEncoding(encName);
                    var text = enc.GetString(bytes);
                    static int CountReplacementChars(string s) => s.Count(ch => ch == '\uFFFD');
                    var replDeclared = CountReplacementChars(text);
                    var utf8Text = Encoding.UTF8.GetString(bytes);
                    var replUtf8 = CountReplacementChars(utf8Text);
                    if (replUtf8 < replDeclared)
                        return utf8Text;
                    return text;
                }
                catch
                {
                    // neznámé kódování – pokračuj heuristikou
                }
            }
        }

        var utf8Candidate = Encoding.UTF8.GetString(bytes);
        static int CountRepl(string s) => s.Count(ch => ch == '\uFFFD');
        var replUtf8Only = CountRepl(utf8Candidate);
        if (replUtf8Only == 0)
            return utf8Candidate;

        try
        {
            var win1250 = Encoding.GetEncoding(1250);
            var win1250Text = win1250.GetString(bytes);
            if (CountRepl(win1250Text) < replUtf8Only)
                return win1250Text;
        }
        catch
        {
            // ignoruj
        }

        return utf8Candidate;
    }
}
