using System.Collections.Concurrent;

namespace GenieCoreLib;

public static class AppGlobals
{
    // Globals that need to be referenced before injection container is available
    // These values are platform specific so they are initiliazed by the main module
    public static OperatingPlatform Platform { get; set; } = OperatingPlatform.Windows;
    public static string AppName { get; set; } = string.Empty;
    public static string CompanyName { get; set; } = string.Empty;
    public static string LocalDirectoryPath { get; set; } = string.Empty;
    public static string LocalUserPath { get; set; } = string.Empty;
    public static string AppVersion { get; set; } = string.Empty;
    public static ConcurrentQueue<TextMessage> LogQueue = new();

}

public enum OperatingPlatform
{
    Windows,
    Linux,
    MacOS,
    Android,
    iOS,
    WebAssembly
}
