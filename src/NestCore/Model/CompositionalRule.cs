namespace NestCore.Model;

/// <summary>Kompoziční pravidlo: IF condition THEN conclusions.</summary>
public sealed class CompositionalRule
{
    public string Id { get; set; } = "";
    public double? Priority { get; set; }
    public string? IdContext { get; set; }
    public double? ContextThreshold { get; set; }
    public double? ConditionThreshold { get; set; }

    public Condition Condition { get; set; } = new();
    public List<Conclusion> Conclusions { get; set; } = new();
    public string? Comment { get; set; }
}
