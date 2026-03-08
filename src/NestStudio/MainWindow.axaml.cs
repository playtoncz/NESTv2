using Avalonia.Controls;
using NestCore.Model;
using NestStudio.Views;

namespace NestStudio;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainContent.Content = new Views.WelcomeView(this);
        WindowState = WindowState.Maximized;
    }

    public void ShowConsultation(KnowledgeBase kb, string? basePath, ConsultationRunConfig? runConfig = null)
    {
        MainContent.Content = new Views.ConsultationView(kb, basePath, this, runConfig);
    }

    public void ShowKbEditor(KnowledgeBase kb, string? basePath)
    {
        MainContent.Content = new Views.KbEditorView(kb, basePath, this);
    }

    public async void ShowWelcome()
    {
        if (MainContent.Content is Views.KbEditorView editor && !await ConfirmSaveBeforeLeaveAsync(editor))
            return;
        MainContent.Content = new Views.WelcomeView(this);
    }

    /// <summary>Zobrazí dialog Uložit změny a podle volby uloží nebo zruší. Vrací true pokud lze opustit editor.</summary>
    public async Task<bool> ConfirmSaveBeforeLeaveAsync(Views.KbEditorView editor)
    {
        if (!editor.IsDirty) return true;
        var dialog = new SaveChangesDialog();
        var result = await dialog.ShowDialog<SaveChangesResult?>(this);
        if (result == SaveChangesResult.Cancel) return false;
        if (result == SaveChangesResult.Discard) return true;
        return await editor.TrySaveAsync();
    }
}
