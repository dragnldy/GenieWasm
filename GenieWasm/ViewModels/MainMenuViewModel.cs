using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.Diagnostics;
using System.Runtime;
using System.Windows.Input;

namespace GenieWasm.ViewModels;

internal class MainMenuViewModel : ViewModelBase
{
    #region Flag Properties
    public bool AutoLogEnabled { get; set; } = true;
    public bool AutoReconnect { get; set; } = true;
    public bool ClassicConnect { get; set; } = true;
    public bool IgnoresEnabled { get; set; } = true;
    public bool TriggersEnabled { get; set; } = true;
    public bool PluginsEnabled { get; set; } = true;
    public bool ImagesEnabled { get; set; } = true;
    public bool AutoMapperEnabled { get; set; } = true;
    public bool MuteSoundsEnabled { get; set; } = false;
    public bool ShowRawDataEnabled { get; set; } = false;
    public bool IconBarEnabled { get; set; } = true;
    public bool ScriptBarEnabled { get; set; } = true;
    public bool HealthBarEnabled { get; set; } = true;
    public bool MagicPanelsEnabled { get; set; } = true;
    public bool StatusBarEnabled { get; set; } = true;
    public bool AlignInputEnabled { get; set; } = true;
    public bool AlwaysOnTopEnabled { get; set; } = false;
    public bool UpdateScriptsEnabled { get; set; } = true;
    public bool NoPluginsLoaded { get; set; } = true;
    public bool AutoUpdateEnabled { get; set; } = false;
    public bool AutoUpdateLampEnabled { get; set; } = false;
    public bool CheckUpdateEnabled { get; set; } = true;
    #endregion Flag Properties

    #region 'File' Commands
    public bool ConnectCommand()
    {
        Debug.WriteLine("Connect command executed.");
        return true; // Return true if the command was executed successfully
        // Implement connection logic here
    }
    public bool ProfileConnectCommand()
    {
        Debug.WriteLine("Profile connect command executed.");
        return true; // Return true if the command was executed successfully
        // Implement profile connection logic here
    }
    public bool OpenGenieDirCommand()
    {
        Debug.WriteLine("Open Genie directory command executed.");
        // Implement logic to open the Genie directory here
        return true; // Return true if the command was executed successfully
    }
    public bool OpenScriptsDirCommand()
    {
        Debug.WriteLine("Open scripts directory command executed.");
        // Implement logic to open the scripts directory here
        return true; // Return true if the command was executed successfully
    }
    public bool OpenMapsDirCommand()
    {
        Debug.WriteLine("Open maps directory command executed.");
        // Implement logic to open the maps directory here
        return true; // Return true if the command was executed successfully
    }
    public bool OpenPluginsDirCommand()
    {
        Debug.WriteLine("Open plugins directory command executed.");
        // Implement logic to open the plugins directory here
        return true; // Return true if the command was executed successfully
    }
    public bool OpenLogsDirCommand()
    {
        Debug.WriteLine("Open logs directory command executed.");
        // Implement logic to open the logs directory here
        return true; // Return true if the command was executed successfully
    }
    public bool OpenArtDirCommand()
    {
        Debug.WriteLine("Open art directory command executed.");
        // Implement logic to open the art directory here
        return true; // Return true if the command was executed successfully
    }
    public bool OpenLogFileCommand()
    {
        Debug.WriteLine("Open log file command executed.");
        // Implement logic to open the log file here
        return true; // Return true if the command was executed successfully
    }
    public bool ShowRawDataCommand()
    {
        Debug.WriteLine("Show raw data command executed.");
        // Implement logic to show raw data here
        return true; // Return true if the command was executed successfully
    }

    public bool ExitAppCommand()
    {         Debug.WriteLine("Exit application command executed.");
        // Implement logic to exit the application here
        Environment.Exit(0); // This will close the application
        return true; // Return true if the command was executed successfully
    }
    #endregion 'File' Commands

    #region 'Edit' Commands
    public bool PasteMultiCommand()
    {
        Debug.WriteLine("Paste multi-command executed.");
        // Implement logic to paste multi-command here
        return true; // Return true if the command was executed successfully
    }
    public bool DoConfigurationCommand()
    {
        Debug.WriteLine("Do configuration command executed.");
        // Implement logic to open configuration settings here
        return true; // Return true if the command was executed successfully
    }
    public bool UpdateImagesCommand()
    {         Debug.WriteLine("Update images command executed.");
        // Implement logic to update images here
        return true; // Return true if the command was executed successfully
    }
    #endregion 'Edit' Commands

    #region 'Profile' Commands
    public bool LoadProfileCommand()
    {
        Debug.WriteLine("Load profile command executed.");
        // Implement logic to load a profile here
        return true; // Return true if the command was executed successfully
    }
    public bool SaveProfileCommand()
    {
        Debug.WriteLine("Save profile command executed.");
        // Implement logic to save a profile here
        return true; // Return true if the command was executed successfully
    }
    public bool SavePasswordCommand()
    {
        Debug.WriteLine("Save paste profile command executed.");
        // Implement logic to save a paste profile here
        return true; // Return true if the command was executed successfully
    }
    #endregion 'Profile' Commands

    #region Layout Commands
    public bool LoadLayoutCommand()
    {
        Debug.WriteLine("Load layout command executed.");
        // Implement logic to load a layout here
        return true; // Return true if the command was executed successfully
    }
    public bool LoadDefaultLayoutCommand()
    {
        Debug.WriteLine("Load default layout command executed.");
        // Implement logic to load the default layout here
        return true; // Return true if the command was executed successfully
    }
    public bool SaveLayoutCommand()
    {
        Debug.WriteLine("Save layout command executed.");
        // Implement logic to save the current layout here
        return true; // Return true if the command was executed successfully
    }
    public bool SaveDefaultLayoutCommand()
    {
        Debug.WriteLine("Save default layout command executed.");
        // Implement logic to save the current layout as the default layout here
        return true; // Return true if the command was executed successfully
    }
    public bool SaveSizedLayoutCommand()
    {
        Debug.WriteLine("Save sized layout command executed.");
        // Implement logic to save the current layout with size here
        return true; // Return true if the command was executed successfully
    }
    public bool BasicLayoutCommand()
    {
        Debug.WriteLine("Basic layout command executed.");
        // Implement logic to switch to a basic layout here
        return true; // Return true if the command was executed successfully
    }
    #endregion 'Layout' Commands

    #region Script Commands
    public bool ScriptExplorerCommand()
    {
        Debug.WriteLine("Script explorer command executed.");
        // Implement logic to open the script explorer here
        return true; // Return true if the command was executed successfully
    }
    public bool UpdateScriptsCommand()
    {
        Debug.WriteLine("Update scripts command executed.");
        // Implement logic to update scripts here
        return true; // Return true if the command was executed successfully
    }
    public bool ShowActiveCommand()
    {
        Debug.WriteLine("Show active scripts command executed.");
        // Implement logic to show active scripts here
        return true; // Return true if the command was executed successfully
    }
    public bool TraceActiveCommand()
    {
        Debug.WriteLine("Trace active scripts command executed.");
        // Implement logic to trace active scripts here
        return true; // Return true if the command was executed successfully
    }
    public bool ResumeAllCommand()
    {
        Debug.WriteLine("Resume all scripts command executed.");
        // Implement logic to resume all scripts here
        return true; // Return true if the command was executed successfully
    }
    public bool PauseAllCommand()
    {
        Debug.WriteLine("Pause all scripts command executed.");
        // Implement logic to pause all scripts here
        return true; // Return true if the command was executed successfully
    }
    public bool AbortAllCommand()
    {
        Debug.WriteLine("Abort all scripts command executed.");
        // Implement logic to abort all scripts here
        return true; // Return true if the command was executed successfully
    }

    #endregion 'Script' Commands

    #region AutoMapper Commands
    public bool ShowAutoMapperCommand()
    {
        Debug.WriteLine("Show AutoMapper command executed.");
        // Implement logic to show the AutoMapper here
        return true; // Return true if the command was executed successfully
    }
    public bool UpdateMapsCommand()
    {
        Debug.WriteLine("Update maps command executed.");
        // Implement logic to update maps here
        return true; // Return true if the command was executed successfully
    }
    public bool ScriptSettingsCommand()
    {
        Debug.WriteLine("Script settings command executed.");
        // Implement logic to open script settings here
        return true; // Return true if the command was executed successfully
    }
    #endregion 'AutoMapper' Commands

    #region 'Plugins' Commands
    public bool UpdatePluginsCommand()
    {
        Debug.WriteLine("Update plugins command executed.");
        // Implement logic to update plugins here
        return true; // Return true if the command was executed successfully
    }
    #endregion 'Plugins' Commands

    #region 'Help' Commands
    public bool CheckForUpdatesCommand()
    {
        Debug.WriteLine("Check for updates command executed.");
        // Implement logic to check for updates here
        return true; // Return true if the command was executed successfully
    }
    public bool ForceUpdateCommand() {
        Debug.WriteLine("Force update command executed.");
        // Implement logic to force an update here
        return true; // Return true if the command was executed successfully
    }
    public bool LoadTestCommand()
    {
        Debug.WriteLine("Load test command executed.");
        // Implement logic to load test commands here
        return true; // Return true if the command was executed successfully
    }
    public bool LatestReleaseCommand()
    {
        Debug.WriteLine("Latest release command executed.");
        // Implement logic to show the latest release here
        return true; // Return true if the command was executed successfully
    }
    public bool DiscordCommand()
    {
        Debug.WriteLine("Discord command executed.");
        // Implement logic to open the Discord link here
        return true; // Return true if the command was executed successfully
    }
    public bool GitHubCommand()
    {
        Debug.WriteLine("GitHub command executed.");
        // Implement logic to open the GitHub link here
        return true; // Return true if the command was executed successfully
    }
    public bool WikiCommand()
    {
        Debug.WriteLine("Wiki command executed.");
        // Implement logic to open the Wiki link here
        return true; // Return true if the command was executed successfully
    }

    public bool PlayNetCommand() {
        Debug.WriteLine("PlayNet command executed.");
        // Implement logic to open the PlayNet link here
        return true; // Return true if the command was executed successfully
    }
    public bool ElanthipediaCommand() {
        Debug.WriteLine("Elanthipedia command executed.");
        // Implement logic to open the Elanthipedia link here
        return true; // Return true if the command was executed successfully
    }
    public bool DRServiceCommand() {
        Debug.WriteLine("DRService command executed.");
        // Implement logic to open the DRService link here
        return true; // Return true if the command was executed successfully
    }
    public bool LichDiscordCommand() {
        Debug.WriteLine("Lich Discord command executed.");
        // Implement logic to open the Lich Discord link here
        return true; // Return true if the command was executed successfully
    }
    public bool IsharonCommand() {
        Debug.WriteLine("Isharon command executed.");
        // Implement logic to open the Isharon link here
        return true; // Return true if the command was executed successfully
    }
    #endregion 'Help' Commands
}


