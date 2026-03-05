using Avalonia.Controls;
using Avalonia.Interactivity;
using NestCore.Model;

namespace NestStudio.Views;

public partial class NewAttributeTypeDialog : Window
{
    public AttributeType ResultType { get; private set; } = AttributeType.Binary;

    public NewAttributeTypeDialog()
    {
        InitializeComponent();
        OkButton.Click += OnOk;
        CancelButton.Click += OnCancel;
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        ResultType = TypeCombo.SelectedIndex switch
        {
            1 => AttributeType.Single,
            2 => AttributeType.Multiple,
            3 => AttributeType.Numeric,
            _ => AttributeType.Binary
        };
        Close((AttributeType?)ResultType);
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close((AttributeType?)null);
    }
}
