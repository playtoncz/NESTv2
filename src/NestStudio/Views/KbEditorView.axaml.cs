using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NestCore.Model;
using NestFormat;

namespace NestStudio.Views;

public partial class KbEditorView : UserControl
{
    private readonly KnowledgeBase? _kb;
    private string? _basePath;
    private readonly MainWindow? _mainWindow;

    /// <summary>Pro runtime loader / XAML. V kódu používej konstruktor s KB a MainWindow.</summary>
    public KbEditorView()
    {
        InitializeComponent();
    }

    public KbEditorView(KnowledgeBase kb, string? basePath, MainWindow mainWindow)
    {
        _kb = kb;
        _basePath = basePath;
        _mainWindow = mainWindow;
        InitializeComponent();
        TitleText.Text = kb.Global.Description ?? "Editor znalostní báze";
        if (!string.IsNullOrEmpty(basePath))
            TitleText.Text += " — " + System.IO.Path.GetFileName(basePath);

        BackButton.Click += (_, _) => _mainWindow.ShowWelcome();
        SaveButton.Click += OnSave;
        ValidateButton.Click += OnValidate;
        GraphButton.Click += OnShowGraph;
        GlobalPropsButton.Click += OnShowGlobalProps;
        StatisticsButton.Click += OnShowStatistics;
        NewAttributeButton.Click += OnNewAttribute;
        AttributesList.SelectionChanged += OnAttributeSelected;
        RulesList.SelectionChanged += OnRuleSelected;
        ContextsList.SelectionChanged += OnContextSelected;
        IntegrityList.SelectionChanged += OnIntegritySelected;

        RefreshLists();
    }

    private void RefreshLists()
    {
        if (_kb == null) return;
        var attrDisplay = _kb.Attributes.Select(a => string.IsNullOrWhiteSpace(a.Name) ? a.Id : $"{a.Name} ({a.Id})").ToList();
        AttributesList.ItemsSource = attrDisplay;
        RulesList.ItemsSource = _kb.CompositionalRules.Select(r => r.Id).ToList();
        ContextsList.ItemsSource = _kb.Contexts.Select(c => string.IsNullOrWhiteSpace(c.Comment) ? c.Id : $"{c.Id} — {c.Comment}").ToList();
        IntegrityList.ItemsSource = _kb.IntegrityConstraints.Select(io => string.IsNullOrWhiteSpace(io.Name) ? io.Id : $"{io.Id} — {io.Name}").ToList();
    }

    private void OnAttributeSelected(object? sender, SelectionChangedEventArgs e)
    {
        RulesList.SelectedIndex = -1;
        ContextsList.SelectedIndex = -1;
        IntegrityList.SelectedIndex = -1;
        var idx = AttributesList.SelectedIndex;
        if (_kb != null && idx >= 0 && idx < _kb.Attributes.Count)
            DetailContent.Content = new AttributeDetailView(_kb.Attributes[idx]);
        else
            DetailContent.Content = null;
    }

    private void OnRuleSelected(object? sender, SelectionChangedEventArgs e)
    {
        AttributesList.SelectedIndex = -1;
        ContextsList.SelectedIndex = -1;
        IntegrityList.SelectedIndex = -1;
        var idx = RulesList.SelectedIndex;
        if (_kb != null && idx >= 0 && idx < _kb.CompositionalRules.Count)
            DetailContent.Content = new RuleDetailView(_kb.CompositionalRules[idx], _kb);
        else
            DetailContent.Content = null;
    }

    private void OnContextSelected(object? sender, SelectionChangedEventArgs e)
    {
        AttributesList.SelectedIndex = -1;
        RulesList.SelectedIndex = -1;
        IntegrityList.SelectedIndex = -1;
        var idx = ContextsList.SelectedIndex;
        if (_kb != null && idx >= 0 && idx < _kb.Contexts.Count)
            DetailContent.Content = new ContextDetailView(_kb.Contexts[idx], _kb);
        else
            DetailContent.Content = null;
    }

    private void OnIntegritySelected(object? sender, SelectionChangedEventArgs e)
    {
        AttributesList.SelectedIndex = -1;
        RulesList.SelectedIndex = -1;
        ContextsList.SelectedIndex = -1;
        var idx = IntegrityList.SelectedIndex;
        if (_kb != null && idx >= 0 && idx < _kb.IntegrityConstraints.Count)
            DetailContent.Content = new IntegrityConstraintDetailView(_kb.IntegrityConstraints[idx], _kb);
        else
            DetailContent.Content = null;
    }

    private void OnShowGraph(object? sender, RoutedEventArgs e)
    {
        if (_kb == null) return;
        AttributesList.SelectedIndex = -1;
        RulesList.SelectedIndex = -1;
        ContextsList.SelectedIndex = -1;
        IntegrityList.SelectedIndex = -1;
        DetailContent.Content = new RulesGraphView(_kb);
    }

    private void OnShowGlobalProps(object? sender, RoutedEventArgs e)
    {
        if (_kb == null) return;
        AttributesList.SelectedIndex = -1;
        RulesList.SelectedIndex = -1;
        ContextsList.SelectedIndex = -1;
        IntegrityList.SelectedIndex = -1;
        DetailContent.Content = new GlobalPropertiesView(_kb.Global);
    }

    private void OnShowStatistics(object? sender, RoutedEventArgs e)
    {
        if (_kb == null) return;
        AttributesList.SelectedIndex = -1;
        RulesList.SelectedIndex = -1;
        ContextsList.SelectedIndex = -1;
        IntegrityList.SelectedIndex = -1;
        DetailContent.Content = new StatisticsView(_kb);
    }

    private async void OnNewAttribute(object? sender, RoutedEventArgs e)
    {
        if (_kb == null || _mainWindow == null) return;
        var dialog = new NewAttributeTypeDialog();
        var type = await dialog.ShowDialog<AttributeType?>(_mainWindow);
        if (!type.HasValue) return;
        var ids = _kb.Attributes.Select(a => a.Id).ToHashSet();
        var id = "atribut_1";
        for (var i = 1; ; i++)
        {
            id = "atribut_" + i;
            if (!ids.Contains(id)) break;
        }
        var attr = new NestCore.Model.Attribute { Id = id, Name = id, Type = type.Value };
        if (type == AttributeType.Numeric)
            attr.LegalValues = new LegalValues { LowerBound = 0, UpperBound = 100 };
        _kb.Attributes.Add(attr);
        RefreshLists();
        var idx = _kb.Attributes.Count - 1;
        AttributesList.SelectedIndex = idx;
        DetailContent.Content = new AttributeDetailView(attr);
    }

    private async void OnSave(object? sender, RoutedEventArgs e)
    {
        if (_kb == null) return;
        if (string.IsNullOrEmpty(_basePath))
        {
            var storage = _mainWindow?.StorageProvider;
            if (storage == null) return;
            var file = await storage.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Uložit znalostní bázi",
                DefaultExtension = "xml",
                FileTypeChoices = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("XML") { Patterns = new[] { "*.xml" } }
                }
            });
            if (file == null) return;
            _basePath = file.Path.LocalPath;
        }
        try
        {
            var writer = new BaseXmlWriter();
            var xml = writer.Write(_kb);
            var utf8NoBom = new System.Text.UTF8Encoding(false);
            var bytes = utf8NoBom.GetBytes(xml);
            await System.IO.File.WriteAllBytesAsync(_basePath, bytes);
            StatusText.Text = "Uloženo.";
        }
        catch (System.Exception ex)
        {
            StatusText.Text = "Chyba: " + ex.Message;
        }
    }

    private void OnValidate(object? sender, RoutedEventArgs e)
    {
        if (_kb == null) return;
        var errors = new List<string>();
        var attrIds = new HashSet<string>();
        foreach (var a in _kb.Attributes)
        {
            if (string.IsNullOrWhiteSpace(a.Id)) { errors.Add("Atribut bez Id."); continue; }
            if (!attrIds.Add(a.Id)) errors.Add($"Duplicitní id atributu: {a.Id}");
        }
        var ruleIds = new HashSet<string>();
        foreach (var r in _kb.CompositionalRules)
        {
            if (string.IsNullOrWhiteSpace(r.Id)) { errors.Add("Pravidlo bez Id."); continue; }
            if (!ruleIds.Add(r.Id)) errors.Add($"Duplicitní id pravidla: {r.Id}");
            foreach (var conj in r.Condition.Conjunctions)
            foreach (var lit in conj.Literals)
            {
                if (!attrIds.Contains(lit.AttributeId))
                    errors.Add($"Pravidlo {r.Id}: neexistující atribut v podmínce: {lit.AttributeId}");
            }
            foreach (var c in r.Conclusions)
            {
                if (!attrIds.Contains(c.AttributeId))
                    errors.Add($"Pravidlo {r.Id}: neexistující atribut v závěru: {c.AttributeId}");
            }
        }
        if (errors.Count == 0)
            StatusText.Text = "Validace OK.";
        else
            StatusText.Text = "Chyby: " + string.Join("; ", errors.Take(5)) + (errors.Count > 5 ? "…" : "");
    }
}
