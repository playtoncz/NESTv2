using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using NestCore.Inference;
using NestCore.Model;
using NestFormat;

using Avalonia.Platform.Storage;

namespace NestStudio.Views;

public partial class ConsultationView : UserControl
{
    private readonly KnowledgeBase? _kb;
    private readonly string? _basePath;
    private readonly MainWindow? _mainWindow;
    private readonly ConsultationRunConfig? _runConfig;
    private readonly List<(NestCore.Model.Attribute attr, Control input, AttributeAnswer aa)> _inputs = new();
    private readonly Dictionary<string, (TextBox? weightBox, StackPanel? manualPanel, Button? manualBtn)> _manualWeightPanels = new();
    /// <summary>U binárních atributů: true = použít ruční váhu z pole, false = použít stav tlačítek (Určitě ano/ne atd.).</summary>
    private readonly Dictionary<string, bool> _binaryUseManualWeight = new();
    /// <summary>U binárních atributů: seznam tlačítek stavu (Určitě ano, …) kvůli odvybrání při přepnutí na „váhu ručně“.</summary>
    private readonly Dictionary<string, List<Button>> _binaryStatusButtons = new();
    /// <summary>U multiple atributů: pro každou propozici vlastní váha (attrId -> seznam (checkbox, weightBox)).</summary>
    private readonly Dictionary<string, List<(CheckBox cb, Proposition prop, TextBox weightBox)>> _multipleWeights = new();
    private AnswerSet? _loadedAnswers;
    private InferenceResult? _lastResult;

    /// <summary>Pro runtime loader / XAML. V kódu používej konstruktor s KB a MainWindow.</summary>
    public ConsultationView()
    {
        InitializeComponent();
    }

    public ConsultationView(KnowledgeBase kb, string? basePath, MainWindow mainWindow, ConsultationRunConfig? runConfig = null)
    {
        _kb = kb;
        _basePath = basePath;
        _mainWindow = mainWindow;
        _runConfig = runConfig;
        InitializeComponent();
        TitleText.Text = kb.Global.Description ?? "Konzultace";
        BackButton.Click += (_, _) => _mainWindow.ShowWelcome();
        RunButton.Click += OnRunInference;
        ExportResultButton.Click += OnExportResult;
        if (_runConfig?.AnsweringMode == AnsweringModeKind.LoadFromFile && !string.IsNullOrWhiteSpace(_runConfig.AnswersFilePath))
            TryLoadAnswersFromFile();
        else
            BuildQuestionnaire();
        ResultsPanel.Children.Add(new TextBlock
        {
            Text = "Spusťte inference pro zobrazení výsledků.",
            Opacity = 0.7,
            Margin = new Thickness(0, 8, 0, 0)
        });
    }

    private void TryLoadAnswersFromFile()
    {
        var path = _runConfig!.AnswersFilePath!.Trim();
        try
        {
            if (!File.Exists(path))
            {
                QuestionnairePanel.Children.Add(new TextBlock { Text = "Soubor neexistuje: " + path, Foreground = new SolidColorBrush(Colors.DarkRed) });
                BuildQuestionnaire();
                return;
            }
            var xml = File.ReadAllText(path);
            var reader = new AnswersXmlReader();
            _loadedAnswers = reader.Read(xml);
            QuestionnairePanel.Children.Clear();
            QuestionnairePanel.Children.Add(new TextBlock
            {
                Text = "Odpovědi načteny ze souboru: " + path,
                FontWeight = FontWeight.SemiBold
            });
            QuestionnairePanel.Children.Add(new TextBlock { Text = $"Počet atributů s odpověďmi: {_loadedAnswers.Attributes.Count}", Margin = new(0, 4, 0, 0) });
        }
        catch (System.Exception ex)
        {
            QuestionnairePanel.Children.Add(new TextBlock { Text = "Chyba načtení: " + ex.Message, Foreground = new SolidColorBrush(Colors.DarkRed) });
            BuildQuestionnaire();
        }
    }

    private void BuildQuestionnaire()
    {
        if (_kb == null) return;
        QuestionnairePanel.Children.Clear();
        _inputs.Clear();
        _manualWeightPanels.Clear();
        _binaryUseManualWeight.Clear();
        _binaryStatusButtons.Clear();
        _multipleWeights.Clear();
        var weightRange = _kb.Global.WeightRange;
        var inputIds = QuestionAnalyzer.GetInputAttributeIds(_kb);
        var orderedAttrs = QuestionAnalyzer.GetInputAttributesInOrder(_kb);
        if (orderedAttrs.Count == 0)
        {
            foreach (var attr in _kb.Attributes)
            {
                if (attr.Scope != ScopeKind.Case) continue;
                orderedAttrs.Add(attr);
            }
        }
        var conditionalBlocks = new List<(Control wrapper, List<string> depAttrIds)>();
        Action? onAnyAnswerChanged = null;
        foreach (var attr in orderedAttrs)
        {
            if (attr.Scope != ScopeKind.Case) continue;
            if (inputIds.Count > 0 && !inputIds.Contains(attr.Id))
                continue;

            var attrTitle = string.IsNullOrWhiteSpace(attr.Name) ? attr.Id : attr.Name!;
            var block = new StackPanel { Spacing = 6 };
            block.Children.Add(new TextBlock
            {
                Text = attrTitle,
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                Margin = new Thickness(0, 0, 0, 2)
            });

            var aa = new AttributeAnswer { Id = attr.Id, Type = attr.Type };
            Control input;
            switch (attr.Type)
            {
                case AttributeType.Binary:
                    // U binárních jen dvě tlačítka – „Určitě ano“ a „Určitě ne“ (žádný checkbox)
                    input = new Border { IsVisible = false };
                    break;
                case AttributeType.Single:
                    var singlePanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
                    foreach (var p in attr.Propositions)
                    {
                        var label = string.IsNullOrWhiteSpace(p.Name) ? p.Id : p.Name;
                        var rb = new RadioButton { Content = new TextBlock { Text = label }, GroupName = attr.Id, Tag = p };
                        singlePanel.Children.Add(rb);
                    }
                    input = singlePanel;
                    break;
                case AttributeType.Multiple:
                    var multiPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 6 };
                    var multiWeights = new List<(CheckBox cb, Proposition prop, TextBox weightBox)>();
                    foreach (var p in attr.Propositions)
                    {
                        var label = string.IsNullOrWhiteSpace(p.Name) ? p.Id : p.Name;
                        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                        var cb = new CheckBox { Content = new TextBlock { Text = label }, Tag = p };
                        var weightBox = new TextBox { Width = 56, Watermark = "váha", Tag = p };
                        row.Children.Add(cb);
                        row.Children.Add(new TextBlock { Text = "váha:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new(8, 0, 0, 0) });
                        row.Children.Add(weightBox);
                        var rowButtons = new List<Button>();
                        void AddRowStatusButton(string lbl, AnswerSpecialStatus status, string weightValue)
                        {
                            var btn = new Button { Content = lbl, Padding = new(6, 2), Tag = status };
                        btn.Click += (_, _) =>
                        {
                            weightBox.Text = weightValue;
                            foreach (var b in rowButtons) b.Classes.Remove("primary");
                            btn.Classes.Add("primary");
                            onAnyAnswerChanged?.Invoke();
                        };
                            row.Children.Add(btn);
                            rowButtons.Add(btn);
                        }
                        AddRowStatusButton("Určitě ano", AnswerSpecialStatus.CertainlyYes, "3");
                        AddRowStatusButton("Určitě ne", AnswerSpecialStatus.CertainlyNo, "-3");
                        AddRowStatusButton("Nerelevantní", AnswerSpecialStatus.Irrelevant, "0");
                        AddRowStatusButton("Neznámá", AnswerSpecialStatus.Unknown, "-3 až 3");
                        AddRowStatusButton("Odložit", AnswerSpecialStatus.PostponeAnswer, "");
                        multiPanel.Children.Add(row);
                        multiWeights.Add((cb, p, weightBox));
                    }
                    multiPanel.Children.Add(new TextBlock
                    {
                        Text = $"Rozsah: -{weightRange:F0} až {weightRange:F0}",
                        Opacity = 0.8,
                        FontSize = 12,
                        Margin = new(0, 4, 0, 0)
                    });
                    _multipleWeights[attr.Id] = multiWeights;
                    input = multiPanel;
                    break;
                case AttributeType.Numeric:
                    input = new TextBox { Watermark = "Číslo (volitelně váha)", MinWidth = 200 };
                    if (attr.LegalValues != null && (attr.LegalValues.LowerBound.HasValue || attr.LegalValues.UpperBound.HasValue))
                    {
                        var lo = attr.LegalValues.LowerBound ?? double.NaN;
                        var hi = attr.LegalValues.UpperBound ?? double.NaN;
                        var rangeText = double.IsNaN(lo) && double.IsNaN(hi) ? "" : (double.IsNaN(lo) ? "" : lo.ToString("F1", System.Globalization.CultureInfo.InvariantCulture))
                            + (double.IsNaN(lo) || double.IsNaN(hi) ? "" : " – ") + (double.IsNaN(hi) ? "" : hi.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
                        if (!string.IsNullOrEmpty(rangeText))
                            block.Children.Add(new TextBlock { Text = "Rozsah: " + rangeText, Opacity = 0.85, Margin = new(0, 0, 0, 4), FontSize = 12 });
                    }
                    break;
                default:
                    continue;
            }
            if (attr.Type != AttributeType.Binary)
                block.Children.Add(input);

            var statusPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Margin = new(0, 4, 0, 0) };
            var buttons = new List<Button>();
            void AddStatusButton(string label, AnswerSpecialStatus status)
            {
                var btn = new Button { Content = label, Padding = new(8, 4), Tag = status };
                btn.Click += (_, _) =>
                {
                    if (attr.Type == AttributeType.Binary)
                    {
                        _binaryUseManualWeight[attr.Id] = false;
                        if (_manualWeightPanels.TryGetValue(attr.Id, out var p))
                        {
                            if (p.manualPanel != null) p.manualPanel.IsVisible = false;
                            if (p.manualBtn != null) p.manualBtn.Classes.Remove("primary");
                        }
                    }
                    aa.SpecialStatus = status;
                    aa.Answers.Clear();
                    if (status == AnswerSpecialStatus.CertainlyYes)
                        aa.Answers.Add(new Answer { Value = "yes", Weight = 1.0 });
                    else if (status == AnswerSpecialStatus.CertainlyNo)
                        aa.Answers.Add(new Answer { Value = "no", Weight = 1.0 });
                    foreach (var b in buttons) b.Classes.Remove("primary");
                    btn.Classes.Add("primary");
                    onAnyAnswerChanged?.Invoke();
                };
                statusPanel.Children.Add(btn);
                buttons.Add(btn);
            }
            if (attr.Type == AttributeType.Binary)
            {
                statusPanel.Children.Add(new TextBlock { Text = "Odpověď:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                AddStatusButton("Určitě ano", AnswerSpecialStatus.CertainlyYes);
                AddStatusButton("Určitě ne", AnswerSpecialStatus.CertainlyNo);
                AddStatusButton("Nerelevantní", AnswerSpecialStatus.Irrelevant);
                AddStatusButton("Neznámá", AnswerSpecialStatus.Unknown);
                AddStatusButton("Odložit", AnswerSpecialStatus.PostponeAnswer);
            }
            else if (attr.Type == AttributeType.Numeric)
            {
                statusPanel.Children.Add(new TextBlock { Text = "Stav:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                AddStatusButton("Nerelevantní", AnswerSpecialStatus.Irrelevant);
                AddStatusButton("Neznámá", AnswerSpecialStatus.Unknown);
                AddStatusButton("Odložit", AnswerSpecialStatus.PostponeAnswer);
            }
            else
            {
                statusPanel.Children.Add(new TextBlock { Text = "Stav:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                AddStatusButton("Určitě ano", AnswerSpecialStatus.CertainlyYes);
                AddStatusButton("Určitě ne", AnswerSpecialStatus.CertainlyNo);
                AddStatusButton("Nerelevantní", AnswerSpecialStatus.Irrelevant);
                AddStatusButton("Neznámá", AnswerSpecialStatus.Unknown);
                AddStatusButton("Odložit", AnswerSpecialStatus.PostponeAnswer);
            }
            if (attr.Type != AttributeType.Multiple)
                block.Children.Add(statusPanel);
            if (attr.Type == AttributeType.Binary)
            {
                _binaryStatusButtons[attr.Id] = buttons;
                var manualPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4, Margin = new(0, 8, 0, 0), IsVisible = false };
                var manualBtn = new Button { Content = "Zadat váhu ručně", Padding = new(8, 4), HorizontalAlignment = HorizontalAlignment.Left };
                var weightBox = new TextBox { Watermark = "Váha", Width = 80, HorizontalAlignment = HorizontalAlignment.Left };
                var rangeHint = new TextBlock
                {
                    Text = $"Rozsah: -{weightRange:F0} až {weightRange:F0}",
                    Opacity = 0.8,
                    FontSize = 12
                };
                manualPanel.Children.Add(weightBox);
                manualPanel.Children.Add(rangeHint);
                manualBtn.Click += (_, _) =>
                {
                    _binaryUseManualWeight[attr.Id] = true;
                    manualPanel.IsVisible = true;
                    manualBtn.Classes.Add("primary");
                    foreach (var b in buttons)
                        b.Classes.Remove("primary");
                    aa.SpecialStatus = AnswerSpecialStatus.None;
                    aa.Answers.Clear();
                    onAnyAnswerChanged?.Invoke();
                };
                weightBox.TextChanged += (_, _) => onAnyAnswerChanged?.Invoke();
                block.Children.Add(manualBtn);
                block.Children.Add(manualPanel);
                _manualWeightPanels[attr.Id] = (weightBox, manualPanel, manualBtn);
            }
            if (attr.Type == AttributeType.Single && input is StackPanel singleSp)
            {
                foreach (var c in singleSp.Children)
                    if (c is RadioButton rb)
                        rb.IsCheckedChanged += (_, _) => onAnyAnswerChanged?.Invoke();
            }
            if (attr.Type == AttributeType.Numeric && input is TextBox numTb)
                numTb.TextChanged += (_, _) => onAnyAnswerChanged?.Invoke();
            _inputs.Add((attr, input, aa));
            var visibilityDeps = QuestionAnalyzer.GetVisibilityDependencies(_kb, attr.Id);
            var card = new Border();
            card.Classes.Add("glass-card");
            card.Child = block;
            if (visibilityDeps.Count > 0)
            {
                var wrapper = new StackPanel();
                wrapper.Children.Add(card);
                wrapper.IsVisible = false;
                conditionalBlocks.Add((wrapper, visibilityDeps));
                QuestionnairePanel.Children.Add(wrapper);
            }
            else
            {
                QuestionnairePanel.Children.Add(card);
            }
        }

        bool HasAttributeAnyValue(string attrId)
        {
            if (_multipleWeights.TryGetValue(attrId, out var list))
                return list.Any(x => x.cb.IsChecked == true);
            var inp = _inputs.FirstOrDefault(x => x.attr.Id == attrId);
            return inp != default && (inp.aa.Answers.Count > 0 || inp.aa.SpecialStatus != AnswerSpecialStatus.None);
        }
        void UpdateConditionalVisibility()
        {
            foreach (var (wrapper, depAttrIds) in conditionalBlocks)
                wrapper.IsVisible = depAttrIds.Any(HasAttributeAnyValue);
        }
        onAnyAnswerChanged = UpdateConditionalVisibility;
        foreach (var (wrapper, depAttrIds) in conditionalBlocks)
        {
            foreach (var depId in depAttrIds)
            {
                if (_multipleWeights.TryGetValue(depId, out var list))
                {
                    foreach (var (cb, _, _) in list)
                        cb.IsCheckedChanged += (_, _) => UpdateConditionalVisibility();
                }
            }
        }
        UpdateConditionalVisibility();

        var saveRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new(0, 16, 0, 0) };
        var saveBtn = new Button { Content = "Uložit odpovědi (XML)" };
        saveBtn.Click += (s, e) => SaveAnswersToFile();
        saveRow.Children.Add(saveBtn);
        QuestionnairePanel.Children.Add(saveRow);
    }

    private async void SaveAnswersToFile()
    {
        if (_mainWindow == null) return;
        var answers = BuildAnswerSet();
        var storage = _mainWindow.StorageProvider;
        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Uložit odpovědi (answers XML)",
            DefaultExtension = "xml",
            FileTypeChoices = new[] { new FilePickerFileType("XML") { Patterns = new[] { "*.xml" } } }
        });
        if (file == null) return;
        try
        {
            var writer = new AnswersXmlWriter();
            var xml = writer.Write(answers);
            await File.WriteAllTextAsync(file.Path.LocalPath, xml, System.Text.Encoding.UTF8);
        }
        catch (System.Exception) { }
    }

    private void OnRunInference(object? sender, RoutedEventArgs e)
    {
        if (_kb == null) return;
        var answers = _loadedAnswers ?? BuildAnswerSet();
        var engine = new InferenceEngine();
        var result = engine.Run(_kb, answers);
        _lastResult = result;
        ShowResults(result);
        ExportResultButton.IsEnabled = true;
    }

    private AnswerSet BuildAnswerSet()
    {
        var set = new AnswerSet();
        foreach (var (attr, input, aa) in _inputs)
        {
            aa.Answers.Clear();
            switch (attr.Type)
            {
                case AttributeType.Binary:
                    // Určitě ano = max v rozsahu (3), Určitě ne = min (-3), Nerelevantní = 0, Neznámý = (-3;3) — viz engine
                    if (_binaryUseManualWeight.GetValueOrDefault(attr.Id) && GetManualWeight(attr.Id) is { } manualW)
                        aa.Answers.Add(new Answer { Value = "yes", Weight = manualW });
                    else if (aa.SpecialStatus == AnswerSpecialStatus.CertainlyYes)
                    {
                        var maxWeight = _kb.Global.WeightRange > 0 && _kb.Global.WeightRange != 1 ? _kb.Global.WeightRange : 3;
                        aa.Answers.Add(new Answer { Value = "yes", Weight = maxWeight });
                    }
                    else if (aa.SpecialStatus == AnswerSpecialStatus.CertainlyNo)
                    {
                        var maxWeight = _kb.Global.WeightRange > 0 && _kb.Global.WeightRange != 1 ? _kb.Global.WeightRange : 3;
                        aa.Answers.Add(new Answer { Value = "no", Weight = -maxWeight });
                    }
                    break;
                case AttributeType.Single:
                    if (input is StackPanel sp)
                    {
                        foreach (var c in sp.Children)
                        {
                            if (c is RadioButton rb && rb.IsChecked == true && rb.Tag is Proposition prop)
                            {
                                aa.Answers.Add(new Answer { Value = prop.Id, Weight = 1.0 });
                                break;
                            }
                        }
                    }
                    break;
                case AttributeType.Multiple:
                    if (_multipleWeights.TryGetValue(attr.Id, out var weightList))
                    {
                        foreach (var (cb, prop, weightBox) in weightList)
                        {
                            if (cb.IsChecked != true) continue;
                            var w = 1.0;
                            var text = weightBox.Text?.Trim().Replace(',', '.');
                            if (!string.IsNullOrEmpty(text) && double.TryParse(text, System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                                w = parsed;
                            aa.Answers.Add(new Answer { Value = prop.Id, Weight = w });
                        }
                    }
                    break;
                case AttributeType.Numeric:
                    if (input is TextBox tb && !string.IsNullOrWhiteSpace(tb.Text))
                    {
                        var parts = tb.Text!.Trim().Replace(',', '.').Split(' ', '\t');
                        if (parts.Length >= 1 && double.TryParse(parts[0], System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out var num))
                        {
                            var w = 1.0;
                            if (parts.Length >= 2) double.TryParse(parts[1], System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out w);
                            aa.Answers.Add(new Answer { Value = num.ToString(System.Globalization.CultureInfo.InvariantCulture), Weight = w });
                        }
                    }
                    break;
            }
            if (aa.Answers.Count > 0 || aa.SpecialStatus != AnswerSpecialStatus.None)
                set.Attributes.Add(aa);
        }
        return set;
    }

    private double? GetManualWeight(string attrId)
    {
        if (!_manualWeightPanels.TryGetValue(attrId, out var pair)) return null;
        var text = pair.weightBox?.Text?.Trim().Replace(',', '.');
        if (string.IsNullOrEmpty(text)) return null;
        if (double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var w))
            return w;
        return null;
    }

    private void ShowResults(InferenceResult result)
    {
        ResultsTitle.IsVisible = true;
        ResultsPanel.Children.Clear();
        var filterPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Margin = new(0, 0, 0, 8) };
        var filterScope = new ComboBox { ItemsSource = new[] { "Cíle", "Všechny propozice" }, SelectedIndex = 0, MinWidth = 140 };
        var filterSign = new ComboBox { ItemsSource = new[] { "Vše", "Kladné", "Záporné" }, SelectedIndex = 0, MinWidth = 90 };
        filterPanel.Children.Add(new TextBlock { Text = "Zobrazit:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
        filterPanel.Children.Add(filterScope);
        filterPanel.Children.Add(filterSign);
        ResultsPanel.Children.Add(filterPanel);

        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16, Margin = new(0, 0, 0, 4) };
        header.Children.Add(new TextBlock { Text = "Název", FontWeight = FontWeight.SemiBold, MinWidth = 160 });
        header.Children.Add(new TextBlock { Text = "Min váha", FontWeight = FontWeight.SemiBold, MinWidth = 64 });
        header.Children.Add(new TextBlock { Text = "Max váha", FontWeight = FontWeight.SemiBold, MinWidth = 64 });
        header.Children.Add(new TextBlock { Text = "Status", FontWeight = FontWeight.SemiBold, MinWidth = 64 });
        header.Children.Add(new TextBlock { Text = "Typ", FontWeight = FontWeight.SemiBold, MinWidth = 52 });
        ResultsPanel.Children.Add(header);

        var btnRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new(0, 12, 0, 0) };
        var storeBtn = new Button { Content = "Uložit případ" };
        storeBtn.Click += (s, e) => StoreCase();
        btnRow.Children.Add(storeBtn);

        // Engine vrací váhy ve zobrazovací škále. Záložně: pokud hodnota vypadá jako vnitřní (-1..1), přeškálujeme.
        var weightRange = _kb?.Global?.WeightRange ?? 3;
        if (weightRange < 3) weightRange = 3;
        double Scale(double v) => v * weightRange;
        double ToDisplayWeight(double w) => (w >= -1.1 && w <= 1.1) ? (w * weightRange) : w;

        void RefreshList()
        {
            while (ResultsPanel.Children.Count > 3)
                ResultsPanel.Children.RemoveAt(ResultsPanel.Children.Count - 1);
            var showAllPropositions = filterScope.SelectedIndex == 1;
            var signIdx = filterSign.SelectedIndex;
            var list = (showAllPropositions && result.AllPropositions.Count > 0 ? result.AllPropositions : result.Goals).AsEnumerable();
            if (signIdx == 1) list = list.Where(g => g.MaxWeight > 0);
            else if (signIdx == 2) list = list.Where(g => g.MinWeight < 0);
            foreach (var g in list)
            {
                var name = !string.IsNullOrEmpty(g.DisplayName) ? g.DisplayName : $"{g.AttributeId}({g.PropositionId})";
                var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Margin = new(0, 2, 0, 2) };
                var nameBlock = new TextBlock { Text = name, MinWidth = 160, TextWrapping = TextWrapping.Wrap };
                var minD = ToDisplayWeight(g.MinWeight);
                var maxD = ToDisplayWeight(g.MaxWeight);
                var weightVal = maxD;
                if (weightVal > 0)
                    nameBlock.Foreground = new SolidColorBrush(Color.FromRgb(0, 128, 0));
                else if (weightVal < 0)
                    nameBlock.Foreground = new SolidColorBrush(Color.FromRgb(180, 0, 0));
                row.Children.Add(nameBlock);
                row.Children.Add(new TextBlock { Text = minD.ToString("F3", System.Globalization.CultureInfo.InvariantCulture), MinWidth = 64 });
                row.Children.Add(new TextBlock { Text = maxD.ToString("F3", System.Globalization.CultureInfo.InvariantCulture), MinWidth = 64 });
                row.Children.Add(new TextBlock { Text = g.Status, MinWidth = 64 });
                row.Children.Add(new TextBlock { Text = g.Type, MinWidth = 52 });
                ResultsPanel.Children.Add(row);
                var contributing = result.FiredRules
                    .SelectMany(fr => fr.AppliedConclusions.Where(ac => ac.AttributeId == g.AttributeId && ac.PropositionId == g.PropositionId)
                        .Select(ac => $"{fr.RuleId}: +{Scale(ac.WeightChange):F3}"))
                    .ToList();
                if (contributing.Count > 0)
                {
                    var exp = new Expander { Header = "Proč (How)", Content = new TextBlock { Text = string.Join("\n", contributing), TextWrapping = TextWrapping.Wrap } };
                    ResultsPanel.Children.Add(exp);
                }
            }
            ResultsPanel.Children.Add(btnRow);
        }
        filterScope.SelectionChanged += (_, _) => RefreshList();
        filterSign.SelectionChanged += (_, _) => RefreshList();
        RefreshList();
    }

    private async void StoreCase()
    {
        if (_lastResult == null || _mainWindow == null) return;
        var storage = _mainWindow.StorageProvider;
        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Uložit případ (Store case)",
            DefaultExtension = "xml",
            FileTypeChoices = new[] { new FilePickerFileType("XML") { Patterns = new[] { "*.xml" } } }
        });
        if (file == null) return;
        try
        {
            var answers = BuildAnswerSet();
            var writer = new CaseStoreWriter();
            var xml = writer.Write(answers, _lastResult, null);
            await File.WriteAllTextAsync(file.Path.LocalPath, xml, System.Text.Encoding.UTF8);
        }
        catch (System.Exception) { }
    }

    private async void OnExportResult(object? sender, RoutedEventArgs e)
    {
        if (_lastResult == null || _mainWindow == null) return;
        var storage = _mainWindow.StorageProvider;
        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Uložit výsledky inference (XML)",
            DefaultExtension = "xml",
            FileTypeChoices = new[] { new FilePickerFileType("XML") { Patterns = new[] { "*.xml" } } }
        });
        if (file == null) return;
        try
        {
            var writer = new ResultXmlWriter();
            var xml = writer.Write(_lastResult);
            await File.WriteAllTextAsync(file.Path.LocalPath, xml, System.Text.Encoding.UTF8);
        }
        catch (System.Exception ex)
        {
            ResultsPanel.Children.Insert(0, new TextBlock
            {
                Text = "Chyba exportu: " + ex.Message,
                Foreground = new SolidColorBrush(Colors.DarkRed),
                Margin = new(0, 0, 0, 8)
            });
        }
    }
}
