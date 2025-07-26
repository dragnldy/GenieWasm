using Avalonia;
using Avalonia.Browser;
using Avalonia.ReactiveUI;
using GenieWasm;
using GenieWasm.Browser;
using System.Reflection;
using System.Threading.Tasks;

internal sealed partial class Program
{
    private static Task Main(string[] args)
    {
        LocalDirectory.InitLocalDirectory();
        GenieCoreLib.Globals.Platform = GenieCoreLib.OperatingPlatform.WebAssembly;
        GenieCoreLib.Globals.AppName = Assembly.GetExecutingAssembly()?.GetName().Name;
        GenieCoreLib.Globals.AppVersion = Assembly.GetExecutingAssembly()?.GetName().Version.ToString();
        return BuildAvaloniaApp()
            .WithInterFont()
            .UseReactiveUI()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
