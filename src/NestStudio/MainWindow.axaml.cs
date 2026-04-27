using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using NestCore.Model;
using NestStudio.Views;

namespace NestStudio;

public partial class MainWindow : Window
{
    private KnowledgeBase? _lastKb;
    private string? _lastKbPath;
    private readonly List<string> _recentKbPaths = new();
    private UpdateCheckResult? _updateInfo;

    public MainWindow()
    {
        InitializeComponent();
        MainContent.Content = new Views.WelcomeView(this, _lastKb, _lastKbPath, _recentKbPaths);
        WindowState = WindowState.Maximized;
        _ = CheckUpdatesOnStartupAsync();
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
        if (result == SaveChangesResult.Discard)
        {
            editor.MarkClean();
            return true;
        }
        return await editor.TrySaveAsync();
    }

    private static string GetCurrentVersionString()
    {
        var asm = typeof(MainWindow).Assembly;
        var attr = asm.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false);
        string? version = null;
        if (attr.Length > 0 && attr[0] is System.Reflection.AssemblyInformationalVersionAttribute info)
            version = info.InformationalVersion;
        version ??= asm.GetName().Version?.ToString() ?? "1.0.0.0";
        var plusIdx = version.IndexOf('+');
        if (plusIdx >= 0)
            version = version[..plusIdx];
        return version;
    }

    private async Task CheckUpdatesOnStartupAsync()
    {
        try
        {
            var currentVersion = GetCurrentVersionString();
            var result = await GithubUpdateChecker.CheckForUpdateAsync(currentVersion, CancellationToken.None);
            if (result == null || !result.IsNewer)
            {
                UpdatePanel.IsVisible = false;
                return;
            }

            _updateInfo = result;
            UpdateText.Text = $"K dispozici je nová verze {result.LatestVersion}.";
            UpdatePanel.IsVisible = true;
        }
        catch
        {
            UpdatePanel.IsVisible = false;
        }
    }

    private async void OnUpdateDownloadClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var info = _updateInfo;
        if (info == null)
            return;

        if (string.IsNullOrWhiteSpace(info.AssetDownloadUrl))
        {
            if (!string.IsNullOrWhiteSpace(info.ReleasePageUrl))
                OpenUrl(info.ReleasePageUrl);
            return;
        }

        try
        {
            var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            UpdateText.Text = $"Stahuji aktualizaci {info.LatestVersion}…";
            var progress = new Progress<double>(p =>
            {
                UpdateText.Text = $"Stahuji aktualizaci {info.LatestVersion}… ({Math.Round(p * 100)} %)";
            });

            var savedPath = await GithubUpdateChecker.DownloadAssetAsync(
                info.AssetDownloadUrl,
                info.AssetFileName,
                downloads,
                progress,
                CancellationToken.None);

            if (savedPath != null)
                UpdateText.Text = $"Aktualizace byla stažena jako: {savedPath}. Spusťte ji dvojklikem.";
            else
                UpdateText.Text = "Aktualizaci se nepodařilo stáhnout.";
        }
        catch (Exception ex)
        {
            UpdateText.Text = "Chyba při stahování aktualizace: " + ex.Message;
        }
    }

    private async void OnUpdateChangelogClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var info = _updateInfo;
        if (info == null)
            return;

        if (!string.IsNullOrWhiteSpace(info.ReleaseNotes))
        {
            var window = new Window
            {
                Title = $"Changelog verze {info.LatestVersion}",
                Width = 640,
                Height = 480,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var contentPanel = MarkdownToPanel(info.ReleaseNotes);

            var closeButton = new Button
            {
                Content = "Zavřít",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                Margin = new Thickness(0, 16, 0, 0),
                Classes = { "primary" }
            };
            closeButton.Click += (_, _) => window.Close();

            var scroll = new ScrollViewer { Content = contentPanel, Margin = new Thickness(0, 0, 4, 0) };
            var grid = new Grid
            {
                RowDefinitions = new RowDefinitions("*,Auto"),
                Children = { scroll, closeButton }
            };
            Grid.SetRow(scroll, 0);
            Grid.SetRow(closeButton, 1);

            window.Content = new Border
            {
                Margin = new Thickness(16),
                Padding = new Thickness(16),
                Classes = { "glass-card" },
                Child = grid
            };
            await window.ShowDialog(this);
        }
        else if (!string.IsNullOrWhiteSpace(info.ReleasePageUrl))
        {
            OpenUrl(info.ReleasePageUrl);
        }
    }

    private void OpenUrl(string url)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
        }
    }

    /// <summary>Převede jednoduchý Markdown (nadpisy #, ##, ### a **tučný**) na StackPanel s TextBlocky.</summary>
    private static StackPanel MarkdownToPanel(string markdown)
    {
        var panel = new StackPanel { Spacing = 6 };
        if (string.IsNullOrEmpty(markdown))
            return panel;

        foreach (var rawLine in markdown.Split('\n'))
        {
            var line = rawLine.TrimEnd();
            if (line.Length == 0)
            {
                panel.Children.Add(new TextBlock { Height = 4 });
                continue;
            }

            double fontSize = 14;
            FontWeight fontWeight = FontWeight.Normal;
            string text = line;

            if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                fontSize = 14;
                fontWeight = FontWeight.SemiBold;
                text = line[4..];
            }
            else if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                fontSize = 16;
                fontWeight = FontWeight.SemiBold;
                text = line[3..];
            }
            else if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                fontSize = 18;
                fontWeight = FontWeight.SemiBold;
                text = line[2..];
            }

            var block = new TextBlock
            {
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontSize = fontSize,
                FontWeight = fontWeight
            };

            if (fontWeight != FontWeight.Normal || !text.Contains("**", StringComparison.Ordinal))
            {
                block.Text = text;
            }
            else
            {
                ParseBoldInlines(block, text);
            }

            panel.Children.Add(block);
        }

        return panel;
    }

    private static void ParseBoldInlines(TextBlock block, string text)
    {
        block.Inlines ??= new InlineCollection();
        block.Inlines.Clear();
        int i = 0;
        while (i < text.Length)
        {
            int start = text.IndexOf("**", i, StringComparison.Ordinal);
            if (start < 0)
            {
                block.Inlines.Add(new Run { Text = text[i..] });
                break;
            }
            if (start > i)
                block.Inlines.Add(new Run { Text = text[i..start] });
            int end = text.IndexOf("**", start + 2, StringComparison.Ordinal);
            if (end < 0)
            {
                block.Inlines.Add(new Run { Text = text[start..], FontWeight = FontWeight.Bold });
                break;
            }
            block.Inlines.Add(new Run { Text = text[(start + 2)..end], FontWeight = FontWeight.Bold });
            i = end + 2;
        }
    }
}
