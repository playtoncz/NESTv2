namespace NestCore.Model;

/// <summary>Příspěvek jednoho závěru při aktivaci pravidla (pro explainability).</summary>
public sealed class AppliedConclusion
{
    public string AttributeId { get; set; } = "";
    public string? PropositionId { get; set; }
    public double WeightChange { get; set; }
    public string? Reason { get; set; }
}
