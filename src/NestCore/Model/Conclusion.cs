namespace NestCore.Model;

/// <summary>Závěr pravidla – cíl (atribut/propozice) s vahou a volitelnou negací.</summary>
public sealed class Conclusion
{
    public string AttributeId { get; set; } = "";
    public string? PropositionId { get; set; }
    public bool Negation { get; set; }
    public double Weight { get; set; }
}
