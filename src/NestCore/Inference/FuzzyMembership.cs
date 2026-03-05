using NestCore.Model;

namespace NestCore.Inference;

/// <summary>Trapezoidální fuzzy membership pro numerickou propozici (dle VyrokNumeric.SpoctiVahu).</summary>
public static class FuzzyMembership
{
    /// <summary>Vypočte váhu výroku pro danou hodnotu (v rozsahu cca -1 až 1).</summary>
    public static double ComputeWeight(double value, FuzzyBounds bounds)
    {
        var fl = bounds.FuzzyLower;
        var cl = bounds.CrispLower;
        var cu = bounds.CrispUpper;
        var fu = bounds.FuzzyUpper;

        if (value >= cl && value <= cu)
            return 1;
        if (value >= fl && value < cl)
            return (2 * (value - fl) / (cl - fl)) - 1;
        if (value > cu && value <= fu)
            return (2 * (fu - value) / (fu - cu)) - 1;
        return -1;
    }
}
