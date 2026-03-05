namespace NestCore.Model;

/// <summary>Celá znalostní báze – odpovídá elementu base v base.dtd.</summary>
public sealed class KnowledgeBase
{
    public Global Global { get; set; } = new();
    public List<Attribute> Attributes { get; set; } = new();
    public List<Context> Contexts { get; set; } = new();
    public List<CompositionalRule> CompositionalRules { get; set; } = new();
    public List<object> AprioriRules { get; set; } = new();
    public List<object> LogicalRules { get; set; } = new();
    public List<IntegrityConstraint> IntegrityConstraints { get; set; } = new();
}
