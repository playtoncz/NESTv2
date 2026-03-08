using Avalonia.Controls;
using Avalonia.Layout;
using NestCore.Model;

namespace NestStudio.Views;

public partial class GlobalPropertiesView : UserControl
{
    /// <summary>Pro runtime loader / XAML. V kódu používej konstruktor s Global.</summary>
    public GlobalPropertiesView()
    {
        InitializeComponent();
    }

    public GlobalPropertiesView(Global g)
    {
        InitializeComponent();
        Build(g, null);
    }

    public GlobalPropertiesView(Global g, Action? onDirty)
    {
        InitializeComponent();
        Build(g, onDirty);
    }

    private void Build(Global g, Action? onDirty)
    {
        Root.Children.Clear();

        Root.Children.Add(new TextBlock { Text = "Globální vlastnosti KB", FontWeight = Avalonia.Media.FontWeight.SemiBold, FontSize = 16, Margin = new(0, 0, 0, 8) });

        AddRow("Popis", out var descBox);
        descBox.Text = g.Description ?? "";
        descBox.LostFocus += (_, _) => { g.Description = descBox.Text?.Trim(); onDirty?.Invoke(); };

        AddRow("Expert", out var expertBox);
        expertBox.Text = g.Expert ?? "";
        expertBox.LostFocus += (_, _) => { g.Expert = expertBox.Text?.Trim(); onDirty?.Invoke(); };

        AddRow("Znalostní inženýr", out var keBox);
        keBox.Text = g.KnowledgeEngineer ?? "";
        keBox.LostFocus += (_, _) => { g.KnowledgeEngineer = keBox.Text?.Trim(); onDirty?.Invoke(); };

        AddRow("Datum", out var dateBox);
        dateBox.Text = g.Date ?? "";
        dateBox.LostFocus += (_, _) => { g.Date = dateBox.Text?.Trim(); onDirty?.Invoke(); };

        AddRow("Inference mechanismus", out var infBox);
        infBox.Text = g.InferenceMechanism ?? "standard";
        infBox.LostFocus += (_, _) => { g.InferenceMechanism = infBox.Text?.Trim() ?? "standard"; onDirty?.Invoke(); };

        AddRow("Rozsah vah (weight_range)", out var wrBox);
        wrBox.Text = g.WeightRange.ToString(System.Globalization.CultureInfo.InvariantCulture);
        wrBox.LostFocus += (_, _) =>
        {
            if (double.TryParse(wrBox.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v))
                g.WeightRange = v;
            onDirty?.Invoke();
        };

        var defaultWeightCombo = new ComboBox
        {
            ItemsSource = new[] { "unknown", "irrelevant" },
            SelectedIndex = g.DefaultWeight == DefaultWeightKind.Unknown ? 0 : 1,
            MinWidth = 140,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
        };
        defaultWeightCombo.SelectionChanged += (_, _) =>
        {
            if (defaultWeightCombo.SelectedIndex == 0) g.DefaultWeight = DefaultWeightKind.Unknown;
            else g.DefaultWeight = DefaultWeightKind.Irrelevant;
            onDirty?.Invoke();
        };
        Root.Children.Add(CreateLabelRow("Výchozí váha (default_weight)", defaultWeightCombo));

        var priorityCombo = new ComboBox
        {
            ItemsSource = new[] { "first", "last", "minlength", "maxlength", "user" },
            SelectedIndex = g.GlobalPriority switch { GlobalPriorityKind.Last => 1, GlobalPriorityKind.MinLength => 2, GlobalPriorityKind.MaxLength => 3, GlobalPriorityKind.User => 4, _ => 0 },
            MinWidth = 140,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
        };
        priorityCombo.SelectionChanged += (_, _) =>
        {
            g.GlobalPriority = priorityCombo.SelectedIndex switch
            {
                1 => GlobalPriorityKind.Last,
                2 => GlobalPriorityKind.MinLength,
                3 => GlobalPriorityKind.MaxLength,
                4 => GlobalPriorityKind.User,
                _ => GlobalPriorityKind.First
            };
            onDirty?.Invoke();
        };
        Root.Children.Add(CreateLabelRow("Globální priorita (global_priority)", priorityCombo));

        AddRow("Práh kontextu (context_global_threshold)", out var cgtBox);
        cgtBox.Text = g.ContextGlobalThreshold.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
        cgtBox.LostFocus += (_, _) =>
        {
            if (double.TryParse(cgtBox.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v))
                g.ContextGlobalThreshold = v;
            onDirty?.Invoke();
        };

        AddRow("Práh podmínky (condition_global_threshold)", out var condBox);
        condBox.Text = g.ConditionGlobalThreshold.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
        condBox.LostFocus += (_, _) =>
        {
            if (double.TryParse(condBox.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v))
                g.ConditionGlobalThreshold = v;
            onDirty?.Invoke();
        };
    }

    private StackPanel AddRow(string label, out TextBox box)
    {
        box = new TextBox { MinWidth = 220 };
        var row = CreateLabelRow(label, box);
        Root.Children.Add(row);
        return row;
    }

    private static StackPanel CreateLabelRow(string label, Control input)
    {
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            Children =
            {
                new TextBlock { Text = label, FontWeight = Avalonia.Media.FontWeight.SemiBold },
                input
            }
        };
    }
}
