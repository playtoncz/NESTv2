namespace NestCore.Model;

/// <summary>Propozice (hodnota) atributu – u numeric obsahuje fuzzy bounds.</summary>
public sealed class Proposition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Comment { get; set; }

    /// <summary>Odvozená role (question/intermediate/goal) – vyplní engine nebo analýza KB.</summary>
    public UsageRole UsageRole { get; set; } = UsageRole.Unknown;

    /// <summary>Pro numeric atributy: fuzzy/crisp hranice. U multiple/binary prázdné.</summary>
    public FuzzyBounds? WeightFunction { get; set; }
}
