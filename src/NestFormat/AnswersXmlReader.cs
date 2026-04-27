using System.Xml;
using NestCore.Model;

namespace NestFormat;

/// <summary>Načte odpovědi z XML kompatibilního s answers.dtd.</summary>
public sealed class AnswersXmlReader
{
    /// <summary>Načte AnswerSet z XML. Před načtením odstraní DOCTYPE.</summary>
    public AnswerSet Read(string xml)
    {
        xml = XmlHelper.StripDoctype(xml);
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var root = doc.DocumentElement;
        if (root == null || root.LocalName != "answers")
            throw new InvalidOperationException("Expected root element 'answers'.");

        var result = new AnswerSet
        {
            CaseDescription = XmlHelper.GetElementText(root, "case_description")
        };

        var attrNodes = root.SelectNodes("attribute");
        if (attrNodes != null)
            foreach (XmlElement attrEl in attrNodes)
                result.Attributes.Add(ReadAttributeAnswer(attrEl));

        return result;
    }

    private static AttributeAnswer ReadAttributeAnswer(XmlElement el)
    {
        var typeStr = XmlHelper.GetElementText(el, "type") ?? "binary";
        var type = typeStr.ToLowerInvariant() switch
        {
            "single" => AttributeType.Single,
            "multiple" => AttributeType.Multiple,
            "numeric" => AttributeType.Numeric,
            _ => AttributeType.Binary
        };

        var aa = new AttributeAnswer
        {
            Id = XmlHelper.GetElementText(el, "id") ?? "",
            Type = type
        };

        var answerNodes = el.SelectNodes("answer");
        if (answerNodes != null)
            foreach (XmlElement ansEl in answerNodes)
            {
                var value = XmlHelper.GetElementText(ansEl, "value");
                var weightStr = XmlHelper.GetElementText(ansEl, "weight");
                var minWeightStr = XmlHelper.GetElementText(ansEl, "min_weight");
                double? weight = null;
                double? minWeight = null;
                if (!string.IsNullOrWhiteSpace(weightStr) && !string.Equals(weightStr, "unknown", StringComparison.OrdinalIgnoreCase))
                    weight = XmlHelper.ParseDouble(weightStr);
                if (!string.IsNullOrWhiteSpace(minWeightStr))
                    minWeight = XmlHelper.ParseDouble(minWeightStr);
                aa.Answers.Add(new Answer { Value = value, Weight = weight, MinWeight = minWeight });
            }

        return aa;
    }
}
