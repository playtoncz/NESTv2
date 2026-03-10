using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NestStudio;

internal sealed record UpdateCheckResult(
    bool IsNewer,
    string CurrentVersion,
    string LatestVersion,
    string? ReleasePageUrl,
    string? AssetDownloadUrl,
    string? AssetFileName,
    string? ReleaseNotes);

/// <summary>
/// Kontroluje nejnovější verzi NEST Studio na GitHub Releases a umí stáhnout .exe asset.
/// </summary>
internal static class GithubUpdateChecker
{
    private const string RepoOwner = "playtoncz";
    private const string RepoName = "NESTv2";

    private static readonly HttpClient Http = CreateHttpClient();

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("NestStudio/1.0");
        return client;
    }

    private static Version ParseVersionOrZero(string? v)
    {
        if (string.IsNullOrWhiteSpace(v))
            return new Version(0, 0, 0, 0);
        if (v.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            v = v[1..];
        return Version.TryParse(v, out var ver) ? ver : new Version(0, 0, 0, 0);
    }

    public static async Task<UpdateCheckResult?> CheckForUpdateAsync(string currentVersion, CancellationToken cancellationToken)
    {
        var apiUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";

        using var response = await Http.GetAsync(apiUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var root = doc.RootElement;
        var tagName = root.TryGetProperty("tag_name", out var tagEl) ? tagEl.GetString() : null;
        var htmlUrl = root.TryGetProperty("html_url", out var htmlEl) ? htmlEl.GetString() : null;
        var body = root.TryGetProperty("body", out var bodyEl) ? bodyEl.GetString() : null;

        string? assetUrl = null;
        string? assetName = null;
        if (root.TryGetProperty("assets", out var assetsEl) && assetsEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var asset in assetsEl.EnumerateArray())
            {
                var name = asset.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;
                var url = asset.TryGetProperty("browser_download_url", out var urlEl) ? urlEl.GetString() : null;
                if (!string.IsNullOrEmpty(url) && url.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    assetUrl = url;
                    assetName = string.IsNullOrWhiteSpace(name) ? Path.GetFileName(url) : name;
                    break;
                }
            }
        }

        var curVer = ParseVersionOrZero(currentVersion);
        var latestVer = ParseVersionOrZero(tagName);
        var isNewer = latestVer > curVer;

        return new UpdateCheckResult(
            isNewer,
            curVer.ToString(),
            latestVer.ToString(),
            htmlUrl,
            assetUrl,
            assetName,
            body);
    }

    public static async Task<string?> DownloadAssetAsync(
        string downloadUrl,
        string? fileName,
        string targetDirectory,
        IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(downloadUrl))
            return null;

        Directory.CreateDirectory(targetDirectory);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            try
            {
                var uri = new Uri(downloadUrl);
                fileName = Path.GetFileName(uri.LocalPath);
            }
            catch
            {
                fileName = "NestStudio-latest.exe";
            }
        }

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = "NestStudio-latest.exe";

        var destinationPath = Path.Combine(targetDirectory, fileName);

        using var response = await Http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength;
        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var output = File.Create(destinationPath);

        var buffer = new byte[81920];
        long readTotal = 0;
        int read;
        while ((read = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            readTotal += read;

            if (total.HasValue && progress != null && total.Value > 0)
            {
                progress.Report((double)readTotal / total.Value);
            }
        }

        return destinationPath;
    }
}

