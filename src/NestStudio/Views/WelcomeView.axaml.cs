using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
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
    {
        _mainWindow = mainWindow;
        InitializeComponent();
        LoadImages();
        SetVersionText();
        CreateProjectButton.Click += OnCreateProject;
        OpenKbButton.Click += OnOpenKb;
        OpenEditorButton.Click += OnOpenEditor;
        FillProjectButton.Click += OnFillProject;
        AboutButton.Click += OnAbout;
        UpdateProjectLoadedState();
    }

    private void LoadImages()
    {
        var baseDir = AppContext.BaseDirectory;
        TrySetImage(LogoImage, Path.Combine(baseDir, "img", "NEST_STUDIO_FULL.png"));
        TrySetImage(OpfImage, Path.Combine(baseDir, "img", "opf.png"));
        TrySetImage(RaplImage, Path.Combine(baseDir, "img", "rapl.png"));
    }

    private static void TrySetImage(Avalonia.Controls.Image imageControl, string path)
    {
        if (File.Exists(path))
        {
            try
            {
                imageControl.Source = new Bitmap(path);
            }
            catch { /* ignore */ }
        }
    }

    private void SetVersionText()
    {
        var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            ?? "1.0.0";
        VersionText.Text = "Verze " + version;
    }

    private void UpdateProjectLoadedState()
    {
        var hasProject = _loadedKb != null;
        ProjectLoadedStack.IsVisible = hasProject;
        if (hasProject)
        {
            var name = _loadedKb!.Global.Description ?? (string.IsNullOrEmpty(_loadedKbPath) ? "Nový projekt" : Path.GetFileName(_loadedKbPath));
            ProjectNameText.Text = "Projekt: " + name;
            if (!string.IsNullOrEmpty(_loadedKbPath))
                ProjectNameText.Text += "\n" + _loadedKbPath;
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
        try
        {
            var xml = ReadXmlWithEncoding(path);
            var reader = new BaseXmlReader();
            _loadedKb = reader.Read(xml);
            _loadedKbPath = path;
            StatusText.IsVisible = true;
            StatusText.Text = $"Načteno: {_loadedKb.Global.Description ?? path} ({_loadedKb.Attributes.Count} atributů, {_loadedKb.CompositionalRules.Count} pravidel)";
            UpdateProjectLoadedState();
        }
        catch (Exception ex)
        {
            StatusText.IsVisible = true;
            StatusText.Text = "Chyba: " + ex.Message;
        }
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
