using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NestStudio.Views;

public partial class AboutDialog : Window
{
    public AboutDialog()
    {
        InitializeComponent();
        OkButton.Click += (_, _) => Close();
    }
}
