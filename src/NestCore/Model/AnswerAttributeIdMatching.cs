using System.Text;

namespace NestCore.Model;

/// <summary>
/// Sladění ID atributu ze souboru odpovědí s atributem v KB: většinou odpovídá <c>attribute.id</c>,
/// některé exporty / ruční soubory ale používají <c>attribute.name</c> (např. „tlustoprst“ místo „tlusté prsty“).
/// </summary>
public static class AnswerAttributeIdMatching
{
    private static string Norm(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        return s.Trim().Normalize(NormalizationForm.FormC);
    }

    /// <summary>Řetězec z answers.xml odpovídá KB atributu (id nebo zobrazené jméno).</summary>
    public static bool AnswerIdMatchesKbAttribute(string? answerAttributeId, Attribute attr)
    {
        var a = Norm(answerAttributeId);
        if (a.Length == 0) return false;
        if (string.Equals(Norm(attr.Id), a, StringComparison.Ordinal)) return true;
        var name = Norm(attr.Name);
        if (name.Length > 0 && string.Equals(name, a, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    /// <summary>Vyhledá atribut v KB podle <c>&lt;id&gt;</c> z answers (id nebo name).</summary>
    public static Attribute? FindKbAttributeForAnswer(IEnumerable<Attribute> attributes, string? answerAttributeId)
    {
        foreach (var attr in attributes)
        {
            if (AnswerIdMatchesKbAttribute(answerAttributeId, attr))
                return attr;
        }

        return null;
    }
}
