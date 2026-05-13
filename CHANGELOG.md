# V 1.0.0.601

### Konzultace / inference
- **Načítání answers.xml ve správném kódování** – Nový `XmlFileEncoding.ReadAllText` v `NestFormat` (BOM, `encoding="windows-1250"`, heuristika UTF-8 vs CP1250). Konzultace „ze souboru“ a `ConsoleRunner` ho používají; dříve `File.ReadAllText` bez kódování kazilo české znaky v ID atributů a inference pak neodpovídala starému NESTu.
- **Regresní test kytara** – `KytaraGoldenInferenceTests` porovnává cílové váhy pro `tests/kytara-all-levels.xml` + `tests/kytara-nylon-konzult.txt` s referencí ze starého NEST.
- **ID odpovědí vs. jméno atributu** – V souboru answers může být v `<id>` krátké jméno z KB (`tlustoprst`) místo kanonického `id` (`tlusté prsty`). Přehled načtení i `InferenceEngine` teď párují i podle `name` a s NFC normalizací.

# V 1.0.0.600

### Graf pravidel / explainability
- **Graf vrstev obrácen** – V `RulesGraphView` je pořadí teď `Cíle → Meziuzly → Vstupy`, takže graf čteš shora dolů podle cíle.
- **Lepší čitelnost uzlů** – Uzel je větší (`160×56`), text není natvrdo useknutý na 14 znaků a používá 2 řádky s ellipsis.
- **How graf + strom** – V `Proč (How)` je tlačítko **Zobrazit graf průběhu**; otevře full-view `HowGraphView` se záložkami `Graf` a `Strom` (barevně +/− příspěvky) a tlačítkem zpět na výsledky.
- **Fix formátu znamének v How** – Odstraněno `+ -0.499`; výpis má korektní `+0.499` / `-0.499`.

### Editor
- **Přepínání atribut/pravidlo bez dvojkliku** – Selection handlery v `KbEditorView` používají `_suppressDetailFromSelection` i při cross-clear ostatních listů, takže přechod mezi sekcemi už nevyžaduje 2 kliknutí.
- **Shrnutí pravidla s negací váhy** – `CompositionalRule.ToSummary()` teď u negovaného závěru ukáže zápornou váhu (`NOT X@-0.500`).
- **Text akce** – Tlačítko `Vyplnit projekt` přejmenováno na `Konzultace projektu` (welcome + editor toolbar).

### Konzultace
- **Typ neurčitosti zjednodušen** – V dialogu konfigurace jsou vidět jen `Standardní`, `Logický`, `Neuronový` (ostatní možnosti zůstávají v kódu).
- **Načítání odpovědí ze souboru** – Po načtení se vlevo zobrazí přehled všech vstupních otázek s tím, co je v souboru (`stav`, `value`, `váha`/`min;max`, nebo `Neuvedeno`).
- **File picker pro odpovědi** – Dialog „Načíst odpovědi ze souboru“ umožňuje i `*` (všechny soubory), nejen `*.xml`.
- **One-by-one auto-next** – Po kliknutí na předpřipravený status (a u single i po volbě radio) se to automaticky posune na další otázku.
- **Auto-check při ruční váze** – U multiple se při ručním vyplnění váhy automaticky zaškrtne příslušná položka.
- **Auto-run inference** – Jakmile jsou zodpovězené všechny aktuálně relevantní otázky, inference se spustí automaticky.

### Vzhled
- **Hover sekundárních tlačítek** – `SecondaryBtnHoverBrush` změněn na světle modrou (`Light: #DBEAFE`, `Dark: #1E40AF`) místo šedé.