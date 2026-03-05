namespace NestCore.Model;

/// <summary>Výsledek inference – skóre cílů a explainability log.</summary>
public sealed class InferenceResult
{
    public List<GoalScore> Goals { get; set; } = new();
    public List<FiredRuleEntry> FiredRules { get; set; } = new();
    public List<string>? QuestionsAsked { get; set; }
    public List<string>? IntegrityViolations { get; set; }
}
