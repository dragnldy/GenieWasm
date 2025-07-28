using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GenieCoreLib;
using GenieWasm.ViewModels;
using GenieWasm.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GenieWasm;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public App()
    {
        ConfigureServices();
    }
    private void ConfigureServices()
    {
        ServiceProvider = new ServiceCollection()
            .AddSingleton<IGlobals, Globals>()
            .AddSingleton<IConfig, Config>()
            .AddSingleton<IConfigSettings, ConfigSettings>()
            .AddSingleton<IGame, Game>()
            .AddTransient<MainViewModel>()
            .BuildServiceProvider();

        (ActivatorUtilities
            .CreateInstance<ConfigSettings>(ServiceProvider) as ConfigSettings)
            ?.LoadSettings(AppGlobals.LocalDirectoryPath + "/config/settings.cfg"); // Load configuration settings on startup

    }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Resolve your main window or main view model here
            desktop.MainWindow = new MainWindow { DataContext = ServiceProvider.GetService<MainViewModel>() };

        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel(ActivatorUtilities
            .CreateInstance<IConfigSettings>(ServiceProvider))};
        }

        // Start the concurrent message pump 
        base.OnFrameworkInitializationCompleted();
        ProcessQueuedMessages.ProcessMessagesAsync().ConfigureAwait(false);
    }

    //public enum MsgBoxStyle
    //{
    //    OK = 0,
    //    OkOnly = 0,
    //    OkCancel = 1,
    //    AbortRetryIgnore = 2,
    //    YesNoCancel = 3,
    //    YesNo = 4,
    //    RetryCancel = 5,
    //    //CancelTryContinue = 6,
    //    //Critical = 16,
    //    //Question = 32,
    //    //Exclamation = 48,
    //    //Information = 64,
    //    //DefaultButton1 = 0,
    //    //DefaultButton2 = 256,
    //    //DefaultButton3 = 512
    //}
    //public enum MsgBoxResult
    //{
    //    OK = ButtonResult.Ok,
    //    Cancel = ButtonResult.Cancel,
    //    Abort = ButtonResult.Abort,
    //    Ignore = ButtonResult.Yes,
    //    Yes = ButtonResult.Yes,
    //    No = ButtonResult.No,
    //}

    //public MsgBoxResult MsgBox(string message, MsgBoxStyle buttons, string title = "Message")
    //{

    //    ButtonEnum buttonEnum = buttons switch
    //    {
    //        MsgBoxStyle.OkOnly => ButtonEnum.Ok,
    //        MsgBoxStyle.OkCancel => ButtonEnum.OkCancel,
    //        MsgBoxStyle.AbortRetryIgnore => ButtonEnum.OkAbort,
    //        MsgBoxStyle.YesNoCancel => ButtonEnum.YesNoCancel,
    //        MsgBoxStyle.YesNo => ButtonEnum.YesNo,
    //        MsgBoxStyle.RetryCancel => ButtonEnum.OkAbort, // No direct equivalent, using AbortRetryIgnore
    //        _ => ButtonEnum.Ok // Default to Ok if no match
    //    };

    //    var msgBoxResult = MsgBoxResult.OK; // Default value
    //    var box = MessageBoxManager
    //        .GetMessageBoxStandard(title, message, buttonEnum);
    //    Task.Run(async () =>
    //    {
    //        var result = await box.ShowAsync();
    //        switch (result)
    //        {
    //            case ButtonResult.Ok:
    //                msgBoxResult = MsgBoxResult.OK;
    //                break;
    //            case ButtonResult.Cancel:
    //                msgBoxResult = MsgBoxResult.Cancel;
    //                break;
    //            case ButtonResult.Abort:
    //                msgBoxResult = MsgBoxResult.Abort;
    //                break;
    //            case ButtonResult.Yes:
    //                msgBoxResult = MsgBoxResult.Yes;
    //                break;
    //            case ButtonResult.No:
    //                msgBoxResult = MsgBoxResult.No;
    //                break;
    //            case ButtonResult.None:
    //                msgBoxResult = MsgBoxResult.Cancel;
    //                break;
    //            default:
    //                msgBoxResult = MsgBoxResult.Cancel;
    //                break;
    //        }
    //    });

    //    return msgBoxResult;
    //}

}
