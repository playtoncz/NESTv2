namespace NestCore.Model;

/// <summary>Kontext – podmínka aktivace (pro pravidla s id_context).</summary>
public sealed class Context
{
    public string Id { get; set; } = "";
    public string? Comment { get; set; }
    public Condition Condition { get; set; } = new();
}
