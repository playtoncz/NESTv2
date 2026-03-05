using NestCore.Model;

namespace NestCore.Inference;

/// <summary>Běh inference: načte KB a AnswerSet, vrátí InferenceResult.</summary>
public sealed class InferenceEngine
{
    private readonly UncertaintyStandard _uncertainty = new(0.99);

    /// <summary>Klíč výroku: (AttributeId, PropositionId nebo null u binary).</summary>
    private sealed record StatementKey(string AttributeId, string? PropositionId);

    public InferenceResult Run(KnowledgeBase kb, AnswerSet answers)
    {
        var result = new InferenceResult();
        if (kb.CompositionalRules.Count == 0)
            return result;

        // 1) Vytvořit mapu skóre výroků (attributeId, propositionId) -> Interval
        var scores = new Dictionary<StatementKey, Interval>();
        var defaultInterval = kb.Global.DefaultWeight == DefaultWeightKind.Unknown
            ? new Interval(-1, 1)
            : new Interval(0, 0);

        foreach (var attr in kb.Attributes)
        {
            if (attr.Type == AttributeType.Binary)
            {
                scores[new StatementKey(attr.Id, null)] = new Interval(defaultInterval);
            }
            else
            {
                foreach (var prop in attr.Propositions)
                    scores[new StatementKey(attr.Id, prop.Id)] = new Interval(defaultInterval);
            }
        }

        // 2) Aplikovat odpovědi na skóre
        foreach (var aa in answers.Attributes)
        {
            var attr = kb.Attributes.FirstOrDefault(a => a.Id == aa.Id);
            if (attr == null) continue;

            if (attr.Type == AttributeType.Binary)
            {
                var key = new StatementKey(attr.Id, null);
                if (scores.TryGetValue(key, out var interval))
                {
                    var w = GetAnswerWeight(aa, 0);
                    interval.Set(w, w);
                }
            }
            else if (attr.Type == AttributeType.Multiple || attr.Type == AttributeType.Single)
            {
                foreach (var ans in aa.Answers)
                {
                    if (string.IsNullOrEmpty(ans.Value)) continue;
                    var key = new StatementKey(attr.Id, ans.Value);
                    if (scores.TryGetValue(key, out var interval))
                    {
                        var w = ans.Weight ?? 1;
                        interval.Set(w, w);
                    }
                }
            }
            else if (attr.Type == AttributeType.Numeric)
            {
                // numeric: jedna hodnota v Answers, rozložit na váhy propozic podle fuzzy
                var numVal = aa.Answers.Count > 0 && !string.IsNullOrEmpty(aa.Answers[0].Value)
                    ? ParseDoubleInvariant(aa.Answers[0].Value, double.NaN)
                    : double.NaN;
                if (double.IsNaN(numVal)) continue;
                foreach (var prop in attr.Propositions)
                {
                    if (prop.WeightFunction == null) continue;
                    var w = ComputeFuzzyWeight(numVal, prop.WeightFunction);
                    var key = new StatementKey(attr.Id, prop.Id);
                    if (scores.TryGetValue(key, out var interval))
                        interval.Set(w, w);
                }
            }
        }

        // 3) Určit cíle (výroky, které jsou v závěrech pravidel)
        var goalsInConclusions = new HashSet<StatementKey>();
        var inConditions = new HashSet<StatementKey>();
        foreach (var rule in kb.CompositionalRules)
        {
            foreach (var conj in rule.Condition.Conjunctions)
                foreach (var lit in conj.Literals)
                    inConditions.Add(Key(lit));
            foreach (var c in rule.Conclusions)
                goalsInConclusions.Add(new StatementKey(c.AttributeId, c.PropositionId));
        }
        var goals = goalsInConclusions.Where(g => !inConditions.Contains(g)).ToList();
        if (goals.Count == 0)
            goals = goalsInConclusions.ToList();

        // 4) Backward chaining: pro každý cíl sbírat příspěvky z pravidel
        foreach (var goalKey in goals)
        {
            if (!scores.TryGetValue(goalKey, out var goalScore))
                continue;
            foreach (var rule in kb.CompositionalRules)
            {
                var relevantConclusions = rule.Conclusions
                    .Where(c => c.AttributeId == goalKey.AttributeId && c.PropositionId == goalKey.PropositionId)
                    .ToList();
                if (relevantConclusions.Count == 0) continue;

                var condScore = EvaluateCondition(rule.Condition, scores);
                if (condScore.Max <= 0) continue;

                var fired = new FiredRuleEntry { RuleId = rule.Id, ConditionScore = condScore.Max };
                foreach (var concl in relevantConclusions)
                {
                    var contrib = _uncertainty.CTR(condScore, new Interval(concl.Weight, concl.Weight));
                    if (concl.Negation)
                        contrib = _uncertainty.NEG(contrib);
                    contrib = _uncertainty.NORM(contrib);
                    goalScore.Min += contrib.Min;
                    goalScore.Max += contrib.Max;
                    fired.AppliedConclusions.Add(new AppliedConclusion
                    {
                        AttributeId = concl.AttributeId,
                        PropositionId = concl.PropositionId,
                        WeightChange = contrib.Max,
                        Reason = $"Rule {rule.Id}"
                    });
                }
                result.FiredRules.Add(fired);
            }
        }

        // 5) Sestavit výsledek – seznam cílů se skóre
        var attrById = kb.Attributes.ToDictionary(a => a.Id);
        foreach (var goalKey in goals)
        {
            if (!scores.TryGetValue(goalKey, out var interval))
                continue;
            var name = goalKey.PropositionId != null
                ? $"{goalKey.AttributeId}({goalKey.PropositionId})"
                : goalKey.AttributeId;
            if (attrById.TryGetValue(goalKey.AttributeId, out var attr))
            {
                var prop = attr.Propositions.FirstOrDefault(p => p.Id == goalKey.PropositionId);
                if (prop != null && !string.IsNullOrEmpty(prop.Name))
                    name = prop.Name;
            }
            result.Goals.Add(new GoalScore
            {
                AttributeId = goalKey.AttributeId,
                PropositionId = goalKey.PropositionId,
                DisplayName = name,
                MinWeight = interval.Min,
                MaxWeight = interval.Max,
                Status = "evaluated",
                Type = "goal"
            });
        }

        result.Goals = result.Goals.OrderByDescending(g => g.MaxWeight).ToList();
        return result;
    }

    private static StatementKey Key(Literal lit)
    {
        return new StatementKey(lit.AttributeId, lit.PropositionId);
    }

    private double GetAnswerWeight(AttributeAnswer aa, int index)
    {
        if (aa.SpecialStatus == AnswerSpecialStatus.CertainlyNo) return -1;
        if (aa.SpecialStatus == AnswerSpecialStatus.Irrelevant) return 0;
        if (aa.SpecialStatus == AnswerSpecialStatus.Unknown) return 0;
        if (aa.Answers.Count > index && aa.Answers[index].Weight.HasValue)
            return aa.Answers[index].Weight!.Value;
        return aa.SpecialStatus == AnswerSpecialStatus.CertainlyYes ? 1 : 0;
    }

    private Interval EvaluateCondition(Condition condition, Dictionary<StatementKey, Interval> scores)
    {
        if (condition.Conjunctions.Count == 0)
            return new Interval(0, 0);
        var disj = new Interval(-1, -1);
        foreach (var conj in condition.Conjunctions)
        {
            var conjVal = new Interval(1, 1);
            foreach (var lit in conj.Literals)
            {
                var key = Key(lit);
                if (!scores.TryGetValue(key, out var v))
                    v = new Interval(0, 0);
                if (lit.Negation)
                    v = _uncertainty.NEG(v);
                conjVal = _uncertainty.CONJ(conjVal, v);
            }
            disj = _uncertainty.DISJ(disj, conjVal);
        }
        return disj;
    }

    private static double ParseDoubleInvariant(string? value, double defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        var normalized = value.Trim().Replace(',', '.');
        return double.TryParse(normalized, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : defaultValue;
    }

    private static double ComputeFuzzyWeight(double value, FuzzyBounds bounds)
    {
        var fl = bounds.FuzzyLower;
        var cl = bounds.CrispLower;
        var cu = bounds.CrispUpper;
        var fu = bounds.FuzzyUpper;
        if (value >= cl && value <= cu) return 1;
        if (value >= fl && value < cl) return (2 * (value - fl) / (cl - fl)) - 1;
        if (value > cu && value <= fu) return (2 * (fu - value) / (fu - cu)) - 1;
        return -1;
    }
}

/// <summary>Neurčitost „standard“: CTR, CONJ, DISJ, NEG, NORM.</summary>
internal sealed class UncertaintyStandard
{
    private readonly double _normRange = 0.99;

    public UncertaintyStandard(double normRange = 0.99) => _normRange = normRange;

    public Interval CTR(Interval a, Interval w) => new(CTROne(a.Min, w.Min), CTROne(a.Max, w.Max));
    private static double CTROne(double a, double w) => a > 0 ? a * w : 0;
    public Interval CONJ(Interval a, Interval b) => new(Math.Min(a.Min, b.Min), Math.Min(a.Max, b.Max));
    public Interval DISJ(Interval a, Interval b) => new(Math.Max(a.Min, b.Min), Math.Max(a.Max, b.Max));
    public Interval NEG(Interval a) => new(-a.Max, -a.Min);
    public Interval NORM(Interval a) => new(
        Math.Min(Math.Max(a.Min, -_normRange), _normRange),
        Math.Min(Math.Max(a.Max, -_normRange), _normRange));
}
