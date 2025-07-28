using GenieCoreLib;
using System;
using System.IO;
using System.Reflection;

namespace GenieWasm.Desktop;

public static class LocalSetup
{
    public static string Path = AppDomain.CurrentDomain.BaseDirectory;
    public static bool IsLocal = true;


    public static void InitSettings()
    {
        InitLocalDirectory();
        PlatformSpecificEventHandlers.Register();
        AppGlobals.Platform = OperatingPlatform.Windows;
        AppGlobals.AppName = Assembly.GetExecutingAssembly()?.GetName().Name;
        AppGlobals.AppVersion = Assembly.GetExecutingAssembly()?.GetName().Version.ToString();
    }

    public static void InitLocalDirectory()
    {

        CheckUserDirectory();
        if (IsLocal)
        {
            AppGlobals.LocalDirectoryPath = Path;
            // Local directory is set, no need to change
            return;
        }
        else
        {
            // User data directory is set, update the path
            Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Path = System.IO.Path.Combine(Path, AppDomain.CurrentDomain.BaseDirectory);
            AppGlobals.LocalDirectoryPath = Path;
        }
    }
    public static void CheckUserDirectory()
    {
        string dir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
        if (!System.IO.Directory.Exists(dir))
        {
            // No local settings, change to user data directory
            SetUserDataDirectory();
        }
    }

    public static void SetUserDataDirectory()
    {
        string dataDirectory = $"Genie Client 4";
        string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        dir = System.IO.Path.Combine(dir, dataDirectory );
        if (!System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
        }

        Path = dir;
        IsLocal = true;
    }

    public static string ValidateDirectory(string path)
    {
        try
        {
            if (!System.IO.Path.IsPathRooted(path)) path = Path + "\\" + path;
            DirectoryInfo directory = new DirectoryInfo(path);
            if (!directory.Exists)
            {
                directory.Create();
                return $"Directory Created: {directory.FullName}";
            }
            return $"Diretory Found: {directory.FullName}";
        }
        catch (Exception ex)
        {
            throw new Exception($"Error Setting Directory: {ex.Message}");
        }
    }
}