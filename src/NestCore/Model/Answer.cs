namespace NestCore.Model;

/// <summary>Jedna odpověď na atribut – value (pro numeric číslo, pro multiple id propozice) a váha.</summary>
public sealed class Answer
{
    public string? Value { get; set; }
    public double? Weight { get; set; }
}
