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

# V 1.0.0.400

### Inference engine
- **Oprava škálování vah** – Engine správně respektuje `weight_range` z globálního nastavení projektu; odstraněno dvojité násobení při zobrazení výsledků
- **Chytrá relevance otázek** – Při vyplňování se automaticky skrývají otázky, které jsou irelevantní na základě AND konjunkcí v pravidlech (např. pokud v `IF A AND B THEN C` odpovíte na A "Určitě ne", otázka na B zmizí, protože celá konjunkce je mrtvá); zodpovězené otázky zůstávají vždy viditelné pro možnost změny odpovědi

### Konzultace (vyplňování)
- **Režim zobrazení otázek** – Slider pro výběr mezi zobrazením všech otázek najednou a zobrazením po jedné (s navigací Předchozí/Další)
- **Výběr typu neurčitosti** – Před spuštěním konzultace lze vybrat typ: Standardní, Logický, Neuronový, Hybridní, Gödel, Součinová
- **Zobrazení komentářů** – Komentáře u atributů (např. "Jak moc je potenciální manželka bohatá") i u výroků (např. "malé: tisíc") se zobrazují přímo v dotazníku

### Editor znalostní báze
- **Typy atributů** – Při vytvoření nového atributu dialog pro výběr typu: Binární, Jednoduchý (vylučující – vybrání jedné propozice automaticky nastaví ostatním nejnižší váhu), Množinový (nevylučující – nezávislé váhy), Numerický (výška, teplota apod.)
- **Komentáře** – Pole pro komentář u atributů (např. "Jak moc má pacient rýmu") i výroků (např. "Velké: Pacient má velkou rýmu")
- **Typy pravidel** – Podpora apriorních (default pravidlo, platí vždy, bez podmínek – např. "vždy spíše půjčí"), logických (pravda/nepravda) a kompozicionálních (váhy od -1 do 1) pravidel s výběrem při vytvoření
- **Editor pravidel** – Konjunkce (AND), disjunkce (OR přes více konjunkcí), negace literálů, negace závěrů, textové shrnutí pravidla (IF ... THEN ...)
- **Váhy závěrů** – Podpora kladných i záporných vah v celém rozsahu ±weight_range

### Vizualizace
- **Strom pravidel** – Nová záložka "Strom" s hierarchickým zobrazením cílů → pravidel → literálů
- **Graf pravidel** – Vylepšený vrstvený graf s barevně odlišenými uzly (otázky, mezivýstupy, cíle) a hranami podle typu pravidla
- **Klikatelné uzly** – Klik na uzel ve stromu i grafu otevře editor atributu

### Design
- **Liquid glass UI** – Kompletní redesign editoru KB, editoru pravidel, editoru atributů a dialogů do jednotného "liquid glass" stylu
- **Nové styly** – `editor-sidebar`, `editor-detail`, `editor-section`, `editor-toolbar`, `section-title`, `field-label`

### XML formát
- **Čtení/zápis typů pravidel** – Podpora `<apriori_rules>`, `<logical_rules>`, `<compositional_rules>` sekcí v XML
- **Fallback jmen** – Pokud v XML chybí `<name>`, použije se `<id>` jako zobrazované jméno

### Ostatní
- **Distribuce** – Podpora single-file portable `.exe` buildu
- **Aktualizace** – Kontrola nových verzí z GitHub Releases s panelem pro stažení
- **.gitignore** – Přidán pro `.vs/` a build artefakty

