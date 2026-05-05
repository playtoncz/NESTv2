using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using NestCore.Model;

namespace NestStudio.Views;

public partial class HowGraphView : UserControl
{
    private const double NodeW = 160;
    private const double NodeH = 56;
    private const double LayerGap = 110;
    private const double NodeGap = 20;
    private const double MX = 40;
    private const double MY = 40;

    public event EventHandler? BackRequested;

    public HowGraphView()
    {
        InitializeComponent();
        BackButton.Click += OnBack;
    }

    public HowGraphView(KnowledgeBase kb, InferenceResult result, string targetAttributeId, string? targetPropositionId)
    {
        InitializeComponent();
        BackButton.Click += OnBack;
        TitleText.Text = $"How: {targetAttributeId}{(string.IsNullOrEmpty(targetPropositionId) ? "" : $"[{targetPropositionId}]")}";
        BuildGraph(kb, result, targetAttributeId, targetPropositionId);
        BuildTree(kb, result, targetAttributeId, targetPropositionId);
    }

    private void OnBack(object? sender, RoutedEventArgs e)
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    private void BuildTree(KnowledgeBase kb, InferenceResult result, string targetAttributeId, string? targetPropositionId)
    {
        HowTree.Items.Clear();
        var root = new TreeViewItem
        {
            Header = $"Cíl: {targetAttributeId}{(string.IsNullOrEmpty(targetPropositionId) ? "" : $"[{targetPropositionId}]")}",
            IsExpanded = true
        };

        var ruleById = kb.CompositionalRules.ToDictionary(r => r.Id);
        var relevantFired = result.FiredRules
            .Where(fr => fr.AppliedConclusions.Any(ac => ac.AttributeId == targetAttributeId && ac.PropositionId == targetPropositionId))
            .ToList();

        foreach (var fr in relevantFired)
        {
            var total = fr.AppliedConclusions
                .Where(ac => ac.AttributeId == targetAttributeId && ac.PropositionId == targetPropositionId)
                .Sum(ac => ac.WeightChange);
            var sign = total >= 0 ? "+" : "";
            var ruleNode = new TreeViewItem
            {
                Header = $"{fr.RuleId} ({sign}{total:F3})",
                IsExpanded = true
            };

            if (ruleById.TryGetValue(fr.RuleId, out var rule))
            {
                foreach (var conj in rule.Condition.Conjunctions)
                {
                    foreach (var lit in conj.Literals)
                    {
                        var litText = $"{(lit.Negation ? "NOT " : "")}{lit.AttributeId}";
                        if (!string.IsNullOrEmpty(lit.PropositionId))
                            litText += $"[{lit.PropositionId}]";
                        ruleNode.Items.Add(new TreeViewItem { Header = litText });
                    }
                }
            }

            root.Items.Add(ruleNode);
        }

        HowTree.Items.Add(root);
    }

    private void BuildGraph(KnowledgeBase kb, InferenceResult result, string targetAttributeId, string? targetPropositionId)
    {
        GraphCanvas.Children.Clear();
        var nodeTextColor = ActualThemeVariant == ThemeVariant.Dark ? Colors.White : Color.FromRgb(0x0F, 0x17, 0x2A);
        var ruleById = kb.CompositionalRules.ToDictionary(r => r.Id);
        var relevantFired = result.FiredRules
            .Where(fr => fr.AppliedConclusions.Any(ac => ac.AttributeId == targetAttributeId && ac.PropositionId == targetPropositionId))
            .ToList();

        if (relevantFired.Count == 0)
        {
            AddText("Pro vybraný cíl nejsou dostupná explainability data.", 20, 20);
            return;
        }

        var nodes = new HashSet<string> { targetAttributeId };
        var goalIds = new HashSet<string> { targetAttributeId };
        var intermediateIds = new HashSet<string>();
        var inputIds = new HashSet<string>();

        foreach (var fr in relevantFired)
        {
            if (!ruleById.TryGetValue(fr.RuleId, out var rule)) continue;
            foreach (var conj in rule.Condition.Conjunctions)
            {
                foreach (var lit in conj.Literals)
                {
                    if (string.IsNullOrWhiteSpace(lit.AttributeId)) continue;
                    nodes.Add(lit.AttributeId);
                    inputIds.Add(lit.AttributeId);
                }
            }

            foreach (var c in rule.Conclusions)
            {
                if (string.IsNullOrWhiteSpace(c.AttributeId)) continue;
                nodes.Add(c.AttributeId);
                if (c.AttributeId != targetAttributeId)
                    intermediateIds.Add(c.AttributeId);
            }
        }

        foreach (var i in intermediateIds)
            inputIds.Remove(i);
        inputIds.Remove(targetAttributeId);

        var layers = new List<List<string>>
        {
            goalIds.OrderBy(x => x).ToList(),
            intermediateIds.OrderBy(x => x).ToList(),
            inputIds.OrderBy(x => x).ToList()
        };
        var layerColors = new[]
        {
            Color.FromRgb(0x66, 0xBB, 0x6A),
            Color.FromRgb(0xFF, 0xCA, 0x28),
            Color.FromRgb(0x42, 0xA5, 0xF5)
        };

        var positions = new Dictionary<string, (double X, double Y)>();
        double maxWidth = 0;
        for (int layer = 0; layer < layers.Count; layer++)
        {
            var layerNodes = layers[layer];
            if (layerNodes.Count == 0) continue;
            var y = MY + layer * (NodeH + LayerGap);
            var totalWidth = layerNodes.Count * NodeW + (layerNodes.Count - 1) * NodeGap;
            maxWidth = Math.Max(maxWidth, totalWidth);
            for (var i = 0; i < layerNodes.Count; i++)
                positions[layerNodes[i]] = (MX + i * (NodeW + NodeGap), y);
        }

        var canvasWidth = maxWidth + 2 * MX;
        var canvasHeight = MY * 2 + 3 * NodeH + 2 * LayerGap + 50;
        GraphCanvas.Width = Math.Max(canvasWidth, 600);
        GraphCanvas.Height = Math.Max(canvasHeight, 420);

        foreach (var fr in relevantFired)
        {
            if (!ruleById.TryGetValue(fr.RuleId, out var rule)) continue;
            var ruleEdgeWeight = fr.AppliedConclusions
                .Where(ac => ac.AttributeId == targetAttributeId && ac.PropositionId == targetPropositionId)
                .Sum(ac => ac.WeightChange);
            var edgeColor = ruleEdgeWeight > 0
                ? Color.FromRgb(0x22, 0xC5, 0x5E)
                : ruleEdgeWeight < 0
                    ? Color.FromRgb(0xEF, 0x44, 0x44)
                    : Color.FromRgb(0x94, 0xA3, 0xB8);

            var fromIds = rule.Condition.Conjunctions
                .SelectMany(c => c.Literals)
                .Select(l => l.AttributeId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
            var toIds = rule.Conclusions.Select(c => c.AttributeId).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
            foreach (var fromId in fromIds)
            {
                foreach (var toId in toIds)
                {
                    if (!positions.TryGetValue(fromId, out var p1) || !positions.TryGetValue(toId, out var p2)) continue;
                    var x1 = p1.X + NodeW / 2;
                    var x2 = p2.X + NodeW / 2;
                    var y1 = p1.Y <= p2.Y ? p1.Y + NodeH : p1.Y;
                    var y2 = p1.Y <= p2.Y ? p2.Y : p2.Y + NodeH;
                    GraphCanvas.Children.Add(new Line
                    {
                        StartPoint = new Point(x1, y1),
                        EndPoint = new Point(x2, y2),
                        Stroke = new SolidColorBrush(edgeColor),
                        StrokeThickness = 2
                    });
                    DrawArrowHead(x1, y1, x2, y2, edgeColor);
                    AddText($"{(ruleEdgeWeight >= 0 ? "+" : "")}{ruleEdgeWeight:F3}", (x1 + x2) / 2 + 4, (y1 + y2) / 2 - 8, edgeColor);
                }
            }
        }

        var attrById = kb.Attributes.ToDictionary(a => a.Id);
        for (int layer = 0; layer < layers.Count; layer++)
        {
            var layerNodes = layers[layer];
            var color = layerColors[layer];
            foreach (var id in layerNodes)
            {
                if (!positions.TryGetValue(id, out var pos)) continue;
                var label = attrById.TryGetValue(id, out var attr) && !string.IsNullOrWhiteSpace(attr.Name)
                    ? attr.Name!
                    : id;
                if (string.IsNullOrWhiteSpace(label))
                    label = id;
                if (string.IsNullOrWhiteSpace(label))
                    label = "(bez názvu)";
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(60, color.R, color.G, color.B)),
                    BorderBrush = new SolidColorBrush(color),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(8),
                    Width = NodeW,
                    Height = NodeH,
                    Padding = new Thickness(6, 4),
                    Child = new TextBlock
                    {
                        Text = label,
                        FontSize = 12,
                        MaxLines = 2,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = new SolidColorBrush(nodeTextColor),
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        TextAlignment = Avalonia.Media.TextAlignment.Center
                    }
                };
                ToolTip.SetTip(border, $"{id}\n{label}");
                Canvas.SetLeft(border, pos.X);
                Canvas.SetTop(border, pos.Y);
                GraphCanvas.Children.Add(border);
            }
        }
    }

    private void DrawArrowHead(double x1, double y1, double x2, double y2, Color color)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        var len = Math.Sqrt(dx * dx + dy * dy);
        if (len <= 0) return;
        var ux = dx / len;
        var uy = dy / len;
        var ax = x2 - ux * 8;
        var ay = y2 - uy * 8;
        var px = -uy * 4;
        var py = ux * 4;
        var brush = new SolidColorBrush(color);
        GraphCanvas.Children.Add(new Line { StartPoint = new Point(x2, y2), EndPoint = new Point(ax + px, ay + py), Stroke = brush, StrokeThickness = 1.5 });
        GraphCanvas.Children.Add(new Line { StartPoint = new Point(x2, y2), EndPoint = new Point(ax - px, ay - py), Stroke = brush, StrokeThickness = 1.5 });
    }

    private void AddText(string text, double x, double y, Color? color = null)
    {
        var tb = new TextBlock { Text = text, FontSize = 11 };
        if (color.HasValue)
            tb.Foreground = new SolidColorBrush(color.Value);
        Canvas.SetLeft(tb, x);
        Canvas.SetTop(tb, y);
        GraphCanvas.Children.Add(tb);
    }
}
