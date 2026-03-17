using System.Xml;
using NestCore.Model;

namespace NestFormat;

/// <summary>Načte znalostní bázi z XML kompatibilního s base.dtd.</summary>
public sealed class BaseXmlReader
{
    /// <summary>Načte KB z XML řetězce. Před načtením odstraní DOCTYPE a normalizuje začátek.</summary>
    public KnowledgeBase Read(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            throw new InvalidOperationException("XML řetězec je prázdný.");
        xml = XmlHelper.StripDoctype(xml);
        if (string.IsNullOrWhiteSpace(xml))
            throw new InvalidOperationException("Po úpravě je XML prázdný.");
        var trimmed = xml.TrimStart();
        if (trimmed.Length > 0 && trimmed[0] != '<')
        {
            var firstLt = xml.IndexOf('<');
            if (firstLt >= 0)
                xml = xml.Substring(firstLt);
            trimmed = xml.TrimStart();
        }
        if (string.IsNullOrEmpty(trimmed) || trimmed[0] != '<')
        {
            var preview = xml.Length > 80 ? xml.Substring(0, 80) + "…" : xml;
            throw new InvalidOperationException($"Neplatné XML: soubor nezačíná elementem. Začátek: {preview.Replace("\r", "").Replace("\n", " ")}");
        }
        var doc = new XmlDocument();
        doc.LoadXml(xml!);

        var baseEl = doc.DocumentElement ?? throw new InvalidOperationException("Missing root element 'base'.");
        if (baseEl.LocalName != "base")
            throw new InvalidOperationException($"Expected root 'base', got '{baseEl.LocalName}'.");

        var kb = new KnowledgeBase();

        var globalEl = baseEl.SelectSingleNode("global") as XmlElement;
        if (globalEl != null)
            ReadGlobal(globalEl, kb.Global);

        var attrNodes = baseEl.SelectNodes("attributes/attribute");
        if (attrNodes != null)
            foreach (XmlElement attrEl in attrNodes)
                kb.Attributes.Add(ReadAttribute(attrEl));

        var ctxNodes = baseEl.SelectNodes("contexts/context");
        if (ctxNodes != null)
            foreach (XmlElement ctxEl in ctxNodes)
                kb.Contexts.Add(ReadContext(ctxEl));

        var aprioriNodes = baseEl.SelectNodes("rules/apriori_rules/apriori_rule");
        if (aprioriNodes != null)
            foreach (XmlElement ruleEl in aprioriNodes)
                kb.CompositionalRules.Add(ReadAprioriRule(ruleEl));

        var logicalNodes = baseEl.SelectNodes("rules/logical_rules/logical_rule");
        if (logicalNodes != null)
            foreach (XmlElement ruleEl in logicalNodes)
                kb.CompositionalRules.Add(ReadLogicalRule(ruleEl));

        var ruleNodes = baseEl.SelectNodes("rules/compositional_rules/compositional_rule");
        if (ruleNodes != null)
            foreach (XmlElement ruleEl in ruleNodes)
                kb.CompositionalRules.Add(ReadCompositionalRule(ruleEl));

        var ioNodes = baseEl.SelectNodes("integrity_constraints/integrity_constraint");
        if (ioNodes != null)
            foreach (XmlElement ioEl in ioNodes)
                kb.IntegrityConstraints.Add(ReadIntegrityConstraint(ioEl));

        var sysInfoEl = baseEl.SelectSingleNode("system_info") as XmlElement;
        if (sysInfoEl != null)
            ReadNestStudioControl(sysInfoEl, kb.Global);

        return kb;
    }

    private static void ReadNestStudioControl(XmlElement systemInfoEl, Global g)
    {
        var ctrlEl = systemInfoEl.SelectSingleNode("nest_studio_control") as XmlElement;
        if (ctrlEl == null) return;
        var att = XmlHelper.GetElementText(ctrlEl, "att_of_type_environment");
        if (string.Equals(att, "clear_values", StringComparison.OrdinalIgnoreCase))
            g.AttOfTypeEnvironment = AttOfTypeEnvironmentKind.ClearValues;
        g.AnsweringMode = XmlHelper.GetElementText(ctrlEl, "answering_mode") ?? g.AnsweringMode;
        g.ReasoningMode = XmlHelper.GetElementText(ctrlEl, "reasoning_mode") ?? g.ReasoningMode;
        g.DisableSourceFiles = string.Equals(XmlHelper.GetElementText(ctrlEl, "disable_source_files"), "1", StringComparison.Ordinal);
        g.DisableSourceExternalFunctions = string.Equals(XmlHelper.GetElementText(ctrlEl, "disable_source_external_functions"), "1", StringComparison.Ordinal);
        g.DisableSourceCalculations = string.Equals(XmlHelper.GetElementText(ctrlEl, "disable_source_calculations"), "1", StringComparison.Ordinal);
        g.CasesStorePath = XmlHelper.GetElementText(ctrlEl, "cases_store_path");
        g.LoadGoalsFrom = XmlHelper.GetElementText(ctrlEl, "load_goals_from") ?? g.LoadGoalsFrom;
        g.CbrType = XmlHelper.GetElementText(ctrlEl, "cbr_type") ?? g.CbrType;
        var sim = XmlHelper.GetElementText(ctrlEl, "similarity_threshold");
        if (!string.IsNullOrWhiteSpace(sim) && double.TryParse(sim.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v))
            g.SimilarityThreshold = v;
    }

    private static void ReadGlobal(XmlElement el, Global g)
    {
        g.Description = XmlHelper.GetElementText(el, "description");
        g.Expert = XmlHelper.GetElementText(el, "expert");
        g.KnowledgeEngineer = XmlHelper.GetElementText(el, "knowledge_engineer");
        g.Date = XmlHelper.GetElementText(el, "date");
        g.InferenceMechanism = XmlHelper.GetElementText(el, "inference_mechanism") ?? "standard";
        g.WeightRange = XmlHelper.ParseDouble(XmlHelper.GetElementText(el, "weight_range"), 1);
        var dw = XmlHelper.GetElementText(el, "default_weight");
        g.DefaultWeight = string.Equals(dw, "unknown", StringComparison.OrdinalIgnoreCase) ? DefaultWeightKind.Unknown : DefaultWeightKind.Irrelevant;
        var gp = XmlHelper.GetElementText(el, "global_priority");
        g.GlobalPriority = gp switch
        {
            "last" => GlobalPriorityKind.Last,
            "minlength" => GlobalPriorityKind.MinLength,
            "maxlength" => GlobalPriorityKind.MaxLength,
            "user" => GlobalPriorityKind.User,
            _ => GlobalPriorityKind.First
        };
        g.ContextGlobalThreshold = XmlHelper.ParseDouble(XmlHelper.GetElementText(el, "context_global_threshold"));
        g.ConditionGlobalThreshold = XmlHelper.ParseDouble(XmlHelper.GetElementText(el, "condition_global_threshold"));
    }

    private static NestCore.Model.Attribute ReadAttribute(XmlElement el)
    {
        var id = XmlHelper.GetElementText(el, "id") ?? "";
        var name = XmlHelper.GetElementText(el, "name");
        var a = new NestCore.Model.Attribute
        {
            Id = id,
            Name = string.IsNullOrWhiteSpace(name) ? id : name,
            Comment = XmlHelper.GetElementText(el, "comment")
        };
        var typeStr = XmlHelper.GetElementText(el, "type") ?? "binary";
        a.Type = typeStr.ToLowerInvariant() switch
        {
            "single" => AttributeType.Single,
            "multiple" => AttributeType.Multiple,
            "numeric" => AttributeType.Numeric,
            _ => AttributeType.Binary
        };
        var scopeStr = XmlHelper.GetElementText(el, "scope");
        a.Scope = string.Equals(scopeStr, "environment", StringComparison.OrdinalIgnoreCase) ? ScopeKind.Environment : ScopeKind.Case;
        a.IdContext = XmlHelper.GetElementText(el, "id_context");
        a.ContextThreshold = XmlHelper.ParseDouble(XmlHelper.GetElementText(el, "context_threshold"));

        var lvEl = el.SelectSingleNode("legal_values") as XmlElement;
        if (lvEl != null)
        {
            a.LegalValues = new LegalValues
            {
                LowerBound = ReadOptionalDouble(lvEl, "lower_bound"),
                UpperBound = ReadOptionalDouble(lvEl, "upper_bound")
            };
            var values = new List<string>();
            var valueNodes = lvEl.SelectNodes("value");
            if (valueNodes != null)
                foreach (XmlNode v in valueNodes)
                    if (v is XmlElement ve && !string.IsNullOrWhiteSpace(ve.InnerText))
                        values.Add(ve.InnerText.Trim());
            if (values.Count > 0)
                a.LegalValues.Values = values;
        }

        XmlNodeList? propList = el.SelectNodes("propositions/proposition");
        if (propList != null)
            foreach (XmlElement propEl in propList)
                a.Propositions.Add(ReadProposition(propEl));

        return a;
    }

    private static double? ReadOptionalDouble(XmlElement parent, string localName)
    {
        var t = XmlHelper.GetElementText(parent, localName);
        if (string.IsNullOrWhiteSpace(t)) return null;
        var d = XmlHelper.ParseDouble(t);
        return double.IsNaN(d) ? null : d;
    }

    private static Proposition ReadProposition(XmlElement el)
    {
        var pid = XmlHelper.GetElementText(el, "id") ?? "";
        var pname = XmlHelper.GetElementText(el, "name");
        var p = new Proposition
        {
            Id = pid,
            Name = string.IsNullOrWhiteSpace(pname) ? pid : pname,
            Comment = XmlHelper.GetElementText(el, "comment")
        };
        var wfEl = el.SelectSingleNode("weight_function") as XmlElement;
        if (wfEl != null)
        {
            p.WeightFunction = new FuzzyBounds
            {
                FuzzyLower = XmlHelper.ParseDouble(XmlHelper.GetElementText(wfEl, "fuzzy_lower_bound")),
                CrispLower = XmlHelper.ParseDouble(XmlHelper.GetElementText(wfEl, "crisp_lower_bound")),
                CrispUpper = XmlHelper.ParseDouble(XmlHelper.GetElementText(wfEl, "crisp_upper_bound")),
                FuzzyUpper = XmlHelper.ParseDouble(XmlHelper.GetElementText(wfEl, "fuzzy_upper_bound"))
            };
        }
        return p;
    }

    private static Context ReadContext(XmlElement el)
    {
        var c = new Context
        {
            Id = XmlHelper.GetElementText(el, "id") ?? "",
            Comment = XmlHelper.GetElementText(el, "comment")
        };
        var condEl = el.SelectSingleNode("condition") as XmlElement;
        if (condEl != null)
            c.Condition = ReadCondition(condEl);
        return c;
    }

    private static Condition ReadCondition(XmlElement condEl)
    {
        var cond = new Condition();
        var conjNodes = condEl.SelectNodes("conjunction");
        if (conjNodes != null)
            foreach (XmlElement conjEl in conjNodes)
            {
                var conj = new Conjunction();
                var litNodes = conjEl.SelectNodes("literal");
                if (litNodes != null)
                    foreach (XmlElement litEl in litNodes)
                        conj.Literals.Add(ReadLiteral(litEl));
                if (conj.Literals.Count > 0)
                    cond.Conjunctions.Add(conj);
            }
        return cond;
    }

    private static Literal ReadLiteral(XmlElement el)
    {
        return new Literal
        {
            AttributeId = XmlHelper.GetElementText(el, "id_attribute") ?? "",
            PropositionId = XmlHelper.GetElementText(el, "id_proposition"),
            Negation = string.Equals(XmlHelper.GetElementText(el, "negation"), "1", StringComparison.Ordinal)
        };
    }

    private static Conclusion ReadConclusion(XmlElement el, double weightRange)
    {
        var w = XmlHelper.ParseDouble(XmlHelper.GetElementText(el, "weight"), 1);
        return new Conclusion
        {
            AttributeId = XmlHelper.GetElementText(el, "id_attribute") ?? "",
            PropositionId = XmlHelper.GetElementText(el, "id_proposition"),
            Negation = string.Equals(XmlHelper.GetElementText(el, "negation"), "1", StringComparison.Ordinal),
            Weight = w
        };
    }

    private static CompositionalRule ReadAprioriRule(XmlElement el)
    {
        var r = new CompositionalRule
        {
            Id = XmlHelper.GetElementText(el, "id") ?? "",
            Kind = RuleKind.Apriori,
            Priority = ReadOptionalDouble(el, "priority"),
            Comment = XmlHelper.GetElementText(el, "comment")
        };
        var conclNodes = el.SelectNodes("conclusions/conclusion");
        if (conclNodes != null)
            foreach (XmlElement cEl in conclNodes)
                r.Conclusions.Add(ReadConclusion(cEl, 1));
        return r;
    }

    private static CompositionalRule ReadLogicalRule(XmlElement el)
    {
        var r = new CompositionalRule
        {
            Id = XmlHelper.GetElementText(el, "id") ?? "",
            Kind = RuleKind.Logical,
            Priority = ReadOptionalDouble(el, "priority"),
            IdContext = XmlHelper.GetElementText(el, "id_context"),
            Comment = XmlHelper.GetElementText(el, "comment")
        };
        var condEl = el.SelectSingleNode("condition") as XmlElement;
        if (condEl != null)
            r.Condition = ReadCondition(condEl);
        var conclNodes = el.SelectNodes("conclusions/conclusion");
        if (conclNodes != null)
            foreach (XmlElement cEl in conclNodes)
                r.Conclusions.Add(ReadConclusion(cEl, 1));
        return r;
    }

    private static CompositionalRule ReadCompositionalRule(XmlElement el)
    {
        var r = new CompositionalRule
        {
            Id = XmlHelper.GetElementText(el, "id") ?? "",
            Priority = ReadOptionalDouble(el, "priority"),
            IdContext = XmlHelper.GetElementText(el, "id_context"),
            ContextThreshold = ReadOptionalDouble(el, "context_threshold"),
            ConditionThreshold = ReadOptionalDouble(el, "condition_threshold"),
            Comment = XmlHelper.GetElementText(el, "comment")
        };
        var condEl = el.SelectSingleNode("condition") as XmlElement;
        if (condEl != null)
            r.Condition = ReadCondition(condEl);
        var conclNodes = el.SelectNodes("conclusions/conclusion");
        if (conclNodes != null)
            foreach (XmlElement cEl in conclNodes)
                r.Conclusions.Add(ReadConclusion(cEl, 1));
        return r;
    }

    private static IntegrityConstraint ReadIntegrityConstraint(XmlElement el)
    {
        var io = new IntegrityConstraint
        {
            Id = XmlHelper.GetElementText(el, "id") ?? "",
            Name = XmlHelper.GetElementText(el, "name"),
            IdContext = XmlHelper.GetElementText(el, "id_context"),
            ContextThreshold = ReadOptionalDouble(el, "context_threshold"),
            Comment = XmlHelper.GetElementText(el, "comment")
        };
        var condEl = el.SelectSingleNode("condition") as XmlElement;
        if (condEl != null)
            io.Condition = ReadCondition(condEl);
        var conclNodes = el.SelectNodes("conclusions/conclusion");
        if (conclNodes != null)
            foreach (XmlElement cEl in conclNodes)
                io.Conclusions.Add(ReadConclusion(cEl, 1));
        return io;
    }
}
