using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Text;
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
            Title = "Vyberte projekt (znalostní báze XML)",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("XML") { Patterns = new[] { "*.xml" } }
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
            var xml = ReadXmlWithEncoding(path);
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

    private static string ReadXmlWithEncoding(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length == 0)
            throw new InvalidOperationException("Soubor je prázdný.");

        Encoding? encodingByBom = null;
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            encodingByBom = Encoding.Unicode;
        else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            encodingByBom = Encoding.BigEndianUnicode;
        else if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            encodingByBom = Encoding.UTF8;

        if (encodingByBom != null)
            return encodingByBom.GetString(bytes);

        var utf8 = Encoding.UTF8.GetString(bytes);
        if (utf8.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
        {
            var encMatch = System.Text.RegularExpressions.Regex.Match(utf8, @"encoding\s*=\s*[""']([^""']+)[""']", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (encMatch.Success)
            {
                try
                {
                    var enc = Encoding.GetEncoding(encMatch.Groups[1].Value.Trim());
                    if (enc != Encoding.UTF8)
                        return enc.GetString(bytes);
                }
                catch { }
            }
            return utf8;
        }

        var utf16 = Encoding.Unicode.GetString(bytes);
        if (utf16.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
            return utf16;

        return utf8;
    }
}
