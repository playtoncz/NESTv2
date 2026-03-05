namespace NestCore.Model;

/// <summary>Podmínka pravidla – disjunkce konjunkcí (OR konjunkcí).</summary>
public sealed class Condition
{
    public List<Conjunction> Conjunctions { get; set; } = new();
}
