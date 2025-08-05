using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace GenieCoreLib;

public interface IConfigSettings
{
    string ConnectString { get; set; }
    char ScriptChar { get; set; }
    char SeparatorChar { get; set; }
    char CommandChar { get; set; }
    char QuickSendChar { get; set; }
    int TypeAhead { get; set; }
    char MyCommandChar { get; set; }
    int ArgumentCount { get; set; }
    bool TriggerOnInput { get; set; }
    int BufferLineSize { get; set; }
    bool ShowSpellTimer { get; set; }
    bool AutoLog { get; set; }
    bool ClassicConnect { get; set; }
    string Editor { get; set; }
    string Prompt { get; set; }
    string IgnoreMonsterList { get; set; }
    int ScriptTimeout { get; set; }
    int MaxGoSubDepth { get; set; }
    bool Reconnect { get; set; }
    bool ReconnectWhenDead { get; set; }
    bool IgnoreScriptWarnings { get; set; }
    bool IgnoreCloseAlert { get; set; }
    bool GagsEnabled { get; set; }
    bool KeepInput { get; set; }
    bool PlaySounds { get; set; }
    bool AbortDupeScript { get; set; }
    bool ParseGameOnly { get; set; }
    int HistorySize { get; set; }
    int HistoryMinLength { get; set; }
}
public partial class ConfigSettings : INotifyPropertyChanged, IConfigSettings, IDisposable
{
    [JsonIgnore]
    public static ConfigSettings Instance => _m_oConfigSettings ?? new ConfigSettings();
    private static ConfigSettings _m_oConfigSettings;

    // We don't want to do property changed notifications for every property initialization
    public static bool bLoading = false;

    public ConfigSettings()
    {
        _m_oConfigSettings = this;
    }

    #region Backing Fields
    // Backing fields
    private string _connectString = "FE:GENIE /VERSION:4.0 /P:WIN_XP /XML";
    private char _scriptChar = '.';
    private char _separatorChar = ';';
    private char _commandChar = '#';
    private char _quickSendChar = '-';
    private int _typeAhead = 2;
    private char _myCommandChar = '/';
    private int _argumentCount = 15;
    private bool _triggerOnInput = true;
    private int _bufferLineSize = 5;
    private bool _showSpellTimer = true;
    private bool _autoLog = true;
    private bool _classicConnect = true;
    private string _editor = "notepad.exe";
    private string _prompt = "> ";
    private string _ignoreMonsterList = "appears dead|(dead)";
    private int _scriptTimeout = 5000;
    private int _maxGoSubDepth = 50;
    private bool _reconnect = true;
    private bool _reconnectWhenDead = false;
    private bool _ignoreScriptWarnings = false;
    private bool _ignoreCloseAlert = false;
    private bool _gagsEnabled = true;
    private bool _keepInput = false;
    private bool _playSounds = true;
    private bool _abortDupeScript = true;
    private bool _parseGameOnly = false;
    private bool _autoMapper = true;
    private int _autoMapperAlpha = 255;
    private int _serverActivityTimeout = 180;
    private string _serverActivityCommand = "fatigue";
    private int _userActivityTimeout = 300;
    private string _userActivityCommand = "quit";
    private double _rtOffset = 0;
    private string _scriptDir = "Scripts";
    private string _soundDir = "Sounds";
    private string _pluginDir = "Plugins";
    private string _mapDir = "Maps";
    private string _configDir = "Config";
    private string _configDirProfile = Path.Combine("Config","Profiles");
    private bool _showLinks = false;
    private bool _showImages = true;
    private string _logDir = "Logs";
    private string _artDir = "Art";
    private bool _webLinkSafety = true;
    private int _historySize = 20;
    private int _historyMinLength = 3;
    #endregion Backing Fields

    #region Additional Backing Fields
    private bool _alwaysOnTop = false;
    private bool _autoUpdate = false;
    private bool _autoUpdateLamp = true;
    private bool _checkForUpdates = true;
    private bool _condensed = false;
    private bool _promptBreak = true;
    private bool _promptForce = true;
    private bool _sizeInputToGame = false;
    private bool _updateMapperScripts = false;
    private string _rubyPath = @"C:\ruby4lich5\bin\ruby.exe";
    private string _cmdPath = @"C:\Windows\System32\cmd.exe";
    private string _lichPath = @"c:\ruby4lich5\lich5\lich.rbw";
    private string _lichArguments = @"--genie --dragonrealms";
    private string _lichServer = "localhost";
    private int _lichPort = 11024;
    private int _lichStartPause = 5;
    private string _connectScript = string.Empty;
    private string _scriptExtension = "cmd";
    private string _scriptRepo = string.Empty;
    private string _artRepo = string.Empty;
    private string _mapRepo = string.Empty;
    private string _pluginRepo = string.Empty;
    #endregion Additional Backing Fields

    #region Properties Loaded from Config File
    public string ConnectString
    {
        get => _connectString;
        set { if (_connectString == value) return; _connectString = value; OnPropertyChanged(nameof(ConnectString), value); }
    }
    public char ScriptChar
    {
        get => _scriptChar;
        set { if (_scriptChar == value) return; _scriptChar = value; OnPropertyChanged(nameof(ScriptChar), value); }
    }
    public char SeparatorChar
    {
        get => _separatorChar;
        set { if (_separatorChar == value) return; _separatorChar = value; OnPropertyChanged(nameof(SeparatorChar), value); }
    }
    public char CommandChar
    {
        get => _commandChar;
        set { if (_commandChar == value) return; _commandChar = value; OnPropertyChanged(nameof(CommandChar), value); }
    }
    public char QuickSendChar
    {
        get => _quickSendChar;
        set { if (_quickSendChar == value) return; _quickSendChar = value; OnPropertyChanged(nameof(QuickSendChar), value); }
    }
    public int TypeAhead
    {
        get => _typeAhead;
        set { if (_typeAhead == value) return; _typeAhead = value; OnPropertyChanged(nameof(TypeAhead), value); }
    }
    public char MyCommandChar
    {
        get => _myCommandChar;
        set { if (_myCommandChar == value) return; _myCommandChar = value; OnPropertyChanged(nameof(MyCommandChar), value); }
    }
    public int ArgumentCount
    {
        get => _argumentCount;
        set { if (_argumentCount == value) return; _argumentCount = value; OnPropertyChanged(nameof(ArgumentCount), value); }
    }
    public bool TriggerOnInput
    {
        get => _triggerOnInput;
        set { if (_triggerOnInput == value) return; _triggerOnInput = value; OnPropertyChanged(nameof(TriggerOnInput), value); }
    }
    public int BufferLineSize
    {
        get => _bufferLineSize;
        set { if (_bufferLineSize == value) return; _bufferLineSize = value; OnPropertyChanged(nameof(BufferLineSize), value); }
    }
    public bool ShowSpellTimer
    {
        get => _showSpellTimer;
        set { if (_showSpellTimer == value) return; _showSpellTimer = value; OnPropertyChanged(nameof(ShowSpellTimer), value); }
    }
    public bool AutoLog
    {
        get => _autoLog;
        set { if (_autoLog == value) return; _autoLog = value; OnPropertyChanged(nameof(AutoLog), value); }
    }
    public bool ClassicConnect
    {
        get => _classicConnect;
        set { if (_classicConnect == value) return; _classicConnect = value; OnPropertyChanged(nameof(ClassicConnect), value); }
    }
    public string Editor
    {
        get => _editor;
        set { if (_editor == value) return; _editor = value; OnPropertyChanged(nameof(Editor), value); }
    }
    public string Prompt
    {
        get => _prompt;
        set { if (_prompt == value) return; _prompt = value; OnPropertyChanged(nameof(Prompt), value); }
    }
    public string IgnoreMonsterList
    {
        get => _ignoreMonsterList;
        set { if (_ignoreMonsterList == value) return; _ignoreMonsterList = value; OnPropertyChanged(nameof(IgnoreMonsterList), value); }
    }
    public int ScriptTimeout
    {
        get => _scriptTimeout;
        set { if (_scriptTimeout == value) return; _scriptTimeout = value; OnPropertyChanged(nameof(ScriptTimeout), value); }
    }
    public int MaxGoSubDepth
    {
        get => _maxGoSubDepth;
        set { if (_maxGoSubDepth == value) return; _maxGoSubDepth = value; OnPropertyChanged(nameof(MaxGoSubDepth), value); }
    }
    public bool Reconnect
    {
        get => _reconnect;
        set { if (_reconnect == value) return; _reconnect = value; OnPropertyChanged(nameof(Reconnect), value); }
    }
    public bool ReconnectWhenDead
    {
        get => _reconnectWhenDead;
        set { if (_reconnectWhenDead == value) return; _reconnectWhenDead = value; OnPropertyChanged(nameof(ReconnectWhenDead), value); }
    }
    public bool IgnoreScriptWarnings
    {
        get => _ignoreScriptWarnings;
        set { if (_ignoreScriptWarnings == value) return; _ignoreScriptWarnings = value; OnPropertyChanged(nameof(IgnoreScriptWarnings), value); }
    }
    public bool IgnoreCloseAlert
    {
        get => _ignoreCloseAlert;
        set { if (_ignoreCloseAlert == value) return; _ignoreCloseAlert = value; OnPropertyChanged(nameof(IgnoreCloseAlert), value); }
    }
    public bool GagsEnabled
    {
        get => _gagsEnabled;
        set { if (_gagsEnabled == value) return; _gagsEnabled = value; OnPropertyChanged(nameof(GagsEnabled), value); }
    }
    public bool KeepInput
    {
        get => _keepInput;
        set { if (_keepInput == value) return; _keepInput = value; OnPropertyChanged(nameof(KeepInput), value); }
    }
    public bool PlaySounds
    {
        get => _playSounds;
        set { if (_playSounds == value) return; _playSounds = value; OnPropertyChanged(nameof(PlaySounds), value); }
    }
    public bool AbortDupeScript
    {
        get => _abortDupeScript;
        set { if (_abortDupeScript == value) return; _abortDupeScript = value; OnPropertyChanged(nameof(AbortDupeScript), value); }
    }
    public bool ParseGameOnly
    {
        get => _parseGameOnly;
        set { if (_parseGameOnly == value) return; _parseGameOnly = value; OnPropertyChanged(nameof(ParseGameOnly), value); }
    }
    public bool AutoMapper
    {
        get => _autoMapper;
        set
        {
            if (_autoMapper == value) return;
            _autoMapper = value; OnPropertyChanged(nameof(AutoMapper), value);
        }
    }
    public int AutoMapperAlpha
    {
        get => _autoMapperAlpha;
        set
        {
            if (_autoMapperAlpha == value) return;
            if (value < 0) value = 0;
            else if (value > 255) value = 255;
            _autoMapperAlpha = value; OnPropertyChanged(nameof(AutoMapperAlpha), value);
        }
    }
    public int ServerActivityTimeout
    {
        get => _serverActivityTimeout;
        set { if (_serverActivityTimeout == value) return; _serverActivityTimeout = value; OnPropertyChanged(nameof(ServerActivityTimeout), value); }
    }
    public string ServerActivityCommand
    {
        get => _serverActivityCommand;
        set { if (_serverActivityCommand == value) return; _serverActivityCommand = value; OnPropertyChanged(nameof(ServerActivityCommand), value); }
    }
    public int UserActivityTimeout
    {
        get => _userActivityTimeout;
        set { if (_userActivityTimeout == value) return; _userActivityTimeout = value; OnPropertyChanged(nameof(UserActivityTimeout), value); }
    }
    public string UserActivityCommand
    {
        get => _userActivityCommand;
        set { if (_userActivityCommand == value) return; _userActivityCommand = value; OnPropertyChanged(nameof(UserActivityCommand), value); }
    }
    public double RTOffset
    {
        get => _rtOffset;
        set { if (_rtOffset == value) return; _rtOffset = value; OnPropertyChanged(nameof(RTOffset), value); }
    }

    public bool ShowLinks
    {
        get => _showLinks;
        set { if (_showLinks == value) return; _showLinks = value; OnPropertyChanged(nameof(ShowLinks), value); }
    }
    public bool ShowImages
    {
        get => _showImages;
        set { if (_showImages == value) return; _showImages = value; OnPropertyChanged(nameof(ShowImages), value); }
    }
    public bool WebLinkSafety
    {
        get => _webLinkSafety;
        set { if (_webLinkSafety == value) return; _webLinkSafety = value; OnPropertyChanged(nameof(WebLinkSafety), value); }
    }

    #endregion Properties Loaded from COnfig File

    #region File Directories
    public string ScriptDir
    {
        get => FixGetDirectory(_scriptDir);
        set
        {
            if (_scriptDir == value) return;
            _scriptDir = FixSetDirectory(value, _scriptDir);
            OnPropertyChanged(nameof(ScriptDir), value);
        }
    }
    public string SoundDir
    {
        get => FixGetDirectory(_soundDir);
        set
        {
            if (_soundDir == value) return;
            _soundDir = FixSetDirectory(value, _soundDir);
            OnPropertyChanged(nameof(SoundDir), value);
        }
    }

    public string PluginDir
    {
        get => FixGetDirectory(_pluginDir);
        set
        {
            if (_pluginDir == value) return;
            _pluginDir = FixSetDirectory(value, _pluginDir);
            OnPropertyChanged(nameof(PluginDir), value);
        }
    }
    public string MapDir
    {
        get => FixGetDirectory(_mapDir);
        set
        {
            if (_mapDir == value) return;
            _mapDir = FixSetDirectory(value, _mapDir);
            OnPropertyChanged(nameof(MapDir), value);
        }
    }
    public string ConfigDir
    {
        get => FixGetDirectory(_configDir);
        set
        {
            if (_configDir == value) return;
            _configDir = FixSetDirectory(value, _configDir);
            OnPropertyChanged(nameof(ConfigDir), value);
        }
    }
    public string ConfigProfileDir
    {
        get => FixGetDirectory(_configDirProfile);
        set
        {
            if (_configDirProfile == value) return;
            _configDirProfile = FixSetDirectory(value, _configDirProfile);
            OnPropertyChanged(nameof(ConfigProfileDir), value);
        }
    }
    public string LogDir
    {
        get => FixGetDirectory(_logDir);
        set
        {
            if (_logDir == value) return;
            _logDir = FixSetDirectory(value, _logDir);
            OnPropertyChanged(nameof(LogDir), value);
        }
    }
    public string ArtDir
    {
        get => FixGetDirectory(_artDir);
        set
        {
            if (_artDir == value) return;
            _artDir = FixSetDirectory(value, _artDir);
            OnPropertyChanged(nameof(ArtDir), value);
        }
    }
    #endregion File Directories



    #region Properties Modifiable at Runtime
    public bool AlwaysOnTop
    {
        get => _alwaysOnTop;
        set { if (_alwaysOnTop == value) return; _alwaysOnTop = value; OnPropertyChanged(nameof(AlwaysOnTop), value); }
    }
    public bool AutoUpdate
    {
        get => _autoUpdate;
        set { if (_autoUpdate == value) return; _autoUpdate = value; OnPropertyChanged(nameof(AutoUpdate), value); }
    }
    public bool AutoUpdateLamp
    {
        get => _autoUpdateLamp;
        set { if (_autoUpdateLamp == value) return; _autoUpdateLamp = value; OnPropertyChanged(nameof(AutoUpdateLamp), value); }
    }
    public bool CheckForUpdates
    {
        get => _checkForUpdates;
        set { if (_checkForUpdates == value) return; _checkForUpdates = value; OnPropertyChanged(nameof(CheckForUpdates), value); }
    }
    public bool Condensed
    {
        get => _condensed;
        set { if (_condensed == value) return; _condensed = value; OnPropertyChanged(nameof(Condensed), value); }
    }
    public bool PromptBreak
    {
        get => _promptBreak;
        set { if (_promptBreak == value) return; _promptBreak = value; OnPropertyChanged(nameof(PromptBreak), value); }
    }
    public bool PromptForce
    {
        get => _promptForce;
        set { if (_promptForce == value) return; _promptForce = value; OnPropertyChanged(nameof(PromptForce), value); }
    }
    public bool SizeInputToGame
    {
        get => _sizeInputToGame;
        set { if (_sizeInputToGame == value) return; _sizeInputToGame = value; OnPropertyChanged(nameof(SizeInputToGame), value); }
    }
    public bool UpdateMapperScripts
    {
        get => _updateMapperScripts;
        set { if (_updateMapperScripts == value) return; _updateMapperScripts = value; OnPropertyChanged(nameof(UpdateMapperScripts), value); }
    }
    public string RubyPath
    {
        get => _rubyPath;
        set { if (_rubyPath == value) return; _rubyPath = value; OnPropertyChanged(nameof(RubyPath), value); }
    }
    public string CmdPath
    {
        get => _cmdPath;
        set { if (_cmdPath == value) return; _cmdPath = value; OnPropertyChanged(nameof(CmdPath), value); }
    }
    public string LichPath
    {
        get => _lichPath;
        set { if (_lichPath == value) return; _lichPath = value; OnPropertyChanged(nameof(LichPath), value); }
    }
    public string LichArguments
    {
        get => _lichArguments;
        set { if (_lichArguments == value) return; _lichArguments = value; OnPropertyChanged(nameof(LichArguments), value); }
    }
    public string LichServer
    {
        get => _lichServer;
        set { if (_lichServer == value) return; _lichServer = value; OnPropertyChanged(nameof(LichServer), value); }
    }
    public int LichPort
    {
        get => _lichPort;
        set { if (_lichPort == value) return; _lichPort = value; OnPropertyChanged(nameof(LichPort), value); }
    }
    public int LichStartPause
    {
        get => _lichStartPause;
        set { if (_lichStartPause == value) return; _lichStartPause = value; OnPropertyChanged(nameof(LichStartPause), value); }
    }
    public string ConnectScript
    {
        get => _connectScript;
        set { if (_connectScript == value) return; _connectScript = value; OnPropertyChanged(nameof(ConnectScript), value); }
    }
    public string ScriptExtension
    {
        get => _scriptExtension;
        set { if (_scriptExtension == value) return; _scriptExtension = value; OnPropertyChanged(nameof(ScriptExtension), value); }
    }
    public string ScriptRepo
    {
        get => _scriptRepo;
        set { if (_scriptRepo == value) return; _scriptRepo = value; OnPropertyChanged(nameof(ScriptRepo), value); }
    }
    public string ArtRepo
    {
        get => _artRepo;
        set { if (_artRepo == value) return; _artRepo = value; OnPropertyChanged(nameof(ArtRepo), value); }
    }
    public string MapRepo
    {
        get => _mapRepo;
        set { if (_mapRepo == value) return; _mapRepo = value; OnPropertyChanged(nameof(MapRepo), value); }
    }
    public string PluginRepo
    {
        get => _pluginRepo;
        set { if (_pluginRepo == value) return; _pluginRepo = value; OnPropertyChanged(nameof(PluginRepo), value); }
    }
    public int HistorySize
    {
        get => _historySize;
        set { if (_historySize == value) return; _historySize = value; OnPropertyChanged(nameof(HistorySize), value); }
    }
    public int HistoryMinLength
    {
        get => _historyMinLength;
        set { if (_historyMinLength == value) return; _historyMinLength = value; OnPropertyChanged(nameof(HistoryMinLength), value); }
    }
    #endregion Properties Modifiable at Runtime


    #region Events and Methods
    public event PropertyChangedEventHandler? PropertyChanged;

    public ConfigSettings Load()
    {
        return LoadSettings("settings");
    }
    public ConfigSettings LoadSettings(string fileName)
    {
        bLoading = true;
        string filePath = !fileName.Contains("config", StringComparison.OrdinalIgnoreCase) ? Path.Combine(ConfigSettings.Instance.ConfigDir, fileName) : fileName;
        string jsonFile = filePath.Replace(".cfg",".json");
        if (!jsonFile.EndsWith(".json"))
        {
            jsonFile += ".json";
        }

        if (File.Exists(jsonFile))
        {
            // Load settings from the file
            string configData = File.ReadAllText(jsonFile);
            try
            {
                ConfigSettings settings = JsonSerializer.Deserialize<ConfigSettings>(configData);
                // Deserialize or parse the configData to populate ConfigSettings properties
                settings.CopyBackToSelf(this); // copy the file contents back on top of the current configuraton
                settings.Dispose();
                bLoading = false;
                return this;
            }
            catch (Exception exc)
            {

            }
            return null;
        }
        else
        {
            // if no json file, look for a legacy config file
            if (!filePath.EndsWith(".cfg"))
                filePath += ".cfg";

            if (File.Exists(filePath))
            {
                string configData = File.ReadAllText(filePath);
                if (configData.StartsWith("#config"))
                {
                    // This is a legacy config file, handle it accordingly
                    LoadLegacySettings(this, configData);
                    bLoading = false;
                    return this;
                }

            }
        }
        bLoading = false;
        return this;
    }
    public string ListAll(string sPattern = "")
    {
        StringBuilder sb = new();
        sb.AppendLine("Active settings:");
        if (!string.IsNullOrEmpty(sPattern))
        {
            sb.AppendLine($"Pattern: {sPattern}");
        }
        PropertyInfo[] properties = (this.GetType()).GetProperties();
        List<PropertyInfo> propertyList = properties.ToList();
        int i = 0;
        foreach (PropertyInfo property in propertyList)
        {
            if (property.CanRead && (sPattern == "" || property.Name.Contains(sPattern, StringComparison.OrdinalIgnoreCase)))
            {
                if (property.Name.Equals("Instance")) continue;
                object? value = property.GetValue(this);
                sb.AppendLine($"{property.Name}= {value}");
                i++;
            }
        }
        if (i == 0)
        {
            sb.AppendLine("None");
        }
        return sb.ToString();
    }
    /*

        EchoText(System.Environment.NewLine + "Active settings: " + System.Environment.NewLine);
        EchoText("alwaysontop=" + m_oConfigSettings.AlwaysOnTop.ToString() + System.Environment.NewLine);
        EchoText("abortdupescript=" + m_oConfigSettings.AbortDupeScript.ToString() + System.Environment.NewLine);
        EchoText("autolog=" + m_oConfigSettings.AutoLog.ToString() + System.Environment.NewLine);
        EchoText("automapper=" + m_oConfigSettings.AutoMapper.ToString() + System.Environment.NewLine);
        EchoText("commandchar=" + m_oConfigSettings.CommandChar.ToString() + System.Environment.NewLine);
        EchoText("connectstring=" + m_oConfigSettings.ConnectString.ToString() + System.Environment.NewLine);
        EchoText("classicconnect=" + m_oConfigSettings.ClassicConnect.ToString() + System.Environment.NewLine);
        EchoText("editor=" + m_oConfigSettings.Editor + System.Environment.NewLine);
        EchoText("ignoreclosealert=" + m_oConfigSettings.IgnoreCloseAlert.ToString() + System.Environment.NewLine);
        EchoText("ignorescriptwarnings=" + m_oConfigSettings.IgnoreScriptWarnings.ToString() + System.Environment.NewLine);
        EchoText("keepinputtext=" + m_oConfigSettings.KeepInput.ToString() + System.Environment.NewLine);
        EchoText("sizeinputtogame=" + m_oConfigSettings.SizeInputToGame.ToString() + System.Environment.NewLine);
        EchoText("maxgosubdepth=" + m_oConfigSettings.MaxGoSubDepth + System.Environment.NewLine);
        EchoText("maxrowbuffer=" + m_oConfigSettings.BufferLineSize.ToString() + System.Environment.NewLine);
        EchoText("monstercountignorelist=" + m_oConfigSettings.IgnoreMonsterList + System.Environment.NewLine);
        EchoText("muted=" + (!m_oConfigSettings.PlaySounds).ToString() + System.Environment.NewLine);
        EchoText("mycommandchar=" + m_oConfigSettings.MyCommandChar.ToString() + System.Environment.NewLine);
        EchoText("parsegameonly=" + m_oConfigSettings.ParseGameOnly.ToString() + System.Environment.NewLine);
        EchoText("prompt=" + m_oConfigSettings.Prompt + System.Environment.NewLine);
        EchoText("promptbreak=" + m_oConfigSettings.PromptBreak + System.Environment.NewLine);
        EchoText("promptforce=" + m_oConfigSettings.PromptForce + System.Environment.NewLine);
        EchoText("condensed=" + m_oConfigSettings.Condensed + System.Environment.NewLine);
        EchoText("reconnect=" + m_oConfigSettings.Reconnect.ToString() + System.Environment.NewLine);
        EchoText("roundtimeoffset=" + m_oConfigSettings.RTOffset + System.Environment.NewLine);
        EchoText("showlinks=" + m_oConfigSettings.ShowLinks.ToString() + System.Environment.NewLine);
        EchoText("showimages=" + m_oConfigSettings.ShowImages.ToString() + System.Environment.NewLine);
        EchoText("artdir=" + m_oConfigSettings.ArtDir + System.Environment.NewLine);
        EchoText("logdir=" + m_oConfigSettings.LogDir + System.Environment.NewLine);
        EchoText("configdir=" + m_oConfigSettings.ConfigDir + System.Environment.NewLine);
        EchoText("plugindir=" + m_oConfigSettings.PluginDir + System.Environment.NewLine);
        EchoText("mapdir=" + m_oConfigSettings.MapDir + System.Environment.NewLine);
        EchoText("scriptdir=" + m_oConfigSettings.ScriptDir + System.Environment.NewLine);
        EchoText("sounddir=" + m_oConfigSettings.SoundDir + System.Environment.NewLine);
        EchoText("scriptchar=" + m_oConfigSettings.ScriptChar.ToString() + System.Environment.NewLine);
        EchoText("scriptrepo=" + m_oConfigSettings.ScriptRepo + System.Environment.NewLine);
        EchoText("maprepo=" + m_oConfigSettings.MapRepo + System.Environment.NewLine);
        EchoText("updatemapperscripts=" + m_oConfigSettings.UpdateMapperScripts.ToString() + System.Environment.NewLine);
        EchoText("pluginrepo=" + m_oConfigSettings.PluginRepo + System.Environment.NewLine);
        EchoText("scriptextension=" + m_oConfigSettings.ScriptExtension + System.Environment.NewLine);
        EchoText("scripttimeout=" + m_oConfigSettings.ScriptTimeout.ToString() + System.Environment.NewLine);
        EchoText("separatorchar=" + m_oConfigSettings.SeparatorChar.ToString() + System.Environment.NewLine);
        EchoText("spelltimer=" + m_oConfigSettings.ShowSpellTimer.ToString() + System.Environment.NewLine);
        EchoText("triggeroninput=" + m_oConfigSettings.TriggerOnInput.ToString() + System.Environment.NewLine);
        EchoText("servertimeout=" + m_oConfigSettings.ServerActivityTimeout.ToString() + System.Environment.NewLine);
        EchoText("servertimeoutcommand=" + m_oConfigSettings.ServerActivityCommand.ToString() + System.Environment.NewLine);
        EchoText("usertimeout=" + m_oConfigSettings.UserActivityTimeout.ToString() + System.Environment.NewLine);
        EchoText("usertimeoutcommand=" + m_oConfigSettings.UserActivityCommand.ToString() + System.Environment.NewLine);
        EchoText("connectscript=" + m_oConfigSettings.ConnectScript.ToString() + System.Environment.NewLine);
        EchoText("checkforupdates=" + m_oConfigSettings.CheckForUpdates.ToString() + System.Environment.NewLine);
        EchoText("autoupdate=" + m_oConfigSettings.AutoUpdate.ToString() + System.Environment.NewLine);
        EchoText("autoupdatelamp=" + m_oConfigSettings.AutoUpdateLamp.ToString() + System.Environment.NewLine);
        EchoText("automapperalpha=" + m_oConfigSettings.AutoMapperAlpha.ToString() + System.Environment.NewLine);
        EchoText("weblinksafety=" + m_oConfigSettings.WebLinkSafety.ToString() + System.Environment.NewLine);
    }
     */

    private void CopyBackToSelf(ConfigSettings configSettings)
    {
        // copy the new settings back on top of the current settings and dispose of the new copy afterwards
        List<PropertyInfo> propertyList = (configSettings.GetType()).GetProperties().ToList();
        foreach (PropertyInfo property in propertyList)
        {
            if (property.CanWrite)
            {
                object? value = property.GetValue(configSettings);
                property.SetValue(this, value);
            }
        }   
    }

    public static ConfigSettings LoadLegacySettings(ConfigSettings settings, string ConfigData)
    {
        StringBuilder sbe = new();
        StringBuilder sbjason = new();
        string[] lines = ConfigData.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        // We will use reflection to set properties
        PropertyInfo[] properties = (settings.GetType()).GetProperties();
        List<PropertyInfo> propertyList = properties.ToList();
        int errors = 0;
        int found = 0;
        foreach (string line in lines)
        {
            var oArgs = Utility.ParseArgs(line);
            if (oArgs.Count == 3)
                try
                {
                    string keyIn = oArgs[1].ToString().Trim().ToLower();
                    string key = keyIn switch
                    {
                        "muted" => "playsounds",
                        "maxrowbuffer" => "BufferLineSize",
                        "spelltimer" => "ShowSpellTimer",
                        "monstercountignorelist" => "IgnoreMonsterList",
                        "roundtimeoffset" => "RTOffset",
                        "keepinputtext" => "KeepInput",
                        "servertimeout" => "ServerActivityTimeout",
                        "servertimeoutcommand" => "ServerActivityCommand",
                        "usertimeout" => "UserActivityTimeout",
                        "usertimeoutcommand" => "UserActivityCommand",
                        _ => keyIn
                    };
                    string svalue = oArgs[2].ToString();
                    string error = SetSetting(key, svalue, settings, propertyList);
                    found++;
                    if (!string.IsNullOrEmpty(error))
                    {
                        sbe.AppendLine(error);
                        errors++;
                    }
                }
                catch
                {
                    sbe.AppendLine("Bad setting: " + line);
                }
        }
        string result = $"Loaded {found} settings with {errors} errors.";
        return settings;
    }

    public bool SaveSettings()
    {
        return SaveSettings(Path.Combine(ConfigSettings.Instance.ConfigDir, "settings"));
    }
    public bool SaveSettings(string fileName)
    {
        string filePath = !fileName.Contains("config", StringComparison.OrdinalIgnoreCase) ? Path.Combine(ConfigSettings.Instance.ConfigDir, fileName) : fileName;
        string jsonFile = filePath.Replace(".cfg", ".json");
        if (!jsonFile.EndsWith(".json"))
        {
            jsonFile += ".json";
        }
        try
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonFile, json);
            return true;
        }
        catch (Exception exc)
        {
            // Handle exceptions, e.g., log them
            return false;
        }
    }
    public bool SetSetting(string key, string value = "")
    {
        try
        {
            PropertyInfo[] properties = (this.GetType()).GetProperties();
            SetSetting(key, value, this, properties.ToList());
            return true;
        }
        catch (Exception exc)
        {
            return false;
        }
    }
    private void OnPropertyChanged(string name, object value)
    {
        if (!bLoading)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    private string FixSetDirectory(string folder, string oldFolder)
    {
        if (folder.EndsWith(@"\"))
        {
            folder = folder.Substring(0, folder.Length - 1);
        }
        return !FileDirectory.ValidateDirectory(folder).StartsWith("Error") ? folder : oldFolder;
    }

    private string FixGetDirectory(string folder)
    {
        string sLocation = string.Empty;
        if (folder.Contains(":"))
        {
            return folder;
        }
        else
        {
            sLocation = AppGlobals.LocalDirectoryPath;
            if (folder.StartsWith(@"\") || sLocation.EndsWith(@"\"))
            {
                sLocation += folder;
            }
            else
            {
                sLocation += @"\" + folder;
            }
            return sLocation;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); // Prevent finalizer from running
     
    }
    bool bDisposed = false;
    public void Dispose(bool disposing)
    {
        if (bDisposed) return; // Prevent multiple disposals

        if (disposing)
        {
            // Dispose of any resources if necessary
            _m_oConfigSettings = null;
            PropertyChanged = null;
        }
        // Free unmanaged resources here if any
    }
    #endregion Events and Private Methods
}