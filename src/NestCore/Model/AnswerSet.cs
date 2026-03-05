namespace NestCore.Model;

/// <summary>Sada odpovědí uživatele – odpovídá elementu answers v answers.dtd.</summary>
public sealed class AnswerSet
{
    public string? CaseDescription { get; set; }
    public List<AttributeAnswer> Attributes { get; set; } = new();
}
