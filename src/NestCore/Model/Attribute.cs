namespace NestCore.Model;

/// <summary>Atribut znalostní báze (binary, single, multiple, numeric).</summary>
public sealed class Attribute
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Comment { get; set; }
    public AttributeType Type { get; set; }
    public ScopeKind Scope { get; set; } = ScopeKind.Case;

    public string? IdContext { get; set; }
    public double ContextThreshold { get; set; }

    public LegalValues? LegalValues { get; set; }
    public List<Proposition> Propositions { get; set; } = new();

    /// <summary>Odvozená role (question/intermediate/goal/alone) – vyplní engine nebo analýza KB.</summary>
    public UsageRole UsageRole { get; set; } = UsageRole.Unknown;
}
