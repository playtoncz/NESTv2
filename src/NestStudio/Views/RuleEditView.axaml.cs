using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
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
    private TextBlock? _summaryBlock;
    private Border? _conditionCard;

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

    private Border MakeSection(string title, StackPanel content)
    {
        var card = new Border();
        card.Classes.Add("editor-section");
        var inner = new StackPanel { Spacing = 8 };
        inner.Children.Add(new TextBlock { Text = title, Classes = { "section-title" } });
        inner.Children.Add(content);
        card.Child = inner;
        return card;
    }

    private static StackPanel MakeField(string label, Control input)
    {
        var sp = new StackPanel { Spacing = 2, Margin = new Thickness(0, 0, 0, 4) };
        sp.Children.Add(new TextBlock { Text = label, Classes = { "field-label" } });
        sp.Children.Add(input);
        return sp;
    }

    private static StackPanel MakeField(string label, out TextBox box)
    {
        box = new TextBox();
        return MakeField(label, box);
    }

    private void Build()
    {
        Root.Children.Clear();

        // Summary banner
        _summaryBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontFamily = new FontFamily("Consolas, Courier New, monospace"),
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xFF)),
            Padding = new Thickness(14, 10),
            Background = new SolidColorBrush(Color.FromArgb(12, 0, 122, 255))
        };
        var summaryBorder = new Border
        {
            CornerRadius = new CornerRadius(12),
            ClipToBounds = true,
            Margin = new Thickness(0, 0, 0, 16),
            Child = _summaryBlock
        };
        Root.Children.Add(summaryBorder);

        // --- Metadata section ---
        var metaContent = new StackPanel { Spacing = 4 };

        var idField = MakeField("ID pravidla", out TextBox idBox);
        idBox.Text = _rule.Id;
        idBox.LostFocus += (_, _) => { _rule.Id = idBox.Text?.Trim() ?? ""; Dirty(); };
        metaContent.Children.Add(idField);

        var kindCombo = new ComboBox
        {
            MinWidth = 220,
            ItemsSource = new[] { "Kompozicionální", "Apriorní (vždy platí)", "Logické (pravda/nepravda)" },
            SelectedIndex = _rule.Kind switch { RuleKind.Apriori => 1, RuleKind.Logical => 2, _ => 0 }
        };
        kindCombo.SelectionChanged += (_, _) =>
        {
            _rule.Kind = kindCombo.SelectedIndex switch { 1 => RuleKind.Apriori, 2 => RuleKind.Logical, _ => RuleKind.Compositional };
            UpdateConditionVisibility();
            RefreshSummary();
            Dirty();
        };
        metaContent.Children.Add(MakeField("Typ pravidla", kindCombo));

        var commentBox = new TextBox
        {
            Text = _rule.Comment ?? "",
            Watermark = "Komentář k pravidlu",
            AcceptsReturn = true,
            MinHeight = 36,
            TextWrapping = TextWrapping.Wrap
        };
        commentBox.LostFocus += (_, _) => { _rule.Comment = string.IsNullOrWhiteSpace(commentBox.Text) ? null : commentBox.Text; Dirty(); };
        metaContent.Children.Add(MakeField("Komentář", commentBox));

        var metaRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        var prioField = MakeField("Priorita", out TextBox prioBox);
        prioBox.Text = _rule.Priority.HasValue ? _rule.Priority.Value.ToString(CultureInfo.InvariantCulture) : "";
        prioBox.Watermark = "volitelné";
        prioBox.Width = 100;
        prioBox.LostFocus += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(prioBox.Text)) _rule.Priority = null;
            else if (double.TryParse(prioBox.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                _rule.Priority = v;
            Dirty();
        };
        metaRow.Children.Add(prioField);

        var ctxCombo = new ComboBox
        {
            MinWidth = 160,
            ItemsSource = new[] { "(žádný)" }.Concat(_kb.Contexts.Select(c => c.Id)).ToList(),
            SelectedItem = string.IsNullOrEmpty(_rule.IdContext) ? "(žádný)" : _rule.IdContext
        };
        ctxCombo.SelectionChanged += (_, _) =>
        {
            var sel = ctxCombo.SelectedItem as string;
            _rule.IdContext = sel == "(žádný)" ? null : sel;
            Dirty();
        };
        metaRow.Children.Add(MakeField("Kontext", ctxCombo));
        metaContent.Children.Add(metaRow);

        Root.Children.Add(MakeSection("Vlastnosti pravidla", metaContent));

        // --- Condition section ---
        var condContent = new StackPanel { Spacing = 6 };
        _conditionPanel = new StackPanel { Spacing = 8 };
        condContent.Children.Add(_conditionPanel);
        var addConjBtn = new Button { Content = "+ Přidat konjunkci (OR)" };
        addConjBtn.Click += (_, _) =>
        {
            _rule.Condition.Conjunctions.Add(new Conjunction());
            RefreshCondition();
            Dirty();
        };
        condContent.Children.Add(addConjBtn);
        _conditionCard = MakeSection("Podmínka (OR konjunkcí)", condContent);
        Root.Children.Add(_conditionCard);

        // --- Conclusions section ---
        var weightRange = _kb.Global.WeightRange > 0 ? _kb.Global.WeightRange : 1;
        var conclContent = new StackPanel { Spacing = 6 };
        _conclusionsPanel = new StackPanel { Spacing = 8 };
        conclContent.Children.Add(_conclusionsPanel);
        var addConcBtn = new Button { Content = "+ Přidat závěr" };
        addConcBtn.Click += (_, _) =>
        {
            _rule.Conclusions.Add(new Conclusion());
            RefreshConclusions();
            Dirty();
        };
        conclContent.Children.Add(addConcBtn);
        Root.Children.Add(MakeSection($"Závěry   (rozsah váhy: −{weightRange:F1} až {weightRange:F1})", conclContent));

        UpdateConditionVisibility();
        RefreshCondition();
        RefreshConclusions();
        RefreshSummary();
    }

    private void UpdateConditionVisibility()
    {
        if (_conditionCard != null)
            _conditionCard.IsVisible = _rule.Kind != RuleKind.Apriori;
    }

    private void RefreshSummary()
    {
        if (_summaryBlock != null)
            _summaryBlock.Text = _rule.ToSummary();
    }

    private void Dirty()
    {
        RefreshSummary();
        _onDirty?.Invoke();
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
            var conjCard = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(20, 0, 122, 255)),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12, 10),
                Margin = new Thickness(0, 0, 0, 4)
            };
            var conjStack = new StackPanel { Spacing = 6 };

            var conjHeader = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            conjHeader.Children.Add(new TextBlock
            {
                Text = i == 0 ? "Konjunkce 1" : $"Konjunkce {i + 1}  (OR)",
                FontWeight = FontWeight.SemiBold,
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center
            });
            var removeConjBtn = new Button { Content = "Odebrat", Padding = new Thickness(8, 4) };
            removeConjBtn.Click += (_, _) =>
            {
                _rule.Condition.Conjunctions.RemoveAt(conjIndex);
                RefreshCondition();
                Dirty();
            };
            conjHeader.Children.Add(removeConjBtn);
            conjStack.Children.Add(conjHeader);

            foreach (var lit in conj.Literals.ToList())
                conjStack.Children.Add(CreateLiteralRow(lit, conj.Literals, attrIds));

            var addLitBtn = new Button { Content = "+ Přidat literál (AND)", Padding = new Thickness(8, 4) };
            addLitBtn.Click += (_, _) =>
            {
                conj.Literals.Add(new Literal());
                RefreshCondition();
                Dirty();
            };
            conjStack.Children.Add(addLitBtn);
            conjCard.Child = conjStack;
            _conditionPanel.Children.Add(conjCard);
        }
        RefreshSummary();
    }

    private void RefreshConclusions()
    {
        if (_conclusionsPanel == null) return;
        _conclusionsPanel.Children.Clear();
        var attrIds = _kb.Attributes.Select(a => a.Id).ToList();
        foreach (var c in _rule.Conclusions)
            _conclusionsPanel.Children.Add(CreateConclusionRow(c, attrIds));
        RefreshSummary();
    }

    private StackPanel CreateLiteralRow(Literal lit, List<Literal> list, List<string> attrIds)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var attrCombo = new ComboBox
        {
            MinWidth = 130,
            ItemsSource = attrIds,
            SelectedItem = string.IsNullOrEmpty(lit.AttributeId) ? null : (attrIds.Contains(lit.AttributeId) ? lit.AttributeId : null)
        };
        if (attrCombo.SelectedItem == null && !string.IsNullOrEmpty(lit.AttributeId))
            attrCombo.SelectedItem = lit.AttributeId;
        var propCombo = new ComboBox { MinWidth = 110 };
        UpdatePropositionCombo(propCombo, lit.AttributeId, lit.PropositionId);
        attrCombo.SelectionChanged += (_, _) =>
        {
            lit.AttributeId = attrCombo.SelectedItem as string ?? "";
            UpdatePropositionCombo(propCombo, lit.AttributeId, null);
            lit.PropositionId = null;
            Dirty();
        };
        propCombo.SelectionChanged += (_, _) => { lit.PropositionId = propCombo.SelectedItem as string; Dirty(); };
        var negCheck = new CheckBox { Content = "Negace", IsChecked = lit.Negation, VerticalAlignment = VerticalAlignment.Center };
        negCheck.IsCheckedChanged += (_, _) => { lit.Negation = negCheck.IsChecked == true; Dirty(); };
        row.Children.Add(new TextBlock { Text = "Atribut:", VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)), FontSize = 12 });
        row.Children.Add(attrCombo);
        row.Children.Add(new TextBlock { Text = "Výrok:", VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)), FontSize = 12 });
        row.Children.Add(propCombo);
        row.Children.Add(negCheck);
        var removeBtn = new Button { Content = "×", Padding = new Thickness(8, 4) };
        removeBtn.Click += (_, _) => { list.Remove(lit); RefreshCondition(); Dirty(); };
        row.Children.Add(removeBtn);
        return row;
    }

    public static StackPanel CreateLiteralRow(Literal lit, List<Literal> list, List<string> attrIds, KnowledgeBase kb, Action? onDirty, Action refreshCondition)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var attrCombo = new ComboBox
        {
            MinWidth = 130,
            ItemsSource = attrIds,
            SelectedItem = string.IsNullOrEmpty(lit.AttributeId) ? null : (attrIds.Contains(lit.AttributeId) ? lit.AttributeId : null)
        };
        if (attrCombo.SelectedItem == null && !string.IsNullOrEmpty(lit.AttributeId))
            attrCombo.SelectedItem = lit.AttributeId;
        var propCombo = new ComboBox { MinWidth = 110 };
        UpdatePropositionComboStatic(kb, propCombo, lit.AttributeId, lit.PropositionId);
        attrCombo.SelectionChanged += (_, _) =>
        {
            lit.AttributeId = attrCombo.SelectedItem as string ?? "";
            UpdatePropositionComboStatic(kb, propCombo, lit.AttributeId, null);
            lit.PropositionId = null;
            onDirty?.Invoke();
        };
        propCombo.SelectionChanged += (_, _) => { lit.PropositionId = propCombo.SelectedItem as string; onDirty?.Invoke(); };
        var negCheck = new CheckBox { Content = "Negace", IsChecked = lit.Negation };
        negCheck.IsCheckedChanged += (_, _) => { lit.Negation = negCheck.IsChecked == true; onDirty?.Invoke(); };
        row.Children.Add(attrCombo);
        row.Children.Add(propCombo);
        row.Children.Add(negCheck);
        var removeBtn = new Button { Content = "×", Padding = new Thickness(8, 4) };
        removeBtn.Click += (_, _) => { list.Remove(lit); refreshCondition(); onDirty?.Invoke(); };
        row.Children.Add(removeBtn);
        return row;
    }

    public static void UpdatePropositionComboStatic(KnowledgeBase kb, ComboBox propCombo, string? attributeId, string? selectedId)
    {
        var attr = kb.Attributes.FirstOrDefault(a => a.Id == attributeId);
        var ids = attr?.Propositions.Select(p => p.Id).ToList() ?? new List<string>();
        propCombo.ItemsSource = ids;
        propCombo.SelectedItem = ids.Contains(selectedId ?? "") ? selectedId : null;
    }

    private StackPanel CreateConclusionRow(Conclusion c, List<string> attrIds)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var attrCombo = new ComboBox { MinWidth = 130, ItemsSource = attrIds, SelectedItem = attrIds.Contains(c.AttributeId) ? c.AttributeId : null };
        var propCombo = new ComboBox { MinWidth = 110 };
        UpdatePropositionCombo(propCombo, c.AttributeId, c.PropositionId);
        attrCombo.SelectionChanged += (_, _) =>
        {
            c.AttributeId = attrCombo.SelectedItem as string ?? "";
            UpdatePropositionCombo(propCombo, c.AttributeId, null);
            c.PropositionId = null;
            Dirty();
        };
        propCombo.SelectionChanged += (_, _) => { c.PropositionId = propCombo.SelectedItem as string; Dirty(); };
        var negCheck = new CheckBox { Content = "Negace", IsChecked = c.Negation, VerticalAlignment = VerticalAlignment.Center };
        negCheck.IsCheckedChanged += (_, _) => { c.Negation = negCheck.IsChecked == true; Dirty(); };
        var weightBox = new TextBox { Text = c.Weight.ToString("F3", CultureInfo.InvariantCulture), Width = 70, Watermark = "váha" };
        weightBox.LostFocus += (_, _) =>
        {
            if (double.TryParse(weightBox.Text?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                c.Weight = v;
            Dirty();
        };
        var removeBtn = new Button { Content = "×", Padding = new Thickness(8, 4) };
        removeBtn.Click += (_, _) => { _rule.Conclusions.Remove(c); RefreshConclusions(); Dirty(); };
        row.Children.Add(new TextBlock { Text = "Atribut:", VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)), FontSize = 12 });
        row.Children.Add(attrCombo);
        row.Children.Add(new TextBlock { Text = "Výrok:", VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)), FontSize = 12 });
        row.Children.Add(propCombo);
        row.Children.Add(negCheck);
        row.Children.Add(new TextBlock { Text = "Váha:", VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)), FontSize = 12 });
        row.Children.Add(weightBox);
        row.Children.Add(removeBtn);
        return row;
    }

    private static List<string> GetPropositionIds(string? attributeId) => new();

    private void UpdatePropositionCombo(ComboBox propCombo, string? attributeId, string? selectedId)
        => UpdatePropositionComboStatic(_kb, propCombo, attributeId, selectedId);
}
