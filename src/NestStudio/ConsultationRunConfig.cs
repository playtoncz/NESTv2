namespace NestStudio;

/// <summary>Konfigurace před spuštěním konzultace (Control).</summary>
public sealed class ConsultationRunConfig
{
    public AnsweringModeKind AnsweringMode { get; set; } = AnsweringModeKind.Questionnaire;
    public string? AnswersFilePath { get; set; }
    public ReasoningModeKind ReasoningMode { get; set; } = ReasoningModeKind.Postpone;
    public QuestionLayoutMode LayoutMode { get; set; } = QuestionLayoutMode.AllAtOnce;
    public NestCore.Model.UncertaintyType? Uncertainty { get; set; }
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

public enum QuestionLayoutMode
{
    AllAtOnce,
    OneByOne
}
