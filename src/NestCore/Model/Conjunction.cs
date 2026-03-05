namespace NestCore.Model;

/// <summary>Konjunkce literálů (AND).</summary>
public sealed class Conjunction
{
    public List<Literal> Literals { get; set; } = new();
}
