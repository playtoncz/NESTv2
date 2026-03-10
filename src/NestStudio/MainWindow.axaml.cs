using Avalonia.Controls;
using NestCore.Model;
using NestStudio.Views;
using System.Collections.Generic;

namespace NestStudio;

public partial class MainWindow : Window
{
    private KnowledgeBase? _lastKb;
    private string? _lastKbPath;
    private readonly List<string> _recentKbPaths = new();

    public MainWindow()
    {
        InitializeComponent();
        MainContent.Content = new Views.WelcomeView(this, _lastKb, _lastKbPath, _recentKbPaths);
        WindowState = WindowState.Maximized;
    }

    public void ShowConsultation(KnowledgeBase kb, string? basePath, ConsultationRunConfig? runConfig = null)
    {
        SetCurrentProject(kb, basePath);
        MainContent.Content = new Views.ConsultationView(kb, basePath, this, runConfig);
    }

    public void ShowKbEditor(KnowledgeBase kb, string? basePath)
    {
        SetCurrentProject(kb, basePath);
        MainContent.Content = new Views.KbEditorView(kb, basePath, this);
    }

    public async void ShowWelcome()
    {
        if (MainContent.Content is Views.KbEditorView editor && !await ConfirmSaveBeforeLeaveAsync(editor))
            return;
        MainContent.Content = new Views.WelcomeView(this, _lastKb, _lastKbPath, _recentKbPaths);
    }

    public void SetCurrentProject(KnowledgeBase kb, string? basePath)
    {
        _lastKb = kb;
        _lastKbPath = basePath;
        if (!string.IsNullOrWhiteSpace(basePath))
        {
            _recentKbPaths.Remove(basePath);
            _recentKbPaths.Insert(0, basePath);
            if (_recentKbPaths.Count > 5)
                _recentKbPaths.RemoveRange(5, _recentKbPaths.Count - 5);
        }
    }

    public IReadOnlyList<string> GetRecentProjects() => _recentKbPaths.AsReadOnly();

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
