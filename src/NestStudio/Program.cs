using System;
using System.Text;
using Avalonia;

namespace NestStudio;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        // Povolit legacy kódové stránky (Windows-1250 atd.) pro práci se starými XML.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        if (args.Length > 0)
            return RunConsole(args);

        return BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static int RunConsole(string[] args)
    {
        return ConsoleRunner.Run(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
