namespace NestCore.Model;

/// <summary>Pravidlo znalostní báze: IF condition THEN conclusions. Druh (RuleKind) rozlišuje apriorní/logické/kompozicionální.</summary>
public sealed class CompositionalRule
{
    public string Id { get; set; } = "";
    public RuleKind Kind { get; set; } = RuleKind.Compositional;
    public double? Priority { get; set; }
    public string? IdContext { get; set; }
    public double? ContextThreshold { get; set; }
    public double? ConditionThreshold { get; set; }

    public Condition Condition { get; set; } = new();
    public List<Conclusion> Conclusions { get; set; } = new();
    public string? Comment { get; set; }

    /// <summary>Textové shrnutí pravidla (IF ... THEN ...).</summary>
    public string ToSummary()
    {
        var sb = new System.Text.StringBuilder();
        if (Kind == RuleKind.Apriori)
        {
            sb.Append("APRIORI ");
        }
        else
        {
            sb.Append("IF ");
            for (int ci = 0; ci < Condition.Conjunctions.Count; ci++)
            {
                if (ci > 0) sb.Append(" OR ");
                var conj = Condition.Conjunctions[ci];
                for (int li = 0; li < conj.Literals.Count; li++)
                {
                    if (li > 0) sb.Append(" AND ");
                    var lit = conj.Literals[li];
                    if (lit.Negation) sb.Append("NOT ");
                    sb.Append(lit.AttributeId);
                    if (!string.IsNullOrEmpty(lit.PropositionId))
                        sb.Append('[').Append(lit.PropositionId).Append(']');
                }
            }
            sb.Append(" THEN ");
        }
        for (int i = 0; i < Conclusions.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            var c = Conclusions[i];
            if (c.Negation) sb.Append("NOT ");
            sb.Append(c.AttributeId);
            if (!string.IsNullOrEmpty(c.PropositionId))
                sb.Append('[').Append(c.PropositionId).Append(']');
            sb.Append('@').Append(c.Weight.ToString("F3", System.Globalization.CultureInfo.InvariantCulture));
        }
        return sb.ToString();
    }
}
