# NEST Studio – TODO

## Milníky

- [ ] **M1 MVP** – Načtení KB z XML, answers, Runner: dotazník → inference → výsledky (skóre, pořadí)
- [ ] **M2 Editor** – KB Editor: atributy, propozice, compositional rules, validace, import/export, undo/redo
- [ ] **M3 Fuzzy** – Plná podpora numeric (fuzzy bounds v UI, membership v engine)
- [ ] **M4 Explainability** – Fired rules log, UI „Proč“, export výsledku s explainability
- [ ] **M5 Graf** – Graf pravidel, liquid glass styling

## Úkoly

### Architektura
- [x] Řešení .sln: NestCore, NestFormat, NestStudio, NestCore.Tests, NestFormat.Tests
- [x] NestCore: datový model (Attribute, Proposition, Literal, Condition, Conclusion, Rule, KB, AnswerSet, InferenceResult, Global)

### NestFormat (XML I/O)
- [x] BaseXmlReader/Writer dle base.dtd, načtení Nemoce.xml, roundtrip test
- [x] AnswersXmlReader/Writer dle answers.dtd
- [x] ResultXmlWriter (výsledky + explainability)
- [x] Store case formát: CaseStoreWriter, CaseStoreReader, tlačítko Store case v Result UI

### NestCore (engine)
- [x] Inference engine „standard“: backward chaining, CTR/CONJ/DISJ/NORM, aplikace odpovědí
- [x] Fuzzy membership pro numeric (trapezoidální bounds)
- [x] Explainability log (FiredRules, příspěvky vah)

### NestStudio (UI)
- [x] Avalonia projekt, liquid glass / Fluent styling, MVVM
- [x] Consultation/Runner: načtení KB, dotazník, answers XML, inference, zobrazení skóre a pořadí
- [x] KB Editor: atributy, propozice, pravidla, validace, import/export
- [x] Graf pravidel
- [x] Globální vlastnosti
- [x] Statistiky
- [x] Editor kontextů
- [x] Konfigurace před spuštěním (režim odpovědí, Reasoning mode, načtení answers ze souboru)
- [ ] Question/Result UI v plném rozsahu — rozšířeno: stavy odpovědi, Uložit odpovědi, Result tabulka + filtry + How, Store case, Export
- [x] Editor integritních omezení

### Testy
- [x] NestFormat: načtení Nemoce.xml, roundtrip
- [x] NestCore: unit testy inference, fuzzy
- [x] Integrační test s referenčními odpověďmi (NestFormat.Tests InferenceTests + NestCore.Tests)
