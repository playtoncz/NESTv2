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
    private bool _isDirty;

    /// <summary>Příznak neuložených změn. MainWindow ho čte před přepnutím na jiné view.</summary>
    public bool IsDirty => _isDirty;

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
        _isDirty = string.IsNullOrEmpty(basePath);
        InitializeComponent();
        TitleText.Text = kb.Global.Description ?? "Editor znalostní báze";
        if (!string.IsNullOrEmpty(basePath))
            TitleText.Text += " — " + System.IO.Path.GetFileName(basePath);

        BackButton.Click += OnBack;
        SaveButton.Click += OnSave;
        FillProjectButton.Click += OnFillProject;
        ValidateButton.Click += OnValidate;
        GraphButton.Click += OnShowGraph;
        GlobalPropsButton.Click += OnShowGlobalProps;
        StatisticsButton.Click += OnShowStatistics;
        NewAttributeButton.Click += OnNewAttribute;
        DeleteAttributeButton.Click += OnDeleteAttribute;
        NewRuleButton.Click += OnNewRule;
        NewContextButton.Click += OnNewContext;
        NewIntegrityButton.Click += OnNewIntegrity;
        DeleteRuleButton.Click += OnDeleteRule;
        DeleteContextButton.Click += OnDeleteContext;
        DeleteIntegrityButton.Click += OnDeleteIntegrity;
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
        RulesList.ItemsSource = _kb.CompositionalRules.Select(r =>
        {
            var prefix = r.Kind switch
            {
                NestCore.Model.RuleKind.Apriori => "[A] ",
                NestCore.Model.RuleKind.Logical => "[L] ",
                _ => "[C] "
            };
            return prefix + r.Id;
        }).ToList();
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
            DetailContent.Content = new AttributeEditView(_kb.Attributes[idx], _kb, () => { SetDirty(); RefreshLists(); });
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
            DetailContent.Content = new RuleEditView(_kb.CompositionalRules[idx], _kb, SetDirty);
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
            DetailContent.Content = new ContextEditView(_kb.Contexts[idx], _kb, SetDirty);
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
            DetailContent.Content = new IntegrityConstraintEditView(_kb.IntegrityConstraints[idx], _kb, SetDirty);
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
        var graph = new RulesGraphView(_kb);
        graph.AttributeClicked += attrId =>
        {
            var idx = _kb.Attributes.FindIndex(a => a.Id == attrId);
            if (idx >= 0)
            {
                AttributesList.SelectedIndex = idx;
                DetailContent.Content = new AttributeEditView(_kb.Attributes[idx], _kb, () => { SetDirty(); RefreshLists(); });
            }
        };
        DetailContent.Content = graph;
    }

    private void OnShowGlobalProps(object? sender, RoutedEventArgs e)
    {
        if (_kb == null) return;
        AttributesList.SelectedIndex = -1;
        RulesList.SelectedIndex = -1;
        ContextsList.SelectedIndex = -1;
        IntegrityList.SelectedIndex = -1;
        DetailContent.Content = new GlobalPropertiesView(_kb.Global, SetDirty);
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
        SetDirty();
        RefreshLists();
        var idx = _kb.Attributes.Count - 1;
        AttributesList.SelectedIndex = idx;
        DetailContent.Content = new AttributeEditView(attr, _kb, () => { SetDirty(); RefreshLists(); });
    }

    private async void OnDeleteAttribute(object? sender, RoutedEventArgs e)
    {
        if (_kb == null || _mainWindow == null || AttributesList.SelectedIndex < 0) return;
        var idx = AttributesList.SelectedIndex;
        var attr = _kb.Attributes[idx];
        var dialog = new ConfirmDialog("Opravdu smazat atribut \"" + attr.Id + "\"?");
        if (await dialog.ShowDialog<bool>(_mainWindow) != true) return;
        _kb.Attributes.RemoveAt(idx);
        SetDirty();
        RefreshLists();
        AttributesList.SelectedIndex = -1;
        DetailContent.Content = null;
    }

    private async void OnNewRule(object? sender, RoutedEventArgs e)
    {
        if (_kb == null || _mainWindow == null) return;
        var dialog = new NewRuleTypeDialog();
        var kind = await dialog.ShowDialog<RuleKind?>(_mainWindow);
        if (!kind.HasValue) return;
        var ruleIds = _kb.CompositionalRules.Select(r => r.Id).ToHashSet();
        var prefix = kind.Value switch { RuleKind.Apriori => "a", RuleKind.Logical => "l", _ => "c" };
        var id = prefix + "1";
        for (var i = 1; ; i++)
        {
            id = prefix + i;
            if (!ruleIds.Contains(id)) break;
        }
        var rule = new CompositionalRule
        {
            Id = id,
            Kind = kind.Value,
            Condition = kind.Value == RuleKind.Apriori
                ? new Condition()
                : new Condition { Conjunctions = new List<Conjunction> { new Conjunction() } },
            Conclusions = new List<Conclusion>()
        };
        _kb.CompositionalRules.Add(rule);
        SetDirty();
        RefreshLists();
        var idx = _kb.CompositionalRules.Count - 1;
        RulesList.SelectedIndex = idx;
        DetailContent.Content = new RuleEditView(rule, _kb, SetDirty);
    }

    private async void OnNewContext(object? sender, RoutedEventArgs e)
    {
        if (_kb == null) return;
        var ids = _kb.Contexts.Select(c => c.Id).ToHashSet();
        var id = "ctx1";
        for (var i = 1; ; i++)
        {
            id = "ctx" + i;
            if (!ids.Contains(id)) break;
        }
        var ctx = new Context { Id = id, Condition = new Condition { Conjunctions = new List<Conjunction> { new Conjunction() } } };
        _kb.Contexts.Add(ctx);
        SetDirty();
        RefreshLists();
        var idx = _kb.Contexts.Count - 1;
        ContextsList.SelectedIndex = idx;
        DetailContent.Content = new ContextEditView(ctx, _kb, SetDirty);
    }

    private async void OnNewIntegrity(object? sender, RoutedEventArgs e)
    {
        if (_kb == null) return;
        var ids = _kb.IntegrityConstraints.Select(io => io.Id).ToHashSet();
        var id = "io1";
        for (var i = 1; ; i++)
        {
            id = "io" + i;
            if (!ids.Contains(id)) break;
        }
        var io = new IntegrityConstraint { Id = id, Condition = new Condition(), Conclusions = new List<Conclusion>() };
        _kb.IntegrityConstraints.Add(io);
        SetDirty();
        RefreshLists();
        var idx = _kb.IntegrityConstraints.Count - 1;
        IntegrityList.SelectedIndex = idx;
        DetailContent.Content = new IntegrityConstraintEditView(io, _kb, SetDirty);
    }

    private async void OnDeleteRule(object? sender, RoutedEventArgs e)
    {
        if (_kb == null || _mainWindow == null || RulesList.SelectedIndex < 0) return;
        var idx = RulesList.SelectedIndex;
        var rule = _kb.CompositionalRules[idx];
        var dialog = new ConfirmDialog("Opravdu smazat pravidlo \"" + rule.Id + "\"?");
        if (await dialog.ShowDialog<bool>(_mainWindow) != true) return;
        _kb.CompositionalRules.RemoveAt(idx);
        SetDirty();
        RefreshLists();
        RulesList.SelectedIndex = -1;
        DetailContent.Content = null;
    }

    private async void OnDeleteContext(object? sender, RoutedEventArgs e)
    {
        if (_kb == null || _mainWindow == null || ContextsList.SelectedIndex < 0) return;
        var idx = ContextsList.SelectedIndex;
        var ctx = _kb.Contexts[idx];
        var dialog = new ConfirmDialog("Opravdu smazat kontext \"" + ctx.Id + "\"?");
        if (await dialog.ShowDialog<bool>(_mainWindow) != true) return;
        _kb.Contexts.RemoveAt(idx);
        SetDirty();
        RefreshLists();
        ContextsList.SelectedIndex = -1;
        DetailContent.Content = null;
    }

    private async void OnDeleteIntegrity(object? sender, RoutedEventArgs e)
    {
        if (_kb == null || _mainWindow == null || IntegrityList.SelectedIndex < 0) return;
        var idx = IntegrityList.SelectedIndex;
        var io = _kb.IntegrityConstraints[idx];
        var dialog = new ConfirmDialog("Opravdu smazat integritní omezení \"" + io.Id + "\"?");
        if (await dialog.ShowDialog<bool>(_mainWindow) != true) return;
        _kb.IntegrityConstraints.RemoveAt(idx);
        SetDirty();
        RefreshLists();
        IntegrityList.SelectedIndex = -1;
        DetailContent.Content = null;
    }

    internal void SetDirty()
    {
        _isDirty = true;
    }

    /// <summary>Uloží projekt. Pokud není cesta, otevře SaveFilePicker. Vrací true pokud bylo uloženo.</summary>
    internal async Task<bool> TrySaveAsync()
    {
        if (_kb == null) return false;
        if (string.IsNullOrEmpty(_basePath))
        {
            var storage = _mainWindow?.StorageProvider;
            if (storage == null) return false;
            var file = await storage.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Uložit znalostní bázi",
                DefaultExtension = "xml",
                FileTypeChoices = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("XML") { Patterns = new[] { "*.xml" } }
                }
            });
            if (file == null) return false;
            _basePath = file.Path.LocalPath;
        }
        try
        {
            var writer = new BaseXmlWriter();
            var xml = writer.Write(_kb);
            var utf8NoBom = new System.Text.UTF8Encoding(false);
            var bytes = utf8NoBom.GetBytes(xml);
            await System.IO.File.WriteAllBytesAsync(_basePath, bytes);
            _isDirty = false;
            StatusText.Text = "Uloženo.";
            return true;
        }
        catch (System.Exception ex)
        {
            StatusText.Text = "Chyba: " + ex.Message;
            return false;
        }
    }

    private async void OnBack(object? sender, RoutedEventArgs e)
    {
        if (_mainWindow == null) return;
        if (await _mainWindow.ConfirmSaveBeforeLeaveAsync(this))
            _mainWindow.ShowWelcome();
    }

    private async void OnSave(object? sender, RoutedEventArgs e)
    {
        await TrySaveAsync();
    }

    private async void OnFillProject(object? sender, RoutedEventArgs e)
    {
        if (_kb == null || _mainWindow == null) return;
        if (!await _mainWindow.ConfirmSaveBeforeLeaveAsync(this)) return;
        var dialog = new RunConfigDialog();
        var config = await dialog.ShowDialog<ConsultationRunConfig?>(_mainWindow);
        if (config != null)
            _mainWindow.ShowConsultation(_kb, _basePath, config);
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
