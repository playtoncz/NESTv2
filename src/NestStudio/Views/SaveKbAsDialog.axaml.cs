using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace NestStudio.Views;

public partial class SaveKbAsDialog : Window
{
    public SaveKbAsDialog()
    {
        InitializeComponent();
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        PathTextBox.Text = Path.Combine(docs, "znalostni_baze.xml");

        OkButton.Click += OnOk;
        CancelButton.Click += (_, _) => Close(null);
        BrowseButton.Click += OnBrowseAsync;
        FormatCombo.SelectionChanged += (_, _) => SyncExtensionFromCombo();
    }

    private string CurrentExtension => FormatCombo.SelectedIndex == 1 ? ".nkb" : ".xml";

    private void SyncExtensionFromCombo()
    {
        var path = PathTextBox.Text?.Trim();
        if (string.IsNullOrEmpty(path))
            return;
        var ext = CurrentExtension;
        var dir = Path.GetDirectoryName(path);
        var stem = Path.GetFileNameWithoutExtension(path);
        if (string.IsNullOrEmpty(stem))
            stem = "znalostni_baze";
        PathTextBox.Text = string.IsNullOrEmpty(dir)
            ? stem + ext
            : Path.Combine(dir, stem + ext);
    }

    private async void OnBrowseAsync(object? sender, RoutedEventArgs e)
    {
        var storage = StorageProvider;
        if (storage == null)
            return;
        var extNoDot = CurrentExtension.TrimStart('.');
        var suggested = Path.GetFileName(PathTextBox.Text ?? "znalostni_baze.xml");
        var pick = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Uložit znalostní bázi",
            DefaultExtension = extNoDot,
            SuggestedFileName = suggested,
            FileTypeChoices = new[]
            {
                new FilePickerFileType("XML") { Patterns = new[] { "*.xml" } },
                new FilePickerFileType("NKB") { Patterns = new[] { "*.nkb" } }
            }
        });
        if (pick != null)
            PathTextBox.Text = pick.Path.LocalPath;
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        var raw = PathTextBox.Text?.Trim();
        if (string.IsNullOrEmpty(raw))
            return;
        try
        {
            var path = NormalizeSavePath(raw, CurrentExtension);
            Close(path);
        }
        catch (Exception)
        {
            // neplatná nebo nedostupná cesta — dialog zůstane otevřený
        }
    }

    private static string NormalizeSavePath(string raw, string ext)
    {
        var withExt = EnsureExtension(raw, ext);
        if (!Path.IsPathFullyQualified(withExt))
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            withExt = Path.Combine(folder, Path.GetFileName(withExt));
        }

        return Path.GetFullPath(withExt);
    }

    private static string EnsureExtension(string path, string ext)
    {
        if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            return path;

        var trimmed = path.TrimEnd('.', ' ');
        if (Path.HasExtension(trimmed))
        {
            var dir = Path.GetDirectoryName(trimmed);
            var stem = Path.GetFileNameWithoutExtension(trimmed);
            var leaf = stem + ext;
            return string.IsNullOrEmpty(dir) ? leaf : Path.Combine(dir, leaf);
        }

        return trimmed + ext;
    }
}
