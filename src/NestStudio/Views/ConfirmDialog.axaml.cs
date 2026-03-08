using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NestStudio.Views;

public partial class ConfirmDialog : Window
{
    public bool Result { get; private set; }

    public ConfirmDialog()
    {
        InitializeComponent();
    }

    public ConfirmDialog(string message)
    {
        InitializeComponent();
        MessageText.Text = message;
        YesButton.Click += (_, _) => { Result = true; Close(true); };
        NoButton.Click += (_, _) => { Result = false; Close(false); };
    }
}
