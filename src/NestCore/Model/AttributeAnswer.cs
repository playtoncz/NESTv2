namespace NestCore.Model;

/// <summary>Odpovědi uživatele pro jeden atribut (answers.dtd: attribute s answer*).</summary>
public sealed class AttributeAnswer
{
    public string Id { get; set; } = "";
    public AttributeType Type { get; set; }
    public List<Answer> Answers { get; set; } = new();
    public AnswerSpecialStatus SpecialStatus { get; set; } = AnswerSpecialStatus.None;
}
