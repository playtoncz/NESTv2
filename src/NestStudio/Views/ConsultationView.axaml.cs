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
        foreach (var attr in _kb.Attributes)
        {
            if (attr.Scope != ScopeKind.Case) continue;

            var attrTitle = string.IsNullOrWhiteSpace(attr.Name) ? attr.Id : attr.Name!;
            var block = new StackPanel { Spacing = 6, Margin = new Thickness(0, 12, 0, 0) };
            block.Children.Add(new TextBlock
            {
                Text = attrTitle,
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                Margin = new Thickness(0, 0, 0, 2)
            });

            Control input;
            switch (attr.Type)
            {
                case AttributeType.Binary:
                    input = new CheckBox { Content = new TextBlock { Text = "Ano" }, IsThreeState = true };
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
                    var multiPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
                    foreach (var p in attr.Propositions)
                    {
                        var label = string.IsNullOrWhiteSpace(p.Name) ? p.Id : p.Name;
                        multiPanel.Children.Add(new CheckBox { Content = new TextBlock { Text = label }, Tag = p });
                    }
                    input = multiPanel;
                    break;
                case AttributeType.Numeric:
                    input = new TextBox { Watermark = "Číslo (volitelně váha)", MinWidth = 200 };
                    break;
                default:
                    continue;
            }
            block.Children.Add(input);

            var aa = new AttributeAnswer { Id = attr.Id, Type = attr.Type };
            var statusPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Margin = new(0, 4, 0, 0) };
            statusPanel.Children.Add(new TextBlock { Text = "Stav:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            var buttons = new List<Button>();
            void AddStatusButton(string label, AnswerSpecialStatus status)
            {
                var btn = new Button { Content = label, Padding = new(8, 4), Tag = status };
                btn.Click += (_, _) =>
                {
                    aa.SpecialStatus = status;
                    foreach (var b in buttons) b.Classes.Remove("primary");
                    btn.Classes.Add("primary");
                };
                statusPanel.Children.Add(btn);
                buttons.Add(btn);
            }
            AddStatusButton("Určitě ano", AnswerSpecialStatus.CertainlyYes);
            AddStatusButton("Určitě ne", AnswerSpecialStatus.CertainlyNo);
            AddStatusButton("Nerelevantní", AnswerSpecialStatus.Irrelevant);
            AddStatusButton("Neznámá", AnswerSpecialStatus.Unknown);
            AddStatusButton("Odložit", AnswerSpecialStatus.PostponeAnswer);
            block.Children.Add(statusPanel);
            _inputs.Add((attr, input, aa));
            QuestionnairePanel.Children.Add(block);
        }

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
                    if (input is CheckBox binaryCb)
                    {
                        if (binaryCb.IsChecked == true)
                            aa.Answers.Add(new Answer { Value = "yes", Weight = 1.0 });
                        else if (binaryCb.IsChecked == false)
                            aa.Answers.Add(new Answer { Value = "no", Weight = 1.0 });
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
                    if (input is StackPanel mp)
                    {
                        foreach (var c in mp.Children)
                        {
                            if (c is CheckBox mcb && mcb.IsChecked == true && mcb.Tag is Proposition prop)
                                aa.Answers.Add(new Answer { Value = prop.Id, Weight = 1.0 });
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

    private void ShowResults(InferenceResult result)
    {
        ResultsPanel.Children.Clear();
        var filterPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Margin = new(0, 0, 0, 8) };
        var filterScope = new ComboBox { ItemsSource = new[] { "Cíle", "Vše" }, SelectedIndex = 0, MinWidth = 80 };
        var filterSign = new ComboBox { ItemsSource = new[] { "Vše", "Kladné", "Záporné" }, SelectedIndex = 0, MinWidth = 90 };
        filterPanel.Children.Add(new TextBlock { Text = "Zobrazit:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
        filterPanel.Children.Add(filterScope);
        filterPanel.Children.Add(filterSign);
        ResultsPanel.Children.Add(filterPanel);

        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16, Margin = new(0, 0, 0, 4) };
        header.Children.Add(new TextBlock { Text = "Název", FontWeight = FontWeight.SemiBold, MinWidth = 180 });
        header.Children.Add(new TextBlock { Text = "Min váha", FontWeight = FontWeight.SemiBold, MinWidth = 70 });
        header.Children.Add(new TextBlock { Text = "Max váha", FontWeight = FontWeight.SemiBold, MinWidth = 70 });
        header.Children.Add(new TextBlock { Text = "Status", FontWeight = FontWeight.SemiBold, MinWidth = 80 });
        header.Children.Add(new TextBlock { Text = "Typ", FontWeight = FontWeight.SemiBold, MinWidth = 60 });
        ResultsPanel.Children.Add(header);

        var btnRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new(0, 12, 0, 0) };
        var storeBtn = new Button { Content = "Store case" };
        storeBtn.Click += (s, e) => StoreCase();
        btnRow.Children.Add(storeBtn);

        void RefreshList()
        {
            while (ResultsPanel.Children.Count > 3)
                ResultsPanel.Children.RemoveAt(ResultsPanel.Children.Count - 1);
            var scopeAll = filterScope.SelectedIndex == 1;
            var signIdx = filterSign.SelectedIndex;
            var list = result.Goals.AsEnumerable();
            if (!scopeAll)
                list = list.Where(g => g.Type == "goal");
            if (signIdx == 1) list = list.Where(g => g.MaxWeight > 0);
            else if (signIdx == 2) list = list.Where(g => g.MinWeight < 0);
            foreach (var g in list)
            {
                var name = !string.IsNullOrEmpty(g.DisplayName) ? g.DisplayName : $"{g.AttributeId}({g.PropositionId})";
                var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16, Margin = new(0, 2, 0, 2) };
                row.Children.Add(new TextBlock { Text = name, MinWidth = 180, TextWrapping = TextWrapping.Wrap });
                row.Children.Add(new TextBlock { Text = g.MinWeight.ToString("F3"), MinWidth = 70 });
                row.Children.Add(new TextBlock { Text = g.MaxWeight.ToString("F3"), MinWidth = 70 });
                row.Children.Add(new TextBlock { Text = g.Status, MinWidth = 80 });
                row.Children.Add(new TextBlock { Text = g.Type, MinWidth = 60 });
                ResultsPanel.Children.Add(row);
                var contributing = result.FiredRules
                    .SelectMany(fr => fr.AppliedConclusions.Where(ac => ac.AttributeId == g.AttributeId && ac.PropositionId == g.PropositionId)
                        .Select(ac => $"{fr.RuleId}: +{ac.WeightChange:F3}"))
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
