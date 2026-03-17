using Avalonia.Controls;
using Avalonia.Interactivity;
using NestCore.Model;

namespace NestStudio.Views;

public partial class NewRuleTypeDialog : Window
{
    public RuleKind ResultKind { get; private set; } = RuleKind.Compositional;

    public NewRuleTypeDialog()
    {
        InitializeComponent();
        OkButton.Click += OnOk;
        CancelButton.Click += OnCancel;
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        ResultKind = TypeCombo.SelectedIndex switch
        {
            1 => RuleKind.Apriori,
            2 => RuleKind.Logical,
            _ => RuleKind.Compositional
        };
        Close((RuleKind?)ResultKind);
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close((RuleKind?)null);
    }
}
