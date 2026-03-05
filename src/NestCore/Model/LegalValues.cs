namespace NestCore.Model;

/// <summary>Právní hodnoty atributu – pro numeric dolní/horní mez, pro multiple/single seznam hodnot (value z propositions).</summary>
public sealed class LegalValues
{
    public double? LowerBound { get; set; }
    public double? UpperBound { get; set; }
    public List<string>? Values { get; set; }
}
