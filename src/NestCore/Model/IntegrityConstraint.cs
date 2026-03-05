namespace NestCore.Model;

/// <summary>Integritní omezení – condition a conclusions (načíst/uložit, vyhodnocení později).</summary>
public sealed class IntegrityConstraint
{
    public string Id { get; set; } = "";
    public string? Name { get; set; }
    public string? IdContext { get; set; }
    public double? ContextThreshold { get; set; }
    public Condition Condition { get; set; } = new();
    public List<Conclusion> Conclusions { get; set; } = new();
    public string? Comment { get; set; }
}
