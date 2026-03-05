namespace NestCore.Model;

/// <summary>Záznam o aktivovaném pravidle (pro explainability).</summary>
public sealed class FiredRuleEntry
{
    public string RuleId { get; set; } = "";
    public double ConditionScore { get; set; }
    public List<AppliedConclusion> AppliedConclusions { get; set; } = new();
}
