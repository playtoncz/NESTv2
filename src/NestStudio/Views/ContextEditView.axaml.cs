using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using NestCore.Model;

namespace NestStudio.Views;

public partial class ContextEditView : UserControl
{
    private readonly Context _context = null!;
    private readonly KnowledgeBase _kb = null!;
    private readonly Action? _onDirty;
    private StackPanel? _conditionPanel;

    public ContextEditView()
    {
        InitializeComponent();
    }

    public ContextEditView(Context context, KnowledgeBase kb, Action? onDirty)
    {
        _context = context;
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
        idBox.Text = _context.Id;
        idBox.MinWidth = 200;
        idBox.LostFocus += (_, _) => { _context.Id = idBox.Text?.Trim() ?? ""; _onDirty?.Invoke(); };
        Root.Children.Add(idRow);

        var commentRow = CreateLabelRow("Komentář", out TextBox commentBox);
        commentBox.Text = _context.Comment ?? "";
        commentBox.MinWidth = 200;
        commentBox.LostFocus += (_, _) => { _context.Comment = commentBox.Text?.Trim(); _onDirty?.Invoke(); };
        Root.Children.Add(commentRow);

        Root.Children.Add(new TextBlock { Text = "Podmínka (OR konjunkcí)", FontWeight = FontWeight.SemiBold, Margin = new(0, 16, 0, 6) });
        _conditionPanel = new StackPanel { Spacing = 8 };
        Root.Children.Add(_conditionPanel);
        var addConjBtn = new Button { Content = "Přidat konjunkci", HorizontalAlignment = HorizontalAlignment.Left };
        addConjBtn.Click += (_, _) =>
        {
            _context.Condition.Conjunctions.Add(new Conjunction());
            RefreshCondition(attrIds);
            _onDirty?.Invoke();
        };
        Root.Children.Add(addConjBtn);

        RefreshCondition(attrIds);
    }

    private void RefreshCondition(List<string> attrIds)
    {
        if (_conditionPanel == null) return;
        _conditionPanel.Children.Clear();
        for (int i = 0; i < _context.Condition.Conjunctions.Count; i++)
        {
            var conj = _context.Condition.Conjunctions[i];
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
                _context.Condition.Conjunctions.RemoveAt(conjIndex);
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

    private static StackPanel CreateLabelRow(string label, out TextBox box)
    {
        box = new TextBox();
        var row = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        row.Children.Add(new TextBlock { Text = label, FontWeight = FontWeight.SemiBold });
        row.Children.Add(box);
        return row;
    }
}
