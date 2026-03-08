using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using NestCore.Model;

namespace NestStudio.Views;

public partial class IntegrityConstraintEditView : UserControl
{
    private readonly IntegrityConstraint _io = null!;
    private readonly KnowledgeBase _kb = null!;
    private readonly Action? _onDirty;
    private StackPanel? _conditionPanel;
    private StackPanel? _conclusionsPanel;

    public IntegrityConstraintEditView()
    {
        InitializeComponent();
    }

    public IntegrityConstraintEditView(IntegrityConstraint io, KnowledgeBase kb, Action? onDirty)
    {
        _io = io;
        _kb = kb;
        _onDirty = onDirty;
        InitializeComponent();
        Build();
    }

    private void Build()
    {
        Root.Children.Clear();
        var attrIds = _kb.Attributes.Select(a => a.Id).ToList();

        var idRow = CreateLabelRow("Id", out TextBox idBox);
        idBox.Text = _io.Id;
        idBox.MinWidth = 200;
        idBox.LostFocus += (_, _) => { _io.Id = idBox.Text?.Trim() ?? ""; _onDirty?.Invoke(); };
        Root.Children.Add(idRow);

        var nameRow = CreateLabelRow("Název", out TextBox nameBox);
        nameBox.Text = _io.Name ?? "";
        nameBox.MinWidth = 200;
        nameBox.LostFocus += (_, _) => { _io.Name = nameBox.Text?.Trim(); _onDirty?.Invoke(); };
        Root.Children.Add(nameRow);

        var ctxCombo = new ComboBox
        {
            MinWidth = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            ItemsSource = new[] { "" }.Concat(_kb.Contexts.Select(c => c.Id)).ToList(),
            SelectedItem = _io.IdContext ?? ""
        };
        ctxCombo.SelectionChanged += (_, _) =>
        {
            _io.IdContext = ctxCombo.SelectedItem as string;
            if (string.IsNullOrEmpty(_io.IdContext)) _io.IdContext = null;
            _onDirty?.Invoke();
        };
        Root.Children.Add(CreateLabelRow("Kontext (volitelné)", ctxCombo));

        var commentRow = CreateLabelRow("Komentář", out TextBox commentBox);
        commentBox.Text = _io.Comment ?? "";
        commentBox.MinWidth = 200;
        commentBox.LostFocus += (_, _) => { _io.Comment = commentBox.Text?.Trim(); _onDirty?.Invoke(); };
        Root.Children.Add(commentRow);

        Root.Children.Add(new TextBlock { Text = "Podmínka (OR konjunkcí)", FontWeight = FontWeight.SemiBold, Margin = new(0, 16, 0, 6) });
        _conditionPanel = new StackPanel { Spacing = 8 };
        Root.Children.Add(_conditionPanel);
        var addConjBtn = new Button { Content = "Přidat konjunkci", HorizontalAlignment = HorizontalAlignment.Left };
        addConjBtn.Click += (_, _) =>
        {
            _io.Condition.Conjunctions.Add(new Conjunction());
            RefreshCondition(attrIds);
            _onDirty?.Invoke();
        };
        Root.Children.Add(addConjBtn);

        Root.Children.Add(new TextBlock { Text = "Závěry", FontWeight = FontWeight.SemiBold, Margin = new(0, 16, 0, 6) });
        _conclusionsPanel = new StackPanel { Spacing = 6 };
        Root.Children.Add(_conclusionsPanel);
        var addConcBtn = new Button { Content = "Přidat závěr", HorizontalAlignment = HorizontalAlignment.Left };
        addConcBtn.Click += (_, _) =>
        {
            _io.Conclusions.Add(new Conclusion());
            RefreshConclusions();
            _onDirty?.Invoke();
        };
        Root.Children.Add(addConcBtn);

        RefreshCondition(attrIds);
        RefreshConclusions();
    }

    private void RefreshCondition(List<string> attrIds)
    {
        if (_conditionPanel == null) return;
        _conditionPanel.Children.Clear();
        for (int i = 0; i < _io.Condition.Conjunctions.Count; i++)
        {
            var conj = _io.Condition.Conjunctions[i];
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
                _io.Condition.Conjunctions.RemoveAt(conjIndex);
                RefreshCondition(attrIds);
                _onDirty?.Invoke();
            };
            conjHeader.Children.Add(removeConjBtn);
            conjStack.Children.Add(conjHeader);
            foreach (var lit in conj.Literals.ToList())
            {
                var litRow = RuleEditView.CreateLiteralRow(lit, conj.Literals, attrIds, _kb, () => _onDirty?.Invoke(), () => RefreshCondition(attrIds));
                conjStack.Children.Add(litRow);
            }
            var addLitBtn = new Button { Content = "Přidat literál", Padding = new(6, 2), HorizontalAlignment = HorizontalAlignment.Left };
            addLitBtn.Click += (_, _) =>
            {
                conj.Literals.Add(new Literal());
                RefreshCondition(attrIds);
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
        foreach (var c in _io.Conclusions)
        {
            var row = CreateConclusionRow(c, attrIds);
            _conclusionsPanel.Children.Add(row);
        }
    }

    private StackPanel CreateConclusionRow(Conclusion c, List<string> attrIds)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var attrCombo = new ComboBox { MinWidth = 140, ItemsSource = attrIds, SelectedItem = attrIds.Contains(c.AttributeId) ? c.AttributeId : null };
        var propCombo = new ComboBox { MinWidth = 120 };
        RuleEditView.UpdatePropositionComboStatic(_kb, propCombo, c.AttributeId, c.PropositionId);
        attrCombo.SelectionChanged += (_, _) =>
        {
            c.AttributeId = attrCombo.SelectedItem as string ?? "";
            RuleEditView.UpdatePropositionComboStatic(_kb, propCombo, c.AttributeId, null);
            propCombo.SelectedItem = c.PropositionId;
            c.PropositionId = null;
            _onDirty?.Invoke();
        };
        propCombo.SelectionChanged += (_, _) => { c.PropositionId = propCombo.SelectedItem as string; _onDirty?.Invoke(); };
        var negCheck = new CheckBox { Content = "Neg", IsChecked = c.Negation };
        negCheck.IsCheckedChanged += (_, _) => { c.Negation = negCheck.IsChecked == true; _onDirty?.Invoke(); };
        var removeBtn = new Button { Content = "×", Padding = new(8, 2) };
        removeBtn.Click += (_, _) =>
        {
            _io.Conclusions.Remove(c);
            RefreshConclusions();
            _onDirty?.Invoke();
        };
        row.Children.Add(attrCombo);
        row.Children.Add(propCombo);
        row.Children.Add(negCheck);
        row.Children.Add(removeBtn);
        return row;
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
