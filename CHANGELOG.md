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

