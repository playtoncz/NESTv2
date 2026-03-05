using System.Globalization;
using System.Xml;
using NestCore.Model;

namespace NestFormat;

/// <summary>Zapíše znalostní bázi do XML kompatibilního s base.dtd.</summary>
public sealed class BaseXmlWriter
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    /// <summary>Zapíše KB do XML řetězce (UTF-8, bez DOCTYPE).</summary>
    public string Write(KnowledgeBase kb)
    {
        var sw = new StringWriter();
        var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = false, Encoding = System.Text.Encoding.UTF8 };
        using (var w = XmlWriter.Create(sw, settings))
        {
            w.WriteStartDocument();
            w.WriteStartElement("base");
            WriteGlobal(w, kb.Global);
            w.WriteStartElement("attributes");
            foreach (var a in kb.Attributes)
                WriteAttribute(w, a);
            w.WriteEndElement();
            w.WriteStartElement("contexts");
            foreach (var c in kb.Contexts)
                WriteContext(w, c);
            w.WriteEndElement();
            w.WriteStartElement("rules");
            w.WriteStartElement("apriori_rules");
            w.WriteEndElement();
            w.WriteStartElement("logical_rules");
            w.WriteEndElement();
            w.WriteStartElement("compositional_rules");
            foreach (var r in kb.CompositionalRules)
                WriteCompositionalRule(w, r, (long)kb.Global.WeightRange);
            w.WriteEndElement();
            w.WriteEndElement();
            w.WriteStartElement("integrity_constraints");
            foreach (var io in kb.IntegrityConstraints)
                WriteIntegrityConstraint(w, io);
            w.WriteEndElement();
            WriteNestStudioControl(w, kb.Global);
            w.WriteEndElement();
            w.WriteEndDocument();
        }
        var result = sw.ToString();
        if (result.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
            result = System.Text.RegularExpressions.Regex.Replace(result, @"encoding\s*=\s*[""'][^""']*[""']", "encoding=\"utf-8\"");
        return result;
    }

    private static void WriteGlobal(XmlWriter w, Global g)
    {
        w.WriteStartElement("global");
        if (!string.IsNullOrEmpty(g.Description)) w.WriteElementString("description", g.Description);
        if (!string.IsNullOrEmpty(g.Expert)) w.WriteElementString("expert", g.Expert);
        if (!string.IsNullOrEmpty(g.KnowledgeEngineer)) w.WriteElementString("knowledge_engineer", g.KnowledgeEngineer);
        if (!string.IsNullOrEmpty(g.Date)) w.WriteElementString("date", g.Date);
        w.WriteElementString("inference_mechanism", g.InferenceMechanism);
        w.WriteElementString("weight_range", g.WeightRange.ToString(Invariant));
        w.WriteElementString("default_weight", g.DefaultWeight == DefaultWeightKind.Unknown ? "unknown" : "irrelevant");
        w.WriteElementString("global_priority", g.GlobalPriority switch
        {
            GlobalPriorityKind.Last => "last",
            GlobalPriorityKind.MinLength => "minlength",
            GlobalPriorityKind.MaxLength => "maxlength",
            GlobalPriorityKind.User => "user",
            _ => "first"
        });
        w.WriteElementString("context_global_threshold", g.ContextGlobalThreshold.ToString("F3", Invariant));
        w.WriteElementString("condition_global_threshold", g.ConditionGlobalThreshold.ToString("F3", Invariant));
        w.WriteEndElement();
    }

    private static void WriteAttribute(XmlWriter w, NestCore.Model.Attribute a)
    {
        w.WriteStartElement("attribute");
        w.WriteElementString("id", a.Id);
        if (!string.IsNullOrEmpty(a.Name)) w.WriteElementString("name", a.Name);
        w.WriteElementString("type", a.Type switch
        {
            AttributeType.Single => "single",
            AttributeType.Multiple => "multiple",
            AttributeType.Numeric => "numeric",
            _ => "binary"
        });
        if (a.Scope == ScopeKind.Environment) w.WriteElementString("scope", "environment");
        if (!string.IsNullOrEmpty(a.IdContext)) w.WriteElementString("id_context", a.IdContext);
        if (a.ContextThreshold != 0) w.WriteElementString("context_threshold", a.ContextThreshold.ToString("F3", Invariant));
        if (a.LegalValues != null)
        {
            w.WriteStartElement("legal_values");
            if (a.LegalValues.LowerBound.HasValue) w.WriteElementString("lower_bound", a.LegalValues.LowerBound.Value.ToString("F3", Invariant));
            if (a.LegalValues.UpperBound.HasValue) w.WriteElementString("upper_bound", a.LegalValues.UpperBound.Value.ToString("F3", Invariant));
            if (a.LegalValues.Values != null)
                foreach (var v in a.LegalValues.Values)
                    w.WriteElementString("value", v);
            w.WriteEndElement();
        }
        if (a.Propositions.Count > 0)
        {
            w.WriteStartElement("propositions");
            foreach (var p in a.Propositions)
                WriteProposition(w, p);
            w.WriteEndElement();
        }
        if (!string.IsNullOrEmpty(a.Comment)) w.WriteElementString("comment", a.Comment);
        w.WriteEndElement();
    }

    private static void WriteProposition(XmlWriter w, Proposition p)
    {
        w.WriteStartElement("proposition");
        w.WriteElementString("id", p.Id);
        if (!string.IsNullOrEmpty(p.Name)) w.WriteElementString("name", p.Name);
        if (p.WeightFunction != null)
        {
            var f = p.WeightFunction;
            w.WriteStartElement("weight_function");
            w.WriteElementString("fuzzy_lower_bound", f.FuzzyLower.ToString("F3", Invariant));
            w.WriteElementString("crisp_lower_bound", f.CrispLower.ToString("F3", Invariant));
            w.WriteElementString("crisp_upper_bound", f.CrispUpper.ToString("F3", Invariant));
            w.WriteElementString("fuzzy_upper_bound", f.FuzzyUpper.ToString("F3", Invariant));
            w.WriteEndElement();
        }
        if (!string.IsNullOrEmpty(p.Comment)) w.WriteElementString("comment", p.Comment);
        w.WriteEndElement();
    }

    private static void WriteContext(XmlWriter w, Context c)
    {
        w.WriteStartElement("context");
        w.WriteElementString("id", c.Id);
        if (!string.IsNullOrEmpty(c.Comment)) w.WriteElementString("comment", c.Comment);
        WriteCondition(w, c.Condition);
        w.WriteEndElement();
    }

    private static void WriteCondition(XmlWriter w, Condition cond)
    {
        w.WriteStartElement("condition");
        foreach (var conj in cond.Conjunctions)
        {
            w.WriteStartElement("conjunction");
            foreach (var lit in conj.Literals)
                WriteLiteral(w, lit);
            w.WriteEndElement();
        }
        w.WriteEndElement();
    }

    private static void WriteLiteral(XmlWriter w, Literal lit)
    {
        w.WriteStartElement("literal");
        w.WriteElementString("id_attribute", lit.AttributeId);
        if (!string.IsNullOrEmpty(lit.PropositionId)) w.WriteElementString("id_proposition", lit.PropositionId);
        w.WriteElementString("negation", lit.Negation ? "1" : "0");
        w.WriteEndElement();
    }

    private static void WriteConclusion(XmlWriter w, Conclusion c, bool writeWeight)
    {
        w.WriteStartElement("conclusion");
        w.WriteElementString("id_attribute", c.AttributeId);
        if (!string.IsNullOrEmpty(c.PropositionId)) w.WriteElementString("id_proposition", c.PropositionId);
        w.WriteElementString("negation", c.Negation ? "1" : "0");
        if (writeWeight) w.WriteElementString("weight", c.Weight.ToString("F3", Invariant));
        w.WriteEndElement();
    }

    private static void WriteCompositionalRule(XmlWriter w, CompositionalRule r, long weightRange)
    {
        w.WriteStartElement("compositional_rule");
        w.WriteElementString("id", r.Id);
        if (r.Priority.HasValue) w.WriteElementString("priority", r.Priority.Value.ToString(Invariant));
        if (!string.IsNullOrEmpty(r.IdContext)) w.WriteElementString("id_context", r.IdContext);
        if (r.ContextThreshold.HasValue) w.WriteElementString("context_threshold", r.ContextThreshold.Value.ToString("F3", Invariant));
        if (r.ConditionThreshold.HasValue) w.WriteElementString("condition_threshold", r.ConditionThreshold.Value.ToString("F3", Invariant));
        WriteCondition(w, r.Condition);
        w.WriteStartElement("conclusions");
        foreach (var c in r.Conclusions)
            WriteConclusion(w, c, true);
        w.WriteEndElement();
        if (!string.IsNullOrEmpty(r.Comment)) w.WriteElementString("comment", r.Comment);
        w.WriteEndElement();
    }

    private static void WriteIntegrityConstraint(XmlWriter w, IntegrityConstraint io)
    {
        w.WriteStartElement("integrity_constraint");
        w.WriteElementString("id", io.Id);
        if (!string.IsNullOrEmpty(io.Name)) w.WriteElementString("name", io.Name);
        if (!string.IsNullOrEmpty(io.IdContext)) w.WriteElementString("id_context", io.IdContext);
        if (io.ContextThreshold.HasValue) w.WriteElementString("context_threshold", io.ContextThreshold.Value.ToString("F3", Invariant));
        WriteCondition(w, io.Condition);
        w.WriteStartElement("conclusions");
        foreach (var c in io.Conclusions)
            WriteConclusion(w, c, false);
        w.WriteEndElement();
        if (!string.IsNullOrEmpty(io.Comment)) w.WriteElementString("comment", io.Comment);
        w.WriteEndElement();
    }

    private static void WriteNestStudioControl(XmlWriter w, Global g)
    {
        w.WriteStartElement("system_info");
        w.WriteStartElement("nest_studio_control");
        w.WriteElementString("att_of_type_environment", g.AttOfTypeEnvironment == AttOfTypeEnvironmentKind.ClearValues ? "clear_values" : "keep_values");
        w.WriteElementString("answering_mode", g.AnsweringMode);
        w.WriteElementString("reasoning_mode", g.ReasoningMode);
        w.WriteElementString("disable_source_files", g.DisableSourceFiles ? "1" : "0");
        w.WriteElementString("disable_source_external_functions", g.DisableSourceExternalFunctions ? "1" : "0");
        w.WriteElementString("disable_source_calculations", g.DisableSourceCalculations ? "1" : "0");
        if (!string.IsNullOrEmpty(g.CasesStorePath)) w.WriteElementString("cases_store_path", g.CasesStorePath);
        w.WriteElementString("load_goals_from", g.LoadGoalsFrom);
        w.WriteElementString("cbr_type", g.CbrType);
        w.WriteElementString("similarity_threshold", g.SimilarityThreshold.ToString(Invariant));
        w.WriteEndElement();
        w.WriteEndElement();
    }
}
