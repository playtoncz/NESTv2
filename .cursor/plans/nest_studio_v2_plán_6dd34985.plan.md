---
name: NEST Studio V2 plán
overview: "NEST Studio je moderní desktopová náhrada starého NEST (VB .NET) v C# .NET 8: třívrstvá architektura (NestStudio UI, NestCore engine, NestFormat XML), deterministický inference engine s váhami a explainability, editor znalostní báze a konzultační režim s liquid glass / Fluent UI (Avalonia)."
todos:
  - id: sln-setup
    content: Vytvořit řešení .sln s projekty NestCore, NestFormat, NestStudio, NestCore.Tests, NestFormat.Tests
    status: completed
  - id: nestcore-model
    content: "Implementovat datový model v NestCore: Attribute, Proposition, Literal, Conjunction, Condition, Conclusion, CompositionalRule, KnowledgeBase, AnswerSet, Result, Global"
    status: completed
  - id: nestformat-base
    content: "NestFormat: BaseXmlReader/Writer dle base.dtd, načtení Nemoce.xml a roundtrip test"
    status: completed
  - id: nestformat-answers
    content: "NestFormat: AnswersXmlReader/Writer dle answers.dtd"
    status: completed
  - id: inference-engine
    content: "NestCore: Inference engine (standard) – backward chaining, CTR/CONJ/DISJ/NORM, aplikace odpovědí na výroky"
    status: completed
  - id: fuzzy-numeric
    content: "NestCore: Fuzzy membership pro numeric propositions (trapezoidální bounds)"
    status: completed
  - id: explainability
    content: "NestCore: Explainability log – FiredRules s příspěvky vah do výsledku"
    status: completed
  - id: studio-avalonia
    content: "NestStudio: Avalonia projekt, liquid glass styling (Acrylic/Blur), MVVM"
    status: completed
  - id: studio-runner
    content: "NestStudio: Consultation/Runner – dotazník z KB, answers XML, volání engine, zobrazení skóre a pořadí"
    status: completed
  - id: studio-kb-editor
    content: "NestStudio: KB Editor – atributy, propositions, compositional rules, validace, import/export"
    status: completed
  - id: studio-graph
    content: "NestStudio: Graf pravidel (uzly atributy/výroky, hrany pravidla)"
    status: completed
  - id: tests-mvp
    content: "Unit testy: parser Nemoce.xml, inference s referenčními odpověďmi, fuzzy"
    status: completed
  - id: todo-md
    content: Vytvořit todo.md v kořeni repo s milníky a checkboxy
    status: completed
  - id: model-scope-global
    content: "Model: Scope (Case/Environment) u Attribute; Global rozšířit o AttOfTypeEnvironment, AnsweringMode, ReasoningMode, DisableSources; CBR parametry (načíst/uložit)"
    status: completed
  - id: model-answer-status
    content: "Model: UserAnswer stavy CertainlyYes/No, Irrelevant, Unknown, Postpone; UsageRole (question/intermediate/goal) pro atributy/propozice"
    status: completed
  - id: studio-global-properties
    content: "NestStudio: Dialog/panel Globální vlastnosti KB (description, expert, inference mechanism, priority, default weight, thresholds)"
    status: completed
  - id: studio-control-config
    content: "NestStudio: Konfigurace před spuštěním – režim odpovědí (Dotazník / Načíst ze souboru), Reasoning mode, případně CBR parametry"
    status: completed
  - id: studio-question-full
    content: "NestStudio: Question UI – Accepted/Selected values, Weight v rozsahu, Why/Through results/Save answers, stavy odpovědi (Certainly yes/no, Irrelevant, Unknown, Postpone)"
    status: completed
  - id: studio-result-full
    content: "NestStudio: Result UI – tabulka Min/Max weight, Status, Type; filtry; How (explainability); Store case, Export results, Save answers"
    status: completed
  - id: studio-contexts
    content: "NestStudio: Editor kontextů – seznam, detail (condition), New context"
    status: completed
  - id: studio-integrity
    content: "NestStudio: Editor integritních omezení – seznam, detail (condition, conclusions), New integrity constraint"
    status: completed
  - id: studio-statistics
    content: "NestStudio: Statistics – přehled KB (počty atributů/propozic/pravidel podle typu a role)"
    status: completed
  - id: studio-new-attribute-type
    content: "NestStudio: Při přidání atributu dialog výběru typu Binary / Single / Set / Numeric"
    status: completed
  - id: format-store-case
    content: "NestFormat: Formát a export/import pro Store case (celá konzultace – odpovědi + výsledky + komentář)"
    status: completed
isProject: false
---

# NEST Studio V2 – architektura a implementační plán

## Kontext ze starého NEST

Starý systém je v [old/](old/): knihovna **NestBase** (VB .NET, Classes/), DTD [old/base.dtd](old/base.dtd) a [old/answers.dtd](old/answers.dtd), referenční KB [old/Nemoce.xml](old/Nemoce.xml). Inference: backward chaining přes cíle (goal výroky), kompoziční pravidla (condition = disjunkce konjunkcí literálů, conclusions s váhami), neurčitost „standard“ (CTR = a*w, CONJ = min, DISJ = max, NORM na ±0.99). Numerické atributy: trapezoidální fuzzy z `fuzzy_lower_bound`, `crisp_lower_bound`, `crisp_upper_bound`, `fuzzy_upper_bound` ([VyrokNumeric.SpoctiVahu](old/NestBase/Classes/VyrokNumeric.vb)).

---

## 1. Datový model (C# třídy)

### NestCore – doménové entity


| Třída                 | Účel                             | Klíčové vlastnosti                                                                                                                                           |
| --------------------- | -------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Attribute**         | Atribut v KB                     | Id, Name, Type (Binary, Single, Multiple, Numeric), LegalValues (bounds nebo value list), Propositions, Sources (načíst/uložit; zdroj „infer“ = odvozování)  |
| **Proposition**       | Jedna hodnota/propozice atributu | Id, Name; u numeric: WeightFunction (FuzzyBounds: fuzzy_lower, crisp_lower, crisp_upper, fuzzy_upper)                                                        |
| **Literal**           | Odkaz na výrok v podmínce        | AttributeId, PropositionId? (u binary prázdné), Negation (bool)                                                                                              |
| **Conjunction**       | AND literálů                     | ListLiteral                                                                                                                                                  |
| **Condition**         | Podmínka pravidla (OR konjunkcí) | ListConjunction                                                                                                                                              |
| **Conclusion**        | Závěr pravidla                   | Literal + Weight (double)                                                                                                                                    |
| **CompositionalRule** | Pravidlo                         | Id, Priority?, IdContext?, ContextThreshold?, Condition, ListConclusion                                                                                      |
| **KnowledgeBase**     | Celá KB                          | Global (metadata), ListAttribute, ListContext, ListCompositionalRule, ListAprioriRule, ListLogicalRule, ListIntegrityConstraint (načíst/uložit i když no-op) |
| **AnswerSet**         | Odpovědi uživatele               | CaseDescription?, ListAttributeAnswer (AttributeId, Type, ListAnswer s Value, Weight?)                                                                       |
| **Result**            | Výstup inference                 | Goals (skóre per atribut/proposition), FiredRules (explainability), QuestionsAsked?, IntegrityViolations?                                                    |


**Global** (z base.dtd): Description, Expert, KnowledgeEngineer, Date, InferenceMechanism (string: standard | logical | …), WeightRange, DefaultWeight (unknown | irrelevant), GlobalPriority (first | last | minlength | maxlength | user), ContextGlobalThreshold, ConditionGlobalThreshold. Vše načítat a ukládat; režim „standard“ implementovat, ostatní no-op.

**Doplňkové globální parametry** (z UI „Control“ / „Global properties“): AttOfTypeEnvironment (Keep values | Clear values) – chování atributů typu environment mezi běhy; AnsweringMode (Dialog | Questionnaire | Load from file | …); ReasoningMode (Postpone | Without postpone); DisableSources (Files, External functions, Calculations) – načíst/uložit, v MVP no-op. Pro CBR: CasesStorePath, LoadGoalsFrom (knowledge base | cases store), CbrType (compositional | logical), SimilarityThreshold – načíst/uložit, no-op v MVP.

**Attribute:** Scope (case | environment) dle base.dtd. UsageRole (question | intermediate | goal | alone) – odvozené z pravidel při analýze KB, nebo uložené pro zobrazení ve Statistics a filtrech.

**UserAnswer / odpověď:** Kromě Value a Weight podporovat speciální stavy: CertainlyYes, CertainlyNo, Irrelevant, Unknown, PostponeAnswer – pro tlačítka v Question dialogu a mapování do vah v engine.

**Pozice (goal/question):** Atribut je „goal“, pokud se vyskytuje v závěrech pravidel ale ne v podmínkách (nebo dle UrciPozici – výroky v SeznamZaveru určují goal). Pro MVP: cíle = všechny výroky atributů, které jsou pouze v conclusions (bez podmínek).

---

## 2. Import/export XML (NestFormat)

- **Base XML → KnowledgeBase:** Parsování podle [base.dtd](old/base.dtd). Odstranit DOCTYPE před Load (kvůli kompatibilitě jako ve starém kódu), nebo volitelně validace proti DTD (XmlReader + DTD). Mapování: `//global` → Global, `//attributes/attribute` → Attribute (type → Binary/Single/Multiple/Numeric, legal_values, propositions s weight_function), `//contexts/context`, `//rules/compositional_rules/compositional_rule` → Condition (conjunction/literal) + Conclusions.
- **KnowledgeBase → Base XML:** Generování XML kompatibilního s base.dtd (encoding UTF-8 nebo windows-1250 dle požadavku kompatibility).
- **Answers XML:** [answers.dtd](old/answers.dtd): `answers` → AnswerSet, `attribute` → AttributeAnswer s `answer` (value, weight). Export z UI odpovědí do answers XML před voláním inference.
- **Result XML:** Zachovat strukturu typu results/goals (jako SaveGoalsToXML ve starém BazeZnalosti) + rozšířit o explainability (fired rules, příspěvky vah).

**Validace:** Unikátní id atributů a pravidel, existence odkazů (id_attribute, id_proposition, id_context), legal bounds u numeric. NestFormat projekt: služby `IBaseXmlReader`, `IBaseXmlWriter`, `IAnswersXmlReader`, `IAnswersXmlWriter`, `IResultXmlWriter`, volitelně `IDtdValidator`.

---

## 3. Inference engine (NestCore)

### Algoritmus (režim „standard“)

1. **Inicializace:** Načíst KB a AnswerSet. Pro každý atribut aplikovat odpovědi: u binary/multiple nastavit váhu výroku (z answer weight nebo 1/-1); u numeric pro každou proposition spočítat fuzzy membership (trapezoidální vzorec z [VyrokNumeric.vb](old/NestBase/Classes/VyrokNumeric.vb) ř. 88–98) a uložit jako váhu výroku. Default (unknown/irrelevant) dle KB.
2. **Cíle:** Určit goal výroky (atributy v conclusions bez vstupu v podmínkách, nebo explicitní seznam z KB).
3. **Backward chaining:** Pro každý goal výrok volat „vyhodnoť výrok backward“. Vyhodnocení výroku: nejdřív zdroje (user = již nastaveno z odpovědí; infer = odvozování). Zdroj odvozování: pro každé pravidlo, v jehož závěru je tento výrok, vyhodnotit podmínku (condition). Podmínka: disjunkce konjunkcí – konjunkce = CONJ(literálů), disjunkce = DISJ(konjunkcí). Literál: váha = váha výroku, při negaci NEG. Pravidlo: pokud condition splněna (MaxHodnota > 0), pro každý závěr CTR(condition.Vaha, conclusion.Weight), případně NEG a NORM; výsledek přičíst k cílovému výroku (součet příspěvků z více pravidel – GLOB nebo součet dle starého chování). Kontext: pokud pravidlo má context, nejdřív vyhodnotit kontext a použít jeho váhu jako modulátor (stejně jako [PravidloKompozicionalni.vb](old/NestBase/Classes/PravidloKompozicionalni.vb)).
4. **Neurčitost standard:** CTR(a, w): pokud a > 0 pak a*w else 0. CONJ = min(Min, Min), min(Max, Max). DISJ = max. NORM = ořez na ±0.99 (nebo dle WeightRange). Interval jako (Min, Max) pro průběžné výpočty.
5. **Explainability:** Při každém „fire“ pravidla zapisovat záznam: RuleId, ConditionScore, Conclusions (AttributeId, PropositionId, Weight, ContributedScore). Výstup: seznam FiredRule + pro každý goal součet příspěvků a seznam pravidel, která k němu přispěla.

### Rozhraní

- `IInferenceEngine.Run(KnowledgeBase kb, AnswerSet answers, InferenceOptions options)` → `InferenceResult` (scores, FiredRules, GoalsOrdered).
- `IUncertaintyCalculation`: Standard (CTR, CONJ, DISJ, NORM), rozšířitelné pro logical později.

---

## 4. Architektura řešení

```
NESTv2/
├── src/
│   ├── NestCore/           # .NET 8 class library
│   │   ├── Model/          # Attribute, Proposition, Rule, Literal, Conclusion, KnowledgeBase, AnswerSet, Result
│   │   ├── Inference/      # Engine, Uncertainty (Standard), BackwardChain, ExplainabilityLog
│   │   └── ...
│   ├── NestFormat/         # .NET 8 class library (ref NestCore)
│   │   ├── BaseXmlReader.cs, BaseXmlWriter.cs
│   │   ├── AnswersXmlReader.cs, AnswersXmlWriter.cs
│   │   ├── ResultXmlWriter.cs
│   │   └── Validation/     # DTD nebo manuální validace
│   └── NestStudio/         # Avalonia MVVM app (ref NestCore, NestFormat)
│       ├── ViewModels/    # KB editor, Consultation, Rule editor
│       ├── Views/          # Liquid glass style okna
│       ├── Services/       # File, Inference, Dialog
│       └── ...
├── tests/
│   ├── NestCore.Tests/     # Unit testy inference, fuzzy, parser
│   └── NestFormat.Tests/   # Načtení Nemoce.xml, answers, roundtrip
└── old/                    # Původní NEST (referenční)
```

- **NestStudio:** Avalonia UI, MVVM, CommunityToolkit.Mvvm nebo ReactiveUI. Undo/redo v editoru (Command pattern nebo Immutable KB snapshoty). Autosave/versioning: periodické ukládání do .neststudio nebo kopie do backup složky.

---

## 5. UI – NEST Studio (liquid glass / Fluent)

- **Technologie:** Avalonia UI (cross-platform), styl „liquid glass“: poloprůhledné plochy, rozostření (Acrylic/Blur), jemné ohraničení, světlá/tmavá tema. Inspirace: Fluent Design, Windows 11 / moderní desktop.
- **Hlavní obrazovky:**
  - **Welcome / Project:** Otevřít KB (XML), nová KB, poslední soubory.
  - **Knowledge Base Editor:** Levý panel – strom nebo seznam atributů a pravidel. Detail atributu: typ (binary/multiple/numeric), propositions (pro multiple/numeric), u numeric editor bounds (fuzzy/crisp). Editor pravidel: podmínka (AND literálů – výběr atribut + proposition + negace), závěry (atribut, proposition, negace, váha). Toolbar: Validace, Import/Export XML, Uložit.
  - **Consultation / Runner:** Načíst KB → zobrazení „dotazníku“ (jedna sekce na atribut: binary = ano/ne, multiple = multi-select, numeric = číslo + slider nebo text). Tlačítko „Spustit inferenci“ → odpovědi export do answers XML → volání engine → výsledky: seznam cílových výroků se skóre (řazení), expandovatelné „Proč“ (fired rules + příspěvky). Export výsledku do XML.
- **Graf pravidel:** Vizualizace závislostí – uzly = atributy/výroky, hrany = pravidla (podmínka → závěr). Záložka „Tree“ (strom podmínek pod cíli) + „Graph“. Možnost použít LiveCharts2 nebo vlastní kreslení na Canvas, nebo export do Graphviz/Mermaid.
- **Globální vlastnosti:** Dialog/záložka v KB Editoru: Base description, Expert, Knowledge engineer, Date, Inference mechanism, Global priority, Default weight, Weight range, Global context/condition threshold – vše editovatelné.
- **Konfigurace před spuštěním (Control):** Rule-based vs case-based; Answering mode (Dotazník / Načíst odpovědi ze souboru), Reasoning mode (Postpone/Without postpone), Uncertainty, Priority, Default weight, Disable sources; pro CBR: Cases store, Load goals, Práh podobnosti (načíst/uložit, MVP no-op).
- **Question UI:** Pro multiple: Accepted values / Selected values + přesun; Weight v rozsahu; tlačítka Why, Through results, Save answers; Certainly yes/no, Irrelevant, Unknown, Postpone; Confirm Answer.
- **Result UI:** Tabulka Name, Min/Max weight, Status, Type; filtry Goals/Vše, pouze kladné/záporné/vše; Comment; Change answers, How (explainability), Store case, Export results, Save answers.
- **Store case:** Uložení celé konzultace (odpovědi + výsledky + komentář) do souboru pro pozdější načtení.
- **Editor pravidel:** Seznam pravidel + pořadí; strom condition, dostupné literály, AND, Negation; metadata (ID, Priority, Context, Context threshold); závěry potential/selected, Weight, Negation; New/Delete rule.
- **Editor kontextů a integritních omezení:** Seznam + detail (condition, conclusions u IO), New context / New integrity constraint.
- **Statistics:** Přehled KB – počty atributů/propozic/pravidel podle typu a role (question, intermediate, goal), globální parametry.
- **Nový atribut:** Dialog výběru typu Binary / Single / Set / Numeric.

---

## 6. Workflow a validace

- **Editor:** Při změně validace na pozadí: unikátní id, reference na existující atributy/propositions, legal bounds. Pro numeric: Lower limit < Upper limit; u propozic numeric: Fuzzy lower ≤ Crisp lower ≤ Crisp upper ≤ Fuzzy upper. Zobrazení chyb v panelu nebo pod poli.
- **Runner:** Před spuštěním ověřit, že všechny „question“ atributy mají odpověď (nebo explicitně označit jako „unknown“). Po inference zobrazit skóre a explainability v jednom panelu.

---

## 7. Milníky


| Milestone              | Obsah                                                                                                                                                                            |
| ---------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **M1 MVP**             | Načtení KB z XML (Nemoce.xml), načtení/export answers, Runner: dotazník z atributů → answers XML → inference → výsledky (skóre cílových atributů, pořadí). Bez editoru pravidel. |
| **M2 Editor pravidel** | KB Editor: seznam atributů + editor typu a propositions, editor compositional rules (condition + conclusions), validace, export/import base XML. Undo/redo.                      |
| **M3 Fuzzy**           | Plná podpora numeric: fuzzy bounds v UI, výpočet membership v engine, testy na Nemoce.xml (teplota normalni/zvysena).                                                            |
| **M4 Explainability**  | Fired rules log v engine, UI: „Proč“ u každého cíle (seznam pravidel + váhy). Export výsledku včetně explainability do XML.                                                      |
| **M5 Graf pravidel**   | Vizualizace grafu pravidel (uzly = atributy/výroky, hrany = pravidla). Liquid glass styling v celé aplikaci.                                                                     |


---

## 8. Referenční soubory a testy

- **Referenční KB:** [old/Nemoce.xml](old/Nemoce.xml) – použít pro unit testy (NestFormat načtení, roundtrip) a integrační test inference (známé odpovědi → očekávané skóre).
- **Unit testy:** Parser base.xml (všechny atributy, pravidla c1–c14), parser answers; inference: jeden rule fire (literal + conclusion), konjunkce dvou literálů, fuzzy numeric (hodnota v crisp vs fuzzy pásmu); explainability: jedna fired rule v logu.

---

## 9. Todo (pro soubor todo.md)

V plánu jsou úkoly v frontmatteru (todos) včetně doplňků z původního editoru: datový model (Scope, globální parametry, stavy odpovědi, UsageRole), UI (Globální vlastnosti, Control konfigurace, plné Question/Result UI, editory kontextů a integritních omezení, Statistics, výběr typu při novém atributu), formát Store case. Po schválení plánu lze vytvořit soubor [todo.md](todo.md) v kořeni repo s těmito položkami a checkboxy pro sledování.