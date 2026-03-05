namespace NestCore.Model;

/// <summary>Literál v podmínce pravidla – odkaz na výrok (atribut + volitelně propozice) s negací.</summary>
public sealed class Literal
{
    public string AttributeId { get; set; } = "";
    public string? PropositionId { get; set; }
    public bool Negation { get; set; }
}
