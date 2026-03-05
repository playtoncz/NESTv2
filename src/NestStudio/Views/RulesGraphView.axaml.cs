using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using NestCore.Model;

namespace NestStudio.Views;

public partial class RulesGraphView : UserControl
{
    private const double NodeRadius = 24;
    private const double GraphMargin = 50;

    /// <summary>Pro runtime loader / XAML. V kódu používej konstruktor s KnowledgeBase.</summary>
    public RulesGraphView()
    {
        InitializeComponent();
    }

    public RulesGraphView(KnowledgeBase kb)
    {
        InitializeComponent();
        BuildGraph(kb);
    }

    private void BuildGraph(KnowledgeBase kb)
    {
        var nodeIds = new List<string>();
        var nodeLabels = new Dictionary<string, string>();
        foreach (var a in kb.Attributes)
        {
            nodeIds.Add(a.Id);
            nodeLabels[a.Id] = string.IsNullOrWhiteSpace(a.Name) ? a.Id : a.Name;
        }

        var edges = new List<(string From, string To, string RuleId)>();
        foreach (var rule in kb.CompositionalRules)
        {
            var fromAttrs = new HashSet<string>();
            foreach (var conj in rule.Condition.Conjunctions)
            foreach (var lit in conj.Literals)
                fromAttrs.Add(lit.AttributeId);

            foreach (var concl in rule.Conclusions)
            {
                foreach (var fa in fromAttrs)
                    edges.Add((fa, concl.AttributeId, rule.Id));
            }
        }

        if (nodeIds.Count == 0)
        {
            var noData = new TextBlock { Text = "Žádné atributy v KB.", Margin = new Thickness(20) };
            Canvas.SetLeft(noData, 0);
            Canvas.SetTop(noData, 0);
            GraphCanvas.Children.Add(noData);
            return;
        }

        double width = 600;
        double height = 500;
        double cx = width / 2;
        double cy = height / 2;
        double radius = Math.Min(width, height) / 2 - GraphMargin;
        var positions = new Dictionary<string, (double X, double Y)>();
        for (int i = 0; i < nodeIds.Count; i++)
        {
            var angle = 2 * Math.PI * i / nodeIds.Count - Math.PI / 2;
            positions[nodeIds[i]] = (cx + radius * Math.Cos(angle), cy + radius * Math.Sin(angle));
        }

        foreach (var (from, to, ruleId) in edges)
        {
            if (!positions.TryGetValue(from, out var p1) || !positions.TryGetValue(to, out var p2)) continue;
            var line = new Line
            {
                StartPoint = new Point(p1.X, p1.Y),
                EndPoint = new Point(p2.X, p2.Y),
                Stroke = new SolidColorBrush(Color.FromArgb(180, 100, 100, 100)),
                StrokeThickness = 1
            };
            GraphCanvas.Children.Add(line);
        }

        foreach (var id in nodeIds)
        {
            if (!positions.TryGetValue(id, out var pos)) continue;
            var label = nodeLabels.TryGetValue(id, out var L) ? L : id;
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(230, 240, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(67, 97, 238)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(NodeRadius),
                Width = NodeRadius * 2,
                Height = NodeRadius * 2,
                Child = new TextBlock
                {
                    Text = label.Length <= 8 ? label : label[..6] + "…",
                    FontSize = 10,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                }
            };
            Canvas.SetLeft(border, pos.X - NodeRadius);
            Canvas.SetTop(border, pos.Y - NodeRadius);
            GraphCanvas.Children.Add(border);
        }
    }
}
