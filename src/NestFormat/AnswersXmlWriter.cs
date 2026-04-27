using System.Globalization;
using System.Xml;
using NestCore.Model;

namespace NestFormat;

/// <summary>Zapíše odpovědi do XML kompatibilního s answers.dtd.</summary>
public sealed class AnswersXmlWriter
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public string Write(AnswerSet answerSet)
    {
        var sw = new StringWriter();
        var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = false, Encoding = System.Text.Encoding.UTF8 };
        using (var w = XmlWriter.Create(sw, settings))
        {
            w.WriteStartDocument();
            w.WriteStartElement("answers");
            if (!string.IsNullOrEmpty(answerSet.CaseDescription))
                w.WriteElementString("case_description", answerSet.CaseDescription);
            foreach (var aa in answerSet.Attributes)
                WriteAttributeAnswer(w, aa);
            w.WriteEndElement();
            w.WriteEndDocument();
        }
        return sw.ToString();
    }

    private static void WriteAttributeAnswer(XmlWriter w, AttributeAnswer aa)
    {
        w.WriteStartElement("attribute");
        w.WriteElementString("id", aa.Id);
        w.WriteElementString("type", aa.Type switch
        {
            AttributeType.Single => "single",
            AttributeType.Multiple => "multiple",
            AttributeType.Numeric => "numeric",
            _ => "binary"
        });
        foreach (var ans in aa.Answers)
        {
            w.WriteStartElement("answer");
            if (!string.IsNullOrEmpty(ans.Value)) w.WriteElementString("value", ans.Value);
            if (ans.MinWeight.HasValue)
                w.WriteElementString("min_weight", ans.MinWeight.Value.ToString(Invariant));
            if (ans.Weight.HasValue)
                w.WriteElementString("weight", ans.Weight.Value.ToString(Invariant));
            else
                w.WriteElementString("weight", "unknown");
            w.WriteEndElement();
        }
        w.WriteEndElement();
    }
}
