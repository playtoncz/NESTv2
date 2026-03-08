namespace NestCore.Model;

/// <summary>Výsledek inference – skóre cílů a explainability log.</summary>
public sealed class InferenceResult
{
    public List<GoalScore> Goals { get; set; } = new();
    /// <summary>Všechny propozice se skóre (vstupy, mezivýstupy, cíle) pro zobrazení „All propositions“.</summary>
    public List<GoalScore> AllPropositions { get; set; } = new();
    public List<FiredRuleEntry> FiredRules { get; set; } = new();
    public List<string>? QuestionsAsked { get; set; }
    public List<string>? IntegrityViolations { get; set; }
}
