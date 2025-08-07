using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Linq;
using System.Net;

namespace GenieWasm.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        ExtractHostAndPort(args, out int port, out string host, out string sourcefile);
        LocalSetup.InitSettings(port,host,sourcefile);
        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();

    // Host and port defaults when using Genie server emulator
    static int DefaultPort = 7910;
    static string DefaultHost = "eaccess.play.net"; // this is the authentication server for Simutronics games
    static string DefaultSourceFile = "";

    private static void ExtractHostAndPort(string[] args, out int port, out string host, out string sourcefile)
    {
        port = (int)GetArg("Port", DefaultPort, args);
        host = (string)GetArg("Host", DefaultHost, args);
        sourcefile = (string)GetArg("Source", DefaultSourceFile, args);
    }

    private static object GetArg(string arg, object defaultValue, string[] args)
    {

        if (args == null || args.Length == 0)
            return defaultValue;

        string argument = args.ToList().Find(x => x.StartsWith(arg + "=", StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(argument))
        {
            string[] values = argument.Split('=');
            if (values.Length == 2)
            {
                if (defaultValue is int && int.TryParse(values[1], out int result))
                    return result;
                if (defaultValue is string && !string.IsNullOrEmpty(values[1]))
                    return values[1];
            }
        }
        return defaultValue;
    }

}
