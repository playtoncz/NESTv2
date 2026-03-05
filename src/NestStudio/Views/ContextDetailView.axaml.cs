using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using NestCore.Model;

namespace NestStudio.Views;

public partial class ContextDetailView : UserControl
{
    public ContextDetailView()
    {
        InitializeComponent();
    }

    public ContextDetailView(Context context, KnowledgeBase kb)
    {
        InitializeComponent();
        Build(context, kb);
    }

    private void Build(Context context, KnowledgeBase kb)
    {
        Root.Children.Clear();
        AddLabel("Id", context.Id);
        if (!string.IsNullOrEmpty(context.Comment)) AddLabel("Komentář", context.Comment);

        Root.Children.Add(new TextBlock { Text = "Podmínka (OR konjunkcí)", FontWeight = FontWeight.SemiBold, Margin = new(0, 12, 0, 4) });
        if (context.Condition.Conjunctions.Count == 0)
            Root.Children.Add(new TextBlock { Text = "  — prázdná", Margin = new(8, 0, 0, 0), Opacity = 0.7 });
        else
        {
            for (int i = 0; i < context.Condition.Conjunctions.Count; i++)
            {
                var conj = context.Condition.Conjunctions[i];
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
