namespace NestCore.Model;

/// <summary>Globální parametry znalostní báze (global v base.dtd) + rozšíření Control/CBR.</summary>
public sealed class Global
{
    public string? Description { get; set; }
    public string? Expert { get; set; }
    public string? KnowledgeEngineer { get; set; }
    public string? Date { get; set; }

    public string InferenceMechanism { get; set; } = "standard";
    public double WeightRange { get; set; } = 1;
    public DefaultWeightKind DefaultWeight { get; set; } = DefaultWeightKind.Irrelevant;
    public GlobalPriorityKind GlobalPriority { get; set; } = GlobalPriorityKind.First;
    public double ContextGlobalThreshold { get; set; }
    public double ConditionGlobalThreshold { get; set; }

    /// <summary>Atributy typu Environment mezi běhy: Keep values | Clear values.</summary>
    public AttOfTypeEnvironmentKind AttOfTypeEnvironment { get; set; } = AttOfTypeEnvironmentKind.KeepValues;

    /// <summary>Režim odpovědí: Dialog | Questionnaire | LoadFromFile.</summary>
    public string AnsweringMode { get; set; } = "Questionnaire";

    /// <summary>Režim odvozování: Postpone | WithoutPostpone.</summary>
    public string ReasoningMode { get; set; } = "Postpone";

    /// <summary>Zakázat zdroje: soubory, externí funkce, výpočty (MVP no-op).</summary>
    public bool DisableSourceFiles { get; set; }
    public bool DisableSourceExternalFunctions { get; set; }
    public bool DisableSourceCalculations { get; set; }

    /// <summary>CBR: cesta k úložišti případů (MVP no-op).</summary>
    public string? CasesStorePath { get; set; }

    /// <summary>CBR: načíst cíle z knowledge_base | cases_store.</summary>
    public string LoadGoalsFrom { get; set; } = "knowledge_base";

    /// <summary>CBR: typ compositional | logical.</summary>
    public string CbrType { get; set; } = "compositional";

    /// <summary>CBR: práh podobnosti.</summary>
    public double SimilarityThreshold { get; set; } = 0.5;
}
