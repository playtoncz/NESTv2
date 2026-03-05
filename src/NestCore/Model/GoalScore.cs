namespace NestCore.Model;

/// <summary>Skóre jednoho cílového výroku (atribut + propozice) po inference.</summary>
public sealed class GoalScore
{
    public string AttributeId { get; set; } = "";
    public string? PropositionId { get; set; }
    public string DisplayName { get; set; } = "";
    public double MinWeight { get; set; }
    public double MaxWeight { get; set; }
    public string Status { get; set; } = "untouched";
    public string Type { get; set; } = "goal";
    public List<FiredRuleEntry> ContributingRules { get; set; } = new();
}
