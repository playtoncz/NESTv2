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

public partial class AttributeEditView : UserControl
{
    private static readonly SolidColorBrush LabelBrush = new(Color.FromRgb(0x64, 0x74, 0x8B));
    private readonly NestCore.Model.Attribute _attr;
    private readonly KnowledgeBase _kb;
    private readonly Action _onDirty;
    private readonly StackPanel _propsPanel;

    public AttributeEditView()
    {
        _attr = new NestCore.Model.Attribute();
        _kb = new KnowledgeBase();
        _onDirty = () => { };
        _propsPanel = new StackPanel();
        InitializeComponent();
    }

    public AttributeEditView(NestCore.Model.Attribute attr, KnowledgeBase kb, Action onDirty)
    {
        _attr = attr;
        _kb = kb;
        _onDirty = onDirty;
        _propsPanel = new StackPanel { Spacing = 8 };
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

    private void Build()
    {
        Root.Children.Clear();

        // --- Type badge ---
        var typeName = _attr.Type switch
        {
            AttributeType.Binary => "Binární",
            AttributeType.Single => "Jednoduchý (vylučující)",
            AttributeType.Multiple => "Množinový (nevylučující)",
            AttributeType.Numeric => "Numerický",
            _ => _attr.Type.ToString()
        };
        var typeColor = _attr.Type switch
        {
            AttributeType.Binary => Color.FromRgb(0x42, 0xA5, 0xF5),
            AttributeType.Single => Color.FromRgb(0xFF, 0xCA, 0x28),
            AttributeType.Multiple => Color.FromRgb(0x66, 0xBB, 0x6A),
            AttributeType.Numeric => Color.FromRgb(0xAB, 0x47, 0xBC),
            _ => Color.FromRgb(0x94, 0xA3, 0xB8)
        };
        var badge = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, typeColor.R, typeColor.G, typeColor.B)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 6),
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 12),
            Child = new TextBlock
            {
                Text = typeName,
                FontWeight = FontWeight.SemiBold,
                FontSize = 13,
                Foreground = new SolidColorBrush(typeColor)
            }
        };
        Root.Children.Add(badge);

        // --- Basic info section ---
        var basicContent = new StackPanel { Spacing = 4 };

        var idBox = new TextBox { Text = _attr.Id };
        idBox.LostFocus += (_, _) => { _attr.Id = idBox.Text ?? ""; _onDirty(); };
        basicContent.Children.Add(MakeField("ID atributu", idBox));

        var nameBox = new TextBox { Text = _attr.Name };
        nameBox.LostFocus += (_, _) => { _attr.Name = nameBox.Text ?? ""; _onDirty(); };
        basicContent.Children.Add(MakeField("Jméno", nameBox));

        var scopeCombo = new ComboBox
        {
            ItemsSource = new[] { "Case (případ)", "Environment (prostředí)" },
            SelectedIndex = _attr.Scope == ScopeKind.Environment ? 1 : 0,
            MinWidth = 180
        };
        scopeCombo.SelectionChanged += (_, _) =>
        {
            _attr.Scope = scopeCombo.SelectedIndex == 1 ? ScopeKind.Environment : ScopeKind.Case;
            _onDirty();
        };
        basicContent.Children.Add(MakeField("Scope", scopeCombo));

        Root.Children.Add(MakeSection("Základní informace", basicContent));

        // --- Comment section ---
        var commentContent = new StackPanel { Spacing = 4 };
        var commentBox = new TextBox
        {
            Text = _attr.Comment ?? "",
            Watermark = "Komentář k atributu (např. Jak moc má pacient rýmu)",
            AcceptsReturn = true,
            MinHeight = 48,
            TextWrapping = TextWrapping.Wrap
        };
        commentBox.LostFocus += (_, _) => { _attr.Comment = string.IsNullOrWhiteSpace(commentBox.Text) ? null : commentBox.Text; _onDirty(); };
        commentContent.Children.Add(commentBox);
        Root.Children.Add(MakeSection("Komentář", commentContent));

        // --- Propositions section ---
        if (_attr.Type != AttributeType.Binary)
        {
            var propsContent = new StackPanel { Spacing = 6 };
            RefreshPropositions();
            propsContent.Children.Add(_propsPanel);

            var addPropBtn = new Button { Content = "+ Nový výrok" };
            addPropBtn.Click += (_, _) =>
            {
                var ids = _attr.Propositions.Select(p => p.Id).ToHashSet();
                var id = "vyrok_1";
                for (var i = 1; ; i++) { id = "vyrok_" + i; if (!ids.Contains(id)) break; }
                _attr.Propositions.Add(new Proposition { Id = id });
                _onDirty();
                RefreshPropositions();
            };
            propsContent.Children.Add(addPropBtn);
            Root.Children.Add(MakeSection("Výroky (propozice)", propsContent));
        }

        // --- Numeric legal values section ---
        if (_attr.Type == AttributeType.Numeric)
        {
            _attr.LegalValues ??= new LegalValues();
            var lv = _attr.LegalValues;
            var lvContent = new StackPanel { Spacing = 4 };
            var loBox = new TextBox { Text = lv.LowerBound?.ToString("F1", CultureInfo.InvariantCulture) ?? "", Watermark = "Dolní mez", Width = 100 };
            var hiBox = new TextBox { Text = lv.UpperBound?.ToString("F1", CultureInfo.InvariantCulture) ?? "", Watermark = "Horní mez", Width = 100 };
            loBox.LostFocus += (_, _) =>
            {
                lv.LowerBound = double.TryParse(loBox.Text?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
                _onDirty();
            };
            hiBox.LostFocus += (_, _) =>
            {
                lv.UpperBound = double.TryParse(hiBox.Text?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
                _onDirty();
            };
            var rangeRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            rangeRow.Children.Add(MakeField("Dolní mez", loBox));
            rangeRow.Children.Add(MakeField("Horní mez", hiBox));
            lvContent.Children.Add(rangeRow);
            Root.Children.Add(MakeSection("Legal values (rozsah)", lvContent));
        }
    }

    private void RefreshPropositions()
    {
        _propsPanel.Children.Clear();
        for (var i = 0; i < _attr.Propositions.Count; i++)
        {
            var prop = _attr.Propositions[i];
            var idx = i;

            var card = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(15, 0, 122, 255)),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(14, 10),
                Margin = new Thickness(0, 0, 0, 4)
            };
            var sp = new StackPanel { Spacing = 6 };

            var topRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            var idBox = new TextBox { Text = prop.Id, Watermark = "ID", Width = 120 };
            idBox.LostFocus += (_, _) => { prop.Id = idBox.Text ?? ""; _onDirty(); };
            var nameBox = new TextBox { Text = prop.Name, Watermark = "Jméno", MinWidth = 140 };
            nameBox.LostFocus += (_, _) => { prop.Name = nameBox.Text ?? ""; _onDirty(); };
            topRow.Children.Add(MakeField("ID", idBox));
            topRow.Children.Add(MakeField("Jméno", nameBox));
            sp.Children.Add(topRow);

            var commentBox = new TextBox
            {
                Text = prop.Comment ?? "",
                Watermark = "Komentář výroku (např. Pacient má velkou rýmu)",
                MinWidth = 260,
                TextWrapping = TextWrapping.Wrap
            };
            commentBox.LostFocus += (_, _) => { prop.Comment = string.IsNullOrWhiteSpace(commentBox.Text) ? null : commentBox.Text; _onDirty(); };
            sp.Children.Add(MakeField("Komentář", commentBox));

            if (_attr.Type == AttributeType.Numeric)
            {
                prop.WeightFunction ??= new FuzzyBounds();
                var wf = prop.WeightFunction;
                var fuzzyRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
                fuzzyRow.Children.Add(MakeField("Fuzzy dolní", MakeNumBox(wf.FuzzyLower, v => { wf.FuzzyLower = v; _onDirty(); })));
                fuzzyRow.Children.Add(MakeField("Crisp dolní", MakeNumBox(wf.CrispLower, v => { wf.CrispLower = v; _onDirty(); })));
                fuzzyRow.Children.Add(MakeField("Crisp horní", MakeNumBox(wf.CrispUpper, v => { wf.CrispUpper = v; _onDirty(); })));
                fuzzyRow.Children.Add(MakeField("Fuzzy horní", MakeNumBox(wf.FuzzyUpper, v => { wf.FuzzyUpper = v; _onDirty(); })));
                sp.Children.Add(fuzzyRow);
            }

            var delBtn = new Button { Content = "Smazat výrok", Padding = new Thickness(8, 4), HorizontalAlignment = HorizontalAlignment.Left };
            delBtn.Click += (_, _) =>
            {
                _attr.Propositions.RemoveAt(idx);
                _onDirty();
                RefreshPropositions();
            };
            sp.Children.Add(delBtn);

            card.Child = sp;
            _propsPanel.Children.Add(card);
        }
    }

    private static TextBox MakeNumBox(double initial, Action<double> onChanged)
    {
        var tb = new TextBox { Text = initial.ToString("F1", CultureInfo.InvariantCulture), Width = 70 };
        tb.LostFocus += (_, _) =>
        {
            if (double.TryParse(tb.Text?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                onChanged(v);
        };
        return tb;
    }
}
