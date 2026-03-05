namespace NestCore.Model;

/// <summary>Fuzzy hranice pro numerickou propozici (trapezoidální membership).</summary>
public sealed class FuzzyBounds
{
    public double FuzzyLower { get; set; }
    public double CrispLower { get; set; }
    public double CrispUpper { get; set; }
    public double FuzzyUpper { get; set; }
}
