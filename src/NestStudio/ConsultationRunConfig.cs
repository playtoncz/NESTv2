namespace NestStudio;

/// <summary>Konfigurace před spuštěním konzultace (Control).</summary>
public sealed class ConsultationRunConfig
{
    public AnsweringModeKind AnsweringMode { get; set; } = AnsweringModeKind.Questionnaire;
    public string? AnswersFilePath { get; set; }
    public ReasoningModeKind ReasoningMode { get; set; } = ReasoningModeKind.Postpone;
}

public enum AnsweringModeKind
{
    Questionnaire,
    LoadFromFile
}

public enum ReasoningModeKind
{
    Postpone,
    WithoutPostpone
}
