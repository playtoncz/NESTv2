namespace NestCore.Model;

/// <summary>Jedna odpověď na atribut – value (pro numeric číslo, pro multiple id propozice) a váha.</summary>
public sealed class Answer
{
    public string? Value { get; set; }
    /// <summary>Horní mez váhy v projektové škále (stejně jako dříve jediná váha).</summary>
    public double? Weight { get; set; }
    /// <summary>Dolní mez váhy při zápisu „min;max“ (volitelné).</summary>
    public double? MinWeight { get; set; }
}
