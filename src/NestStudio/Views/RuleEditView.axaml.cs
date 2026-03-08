using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using NestCore.Model;

namespace NestStudio.Views;

public partial class RuleEditView : UserControl
{
    private readonly CompositionalRule _rule = null!;
    private readonly KnowledgeBase _kb = null!;
    private readonly Action? _onDirty;
    private StackPanel? _conditionPanel;
    private StackPanel? _conclusionsPanel;

    public RuleEditView()
    {
        InitializeComponent();
    }

    public RuleEditView(CompositionalRule rule, KnowledgeBase kb, Action? onDirty)
    {
        _rule = rule;
        _kb = kb;
        _onDirty = onDirty;
        InitializeComponent();
        Build();
    }

    private void Build()
    {
        Root.Children.Clear();

        var idRow = CreateLabelRow("Id", out TextBox idBox);
        idBox.Text = _rule.Id;
        idBox.MinWidth = 200;
        idBox.LostFocus += (_, _) => { _rule.Id = idBox.Text?.Trim() ?? ""; _onDirty?.Invoke(); };
        Root.Children.Add(idRow);

        var prioRow = CreateLabelRow("Priorita (volitelné)", out TextBox prioBox);
        prioBox.Text = _rule.Priority.HasValue ? _rule.Priority.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "";
        prioBox.Watermark = "prázdné = bez priority";
        prioBox.MinWidth = 120;
        prioBox.LostFocus += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(prioBox.Text)) _rule.Priority = null;
            else if (double.TryParse(prioBox.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v))
                _rule.Priority = v;
            _onDirty?.Invoke();
        };
        Root.Children.Add(prioRow);

        var ctxCombo = new ComboBox
        {
            MinWidth = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            ItemsSource = new[] { "" }.Concat(_kb.Contexts.Select(c => c.Id)).ToList(),
            SelectedItem = _rule.IdContext ?? ""
        };
        ctxCombo.SelectionChanged += (_, _) =>
        {
            _rule.IdContext = ctxCombo.SelectedItem as string;
            if (string.IsNullOrEmpty(_rule.IdContext)) _rule.IdContext = null;
            _onDirty?.Invoke();
        };
        Root.Children.Add(CreateLabelRow("Kontext (volitelné)", ctxCombo));

        Root.Children.Add(new TextBlock { Text = "Podmínka (OR konjunkcí)", FontWeight = FontWeight.SemiBold, Margin = new(0, 16, 0, 6) });
        _conditionPanel = new StackPanel { Spacing = 8 };
        Root.Children.Add(_conditionPanel);
        var addConjBtn = new Button { Content = "Přidat konjunkci", HorizontalAlignment = HorizontalAlignment.Left };
        addConjBtn.Click += (_, _) =>
        {
            _rule.Condition.Conjunctions.Add(new Conjunction());
            RefreshCondition();
            _onDirty?.Invoke();
        };
        Root.Children.Add(addConjBtn);

        Root.Children.Add(new TextBlock { Text = "Závěry", FontWeight = FontWeight.SemiBold, Margin = new(0, 16, 0, 6) });
        _conclusionsPanel = new StackPanel { Spacing = 6 };
        Root.Children.Add(_conclusionsPanel);
        var addConcBtn = new Button { Content = "Přidat závěr", HorizontalAlignment = HorizontalAlignment.Left };
        addConcBtn.Click += (_, _) =>
        {
            _rule.Conclusions.Add(new Conclusion());
            RefreshConclusions();
            _onDirty?.Invoke();
        };
        Root.Children.Add(addConcBtn);

        RefreshCondition();
        RefreshConclusions();
    }

    private void RefreshCondition()
    {
        if (_conditionPanel == null) return;
        _conditionPanel.Children.Clear();
        var attrIds = _kb.Attributes.Select(a => a.Id).ToList();
        for (int i = 0; i < _rule.Condition.Conjunctions.Count; i++)
        {
            var conj = _rule.Condition.Conjunctions[i];
            var conjIndex = i;
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
                BorderThickness = new(1),
                CornerRadius = new(4),
                Padding = new(8),
                Margin = new(0, 0, 0, 4),
                Child = new StackPanel { Spacing = 6 }
            };
            var conjStack = (StackPanel)border.Child;
            var conjHeader = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            conjHeader.Children.Add(new TextBlock { Text = "Konjunkce " + (i + 1), FontWeight = FontWeight.SemiBold });
            var removeConjBtn = new Button { Content = "Odebrat konjunkci", Padding = new(6, 2) };
            removeConjBtn.Click += (_, _) =>
            {
                _rule.Condition.Conjunctions.RemoveAt(conjIndex);
                RefreshCondition();
                _onDirty?.Invoke();
            };
            conjHeader.Children.Add(removeConjBtn);
            conjStack.Children.Add(conjHeader);
            foreach (var lit in conj.Literals.ToList())
            {
                var litRow = CreateLiteralRow(lit, conj.Literals, attrIds);
                conjStack.Children.Add(litRow);
            }
            var addLitBtn = new Button { Content = "Přidat literál", Padding = new(6, 2), HorizontalAlignment = HorizontalAlignment.Left };
            addLitBtn.Click += (_, _) =>
            {
                conj.Literals.Add(new Literal());
                RefreshCondition();
                _onDirty?.Invoke();
            };
            conjStack.Children.Add(addLitBtn);
            _conditionPanel.Children.Add(border);
        }
    }

    private void RefreshConclusions()
    {
        if (_conclusionsPanel == null) return;
        _conclusionsPanel.Children.Clear();
        var attrIds = _kb.Attributes.Select(a => a.Id).ToList();
        foreach (var c in _rule.Conclusions)
        {
            var row = CreateConclusionRow(c, attrIds);
            _conclusionsPanel.Children.Add(row);
        }
    }

    private StackPanel CreateLiteralRow(Literal lit, List<Literal> list, List<string> attrIds)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var attrCombo = new ComboBox
        {
            MinWidth = 140,
            ItemsSource = attrIds,
            SelectedItem = string.IsNullOrEmpty(lit.AttributeId) ? null : (attrIds.Contains(lit.AttributeId) ? lit.AttributeId : null)
        };
        if (attrCombo.SelectedItem == null && attrIds.Count > 0 && !string.IsNullOrEmpty(lit.AttributeId))
            attrCombo.SelectedItem = lit.AttributeId;
        var propCombo = new ComboBox { MinWidth = 120, ItemsSource = GetPropositionIds(lit.AttributeId) };
        UpdatePropositionCombo(propCombo, lit.AttributeId, lit.PropositionId);
        attrCombo.SelectionChanged += (_, _) =>
        {
            lit.AttributeId = attrCombo.SelectedItem as string ?? "";
            UpdatePropositionCombo(propCombo, lit.AttributeId, null);
            propCombo.SelectedItem = null;
            lit.PropositionId = null;
            _onDirty?.Invoke();
        };
        propCombo.SelectionChanged += (_, _) =>
        {
            lit.PropositionId = propCombo.SelectedItem as string;
            _onDirty?.Invoke();
        };
        var negCheck = new CheckBox { Content = "Negace", IsChecked = lit.Negation };
        negCheck.IsCheckedChanged += (_, _) => { lit.Negation = negCheck.IsChecked == true; _onDirty?.Invoke(); };
        row.Children.Add(attrCombo);
        row.Children.Add(propCombo);
        row.Children.Add(negCheck);
        var removeBtn = new Button { Content = "×", Padding = new(8, 2) };
        removeBtn.Click += (_, _) =>
        {
            list.Remove(lit);
            RefreshCondition();
            _onDirty?.Invoke();
        };
        row.Children.Add(removeBtn);
        return row;
    }

    /// <summary>Statická varianta pro použití z ContextEditView a IntegrityConstraintEditView.</summary>
    public static StackPanel CreateLiteralRow(Literal lit, List<Literal> list, List<string> attrIds, KnowledgeBase kb, Action? onDirty, Action refreshCondition)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var attrCombo = new ComboBox
        {
            MinWidth = 140,
            ItemsSource = attrIds,
            SelectedItem = string.IsNullOrEmpty(lit.AttributeId) ? null : (attrIds.Contains(lit.AttributeId) ? lit.AttributeId : null)
        };
        if (attrCombo.SelectedItem == null && attrIds.Count > 0 && !string.IsNullOrEmpty(lit.AttributeId))
            attrCombo.SelectedItem = lit.AttributeId;
        var propCombo = new ComboBox { MinWidth = 120 };
        UpdatePropositionComboStatic(kb, propCombo, lit.AttributeId, lit.PropositionId);
        attrCombo.SelectionChanged += (_, _) =>
        {
            lit.AttributeId = attrCombo.SelectedItem as string ?? "";
            UpdatePropositionComboStatic(kb, propCombo, lit.AttributeId, null);
            propCombo.SelectedItem = null;
            lit.PropositionId = null;
            onDirty?.Invoke();
        };
        propCombo.SelectionChanged += (_, _) => { lit.PropositionId = propCombo.SelectedItem as string; onDirty?.Invoke(); };
        var negCheck = new CheckBox { Content = "Negace", IsChecked = lit.Negation };
        negCheck.IsCheckedChanged += (_, _) => { lit.Negation = negCheck.IsChecked == true; onDirty?.Invoke(); };
        row.Children.Add(attrCombo);
        row.Children.Add(propCombo);
        row.Children.Add(negCheck);
        var removeBtn = new Button { Content = "×", Padding = new(8, 2) };
        removeBtn.Click += (_, _) => { list.Remove(lit); refreshCondition(); onDirty?.Invoke(); };
        row.Children.Add(removeBtn);
        return row;
    }

    public static void UpdatePropositionComboStatic(KnowledgeBase kb, ComboBox propCombo, string? attributeId, string? selectedId)
    {
        var attr = kb.Attributes.FirstOrDefault(a => a.Id == attributeId);
        var ids = attr?.Propositions.Select(p => p.Id).ToList() ?? new List<string>();
        propCombo.ItemsSource = ids;
        if (ids.Contains(selectedId ?? ""))
            propCombo.SelectedItem = selectedId;
        else
            propCombo.SelectedItem = null;
    }

    private StackPanel CreateConclusionRow(Conclusion c, List<string> attrIds)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var attrCombo = new ComboBox { MinWidth = 140, ItemsSource = attrIds, SelectedItem = attrIds.Contains(c.AttributeId) ? c.AttributeId : null };
        var propCombo = new ComboBox { MinWidth = 120, ItemsSource = GetPropositionIds(c.AttributeId) };
        UpdatePropositionCombo(propCombo, c.AttributeId, c.PropositionId);
        attrCombo.SelectionChanged += (_, _) =>
        {
            c.AttributeId = attrCombo.SelectedItem as string ?? "";
            UpdatePropositionCombo(propCombo, c.AttributeId, null);
            propCombo.SelectedItem = c.PropositionId;
            c.PropositionId = null;
            _onDirty?.Invoke();
        };
        propCombo.SelectionChanged += (_, _) => { c.PropositionId = propCombo.SelectedItem as string; _onDirty?.Invoke(); };
        var negCheck = new CheckBox { Content = "Neg", IsChecked = c.Negation };
        negCheck.IsCheckedChanged += (_, _) => { c.Negation = negCheck.IsChecked == true; _onDirty?.Invoke(); };
        var weightBox = new TextBox { Text = c.Weight.ToString("F2", System.Globalization.CultureInfo.InvariantCulture), Width = 60 };
        weightBox.LostFocus += (_, _) =>
        {
            if (double.TryParse(weightBox.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v))
                c.Weight = v;
            _onDirty?.Invoke();
        };
        var removeBtn = new Button { Content = "×", Padding = new(8, 2) };
        removeBtn.Click += (_, _) =>
        {
            _rule.Conclusions.Remove(c);
            RefreshConclusions();
            _onDirty?.Invoke();
        };
        row.Children.Add(attrCombo);
        row.Children.Add(propCombo);
        row.Children.Add(negCheck);
        row.Children.Add(weightBox);
        row.Children.Add(removeBtn);
        return row;
    }

    private static List<string> GetPropositionIds(string? attributeId)
    {
        return new List<string>(); // filled per KB in UpdatePropositionCombo
    }

    private void UpdatePropositionCombo(ComboBox propCombo, string? attributeId, string? selectedId)
    {
        UpdatePropositionComboStatic(_kb, propCombo, attributeId, selectedId);
    }

    private static StackPanel CreateLabelRow(string label, out TextBox box)
    {
        box = new TextBox();
        var row = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        row.Children.Add(new TextBlock { Text = label, FontWeight = FontWeight.SemiBold });
        row.Children.Add(box);
        return row;
    }

    private static StackPanel CreateLabelRow(string label, Control input)
    {
        var row = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        row.Children.Add(new TextBlock { Text = label, FontWeight = FontWeight.SemiBold });
        row.Children.Add(input);
        return row;
    }
}
