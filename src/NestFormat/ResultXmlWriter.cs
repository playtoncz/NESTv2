using System.Globalization;
using System.Xml;
using NestCore.Model;

namespace NestFormat;

/// <summary>Zapíše výsledek inference do XML (results/goals + explainability fired_rules).</summary>
public sealed class ResultXmlWriter
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    /// <summary>Zapíše InferenceResult do XML řetězce (UTF-8).</summary>
    public string Write(InferenceResult result)
    {
        var sw = new StringWriter();
        var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = false, Encoding = System.Text.Encoding.UTF8 };
        using (var w = XmlWriter.Create(sw, settings))
        {
            w.WriteStartDocument();
            w.WriteStartElement("results");

            w.WriteStartElement("goals");
            foreach (var g in result.Goals)
            {
                w.WriteStartElement("goal");
                w.WriteElementString("attribute_id", g.AttributeId);
                if (!string.IsNullOrEmpty(g.PropositionId))
                    w.WriteElementString("proposition_id", g.PropositionId);
                if (!string.IsNullOrEmpty(g.DisplayName))
                    w.WriteElementString("display_name", g.DisplayName);
                w.WriteElementString("min_weight", g.MinWeight.ToString(Invariant));
                w.WriteElementString("max_weight", g.MaxWeight.ToString(Invariant));
                w.WriteElementString("status", g.Status);
                w.WriteElementString("type", g.Type);
                w.WriteEndElement();
            }
            w.WriteEndElement();

            if (result.FiredRules.Count > 0)
            {
                w.WriteStartElement("fired_rules");
                foreach (var fr in result.FiredRules)
                {
                    w.WriteStartElement("fired_rule");
                    w.WriteElementString("rule_id", fr.RuleId);
                    w.WriteElementString("condition_score", fr.ConditionScore.ToString(Invariant));
                    foreach (var ac in fr.AppliedConclusions)
                    {
                        w.WriteStartElement("applied_conclusion");
                        w.WriteElementString("attribute_id", ac.AttributeId);
                        if (!string.IsNullOrEmpty(ac.PropositionId))
                            w.WriteElementString("proposition_id", ac.PropositionId);
                        w.WriteElementString("weight_change", ac.WeightChange.ToString(Invariant));
                        if (!string.IsNullOrEmpty(ac.Reason))
                            w.WriteElementString("reason", ac.Reason);
                        w.WriteEndElement();
                    }
                    w.WriteEndElement();
                }
                w.WriteEndElement();
            }

            if (result.IntegrityViolations != null && result.IntegrityViolations.Count > 0)
            {
                w.WriteStartElement("integrity_violations");
                foreach (var v in result.IntegrityViolations)
                    w.WriteElementString("violation", v);
                w.WriteEndElement();
            }

            w.WriteEndElement();
            w.WriteEndDocument();
        }
        var xml = sw.ToString();
        if (xml.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
            xml = System.Text.RegularExpressions.Regex.Replace(xml, @"encoding\s*=\s*[""'][^""']*[""']", "encoding=\"utf-8\"");
        return xml;
    }
}
