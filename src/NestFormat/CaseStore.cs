using System.Xml;
using NestCore.Model;

namespace NestFormat;

/// <summary>Zapíše a načte Store case (odpovědi + výsledky + komentář).</summary>
public sealed class CaseStoreWriter
{
    /// <summary>Zapíše případ do XML: &lt;case&gt;&lt;answers&gt;...&lt;/answers&gt;&lt;results&gt;...&lt;/results&gt;&lt;comment&gt;...&lt;/comment&gt;&lt;/case&gt;</summary>
    public string Write(AnswerSet answers, InferenceResult results, string? comment = null)
    {
        var answersXml = new AnswersXmlWriter().Write(answers);
        var resultsXml = new ResultXmlWriter().Write(results);
        var answersDoc = new XmlDocument();
        answersDoc.LoadXml(answersXml);
        var resultsDoc = new XmlDocument();
        resultsDoc.LoadXml(resultsXml);
        var doc = new XmlDocument();
        doc.AppendChild(doc.CreateElement("case"));
        doc.DocumentElement!.AppendChild(doc.ImportNode(answersDoc.DocumentElement!, true));
        doc.DocumentElement.AppendChild(doc.ImportNode(resultsDoc.DocumentElement!, true));
        if (!string.IsNullOrEmpty(comment))
        {
            var commentEl = doc.CreateElement("comment");
            commentEl.InnerText = comment;
            doc.DocumentElement.AppendChild(commentEl);
        }
        using var sw = new StringWriter();
        var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = false, Encoding = System.Text.Encoding.UTF8 };
        using (var w = XmlWriter.Create(sw, settings))
            doc.Save(w);
        var xml = sw.ToString();
        if (xml.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
            xml = System.Text.RegularExpressions.Regex.Replace(xml, @"encoding\s*=\s*[""'][^""']*[""']", "encoding=\"utf-8\"");
        return xml;
    }
}

/// <summary>Načte Store case z XML.</summary>
public sealed class CaseStoreReader
{
    /// <summary>Načte případ; vrací odpovědi, výsledky a volitelný komentář.</summary>
    public (AnswerSet answers, InferenceResult results, string? comment) Read(string xml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        var root = doc.DocumentElement;
        if (root == null || root.LocalName != "case")
            return (new AnswerSet(), new InferenceResult(), null);
        var answersEl = root.SelectSingleNode("answers") as XmlElement;
        var resultsEl = root.SelectSingleNode("results") as XmlElement;
        var commentEl = root.SelectSingleNode("comment") as XmlElement;
        var answers = new AnswerSet();
        var results = new InferenceResult();
        if (answersEl != null)
        {
            var answersReader = new AnswersXmlReader();
            answers = answersReader.Read(answersEl.OuterXml);
        }
        if (resultsEl != null)
        {
            var resultsReader = new ResultXmlReader();
            results = resultsReader.Read(resultsEl.OuterXml);
        }
        var comment = commentEl?.InnerText?.Trim();
        return (answers, results, comment);
    }
}
