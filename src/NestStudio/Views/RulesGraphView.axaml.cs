using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using NestCore.Inference;
using NestCore.Model;

namespace NestStudio.Views;

public partial class RulesGraphView : UserControl
{
    private const double NodeW = 160;
    private const double NodeH = 56;
    private const double LayerGap = 110;
    private const double NodeGap = 20;
    private const double MX = 40;
    private const double MY = 40;

    private KnowledgeBase? _kb;

    public event Action<string>? AttributeClicked;

    public RulesGraphView()
    {
        InitializeComponent();
    }

    public RulesGraphView(KnowledgeBase kb)
    {
        _kb = kb;
        InitializeComponent();
        BuildTree(kb);
        BuildGraph(kb);
    }

    // ─── Tree ──────────────────────────────────────────────────────

    private void BuildTree(KnowledgeBase kb)
    {
        RuleTree.Items.Clear();
        TreeDetailPanel.Children.Clear();

        var inputIds = QuestionAnalyzer.GetInputAttributeIds(kb);
        var derivedIds = QuestionAnalyzer.GetDerivedAttributeIds(kb);
        var attrById = kb.Attributes.ToDictionary(a => a.Id);

        var goalIds = new HashSet<string>();
        foreach (var a in kb.Attributes)
        {
            var isDerived = derivedIds.Contains(a.Id);
            var isInput = inputIds.Contains(a.Id);
            if (isDerived && !isInput)
            {
                bool usedInCondition = kb.CompositionalRules.Any(r =>
                    r.Condition.Conjunctions.Any(conj =>
                        conj.Literals.Any(lit => lit.AttributeId == a.Id)));
                if (!usedInCondition) goalIds.Add(a.Id);
            }
        }
        if (goalIds.Count == 0)
            foreach (var id in derivedIds) goalIds.Add(id);

        foreach (var goalId in goalIds.OrderBy(id => id))
        {
            var goalLabel = attrById.TryGetValue(goalId, out var ga)
                ? (string.IsNullOrWhiteSpace(ga.Name) ? ga.Id : ga.Name)
                : goalId;

            var goalNode = new TreeViewItem { Header = goalLabel, Tag = goalId, IsExpanded = true };
            goalNode.Classes.Add("tree-goal");

            foreach (var rule in kb.CompositionalRules)
            {
                bool concludesToGoal = rule.Conclusions.Any(c => c.AttributeId == goalId);
                if (!concludesToGoal) continue;

                var kindPrefix = rule.Kind switch
                {
                    RuleKind.Apriori => "A",
                    RuleKind.Logical => "L",
                    _ => "C"
                };
                var ruleNode = new TreeViewItem
                {
                    Header = $"{kindPrefix} & ({rule.Id}){FormatRuleInputs(rule, attrById)}",
                    Tag = rule.Id,
                    IsExpanded = false
                };

                foreach (var conj in rule.Condition.Conjunctions)
                {
                    foreach (var lit in conj.Literals)
                    {
                        var litLabel = attrById.TryGetValue(lit.AttributeId, out var la)
                            ? (string.IsNullOrWhiteSpace(la.Name) ? la.Id : la.Name)
                            : lit.AttributeId;
                        if (!string.IsNullOrEmpty(lit.PropositionId))
                            litLabel += $"[{lit.PropositionId}]";
                        if (lit.Negation)
                            litLabel = "NOT " + litLabel;

                        var litNode = new TreeViewItem { Header = litLabel, Tag = lit.AttributeId };
                        ruleNode.Items.Add(litNode);
                    }
                }

                goalNode.Items.Add(ruleNode);
            }

            RuleTree.Items.Add(goalNode);
        }

        RuleTree.SelectionChanged += OnTreeSelectionChanged;

        BuildTreeDetailDump(kb, attrById);
    }

    private static string FormatRuleInputs(CompositionalRule rule, Dictionary<string, NestCore.Model.Attribute> attrById)
    {
        var inputs = new HashSet<string>();
        foreach (var conj in rule.Condition.Conjunctions)
            foreach (var lit in conj.Literals)
                if (!string.IsNullOrEmpty(lit.AttributeId))
                    inputs.Add(lit.AttributeId);

        if (inputs.Count == 0) return "";
        var names = inputs.Select(id =>
            attrById.TryGetValue(id, out var a) && !string.IsNullOrWhiteSpace(a.Name) ? a.Name : id);
        return string.Join(", ", names);
    }

    private void BuildTreeDetailDump(KnowledgeBase kb, Dictionary<string, NestCore.Model.Attribute> attrById)
    {
        TreeDetailPanel.Children.Clear();
        TreeDetailPanel.Children.Add(new TextBlock
        {
            Text = "Atributy:",
            FontWeight = FontWeight.Bold,
            FontSize = 16,
            Margin = new Thickness(0, 0, 0, 8)
        });

        foreach (var a in kb.Attributes)
        {
            var card = new Border();
            card.Classes.Add("editor-section");
            var sp = new StackPanel { Spacing = 2 };

            sp.Children.Add(new TextBlock { Text = "Atribut:", FontWeight = FontWeight.SemiBold });
            sp.Children.Add(MakeDetailLine("ID", a.Id));
            sp.Children.Add(MakeDetailLine("Jméno", a.Name));
            if (!string.IsNullOrWhiteSpace(a.Comment))
                sp.Children.Add(MakeDetailLine("Komentář", a.Comment));
            sp.Children.Add(MakeDetailLine("Typ", a.Type switch
            {
                AttributeType.Binary => "binární",
                AttributeType.Single => "jednoduchý",
                AttributeType.Multiple => "množinový",
                AttributeType.Numeric => "numerický",
                _ => a.Type.ToString()
            }));
            sp.Children.Add(MakeDetailLine("Rozsah", a.Scope == ScopeKind.Environment ? "Obecný" : "Případ"));

            foreach (var p in a.Propositions)
            {
                var propSp = new StackPanel { Spacing = 1, Margin = new Thickness(16, 4, 0, 4) };
                propSp.Children.Add(new TextBlock { Text = "Výrok:", FontWeight = FontWeight.SemiBold, FontSize = 12 });
                propSp.Children.Add(MakeDetailLine("ID", p.Id, 12));
                propSp.Children.Add(MakeDetailLine("Jméno", string.IsNullOrWhiteSpace(p.Name) ? $"{a.Id}({p.Id})" : p.Name, 12));
                if (!string.IsNullOrWhiteSpace(p.Comment))
                    propSp.Children.Add(MakeDetailLine("Komentář", p.Comment, 12));
                if (p.WeightFunction != null)
                    propSp.Children.Add(MakeDetailLine("Fuzzy",
                        $"{p.WeightFunction.FuzzyLower:F1} – {p.WeightFunction.CrispLower:F1} – {p.WeightFunction.CrispUpper:F1} – {p.WeightFunction.FuzzyUpper:F1}", 12));
                sp.Children.Add(propSp);
            }

            card.Child = sp;
            TreeDetailPanel.Children.Add(card);
        }
    }

    private static TextBlock MakeDetailLine(string label, string value, double fontSize = 13)
    {
        return new TextBlock
        {
            Text = $"  {label}: {value}",
            FontSize = fontSize,
            Margin = new Thickness(8, 0, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private void OnTreeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (RuleTree.SelectedItem is TreeViewItem item && item.Tag is string id)
            AttributeClicked?.Invoke(id);
    }

    // ─── Graph (canvas) ────────────────────────────────────────────

    private void BuildGraph(KnowledgeBase kb)
    {
        GraphCanvas.Children.Clear();

        if (kb.Attributes.Count == 0)
        {
            AddText("Žádné atributy v KB.", 20, 20);
            return;
        }

        var inputIds = QuestionAnalyzer.GetInputAttributeIds(kb);
        var derivedIds = QuestionAnalyzer.GetDerivedAttributeIds(kb);

        var goalIds = new HashSet<string>();
        var intermediateIds = new HashSet<string>();
        var questionIds = new HashSet<string>();

        foreach (var a in kb.Attributes)
        {
            var isInput = inputIds.Contains(a.Id);
            var isDerived = derivedIds.Contains(a.Id);
            if (isDerived && !isInput)
            {
                bool isGoal = !kb.CompositionalRules.Any(r =>
                    r.Condition.Conjunctions.Any(conj =>
                        conj.Literals.Any(lit => lit.AttributeId == a.Id)));
                if (isGoal) goalIds.Add(a.Id);
                else intermediateIds.Add(a.Id);
            }
            else if (isInput && !isDerived) questionIds.Add(a.Id);
            else if (isInput && isDerived) intermediateIds.Add(a.Id);
            else questionIds.Add(a.Id);
        }

        var layers = new List<List<string>>
        {
            goalIds.OrderBy(id => id).ToList(),
            intermediateIds.OrderBy(id => id).ToList(),
            questionIds.OrderBy(id => id).ToList()
        };
        var layerColors = new[]
        {
            Color.FromRgb(0x66, 0xBB, 0x6A),
            Color.FromRgb(0xFF, 0xCA, 0x28),
            Color.FromRgb(0x42, 0xA5, 0xF5)
        };
        var layerNames = new[] { "Cíle", "Meziuzly", "Vstupy (otázky)" };

        var positions = new Dictionary<string, (double X, double Y)>();
        double maxWidth = 0;
        for (int layer = 0; layer < layers.Count; layer++)
        {
            var nodes = layers[layer];
            if (nodes.Count == 0) continue;
            double y = MY + layer * (NodeH + LayerGap);
            double totalWidth = nodes.Count * NodeW + (nodes.Count - 1) * NodeGap;
            if (totalWidth > maxWidth) maxWidth = totalWidth;
            for (int i = 0; i < nodes.Count; i++)
                positions[nodes[i]] = (MX + i * (NodeW + NodeGap), y);
        }

        double canvasWidth = maxWidth + 2 * MX;
        double canvasHeight = MY * 2 + 3 * NodeH + 2 * LayerGap + 60;
        GraphCanvas.Width = Math.Max(canvasWidth, 600);
        GraphCanvas.Height = Math.Max(canvasHeight, 400);

        var edges = new List<(string From, string To, string RuleId, RuleKind Kind)>();
        foreach (var rule in kb.CompositionalRules)
        {
            var fromAttrs = new HashSet<string>();
            foreach (var conj in rule.Condition.Conjunctions)
                foreach (var lit in conj.Literals)
                    if (!string.IsNullOrEmpty(lit.AttributeId))
                        fromAttrs.Add(lit.AttributeId);
            foreach (var concl in rule.Conclusions)
            {
                if (fromAttrs.Count == 0)
                    edges.Add(("", concl.AttributeId, rule.Id, rule.Kind));
                else
                    foreach (var fa in fromAttrs)
                        edges.Add((fa, concl.AttributeId, rule.Id, rule.Kind));
            }
        }

        foreach (var (from, to, ruleId, kind) in edges)
        {
            if (string.IsNullOrEmpty(from)) continue;
            if (!positions.TryGetValue(from, out var p1) || !positions.TryGetValue(to, out var p2)) continue;
            var x1 = p1.X + NodeW / 2;
            var x2 = p2.X + NodeW / 2;
            // Držíme tok vizuálně shora dolů bez ohledu na směr rule hrany.
            var fromAbove = p1.Y <= p2.Y;
            var y1 = fromAbove ? p1.Y + NodeH : p1.Y;
            var y2 = fromAbove ? p2.Y : p2.Y + NodeH;
            var edgeColor = kind switch
            {
                RuleKind.Apriori => Color.FromRgb(0xFF, 0x98, 0x00),
                RuleKind.Logical => Color.FromRgb(0xAB, 0x47, 0xBC),
                _ => Color.FromArgb(180, 100, 100, 100)
            };
            GraphCanvas.Children.Add(new Line
            {
                StartPoint = new Point(x1, y1),
                EndPoint = new Point(x2, y2),
                Stroke = new SolidColorBrush(edgeColor),
                StrokeThickness = 1.5
            });
            DrawArrowHead(x1, y1, x2, y2, edgeColor);
        }

        var attrById = kb.Attributes.ToDictionary(a => a.Id);
        for (int layer = 0; layer < layers.Count; layer++)
        {
            var nodes = layers[layer];
            if (nodes.Count == 0) continue;
            var color = layerColors[layer];
            foreach (var id in nodes)
            {
                if (!positions.TryGetValue(id, out var pos)) continue;
                var label = attrById.TryGetValue(id, out var a) && !string.IsNullOrWhiteSpace(a.Name)
                    ? a.Name : id;
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(60, color.R, color.G, color.B)),
                    BorderBrush = new SolidColorBrush(color),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(8),
                    Width = NodeW, Height = NodeH,
                    Padding = new Thickness(6, 4),
                    Cursor = new Cursor(StandardCursorType.Hand),
                    Tag = id,
                    Child = new TextBlock
                    {
                        Text = label,
                        FontSize = 12,
                        MaxLines = 2,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        Padding = new Thickness(6, 2),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    }
                };
                border.PointerPressed += OnNodeClicked;
                ToolTip.SetTip(border, $"{id}\n{label}");
                Canvas.SetLeft(border, pos.X);
                Canvas.SetTop(border, pos.Y);
                GraphCanvas.Children.Add(border);
            }
        }

        double legendY = canvasHeight - 50;
        AddLegendItem(MX, legendY, layerColors[0], layerNames[0]);
        AddLegendItem(MX + 170, legendY, layerColors[1], layerNames[1]);
        AddLegendItem(MX + 310, legendY, layerColors[2], layerNames[2]);
    }

    private void DrawArrowHead(double x1, double y1, double x2, double y2, Color color)
    {
        var dx = x2 - x1; var dy = y2 - y1;
        var len = Math.Sqrt(dx * dx + dy * dy);
        if (len <= 0) return;
        var ux = dx / len; var uy = dy / len;
        var ax = x2 - ux * 8; var ay = y2 - uy * 8;
        var px = -uy * 4; var py = ux * 4;
        var brush = new SolidColorBrush(color);
        GraphCanvas.Children.Add(new Line { StartPoint = new Point(x2, y2), EndPoint = new Point(ax + px, ay + py), Stroke = brush, StrokeThickness = 1.5 });
        GraphCanvas.Children.Add(new Line { StartPoint = new Point(x2, y2), EndPoint = new Point(ax - px, ay - py), Stroke = brush, StrokeThickness = 1.5 });
    }

    private void OnNodeClicked(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border b && b.Tag is string id)
            AttributeClicked?.Invoke(id);
    }

    private void AddText(string text, double x, double y)
    {
        var tb = new TextBlock { Text = text };
        Canvas.SetLeft(tb, x); Canvas.SetTop(tb, y);
        GraphCanvas.Children.Add(tb);
    }

    private void AddLegendItem(double x, double y, Color color, string label)
    {
        var rect = new Rectangle
        {
            Width = 14, Height = 14,
            Fill = new SolidColorBrush(Color.FromArgb(60, color.R, color.G, color.B)),
            Stroke = new SolidColorBrush(color), StrokeThickness = 2,
            RadiusX = 3, RadiusY = 3
        };
        Canvas.SetLeft(rect, x); Canvas.SetTop(rect, y + 2);
        GraphCanvas.Children.Add(rect);
        var tb = new TextBlock { Text = label, FontSize = 11 };
        Canvas.SetLeft(tb, x + 20); Canvas.SetTop(tb, y);
        GraphCanvas.Children.Add(tb);
    }
}
