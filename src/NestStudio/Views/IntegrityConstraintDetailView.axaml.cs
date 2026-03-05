using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using NestCore.Model;

namespace NestStudio.Views;

public partial class IntegrityConstraintDetailView : UserControl
{
    public IntegrityConstraintDetailView()
    {
        InitializeComponent();
    }

    public IntegrityConstraintDetailView(IntegrityConstraint io, KnowledgeBase kb)
    {
        InitializeComponent();
        Build(io, kb);
    }

    private void Build(IntegrityConstraint io, KnowledgeBase kb)
    {
        Root.Children.Clear();
        AddLabel("Id", io.Id);
        if (!string.IsNullOrEmpty(io.Name)) AddLabel("Název", io.Name);
        if (!string.IsNullOrEmpty(io.IdContext)) AddLabel("Kontext", io.IdContext);
        if (io.ContextThreshold.HasValue) AddLabel("Práh kontextu", io.ContextThreshold.Value.ToString("F3"));
        if (!string.IsNullOrEmpty(io.Comment)) AddLabel("Komentář", io.Comment);

        Root.Children.Add(new TextBlock { Text = "Podmínka (OR konjunkcí)", FontWeight = FontWeight.SemiBold, Margin = new(0, 12, 0, 4) });
        if (io.Condition.Conjunctions.Count == 0)
            Root.Children.Add(new TextBlock { Text = "  — prázdná", Margin = new(8, 0, 0, 0), Opacity = 0.7 });
        else
        {
            for (int i = 0; i < io.Condition.Conjunctions.Count; i++)
            {
                var conj = io.Condition.Conjunctions[i];
                var parts = new List<string>();
                foreach (var lit in conj.Literals)
                {
                    var name = ResolveLiteral(lit, kb);
                    parts.Add(lit.Negation ? "¬" + name : name);
                }
                Root.Children.Add(new TextBlock
                {
                    Text = "  " + (i + 1) + ". " + string.Join(" ∧ ", parts),
                    Margin = new(8, 0, 0, 2),
                    TextWrapping = TextWrapping.Wrap
                });
            }
        }

        Root.Children.Add(new TextBlock { Text = "Závěry", FontWeight = FontWeight.SemiBold, Margin = new(0, 12, 0, 4) });
        if (io.Conclusions.Count == 0)
            Root.Children.Add(new TextBlock { Text = "  — žádné", Margin = new(8, 0, 0, 0), Opacity = 0.7 });
        else
        {
            foreach (var c in io.Conclusions)
            {
                var name = ResolveConclusion(c, kb);
                Root.Children.Add(new TextBlock
                {
                    Text = "  • " + name + (c.Negation ? " (negace)" : ""),
                    Margin = new(8, 0, 0, 2)
                });
            }
        }
    }

    private static string ResolveLiteral(Literal lit, KnowledgeBase kb)
    {
        var attr = kb.Attributes.FirstOrDefault(a => a.Id == lit.AttributeId);
        if (attr == null) return lit.AttributeId + (lit.PropositionId != null ? "(" + lit.PropositionId + ")" : "");
        var name = attr.Name ?? attr.Id;
        if (string.IsNullOrEmpty(lit.PropositionId)) return name;
        var prop = attr.Propositions.FirstOrDefault(p => p.Id == lit.PropositionId);
        return name + "(" + (prop?.Name ?? lit.PropositionId) + ")";
    }

    private static string ResolveConclusion(Conclusion c, KnowledgeBase kb)
    {
        var attr = kb.Attributes.FirstOrDefault(a => a.Id == c.AttributeId);
        if (attr == null) return c.AttributeId + (c.PropositionId != null ? "(" + c.PropositionId + ")" : "");
        var name = attr.Name ?? attr.Id;
        if (string.IsNullOrEmpty(c.PropositionId)) return name;
        var prop = attr.Propositions.FirstOrDefault(p => p.Id == c.PropositionId);
        return name + "(" + (prop?.Name ?? c.PropositionId) + ")";
    }

    private void AddLabel(string label, string value)
    {
        Root.Children.Add(new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                new TextBlock { Text = label + ":", FontWeight = FontWeight.SemiBold, MinWidth = 80 },
                new TextBlock { Text = value }
            }
        });
    }
}
