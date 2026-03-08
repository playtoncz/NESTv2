using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NestStudio.Views;

public enum SaveChangesResult
{
    Cancel,
    Discard,
    Save
}

public partial class SaveChangesDialog : Window
{
    public SaveChangesResult Result { get; private set; } = SaveChangesResult.Cancel;

    public SaveChangesDialog()
    {
        InitializeComponent();
        SaveButton.Click += (_, _) => { Result = SaveChangesResult.Save; Close(Result); };
        DiscardButton.Click += (_, _) => { Result = SaveChangesResult.Discard; Close(Result); };
        CancelButton.Click += (_, _) => { Result = SaveChangesResult.Cancel; Close(Result); };
    }
}
