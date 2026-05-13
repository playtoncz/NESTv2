using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using NestCore.Model;
using NestFormat;

namespace NestStudio.Views;

public partial class WelcomeView : UserControl
{
    private readonly MainWindow? _mainWindow;
    private KnowledgeBase? _loadedKb;
    private string? _loadedKbPath;

    public WelcomeView()
    {
        InitializeComponent();
    }

    public WelcomeView(MainWindow mainWindow)
        : this(mainWindow, null, null, null)
    {
    }

    public WelcomeView(MainWindow mainWindow, KnowledgeBase? lastKb, string? lastKbPath, IReadOnlyList<string>? recentProjects)
    {
        _mainWindow = mainWindow;
        _loadedKb = lastKb;
        _loadedKbPath = lastKbPath;
        InitializeComponent();
        SetVersionText();
        CreateProjectButton.Click += OnCreateProject;
        OpenKbButton.Click += OnOpenKb;
        OpenEditorButton.Click += OnOpenEditor;
        FillProjectButton.Click += OnFillProject;
        AboutButton.Click += OnAbout;
        CheckUpdatesButton.Click += OnCheckUpdates;
        UpdateProjectLoadedState();
        PopulateRecentProjects(recentProjects);
    }

    private void SetVersionText()
    {
        var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            ?? "1.0.0.0";
        // schovat build metadata za '+'
        var plusIdx = version.IndexOf('+');
        if (plusIdx >= 0)
            version = version[..plusIdx];
        VersionText.Text = "Verze " + version;
    }

    private void UpdateProjectLoadedState()
    {
        var hasProject = _loadedKb != null;
        ProjectLoadedStack.IsVisible = hasProject;
        if (hasProject)
        {
            var name = _loadedKb!.Global.Description ?? (string.IsNullOrEmpty(_loadedKbPath) ? "Nový projekt" : Path.GetFileName(_loadedKbPath));
            CurrentProjectNameText.Text = name;
            if (!string.IsNullOrEmpty(_loadedKbPath))
            {
                CurrentProjectPathText.Text = _loadedKbPath;
                CurrentProjectPathText.IsVisible = true;
            }
            else
            {
                CurrentProjectPathText.Text = "";
                CurrentProjectPathText.IsVisible = false;
            }
        }
    }

    private void PopulateRecentProjects(IReadOnlyList<string>? recentProjects)
    {
        RecentProjectsStack.Children.Clear();
        NoRecentProjectsText.IsVisible = false;

        if (recentProjects == null || recentProjects.Count == 0)
        {
            NoRecentProjectsText.IsVisible = true;
            return;
        }

        foreach (var path in recentProjects)
        {
            if (string.IsNullOrWhiteSpace(path)) continue;
            var name = System.IO.Path.GetFileNameWithoutExtension(path);

            var panel = new StackPanel { Spacing = 2 };
            panel.Children.Add(new TextBlock
            {
                Text = name,
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis
            });
            panel.Children.Add(new TextBlock
            {
                Text = path,
                FontSize = 11,
                Opacity = 0.8,
                TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis
            });

            var btn = new Button
            {
                Content = panel,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Tag = path
            };
            btn.Click += OnRecentProjectClick;
            RecentProjectsStack.Children.Add(btn);
        }
    }

    private void OnCreateProject(object? sender, RoutedEventArgs e)
    {
        if (_mainWindow == null) return;
        var kb = new KnowledgeBase();
        kb.Global.Description = "Nový projekt";
        _mainWindow.ShowKbEditor(kb, null);
    }

    private void OnOpenEditor(object? sender, RoutedEventArgs e)
    {
        if (_loadedKb == null || _mainWindow == null) return;
        _mainWindow.ShowKbEditor(_loadedKb, _loadedKbPath);
    }

    private async void OnOpenKb(object? sender, RoutedEventArgs e)
    {
        var storage = _mainWindow?.StorageProvider;
        if (storage == null) return;
        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Vyberte projekt (znalostní báze)",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("XML / NKB (znalostní báze)") { Patterns = new[] { "*.xml", "*.nkb" } }
            }
        });
        if (files.Count == 0) return;
        var path = files[0].Path.LocalPath;
        await LoadProjectFromPathAsync(path);
    }

    private async void OnFillProject(object? sender, RoutedEventArgs e)
    {
        if (_loadedKb == null || _mainWindow == null) return;
        var dialog = new RunConfigDialog();
        var config = await dialog.ShowDialog<ConsultationRunConfig?>(_mainWindow);
        if (config != null)
            _mainWindow.ShowConsultation(_loadedKb, _loadedKbPath, config);
    }

    private async void OnAbout(object? sender, RoutedEventArgs e)
    {
        if (_mainWindow == null) return;
        var dialog = new AboutDialog();
        await dialog.ShowDialog(_mainWindow);
    }

    private async Task LoadProjectFromPathAsync(string path)
    {
        try
        {
            var xml = XmlFileEncoding.ReadAllText(path);
            var reader = new BaseXmlReader();
            _loadedKb = reader.Read(xml);
            _loadedKbPath = path;
            _mainWindow?.SetCurrentProject(_loadedKb, _loadedKbPath);
            StatusText.IsVisible = true;
            StatusText.Text = $"Načteno: {_loadedKb.Global.Description ?? path} ({_loadedKb.Attributes.Count} atributů, {_loadedKb.CompositionalRules.Count} pravidel)";
            UpdateProjectLoadedState();
            PopulateRecentProjects(_mainWindow?.GetRecentProjects());
        }
        catch (Exception ex)
        {
            StatusText.IsVisible = true;
            StatusText.Text = "Chyba: " + ex.Message;
        }
    }

    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            StatusText.IsVisible = true;
            StatusText.Text = "Nelze otevřít odkaz: " + ex.Message;
        }
    }

    private void OnOpfLogoClick(object? sender, RoutedEventArgs e)
        => OpenUrl("https://opf.slu.cz");

    private void OnRaplLogoClick(object? sender, RoutedEventArgs e)
        => OpenUrl("https://rapl-group.eu");

    private async void OnRecentProjectClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button b && b.Tag is string path && !string.IsNullOrWhiteSpace(path))
            await LoadProjectFromPathAsync(path);
    }


    private string GetCurrentVersionString()
    {
        var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            ?? "1.0.0.0";
        var plusIdx = version.IndexOf('+');
        if (plusIdx >= 0)
            version = version[..plusIdx];
        return version;
    }

    private async void OnCheckUpdates(object? sender, RoutedEventArgs e)
    {
        StatusText.IsVisible = true;
        StatusText.Text = "Kontroluji aktualizace…";

        try
        {
            var currentVersion = GetCurrentVersionString();
            var result = await GithubUpdateChecker.CheckForUpdateAsync(currentVersion, CancellationToken.None);
            if (result == null)
            {
                StatusText.Text = "Nepodařilo se zjistit nejnovější verzi na GitHubu.";
                return;
            }

            if (!result.IsNewer)
            {
                StatusText.Text = $"Máte aktuální verzi {result.CurrentVersion}.";
                return;
            }

            StatusText.Text = $"K dispozici je nová verze {result.LatestVersion}. Stahuji do složky Stažené soubory…";

            var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string progressTextPrefix = StatusText.Text;
            var progress = new Progress<double>(p =>
            {
                StatusText.Text = $"{progressTextPrefix} ({Math.Round(p * 100)} %)";
            });

            if (string.IsNullOrWhiteSpace(result.AssetDownloadUrl))
            {
                if (!string.IsNullOrWhiteSpace(result.ReleasePageUrl))
                {
                    OpenUrl(result.ReleasePageUrl);
                    StatusText.Text = "Nová verze je dostupná na GitHubu (otevřený prohlížeč).";
                }
                else
                {
                    StatusText.Text = "Nová verze je k dispozici, ale nepodařilo se najít odkaz ke stažení.";
                }
                return;
            }

            var savedPath = await GithubUpdateChecker.DownloadAssetAsync(
                result.AssetDownloadUrl,
                result.AssetFileName,
                downloads,
                progress,
                CancellationToken.None);

            if (savedPath == null)
            {
                StatusText.Text = "Aktualizaci se nepodařilo stáhnout.";
                return;
            }

            StatusText.Text = $"Nová verze byla stažena jako: {savedPath}.\nSpusťte ji dvojklikem (případně si ji přesuňte jinam).";
        }
        catch (Exception ex)
        {
            StatusText.Text = "Chyba při kontrole aktualizací: " + ex.Message;
        }
    }
}
