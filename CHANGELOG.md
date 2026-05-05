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

# V 1.0.0.500

### Opravené bugy z plánu fix_5_bugs (původní úkol)
- **Dark mode podle systému** – `App.axaml` přepsán na `ResourceDictionary.ThemeDictionaries` se samostatnou `Light` i `Dark` paletou (`AppWindowBgBrush`, `MainPanelBgBrush`, `GlassPanelBgBrush`, `WelcomePanelBgBrush`, `EditorSidebarBgBrush`, `EditorDetailBgBrush`, `EditorToolbarBgBrush`, `WelcomeGridBgBrush`, `WelcomeCardBgBrush`, `WelcomeCardBorderBrush`, `WelcomeDividerBrush`, `DialogTitleFgBrush`, `DialogLabelFgBrush`, …); hardcoded barvy ve stylech nahrazeny `DynamicResource`, takže přepnutí systému na tmavý režim aplikaci skutečně přebarví. `RequestedThemeVariant="Default"` zůstává.
- **Přejmenování atributu v pravidlech** – Přidán `KnowledgeBaseReferenceUpdater` v `NestCore.Model`. Při změně ID atributu/výroku v `AttributeEditView` projde všechny `CompositionalRules`, `LogicalRules`, `AprioriRules`, kontexty i integritní omezení a přepíše `Literal.AttributeId`, `Literal.PropositionId`, `Conclusion.AttributeId`, `Conclusion.PropositionId` ze starého ID na nové; pravidla a kontexty se po přejmenování nerozsypou. Volá se jen když se ID skutečně změnilo (ne při každém `LostFocus`).
- **TAB navigace v editoru atributů** – Na obou `ScrollViewer`ech v `KbEditorView.axaml` (sidebar i detail) je `input:KeyboardNavigation.TabNavigation="Continue"`. Programatický `AttributeEditView` přiděluje `TabIndex` všem textboxům/ComboBoxům v pořadí vytvoření a `commentBox` má `AcceptsTab=false` (i přes `AcceptsReturn=true`), aby tabulátor procházel hladce mezi poli a nezasekl se v komentáři.
- **Intervalové váhy `min;max`** – `Answer` v `NestCore` má novou property `MinWeight`. `ConsultationView.BuildAnswerSet` parsuje vstup: pokud obsahuje `;`, rozdělí na dvojici (`min;max`) a nastaví `ans.MinWeight = min`, `ans.Weight = max`; jinak chování beze změny. `InferenceEngine.Run` při aplikování odpovědi vytvoří interval `[ToInternal(MinWeight ?? Weight), ToInternal(Weight)]`. Watermark inputu napovídá `„váha (nebo min;max)"`. Parser akceptuje `.` i `,` jako oddělovač desetin.

### Editor znalostní báze
- **Stabilní fokus textových polí** – Editace ID/jména/komentáře už neresetuje výběr v postranním panelu; přepis `ItemsSource` listů je obalen suppress flagem (`_suppressDetailFromSelection`), takže při ztrátě fokusu nedochází k mizení detailu ani k „rozbití" kliknutí na další textbox
- **Sidebar – přerovnání** – Hlavičky sekcí (ATRIBUTY, PRAVIDLA, KONTEXTY, INTEGRITNÍ OMEZENÍ) jsou nad tlačítky **+ Nový / Smazat**, takže se „Smazat" vejde i u INTEGRITNÍCH OMEZENÍ
- **Sloupce do plné výšky** – Postranní panel a detail teď roztáhnou na celou výšku okna; zaoblené rohy se nezařezávají
- **Vlastní dialog Uložit jako** – Místo nativního Win SaveFilePicker se otevírá `SaveKbAsDialog` ve stejném `glass-panel` stylu (cesta + typ XML/NKB + tlačítko Procházet)
- **Otevírání `.nkb`** – Filtr otevíracího dialogu rozšířen o `*.nkb` (export starého NESTu, formát XML; parsování beze změny)
- **„Uložit XML"** – `FileTypeChoices` dialogu nabízí XML i NKB; přípona se synchronizuje s výběrem
- **Bug – dvojí dotaz na uložení** – `OnBack` v editoru už neduplikuje confirm; po volbě „Neukládat" se editor označí čistý (`MarkClean()`), takže `ShowWelcome` druhý dotaz neukáže

### Inference / konzultace
- **CTROne podpora záporných vah** – Logika `a*w + b*(1-|w|)` rozeznává znaménko vlivu i pro záporné váhy
- **Konzultace** – Hlavička s tlačítkem Zpět ve stejném design jazyku (rounded `toolbar-back-btn`)

### Welcome / hlavní menu
- **Pevný layout uvítací karty** – Karta má pevné `Width × Height` a strukturu „logo / sloupce (\*) / O aplikaci / patička“; pozice tlačítka **O aplikaci** a patičky se nemění podle toho, jestli je projekt načtený
- **Kompaktnější patička** – Verze, tlačítko aktualizace a partnerská loga (OPF / RAPL) ve vlastní třídě `welcome-updates-btn`
- **Filtr otevíracího dialogu** – Doplněn `*.nkb` vedle `*.xml`

### Vzhled & témata
- **Sjednocené plátno** – Plátno aplikace `#F2F4F9`, okno úplně vzadu bílé; bílé karty (uvítací panel, editor, dialog) na šedém pozadí mají viditelný 1px obrys (`#D8DEE7`)
- **Zaoblený design language napříč UI** – Sekundární tlačítka (welcome, glass-panel, glass-card, editor-toolbar, editor-sidebar, editor-detail) mají `CornerRadius=12`, jednotný padding, viditelný rámeček i bez hoveru
- **Tlačítko Zpět** – Třída `toolbar-back-btn` (kulaté, bordered, SemiBold) – stejný styl v editoru i konzultaci
- **Editor toolbar tlačítka** – Sekundární akce (Vyplnit projekt, Validace, Graf pravidel, Globální vlastnosti, Statistiky) mají vlastní zaoblený styl, viditelné jsou už bez hoveru
- **Light/Dark dictionaries** – `App.axaml` má kompletní paletu pro `Light` i `Dark` variantu, přepínání přes `RequestedThemeVariant`
- **Stínové efekty** – Glass i welcome panely používají dvouvrstvý `BoxShadow` (přímý SolidColorBrush, ne `DynamicResource`), aby se aplikace nesypala při startu

### Dialogy
- **`SaveChangesDialog`** – Předělán do `glass-panel` stylu (titulek + popisek + 3 tlačítka)
- **`SaveKbAsDialog`** – Nový dialog pro uložení znalostní báze (XML / NKB)
- **Konzistence** – Všechny modální dialogy (`NewAttributeTypeDialog`, `NewRuleTypeDialog`, `RunConfigDialog`, …) nasazené na `AppWindowBgBrush`, obsah v `glass-panel`

### Okno aplikace
- **Default `1100×900`, MinSize `900×600`** – Vejde se uvítací karta i editor bez vnějšího scrollu i v non-maximized režimu
- **`MainContent`** – Z `ScrollViewer` nahrazen za `ContentControl`, takže `KbEditorView` se roztáhne na celou výšku a respektuje stretch
- **Komprimovaný titulek hlavičky** – `Border.panel` margin/padding sníženy, podtitulek `Konzultace / Runner` užší řádek
