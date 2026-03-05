using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Text;
using NestCore.Model;
using NestFormat;

namespace NestStudio.Views;

public partial class WelcomeView : UserControl
{
    private readonly MainWindow? _mainWindow;
    private KnowledgeBase? _loadedKb;
    private string? _loadedKbPath;

    /// <summary>Pro runtime loader / XAML. V kódu používej konstruktor s MainWindow.</summary>
    public WelcomeView()
    {
        InitializeComponent();
    }

    public WelcomeView(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        InitializeComponent();
        OpenKbButton.Click += OnOpenKb;
        OpenEditorButton.Click += OnOpenEditor;
        RunConsultationButton.Click += OnRunConsultation;
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
        var files = await storage.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Vyberte znalostní bázi (base.xml)",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new Avalonia.Platform.Storage.FilePickerFileType("XML") { Patterns = new[] { "*.xml" } }
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
            RunConsultationButton.IsEnabled = true;
            OpenEditorButton.IsEnabled = true;
            StatusText.Text = $"Načteno: {_loadedKb.Global.Description ?? path} ({_loadedKb.Attributes.Count} atributů, {_loadedKb.CompositionalRules.Count} pravidel)";
        }
        catch (System.Exception ex)
        {
            StatusText.Text = "Chyba: " + ex.Message;
        }
    }

    private async void OnRunConsultation(object? sender, RoutedEventArgs e)
    {
        if (_loadedKb == null || _mainWindow == null) return;
        var dialog = new RunConfigDialog();
        var config = await dialog.ShowDialog<ConsultationRunConfig?>(_mainWindow);
        if (config != null)
            _mainWindow.ShowConsultation(_loadedKb, _loadedKbPath, config);
    }

    /// <summary>Načte XML s respektováním BOM a deklarace encoding.</summary>
    private static string ReadXmlWithEncoding(string path)
    {
        var bytes = System.IO.File.ReadAllBytes(path);
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
