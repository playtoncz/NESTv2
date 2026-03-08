namespace NestCore.Model;

/// <summary>Typ atributu dle base.dtd (binary | single | multiple | numeric).</summary>
public enum AttributeType
{
    Binary,
    Single,
    Multiple,
    Numeric
}

/// <summary>Scope atributu: case | environment.</summary>
public enum ScopeKind
{
    Case,
    Environment
}

/// <summary>Výchozí váha: unknown | irrelevant.</summary>
public enum DefaultWeightKind
{
    Unknown,
    Irrelevant
}

/// <summary>Globální priorita pravidel: first | last | minlength | maxlength | user.</summary>
public enum GlobalPriorityKind
{
    First,
    Last,
    MinLength,
    MaxLength,
    User
}

/// <summary>Chování atributů typu Environment mezi běhy (Keep values | Clear values).</summary>
public enum AttOfTypeEnvironmentKind
{
    KeepValues,
    ClearValues
}

/// <summary>Role atributu/propozice (question, intermediate, goal, alone) – odvozená z pravidel.</summary>
public enum UsageRole
{
    Unknown,
    Question,
    Intermediate,
    Goal,
    Alone
}

/// <summary>Speciální stav odpovědi uživatele. Sémantika vah v rozsahu KB (typicky -3 až 3):
/// Určitě ano = max váha (3), Určitě ne = min váha (-3), Nerelevantní = 0, Neznámý = celý rozsah (-3;3).</summary>
public enum AnswerSpecialStatus
{
    None,
    CertainlyYes,
    CertainlyNo,
    Irrelevant,
    Unknown,
    PostponeAnswer
}
