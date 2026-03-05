using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using NestCore.Model;

namespace NestStudio.Views;

public partial class StatisticsView : UserControl
{
    /// <summary>Pro runtime loader / XAML. V kódu používej konstruktor s KnowledgeBase.</summary>
    public StatisticsView()
    {
        InitializeComponent();
    }

    public StatisticsView(KnowledgeBase kb)
    {
        InitializeComponent();
        Build(kb);
    }

    private void Build(KnowledgeBase kb)
    {
        Root.Children.Clear();

        Root.Children.Add(new TextBlock
        {
            Text = "Statistiky znalostní báze",
            FontWeight = FontWeight.SemiBold,
            FontSize = 16,
            Margin = new(0, 0, 0, 8)
        });

        var binary = kb.Attributes.Count(a => a.Type == AttributeType.Binary);
        var single = kb.Attributes.Count(a => a.Type == AttributeType.Single);
        var multiple = kb.Attributes.Count(a => a.Type == AttributeType.Multiple);
        var numeric = kb.Attributes.Count(a => a.Type == AttributeType.Numeric);
        var totalProps = kb.Attributes.Sum(a => a.Propositions.Count);
        var caseScope = kb.Attributes.Count(a => a.Scope == ScopeKind.Case);
        var envScope = kb.Attributes.Count(a => a.Scope == ScopeKind.Environment);

        AddSection("Atributy", new[]
        {
            ("Celkem", kb.Attributes.Count.ToString()),
            ("Binary", binary.ToString()),
            ("Single", single.ToString()),
            ("Multiple", multiple.ToString()),
            ("Numeric", numeric.ToString()),
            ("Scope: case", caseScope.ToString()),
            ("Scope: environment", envScope.ToString())
        });

        AddSection("Propozice", new[] { ("Celkem", totalProps.ToString()) });

        AddSection("Pravidla a omezení", new[]
        {
            ("Kompoziční pravidla", kb.CompositionalRules.Count.ToString()),
            ("Kontexty", kb.Contexts.Count.ToString()),
            ("Integritní omezení", kb.IntegrityConstraints.Count.ToString())
        });

        Root.Children.Add(new TextBlock
        {
            Text = "Globální nastavení",
            FontWeight = FontWeight.SemiBold,
            Margin = new(0, 16, 0, 4)
        });
        Root.Children.Add(new TextBlock { Text = $"Inference: {kb.Global.InferenceMechanism}", Margin = new(8, 0, 0, 2) });
        Root.Children.Add(new TextBlock { Text = $"Výchozí váha: {kb.Global.DefaultWeight}", Margin = new(8, 0, 0, 2) });
        Root.Children.Add(new TextBlock { Text = $"Globální priorita: {kb.Global.GlobalPriority}", Margin = new(8, 0, 0, 2) });
    }

    private void AddSection(string title, (string Label, string Value)[] rows)
    {
        Root.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeight.SemiBold,
            Margin = new(0, 12, 0, 4)
        });
        foreach (var (label, value) in rows)
        {
            Root.Children.Add(new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 12,
                Margin = new(8, 0, 0, 2),
                Children =
                {
                    new TextBlock { Text = label + ":", MinWidth = 140 },
                    new TextBlock { Text = value }
                }
            });
        }
    }
}
