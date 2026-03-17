using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace NestStudio.Views;

public partial class RunConfigDialog : Window
{
    public ConsultationRunConfig? Result { get; private set; }

    public RunConfigDialog()
    {
        InitializeComponent();
        AnsweringModeCombo.SelectionChanged += (_, _) => UpdatePathVisibility();
        OkButton.Click += OnOk;
        CancelButton.Click += OnCancel;
        BrowseAnswersButton.Click += OnBrowse;
        UpdatePathVisibility();
    }

    private void UpdatePathVisibility()
    {
        var useFile = AnsweringModeCombo.SelectedIndex == 1;
        PathSection.IsVisible = useFile;
    }

    private async void OnBrowse(object? sender, RoutedEventArgs e)
    {
        var storage = StorageProvider;
        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Vyberte soubor odpovědí (answers XML)",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("XML") { Patterns = new[] { "*.xml" } }
            }
        });
        if (files.Count > 0)
            AnswersPathBox.Text = files[0].Path.LocalPath;
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        Result = new ConsultationRunConfig
        {
            AnsweringMode = AnsweringModeCombo.SelectedIndex == 1 ? AnsweringModeKind.LoadFromFile : AnsweringModeKind.Questionnaire,
            AnswersFilePath = AnsweringModeCombo.SelectedIndex == 1 ? (AnswersPathBox.Text?.Trim() ?? "") : null,
            ReasoningMode = ReasoningModeCombo.SelectedIndex == 1 ? ReasoningModeKind.WithoutPostpone : ReasoningModeKind.Postpone,
            LayoutMode = LayoutModeCombo.SelectedIndex == 1 ? QuestionLayoutMode.OneByOne : QuestionLayoutMode.AllAtOnce,
            Uncertainty = UncertaintyCombo.SelectedIndex switch
            {
                1 => NestCore.Model.UncertaintyType.Logical,
                2 => NestCore.Model.UncertaintyType.Neural,
                3 => NestCore.Model.UncertaintyType.Hybrid,
                4 => NestCore.Model.UncertaintyType.Godel,
                5 => NestCore.Model.UncertaintyType.Product,
                _ => NestCore.Model.UncertaintyType.Standard
            }
        };
        Close(Result);
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}
