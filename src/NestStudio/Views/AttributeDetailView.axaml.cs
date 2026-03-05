using Avalonia.Controls;
using Avalonia.Media;
using NestCore.Model;

namespace NestStudio.Views;

public partial class AttributeDetailView : UserControl
{
    /// <summary>Pro runtime loader / XAML. V kódu používej konstruktor s Attribute.</summary>
    public AttributeDetailView()
    {
        InitializeComponent();
    }

    public AttributeDetailView(NestCore.Model.Attribute attr)
    {
        InitializeComponent();
        Build(attr);
    }

    private void Build(NestCore.Model.Attribute attr)
    {
        Root.Children.Clear();
        AddLabel("Id", attr.Id);
        AddLabel("Název", attr.Name ?? "—");
        AddLabel("Typ", attr.Type.ToString());
        AddLabel("Scope", attr.Scope.ToString());
        if (!string.IsNullOrEmpty(attr.Comment)) AddLabel("Komentář", attr.Comment);

        if (attr.LegalValues != null)
        {
            Root.Children.Add(new TextBlock { Text = "Legal values", FontWeight = FontWeight.SemiBold, Margin = new(0, 12, 0, 4) });
            var bounds = "";
            if (attr.LegalValues.LowerBound.HasValue) bounds += $" [{attr.LegalValues.LowerBound}";
            if (attr.LegalValues.UpperBound.HasValue) bounds += " .. " + attr.LegalValues.UpperBound + "]";
            if (!string.IsNullOrEmpty(bounds)) Root.Children.Add(new TextBlock { Text = bounds.TrimStart(), Margin = new(8, 0, 0, 0) });
        }

        if (attr.Propositions.Count > 0)
        {
            Root.Children.Add(new TextBlock { Text = "Propozice", FontWeight = FontWeight.SemiBold, Margin = new(0, 12, 0, 4) });
            foreach (var p in attr.Propositions)
            {
                var line = p.Name ?? p.Id;
                if (p.WeightFunction != null)
                    line += $"  [fuzzy: {p.WeightFunction.FuzzyLower:F1}–{p.WeightFunction.CrispLower:F1}–{p.WeightFunction.CrispUpper:F1}–{p.WeightFunction.FuzzyUpper:F1}]";
                Root.Children.Add(new TextBlock { Text = "  • " + line, Margin = new(8, 0, 0, 2) });
            }
        }
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
