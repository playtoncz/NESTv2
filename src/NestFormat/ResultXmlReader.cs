using System.Xml;
using NestCore.Model;

namespace NestFormat;

/// <summary>Načte výsledek inference z XML (results/goals + fired_rules).</summary>
public sealed class ResultXmlReader
{
    public InferenceResult Read(string xml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        var root = doc.DocumentElement;
        if (root == null || root.LocalName != "results")
            return new InferenceResult();
        var result = new InferenceResult();
        var goalNodes = root.SelectNodes("goals/goal");
        if (goalNodes != null)
            foreach (XmlElement goalEl in goalNodes)
        {
            result.Goals.Add(new GoalScore
            {
                AttributeId = GetText(goalEl, "attribute_id") ?? "",
                PropositionId = GetText(goalEl, "proposition_id"),
                DisplayName = GetText(goalEl, "display_name") ?? "",
                MinWeight = ParseDouble(GetText(goalEl, "min_weight"), 0),
                MaxWeight = ParseDouble(GetText(goalEl, "max_weight"), 0),
                Status = GetText(goalEl, "status") ?? "evaluated",
                Type = GetText(goalEl, "type") ?? "goal"
            });
        }
        var frNodes = root.SelectNodes("fired_rules/fired_rule");
        if (frNodes != null)
            foreach (XmlElement frEl in frNodes)
        {
            var fr = new FiredRuleEntry
            {
                RuleId = GetText(frEl, "rule_id") ?? "",
                ConditionScore = ParseDouble(GetText(frEl, "condition_score"), 0)
            };
            var acNodes = frEl.SelectNodes("applied_conclusion");
            if (acNodes != null)
                foreach (XmlElement acEl in acNodes)
            {
                fr.AppliedConclusions.Add(new AppliedConclusion
                {
                    AttributeId = GetText(acEl, "attribute_id") ?? "",
                    PropositionId = GetText(acEl, "proposition_id"),
                    WeightChange = ParseDouble(GetText(acEl, "weight_change"), 0),
                    Reason = GetText(acEl, "reason")
                });
            }
            result.FiredRules.Add(fr);
        }
        var violNodes = root.SelectNodes("integrity_violations/violation");
        if (violNodes != null)
            foreach (XmlElement vEl in violNodes)
            {
                result.IntegrityViolations ??= new List<string>();
                if (!string.IsNullOrWhiteSpace(vEl.InnerText))
                    result.IntegrityViolations.Add(vEl.InnerText.Trim());
            }
        return result;
    }

    private static string? GetText(XmlElement parent, string localName)
    {
        var el = parent.SelectSingleNode(localName) as XmlElement;
        return el?.InnerText?.Trim();
    }

    private static double ParseDouble(string? value, double defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        return double.TryParse(value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : defaultValue;
    }
}
