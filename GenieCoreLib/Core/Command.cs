using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace GenieCoreLib;

public class Command
{
    #region Events

    public event EventDisconnectEventHandler? EventDisconnect;
    public delegate void EventDisconnectEventHandler();

    public event EventExitEventHandler? EventExit;
    public delegate void EventExitEventHandler();

    public event EventAddImageHandler? EventAddImage;
    public delegate void EventAddImageHandler(string filename, string window, int width, int height);

    public event EventEchoTextEventHandler? EventEchoText;
    public delegate void EventEchoTextEventHandler(string sText, string sWindow);

    public event EventLinkTextEventHandler? EventLinkText;
    public delegate void EventLinkTextEventHandler(string sText, string sLink, string sWindow);

    public event EventEchoColorTextEventHandler? EventEchoColorText;
    public delegate void EventEchoColorTextEventHandler(string sText, Color oColor, Color oBgColor, string sWindow);

    public event EventSendTextEventHandler? EventSendText;
    public delegate void EventSendTextEventHandler(string sText, bool bUserInput, string sOrigin);

    public event EventRunScriptEventHandler? EventRunScript;
    public delegate void EventRunScriptEventHandler(string sText);

    public event EventClearWindowEventHandler? EventClearWindow;
    public delegate void EventClearWindowEventHandler(string sWindow);

    public event EventVariableChangedEventHandler? EventVariableChanged;
    public delegate void EventVariableChangedEventHandler(string sVariable);

    public event EventParseLineEventHandler? EventParseLine;
    public delegate void EventParseLineEventHandler(string sText);

    public event EventStatusBarEventHandler? EventStatusBar;
    public delegate void EventStatusBarEventHandler(string sText, int iIndex);

    public event EventCopyDataEventHandler? EventCopyData;
    public delegate void EventCopyDataEventHandler(string sDestination, string sData);

    public event EventListScriptsEventHandler? EventListScripts;
    public delegate void EventListScriptsEventHandler(string sFilter);

    public event EventScriptTraceEventHandler? EventScriptTrace;
    public delegate void EventScriptTraceEventHandler(string sScript);

    public event EventScriptAbortEventHandler? EventScriptAbort;
    public delegate void EventScriptAbortEventHandler(string sScript);

    public event EventScriptPauseEventHandler? EventScriptPause;
    public delegate void EventScriptPauseEventHandler(string sScript);

    public event EventScriptPauseOrResumeEventHandler? EventScriptPauseOrResume;
    public delegate void EventScriptPauseOrResumeEventHandler(string sScript);

    public event EventScriptResumeEventHandler? EventScriptReload;
    public delegate void EventScriptReloadEventHandler(string sScript);

    public event EventScriptResumeEventHandler? EventScriptResume;
    public delegate void EventScriptResumeEventHandler(string sScript);

    public event EventScriptDebugEventHandler? EventScriptDebug;
    public delegate void EventScriptDebugEventHandler(int iDebugLevel, string sScript);

    public event EventScriptVariablesEventHandler? EventScriptVariables;
    public delegate void EventScriptVariablesEventHandler(string sScript, string sFilter);

    public event EventPresetChangedEventHandler? EventPresetChanged;
    public delegate void EventPresetChangedEventHandler(string sPreset);

    public event EventShowScriptExplorerEventHandler? EventShowScriptExplorer;
    public delegate void EventShowScriptExplorerEventHandler();

    public event EventLoadLayoutEventHandler? EventLoadLayout;
    public delegate void EventLoadLayoutEventHandler(string sFile);

    public event EventSaveLayoutEventHandler? EventSaveLayout;
    public delegate void EventSaveLayoutEventHandler(string sFile);

    public event EventLoadProfileEventHandler? EventLoadProfile;
    public delegate void EventLoadProfileEventHandler();

    public event EventSaveProfileEventHandler? EventSaveProfile;
    public delegate void EventSaveProfileEventHandler();

    public event EventFlashWindowEventHandler? EventFlashWindow;
    public delegate void EventFlashWindowEventHandler();

    public event EventClassChangeEventHandler? EventClassChange;
    public delegate void EventClassChangeEventHandler();

    public event EventMapperCommandEventHandler? EventMapperCommand;
    public delegate void EventMapperCommandEventHandler(string cmd);

    public event EventAddWindowEventHandler? EventAddWindow;
    public delegate void EventAddWindowEventHandler(string sWindow, int sWidth, int sHeight, int? sTop, int? sLeft);

    public event EventPositionWindowEventHandler? EventPositionWindow;
    public delegate void EventPositionWindowEventHandler(string sWindow, int? sWidth, int? sHeight, int? sTop, int? sLeft);

    public event EventRemoveWindowEventHandler? EventRemoveWindow;
    public delegate void EventRemoveWindowEventHandler(string sWindow);

    public event EventCloseWindowEventHandler? EventCloseWindow;
    public delegate void EventCloseWindowEventHandler(string sWindow);

    public event EventChangeWindowTitleEventHandler? EventChangeWindowTitle;
    public delegate void EventChangeWindowTitleEventHandler(string sWindow, string sComment);

    public event EventRawToggleEventHandler? EventRawToggle;
    public delegate void EventRawToggleEventHandler(string sToggle);

    public event EventSendRawEventHandler? EventSendRaw;
    public delegate void EventSendRawEventHandler(string sText);

    public event EventChangeIconEventHandler? EventChangeIcon;
    public delegate void EventChangeIconEventHandler(string sPath);

    public event LoadPluginEventHandler? LoadPlugin;
    public delegate void LoadPluginEventHandler(string filename);

    public event UnloadPluginEventHandler? UnloadPlugin;
    public delegate void UnloadPluginEventHandler(string filename);

    public event EnablePluginEventHandler? EnablePlugin;
    public delegate void EnablePluginEventHandler(string filename);

    public event DisablePluginEventHandler? DisablePlugin;
    public delegate void DisablePluginEventHandler(string filename);

    public event LaunchBrowserEventHandler? LaunchBrowser;
    public delegate void LaunchBrowserEventHandler(string url);

    public event ReloadPluginsEventHandler? ReloadPlugins;
    public delegate void ReloadPluginsEventHandler();

    public event ListPluginsEventHandler? ListPlugins;
    public delegate void ListPluginsEventHandler();
    #endregion Events

    public static Command Instance => _m_oCommand ?? new Command();
    private static Command? _m_oCommand;

    private Game m_oGame;
    private Globals m_oGlobals = Globals.Instance;

    private Evaluator m_oEval;
    private MathEval m_oMathEval = new MathEval();

    public bool TriggersEnabled = true;
    public Command()
    {
        _m_oCommand = this;
        m_oGlobals = Globals.Instance;
        m_oGame = Game.Instance;
        m_oEval = Evaluator.GetInstance();
    }

    public async Task<string> ParseCommand(string sText, bool bSendToGame = false, bool bUserInput = false, string sOrigin = "", bool bParseQuickSend = true)
    {
        /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
        string sResult = string.Empty;
        if (sText.ToLower().StartsWith("bug ") | sText.ToLower().StartsWith("sing ") | sText.ToLower().StartsWith(";"))
        {
            if (bSendToGame == true)
            {
                // Send Text To Game
                string argsText = Globals.ParseGlobalVars(sText);
                SendTextToGame(argsText, bUserInput, sOrigin);
            }

            return sText;
        }

        foreach (string stemp in Utility.SafeSplit(sText,ConfigSettings.Instance.SeparatorChar))
        {
            var sStringTemp = stemp;
            if (Aliases.Instance.ContainsKey(GetKeywordString(sStringTemp).ToLower()) == true) // Alias
            {
                sStringTemp = ParseAlias(sStringTemp);
            }

            foreach (string row in Utility.SafeSplit(sStringTemp, ConfigSettings.Instance.SeparatorChar))
            {
                var sRow = row;
                // Quick #send
                if (bParseQuickSend)
                {
                    if (sRow.StartsWith(Conversions.ToString(ConfigSettings.Instance.QuickSendChar)))
                    {
                        sRow = "#send " + sRow.Substring(1);
                    }
                }

                sResult = string.Empty;
                if (sRow.Trim().StartsWith(ConfigSettings.Instance.CommandChar))
                {
                    // Get result from function then send result to game
                    var oArgs = Utility.ParseArgs(sRow);
                    if (oArgs is not null && oArgs.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(oArgs[0].ToString()))
                        {
                            string switchExpr = oArgs[0].ToString().Substring(1).ToLower();
                            switch (switchExpr)
                            {
                                case "echo":
                                    {
                                        // #echo <color> >window text
                                        string sOutputWindow = string.Empty;
                                        int iColorIndex = 0;
                                        Color oColor = default;
                                        Color oBgcolor = default;
                                        if (oArgs.Count > 1 && oArgs[1].ToString().StartsWith(">"))
                                        {
                                            sOutputWindow = Globals.ParseGlobalVars(oArgs[1].ToString().Substring(1));
                                            oArgs[1] = null;
                                            if (oArgs.Count > 2)
                                            {
                                                iColorIndex = 2;
                                            }
                                        }
                                        else if (oArgs.Count > 2 && oArgs[2].ToString().StartsWith(">"))
                                        {
                                            sOutputWindow = Globals.ParseGlobalVars(oArgs[2].ToString().Substring(1));
                                            oArgs[2] = null;
                                            iColorIndex = 1;
                                        }
                                        else if (oArgs.Count > 1)
                                        {
                                            iColorIndex = 1;
                                        }

                                        if (iColorIndex > 0)
                                        {
                                            string sColorName = oArgs[iColorIndex].ToString();
                                            if (sColorName.Contains(",") == true && sColorName.EndsWith(",") == false)
                                            {
                                                string sColor = sColorName.Substring(0, sColorName.IndexOf(",")).Trim();
                                                string sBgColor = sColorName.Substring(sColorName.IndexOf(",") + 1).Trim();
                                                oColor = ColorCode.StringToColor(sColor);
                                                oBgcolor = ColorCode.StringToColor(sBgColor);
                                            }
                                            else
                                            {
                                                oColor = ColorCode.StringToColor(sColorName);
                                                oBgcolor = Color.Transparent;
                                            }

                                            if (!Information.IsNothing(oColor) && oColor != Color.Empty)
                                            {
                                                oArgs[iColorIndex] = null;
                                            }
                                        }

                                        if (!Information.IsNothing(oColor) && oColor != Color.Empty)
                                        {
                                            string argsText1 = Globals.ParseGlobalVars(ParseAllArgs(oArgs, 1, false) + System.Environment.NewLine);
                                            EchoColorText(argsText1, oColor, oBgcolor, sOutputWindow);
                                        }
                                        else
                                        {
                                            EchoText(Globals.ParseGlobalVars(ParseAllArgs(oArgs, 1, false) + System.Environment.NewLine), sOutputWindow);
                                        }

                                        sResult = "";
                                        break;
                                    }
                                case "link":
                                    {
                                        string sWindow = string.Empty;
                                        string sLinkText = string.Empty;
                                        string sLinkCommand = string.Empty;
                                        if (oArgs.Count > 3 && oArgs[1].ToString().StartsWith(">"))
                                        {
                                            sWindow = Globals.ParseGlobalVars(oArgs[1].ToString().Substring(1));
                                            sLinkText = Globals.ParseGlobalVars(oArgs[2].ToString());
                                            sLinkCommand = Utility.ArrayToString(oArgs, 3);
                                        }
                                        else if (oArgs.Count > 2)
                                        {
                                            sLinkText = Globals.ParseGlobalVars(oArgs[1].ToString());
                                            sLinkCommand = Utility.ArrayToString(oArgs, 2);
                                        }

                                        if (sLinkText.Length > 0)
                                        {
                                            EventLinkText?.Invoke(sLinkText, sLinkCommand, sWindow);
                                        }

                                        break;
                                    }

                                case "icon":
                                    {
                                        EventChangeIcon?.Invoke(Globals.ParseGlobalVars(oArgs[1].ToString()));
                                        break;
                                    }

                                case "log":
                                    {
                                        if (oArgs.Count > 0)
                                        {
                                            string sLogText = string.Empty;
                                            if (oArgs[1].ToString().StartsWith(">"))
                                            {
                                                string sFileName = Globals.ParseGlobalVars(oArgs[1].ToString().Substring(1));
                                                if (oArgs.Count > 2)
                                                {
                                                    sLogText = Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2, false));
                                                    Log.LogLine(sLogText, sFileName);
                                                }
                                            }
                                            else
                                            {
                                                string sTargetChar = Conversions.ToString(Variables.Instance["charactername"]);
                                                string sGameName = Conversions.ToString(Variables.Instance["game"]);
                                                sLogText = Globals.ParseGlobalVars(ParseAllArgs(oArgs, 1, false));
                                                Log.LogText(sLogText + System.Environment.NewLine, sTargetChar, sGameName);
                                            }
                                        }

                                        break;
                                    }

                                case "connect":
                                    {
                                        Connect(oArgs);
                                        break;
                                    }
                                case "img":
                                case "image":
                                    {
                                        string sOutputWindow = string.Empty;
                                        string filename = string.Empty;
                                        int width = 0;
                                        int height = 0;
                                        foreach(string arg in oArgs)
                                        {
                                            if (arg.StartsWith("#")) continue;
                                            else if (arg.StartsWith(">")) sOutputWindow = arg.Substring(1);
                                            else if ((arg.Length > 2 && arg.StartsWith("w:") || (arg.Length > 6 && arg.StartsWith("width:"))))
                                            {
                                                if (!int.TryParse(arg.Split(":")[1], out width)) EchoText($"Invalid Width Specified: {arg}");
                                            }
                                            else if ((arg.Length > 2 && arg.StartsWith("h:") || (arg.Length > 7 && arg.StartsWith("height:"))))
                                            {
                                                if (!int.TryParse(arg.Split(":")[1], out height)) EchoText($"Invalid Height Specified: {arg}");
                                            }
                                            else filename = arg;
                                        }
                                        if (string.IsNullOrEmpty(filename)) EchoText("No File Name was specified for the Image Command.");
                                        else DisplayImage(filename, sOutputWindow, width, height);
                                        break;   
                                    }

                                case "lc":
                                case "lconnect":
                                case "lichconnect":
                                    {
                                        string failure = string.Empty;
                                        if (!File.Exists(ConfigSettings.Instance.CmdPath)) failure += "CMD not found at Path:\t" + ConfigSettings.Instance.CmdPath + System.Environment.NewLine;
                                        if (!File.Exists(ConfigSettings.Instance.RubyPath)) failure += "Ruby not found at Path:\t" + ConfigSettings.Instance.RubyPath + System.Environment.NewLine;
                                        if (!File.Exists(ConfigSettings.Instance.LichPath)) failure += "Lich not found at Path:\t" + ConfigSettings.Instance.LichPath + System.Environment.NewLine;
                                        if (string.IsNullOrWhiteSpace(failure))
                                        {
                                            EchoText("Starting Lich Server\n");
                                            string lichLaunch = $"/C {ConfigSettings.Instance.RubyPath} {ConfigSettings.Instance.LichPath} {ConfigSettings.Instance.LichArguments}";
                                            await Utility.ExecuteProcess(ConfigSettings.Instance.CmdPath, lichLaunch, false, false);
                                            int count = 0;
                                            while (count < ConfigSettings.Instance.LichStartPause)
                                            {
                                                await Task.Delay(1000);
                                                count++;
                                            }
                                            Connect(oArgs, true);
                                        }
                                        else
                                        {
                                            failure = "Fix the following file paths in your #Config" + System.Environment.NewLine + failure;
                                            EchoColorText(failure, Color.OrangeRed, Color.Transparent);
                                        }
                                        break;
                                    }

                                case "ls":
                                case "lichsettings":
                                    {
                                        EchoText($"\nLich Settings\n");
                                        EchoText($"----------------------------------------------------\n");
                                        EchoText($"Cmd Path:\t\t {ConfigSettings.Instance.CmdPath}\n");
                                        EchoText($"Ruby Path:\t\t {ConfigSettings.Instance.RubyPath}\n");
                                        EchoText($"Lich Path:\t\t {ConfigSettings.Instance.LichPath}\n");
                                        EchoText($"Lich Arguments:\t {ConfigSettings.Instance.LichArguments}\n");
                                        EchoText($"Lich Start Pause:\t {ConfigSettings.Instance.LichStartPause}\n");
                                        EchoText($"Lich Server:\t\t {ConfigSettings.Instance.LichServer}\n");
                                        EchoText($"Lich Port:\t\t {ConfigSettings.Instance.LichPort}\n\n");
                                        break;
                                    }

                                case "disconnect":
                                    {
                                        EventDisconnect?.Invoke();
                                        break;
                                    }
                                case "exit":
                                    {
                                        EventExit?.Invoke();
                                        break;
                                    }
                                case "browser":
                                    {
                                        string url = Globals.ParseGlobalVars(oArgs[1].ToString());
                                        if (!url.StartsWith("http://") && !url.StartsWith("https://")) url = "http://" + url;
                                        LaunchBrowser?.Invoke(url);
                                        break;
                                    }
                                case "clear":
                                    {
                                        ClearWindow(Globals.ParseGlobalVars(ParseAllArgs(oArgs, 1)));
                                        break;
                                    }

                                case "save":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            for (int I = 1, loopTo = oArgs.Count - 1; I <= loopTo; I++)
                                            {
                                                var switchExpr1 = oArgs[I].ToString().ToLower();
                                                switch (switchExpr1)
                                                {
                                                    case "var":
                                                    case "vars":
                                                    case "variable":
                                                    case "setvar":
                                                    case "setvariable":
                                                        {
                                                            EchoText("Variables Saved" + System.Environment.NewLine);
                                                            Variables.Instance.Save();
                                                            break;
                                                        }

                                                    case "alias":
                                                    case "aliases":
                                                        {
                                                            EchoText("Aliases Saved" + System.Environment.NewLine);
                                                            Aliases.Instance.Save();
                                                            break;
                                                        }

                                                    case "class":
                                                    case "classes":
                                                        {
                                                            EchoText("Classes Saved" + System.Environment.NewLine);
                                                            Classes.Instance.Save();
                                                            break;
                                                        }

                                                    case "trigger":
                                                    case "triggers":
                                                    case "action":
                                                    case "actions":
                                                        {
                                                            EchoText("Triggers Saved" + System.Environment.NewLine);
                                                            Triggers.Instance.Save();
                                                            break;
                                                        }

                                                    case "config":
                                                    case "set":
                                                    case "setting":
                                                    case "settings":
                                                        {
                                                            EchoText("Settings Saved" + System.Environment.NewLine);
                                                            ConfigSettings.Instance.SaveSettings();
                                                            break;
                                                        }

                                                    case "macro":
                                                    case "macros":
                                                        {
                                                            EchoText("Macros Saved" + System.Environment.NewLine);
                                                            Macros.Instance.Save();
                                                            break;
                                                        }

                                                    case "sub":
                                                    case "subs":
                                                    case "substitute":
                                                        {
                                                            EchoText("Substitutes Saved" + System.Environment.NewLine);
                                                            SubstituteRegExp.Instance.Save();
                                                            break;
                                                        }

                                                    case "gag":
                                                    case "gags":
                                                    case "squelch":
                                                    case "ignore":
                                                    case "ignores":
                                                        {
                                                            EchoText("Gags Saved" + System.Environment.NewLine);
                                                            GagRegExp.Instance.Save();
                                                            break;
                                                        }

                                                    case "highlight":
                                                    case "highlights":
                                                        {
                                                            EchoText("Highlights Saved" + System.Environment.NewLine);
                                                            HighlightBase.SaveHighlights();
                                                            break;
                                                        }

                                                    case "name":
                                                    case "names":
                                                        {
                                                            EchoText("Names Saved" + System.Environment.NewLine);
                                                            Names.Instance.Save();
                                                            break;
                                                        }

                                                    case "preset":
                                                    case "presets":
                                                        {
                                                            EchoText("Presets Saved" + System.Environment.NewLine);
                                                            Presets.Instance.Save();
                                                            break;
                                                        }

                                                    case "layout":
                                                        {
                                                            EchoText("Layout Saved" + System.Environment.NewLine);
                                                            var tmp = string.Empty;
                                                            EventSaveLayout?.Invoke(tmp);
                                                            break;
                                                        }

                                                    case "profile":
                                                        {
                                                            EchoText("Profile Saved" + System.Environment.NewLine);
                                                            EventSaveProfile?.Invoke();
                                                            break;
                                                        }

                                                    case "all":
                                                        {
                                                            EchoText("Variables Saved" + System.Environment.NewLine);
                                                            Variables.Instance.Save();
                                                            EchoText("Aliases Saved" + System.Environment.NewLine);
                                                            Aliases.Instance.Save();
                                                            EchoText("Classes Saved" + System.Environment.NewLine);
                                                            Classes.Instance.Save();
                                                            EchoText("Triggers Saved" + System.Environment.NewLine);
                                                            Triggers.Instance.Save();
                                                            EchoText("Settings Saved" + System.Environment.NewLine);
                                                            ConfigSettings.Instance.SaveSettings();
                                                            EchoText("Macros Saved" + System.Environment.NewLine);
                                                            Macros.Instance.Save();
                                                            EchoText("Substitutes Saved" + System.Environment.NewLine);
                                                            SubstituteRegExp.Instance.Save();
                                                            EchoText("Gags Saved" + System.Environment.NewLine);
                                                            GagRegExp.Instance.Save();
                                                            EchoText("Highlights Saved" + System.Environment.NewLine);
                                                            HighlightBase.SaveHighlights();
                                                            EchoText("Names Saved" + System.Environment.NewLine);
                                                            Names.Instance.Save();
                                                            EchoText("Presets Saved" + System.Environment.NewLine);
                                                            Presets.Instance.Save();
                                                            break;
                                                        }
                                                }
                                            }
                                        }

                                        break;
                                    }

                                case "load":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            for (int I = 1, loopTo1 = oArgs.Count - 1; I <= loopTo1; I++)
                                            {
                                                var switchExpr2 = oArgs[I].ToString().ToLower();
                                                switch (switchExpr2)
                                                {
                                                    case "var":
                                                    case "vars":
                                                    case "variable":
                                                    case "setvar":
                                                    case "setvariable":
                                                        {
                                                            EchoText("Variables Loaded" + System.Environment.NewLine);
                                                            Variables.Instance.ClearUser();
                                                            Variables.Instance.Load();
                                                            break;
                                                        }

                                                    case "alias":
                                                    case "aliases":
                                                        {
                                                            EchoText("Aliases Loaded" + System.Environment.NewLine);
                                                            Aliases.Instance.Clear();
                                                            Aliases.Instance.Load();
                                                            break;
                                                        }

                                                    case "class":
                                                    case "classes":
                                                        {
                                                            EchoText("Classes Loaded" + System.Environment.NewLine);
                                                            Classes.Instance.Clear();
                                                            Classes.Instance.Load();
                                                            EventClassChange?.Invoke();
                                                            break;
                                                        }

                                                    case "trigger":
                                                    case "triggers":
                                                    case "action":
                                                    case "actions":
                                                        {
                                                            EchoText("Triggers Loaded" + System.Environment.NewLine);
                                                            Triggers.Instance.Clear();
                                                            Triggers.Instance.Load();
                                                            break;
                                                        }

                                                    case "config":
                                                    case "set":
                                                    case "setting":
                                                    case "settings":
                                                        {
                                                            EchoText("Settings Loaded" + System.Environment.NewLine);
                                                            ConfigSettings.Instance.Load();
                                                            break;
                                                        }

                                                    case "macro":
                                                    case "macros":
                                                        {
                                                            EchoText("Macros Loaded" + System.Environment.NewLine);
                                                            Macros.Instance.Clear();
                                                            Macros.Instance.Load();
                                                            break;
                                                        }

                                                    case "sub":
                                                    case "subs":
                                                    case "substitute":
                                                        {
                                                            EchoText("Substitutes Loaded" + System.Environment.NewLine);
                                                            SubstituteRegExp.Instance.Clear();
                                                            SubstituteRegExp.Instance.Load();
                                                            break;
                                                        }

                                                    case "gag":
                                                    case "gags":
                                                    case "squelch":
                                                    case "ignore":
                                                    case "ignores":
                                                        {
                                                            EchoText("Gags Loaded" + System.Environment.NewLine);
                                                            GagRegExp.Instance.Clear();
                                                            GagRegExp.Instance.Load();
                                                            break;
                                                        }

                                                    case "highlight":
                                                    case "highlights":
                                                        {
                                                            HighlightsList.Instance.Clear();
                                                            HighlightsRegExpList.Instance.Clear();
                                                            HighlightsBeginWithList.Instance.Clear();
                                                            EchoText("Highlights Loaded" + System.Environment.NewLine);
                                                            HighlightBase.LoadHighlights();
                                                            break;
                                                        }

                                                    case "name":
                                                    case "names":
                                                        {
                                                            EchoText("Names Loaded" + System.Environment.NewLine);
                                                            Names.Instance.Clear();
                                                            Names.Instance.Load();
                                                            break;
                                                        }

                                                    case "preset":
                                                    case "presets":
                                                        {
                                                            EchoText("Presets Loaded" + System.Environment.NewLine);
                                                            Presets.Instance.Clear();
                                                            Presets.Instance.Load();
                                                            break;
                                                        }

                                                    case "layout":
                                                        {
                                                            EchoText("Layout Loaded" + System.Environment.NewLine);
                                                            var tmp = string.Empty;
                                                            EventLoadLayout?.Invoke(tmp);
                                                            break;
                                                        }

                                                    case "profile":
                                                        {
                                                            EchoText("Profile Loaded" + System.Environment.NewLine);
                                                            EventLoadProfile?.Invoke();
                                                            break;
                                                        }

                                                    case "all":
                                                        {
                                                            EchoText("Variables Loaded" + System.Environment.NewLine);
                                                            Variables.Instance.ClearUser();
                                                            Variables.Instance.Load();
                                                            EchoText("Aliases Loaded" + System.Environment.NewLine);
                                                            Aliases.Instance.Clear();
                                                            Aliases.Instance.Load();
                                                            EchoText("Classes Loaded" + System.Environment.NewLine);
                                                            Classes.Instance.Clear();
                                                            Classes.Instance.Load();
                                                            EchoText("Triggers Loaded" + System.Environment.NewLine);
                                                            Triggers.Instance.Clear();
                                                            Triggers.Instance.Load();
                                                            EchoText("Settings Loaded" + System.Environment.NewLine);
                                                            ConfigSettings.Instance.Load();
                                                            EchoText("Macros Loaded" + System.Environment.NewLine);
                                                            Macros.Instance.Clear();
                                                            Macros.Instance.Load();
                                                            EchoText("Substitutes Loaded" + System.Environment.NewLine);
                                                            SubstituteRegExp.Instance.Clear();
                                                            SubstituteRegExp.Instance.Load();
                                                            EchoText("Gags Loaded" + System.Environment.NewLine);
                                                            GagRegExp.Instance.Clear();
                                                            GagRegExp.Instance.Load();
                                                            HighlightsList.Instance.Clear();
                                                            HighlightsRegExpList.Instance.Clear();
                                                            HighlightsBeginWithList.Instance.Clear();
                                                            HighlightBase.LoadHighlights();
                                                            EchoText("Highlights Loaded" + System.Environment.NewLine);
                                                            EchoText("Names Loaded" + System.Environment.NewLine);
                                                            Names.Instance.Clear();
                                                            Names.Instance.Load();
                                                            EchoText("Presets Loaded" + System.Environment.NewLine);
                                                            Presets.Instance.Clear();
                                                            Presets.Instance.Load();
                                                            break;
                                                        }
                                                }
                                            }
                                        }

                                        break;
                                    }

                                case "nop": // Do nothing
                                    {
                                        sResult = "";
                                        break;
                                    }

                                case "cr":
                                    {
                                        sResult = System.Environment.NewLine;
                                        break;
                                    }

                                case "send":
                                    {
                                        Send(oArgs);

                                        sResult = "";
                                        break;
                                    }

                                case "do":
                                    {
                                        Do(oArgs);
                                        sResult = "";
                                        break;
                                    }
                                case "put":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            // Send Text To Game
                                            string argsText2 = Globals.ParseGlobalVars(ParseAllArgs(oArgs, 1));
                                            bool argbUserInput = false;
                                            SendTextToGame(argsText2, argbUserInput, sOrigin);
                                        }

                                        sResult = "";
                                        break;
                                    }

                                case "push":
                                    {
                                        // If oArgs.Count > 2 Then
                                        // RaiseEvent EventCopyData(Globals.ParseGlobalVars(oArgs.Item(1).ToString).ToLower, Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 2)))
                                        // End If
                                        sResult = "";
                                        break;
                                    }

                                case "var":
                                case "vars":
                                case "variable":
                                case "setvar":
                                case "setvariable":
                                    {
                                        if (oArgs.Count < 3)
                                        {
                                            if (oArgs.Count < 2)
                                            {
                                                ListVariables("");
                                            }
                                            else
                                            {
                                                var switchExpr3 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr3)
                                                {
                                                    case "load":
                                                        {
                                                            EchoText("Variables Loaded" + System.Environment.NewLine);
                                                            Variables.Instance.Load();
                                                            break;
                                                        }

                                                    case "save":
                                                        {
                                                            EchoText("Variables Saved" + System.Environment.NewLine);
                                                            Variables.Instance.Save();
                                                            break;
                                                        }

                                                    case "clear":
                                                        {
                                                            EchoText("Variables Cleared" + System.Environment.NewLine);
                                                            Variables.Instance.ClearUser();
                                                            break;
                                                        }

                                                    case "edit":
                                                        {
                                                            Interaction.Shell("\"" + ConfigSettings.Instance.Editor + "\" \"" + ConfigSettings.Instance.ConfigProfileDir + @"\variables.cfg""", 0, false);
                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            string textToEcho = Variables.Instance.ListAll("");
                                                            if (!string.IsNullOrEmpty(textToEcho)) EchoText(textToEcho);
                                                            break;
                                                        }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Add
                                            string argkey = Globals.ParseGlobalVars(oArgs[1].ToString());
                                            string argvalue = Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2));
                                            Variables.Instance.Add(argkey, argvalue);
                                            string argsVariable = "$" + oArgs[1].ToString();
                                            VariableChanged(argsVariable);
                                        }

                                        break;
                                    }

                                case "tvar":
                                case "tempvar":
                                case "tempvariable":
                                    {
                                        if (oArgs.Count < 3)
                                        {
                                            ListVariables("");
                                        }
                                        else
                                        {
                                            // Add
                                            string argkey1 = Globals.ParseGlobalVars(oArgs[1].ToString());
                                            string argvalue1 = Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2));
                                            Variables.Instance.Add(argkey1, argvalue1, Variables.VariablesType.Temporary);
                                            string argsVariable1 = "$" + oArgs[1].ToString();
                                            VariableChanged(argsVariable1);
                                        }

                                        break;
                                    }

                                case "svar":
                                case "servervar":
                                case "servervariable":
                                    {
                                        if (oArgs.Count < 3)
                                        {
                                            ListVariables("");
                                        }
                                        else
                                        {
                                            // Add
                                            string sName = Globals.ParseGlobalVars(oArgs[1].ToString());
                                            string sValue = Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2));
                                            string sCmdRaw = "<stgupd>";
                                            if (Variables.Instance.ContainsKey(sName))
                                            {
                                                if (Variables.Instance.get_GetVariable(sName).oType == Variables.VariablesType.Server)
                                                {
                                                    sCmdRaw += "<vars><<d><k name=\"" + sName + "\" value=\"" + Variables.Instance[sName] + "\"/></<d></vars>";
                                                }
                                            }

                                            sCmdRaw += "<vars><<a><k name=\"" + sName + "\" value=\"" + sValue + "\"/></<a></vars>";
                                            Variables.Instance.Add(sName, sValue, Variables.VariablesType.Server);
                                            string argsVariable2 = "$" + oArgs[1].ToString();
                                            VariableChanged(argsVariable2);
                                            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                                            EventSendRaw?.Invoke(sCmdRaw + Constants.vbLf);
                                        }

                                        break;
                                    }

                                case "unvar":
                                case "unvariable":
                                case "unsetvar":
                                case "unsetvariable":
                                    {
                                        if (oArgs.Count >= 1)
                                        {
                                            string sName = oArgs[1].ToString();
                                            if (Variables.Instance.ContainsKey(sName))
                                            {
                                                if (Variables.Instance.get_GetVariable(sName).oType == Variables.VariablesType.Server)
                                                {
                                                    string sCmdRaw = Conversions.ToString("<stgupd><vars><<d><k name=\"" + sName + "\" value=\"" + Variables.Instance[sName] + "\"/></<d></vars>");
                                                    /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                                                    EventSendRaw?.Invoke(sCmdRaw + Constants.vbLf);
                                                }

                                                Variables.Instance.Remove(sName);
                                            }
                                        }

                                        break;
                                    }

                                case "eval":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            string argsText3 = Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 1));
                                            sResult = Eval(argsText3);
                                            // EchoText("Eval Result: " & sResult & vbNewLine)
                                        }

                                        break;
                                    }

                                case "math":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            double dValue;
                                            if (!double.TryParse(Conversions.ToString(Variables.Instance[oArgs[1].ToString()]), out dValue))
                                                dValue = 0;
                                            try
                                            {
                                                string argsExpression = Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 2));
                                                dValue = Utility.MathCalc(dValue, argsExpression);
                                                Variables.Instance.Add(oArgs[1].ToString(), dValue.ToString());
                                                string argsVariable3 = "$" + oArgs[1].ToString();
                                                VariableChanged(argsVariable3);
                                            }
#pragma warning disable CS0168
                                            catch (Exception ex)
#pragma warning restore CS0168
                                            {
                                                EchoText("Invalid #math expression: " + Utility.ArrayToString(oArgs, 1));
                                            }
                                        }

                                        break;
                                    }

                                case "evalmath":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            string argsText4 = Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 1));
                                            sResult = EvalMath(argsText4);
                                            // EchoText("Math Result: " & sResult & vbNewLine)
                                        }

                                        break;
                                    }

                                case "if":
                                    {
                                        if (oArgs.Count > 2)
                                        {
                                            if (oArgs[1].ToString().StartsWith("#"))
                                            {
                                                oArgs[1] = await ParseCommand(Globals.ParseGlobalVars(oArgs[1].ToString()));
                                            }

                                            if (EvalIf(Globals.ParseGlobalVars(oArgs[1])) == true)
                                            {
                                                if (oArgs[2].ToString().StartsWith("#"))
                                                {
                                                    sResult = await ParseCommand(Globals.ParseGlobalVars(oArgs[2].ToString()));
                                                }
                                                else
                                                {
                                                    sResult = await ParseCommand(Utility.ArrayToString(oArgs, 2));
                                                }
                                            }
                                            else if (oArgs.Count > 3)
                                            {
                                                if (oArgs[3].ToString().StartsWith("#"))
                                                {
                                                    sResult = await ParseCommand(Globals.ParseGlobalVars(oArgs[3].ToString()));
                                                }
                                                else
                                                {
                                                    sResult = await ParseCommand(Utility.ArrayToString(oArgs, 3));
                                                }
                                            }
                                        }

                                        break;
                                    }

                                case "event":
                                case "events":
                                    {
                                        if (oArgs.Count > 2)
                                        {
                                            string sAction = Utility.ArrayToString(oArgs, 2);
                                            if (sAction.Trim().Length > 0)
                                            {
                                                double argdSeconds = Utility.StringToDouble(oArgs[1].ToString());
                                                QueueList.Instance.AddToQueue(argdSeconds, sAction);
                                            }
                                        }
                                        else if (oArgs.Count == 2)
                                        {
                                            if ((oArgs[1].ToString().ToLower() ?? "") == "clear")
                                            {
                                                QueueList.Instance.Clear();
                                            }
                                            else
                                            {
                                                ListEvents(Utility.ArrayToString(oArgs, 1));
                                            }
                                        }
                                        else
                                        {
                                            ListEvents(Utility.ArrayToString(oArgs, 1));
                                        }

                                        break;
                                    }

                                case "que":
                                case "queue":
                                    {
                                        if (oArgs.Count > 2)
                                        {
                                            string sAction = Utility.ArrayToString(oArgs, 2);
                                            if (sAction.Trim().Length > 0)
                                            {
                                                double argdDelay = Utility.StringToDouble(oArgs[1].ToString());
                                                Globals.Instance.CommandQueue.AddToQueue(argdDelay, sAction, false, false, false);
                                            }
                                        }
                                        else if (oArgs.Count == 2)
                                        {
                                            if ((oArgs[1].ToString().ToLower() ?? "") == "clear")
                                            {
                                                // EchoText("QueueList Cleared" & vbNewLine)
                                                Globals.Instance.CommandQueue.Clear();
                                            }
                                            else
                                            {
                                                ListCommandQueue(Utility.ArrayToString(oArgs, 1));
                                            }
                                        }
                                        else
                                        {
                                            ListCommandQueue(Utility.ArrayToString(oArgs, 1));
                                        }

                                        break;
                                    }

                                case "alias":
                                case "aliases":
                                    {
                                        if (oArgs.Count < 3)
                                        {
                                            if (oArgs.Count < 2)
                                            {
                                                ListAliases("");
                                            }
                                            else // 2 Arguments
                                            {
                                                var switchExpr4 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr4)
                                                {
                                                    case "load":
                                                        {
                                                            EchoText("Aliases Loaded" + System.Environment.NewLine);
                                                            Aliases.Instance.Load();
                                                            break;
                                                        }

                                                    case "save":
                                                        {
                                                            EchoText("Aliases Saved" + System.Environment.NewLine);
                                                            Aliases.Instance.Save();
                                                            break;
                                                        }

                                                    case "clear":
                                                        {
                                                            EchoText("Aliases Cleared" + System.Environment.NewLine);
                                                            Aliases.Instance.Clear();
                                                            break;
                                                        }

                                                    case "edit":
                                                        {
                                                            Interaction.Shell("\"" + ConfigSettings.Instance.Editor + "\" \"" + ConfigSettings.Instance.ConfigProfileDir + @"\aliases.cfg""", 0, false);
                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            Aliases.Instance.ListSubset(Utility.ArrayToString(oArgs, 1));
                                                            break;
                                                        }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // TODO: #alias {asdf asdf} {asdf} does not work with GetArg/GetArg(sRow)
                                            Aliases.Instance.Add(oArgs[1].ToString(), oArgs[2].ToString());
                                        }

                                        break;
                                    }

                                case "unalias":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            Aliases.Instance.Remove(oArgs[1].ToString());
                                        }

                                        break;
                                    }

                                case "class":
                                case "classes":
                                    {
                                        if (oArgs.Count == 1)
                                        {
                                            ListClasses("");
                                        }
                                        else if (oArgs.Count > 1)	// 2 Arguments or more
                                        {
                                            if (oArgs[1].ToString().StartsWith("+") | oArgs[1].ToString().StartsWith("-"))
                                            {
                                                foreach (string sItem in oArgs)
                                                {
                                                    if (sItem.StartsWith("+") & sItem.Length > 1)
                                                    {
                                                        if ((sItem.Substring(1).ToLower() ?? "") == "all")
                                                        {
                                                            EchoText("All Classes Activated" + System.Environment.NewLine);
                                                            Classes.Instance.ActivateAll();
                                                        }
                                                        else
                                                        {
                                                            string argsValue = "True";
                                                            Classes.Instance.Add(sItem.Substring(1).ToLower(), argsValue);
                                                        }
                                                    }
                                                    else if (sItem.StartsWith("-") && sItem.Length > 1)
                                                    {
                                                        if ((sItem.Substring(1).ToLower() ?? "") == "all")
                                                        {
                                                            EchoText("All Classes InActivated" + System.Environment.NewLine);
                                                            Classes.Instance.InActivateAll();
                                                        }
                                                        else
                                                        {
                                                            string argsValue1 = "False";
                                                            Classes.Instance.Add(sItem.Substring(1).ToLower(), argsValue1);
                                                        }
                                                    }
                                                }

                                                EventClassChange?.Invoke();
                                            }
                                            else if (oArgs.Count < 3)
                                            {
                                                var switchExpr6 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr6)
                                                {
                                                    case "load":
                                                        {
                                                            EchoText("Classes Loaded" + System.Environment.NewLine);
                                                            Classes.Instance.Load();
                                                            EventClassChange?.Invoke();
                                                            break;
                                                        }

                                                    case "save":
                                                        {
                                                            EchoText("Classes Saved" + System.Environment.NewLine);
                                                            Classes.Instance.Save();
                                                            break;
                                                        }

                                                    case "clear":
                                                        {
                                                            EchoText("Classes Cleared" + System.Environment.NewLine);
                                                            Classes.Instance.Clear();
                                                            break;
                                                        }

                                                    case "edit":
                                                        {
                                                            Interaction.Shell("\"" + ConfigSettings.Instance.Editor + "\" \"" + ConfigSettings.Instance.ConfigProfileDir + @"\classes.cfg""", 
                                                                0, false);
                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            ListClasses(Utility.ArrayToString(oArgs, 1));
                                                            EventClassChange?.Invoke();
                                                            break;
                                                        }
                                                }
                                            }
                                            else
                                            {
                                                if ((oArgs[1].ToString().ToLower() ?? "") == "all")
                                                {
                                                    var switchExpr5 = oArgs[2].ToString().ToLower();
                                                    switch (switchExpr5)
                                                    {
                                                        case "true":
                                                        case "on":
                                                        case "1":
                                                            {
                                                                EchoText("All Classes Activated" + System.Environment.NewLine);
                                                                Classes.Instance.ActivateAll();
                                                                break;
                                                            }

                                                        case "false":
                                                        case "off":
                                                        case "0":
                                                            {
                                                                EchoText("All Classes InActivated" + System.Environment.NewLine);
                                                                Classes.Instance.InActivateAll();
                                                                break;
                                                            }
                                                    }
                                                }
                                                else
                                                {
                                                    Classes.Instance.Add(oArgs[1].ToString(), oArgs[2].ToString());
                                                }

                                                EventClassChange?.Invoke();
                                            }
                                        }

                                        break;
                                    }

                                case "unclass":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            Aliases.Instance.Remove(oArgs[1].ToString());
                                        }

                                        break;
                                    }

                                case "trigger":
                                case "triggers":
                                case "action":
                                case "actions":
                                    {
                                        if (oArgs.Count < 2)
                                        {
                                            ListTriggers("");
                                        }
                                        else if (oArgs.Count == 2)	// 2 Arguments
                                        {
                                            var switchExpr7 = oArgs[1].ToString().ToLower();
                                            switch (switchExpr7)
                                            {
                                                case "load":
                                                    {
                                                        EchoText("Triggers Loaded" + System.Environment.NewLine);
                                                        Triggers.Instance.Load();
                                                        break;
                                                    }

                                                case "save":
                                                    {
                                                        EchoText("Triggers Saved" + System.Environment.NewLine);
                                                        Triggers.Instance.Save();
                                                        break;
                                                    }

                                                case "clear":
                                                    {
                                                        EchoText("Triggers Cleared" + System.Environment.NewLine);
                                                        Triggers.Instance.Clear();
                                                        break;
                                                    }

                                                case "edit":
                                                    {
                                                        Interaction.Shell("\"" + ConfigSettings.Instance.Editor + "\" \"" + ConfigSettings.Instance.ConfigDir + @"\triggers.cfg""", 0, false);
                                                        break;
                                                    }

                                                default:
                                                    {
                                                        ListTriggers(Utility.ArrayToString(oArgs, 1));
                                                        break;
                                                    }
                                            }
                                        }
                                        else
                                        {
                                            string sClass = string.Empty;
                                            if (oArgs.Count > 3)
                                            {
                                                sClass = oArgs[3].ToString().Trim();
                                            }

                                            if (Triggers.Instance.AddTrigger(oArgs[1].ToString(), oArgs[2].ToString(), false, false, sClass) == false)
                                            {
                                                EchoText("Invalid regexp in trigger: " + oArgs[1].ToString() + System.Environment.NewLine);
                                            }
                                        }

                                        break;
                                    }

                                case "untrigger":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            Triggers.Instance.Remove(oArgs[1].ToString());
                                        }

                                        break;
                                    }

                                case "random":
                                    {
                                        if (oArgs.Count > 2)
                                        {
                                            string argsValue2 = await ParseCommand(Globals.ParseGlobalVars(oArgs[1].ToString()));
                                            string argsValue3 = await ParseCommand(Globals.ParseGlobalVars(oArgs[2].ToString()));
                                            sResult = Utility.RandomNumber(Utility.StringToInteger(argsValue2), Utility.StringToInteger(argsValue3)).ToString();
                                        }
                                        else
                                        {
                                            sResult = Utility.RandomNumber(1, 1000).ToString();
                                        }

                                        break;
                                    }

                                case "config":
                                case "set":
                                case "setting":
                                case "settings":
                                    {
                                        if (oArgs.Count <= 1)
                                        {
                                            ListSettings();
                                        }
                                        else if (oArgs.Count == 2)
                                        {
                                            var switchExpr8 = oArgs[1].ToString().ToLower();
                                            switch (switchExpr8)
                                            {
                                                case "load":
                                                    {
                                                        EchoText("Settings Loaded" + System.Environment.NewLine);
                                                        ConfigSettings.Instance.Load();
                                                        break;
                                                    }

                                                case "save":
                                                    {
                                                        EchoText("Settings Saved" + System.Environment.NewLine);
                                                        ConfigSettings.Instance.SaveSettings();
                                                        break;
                                                    }

                                                case "edit":
                                                    {
                                                        Interaction.Shell("\"" + ConfigSettings.Instance.Editor + "\" \"" + ConfigSettings.Instance.ConfigDir + @"\settings.cfg""", 0, false);
                                                        break;
                                                    }

                                                default:
                                                    {
                                                        try
                                                        {
                                                            if (!ConfigSettings.Instance.SetSetting(oArgs[1].ToString(), string.Empty ))
                                                            EchoText($"Can't clear setting {oArgs[1].ToString()}" + System.Environment.NewLine);
                                                        }
                                                        catch(Exception ex)
                                                        {
                                                            EchoText("Invalid syntax: " + sRow + System.Environment.NewLine);
                                                            EchoText(ex.Message + System.Environment.NewLine);
                                                        }
                                                        break;
                                                    }
                                            }
                                        }
                                        else if (oArgs.Count > 2)
                                        {
                                            try
                                            {
                                                if(!ConfigSettings.Instance.SetSetting(oArgs[1].ToString(), Utility.ArrayToString(oArgs, 2)))
                                                {
                                                    EchoText($"Can't set setting {oArgs[1].ToString()} to {Utility.ArrayToString(oArgs, 2)}" + System.Environment.NewLine);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                EchoText("Invalid syntax: " + sRow + System.Environment.NewLine);
                                                EchoText(ex.Message + System.Environment.NewLine);
                                            }
                                        }

                                        break;
                                    }

                                case "parse":
                                    {
                                        string argsText5 = Globals.ParseGlobalVars(GetArgumentString(sRow));
                                        ParseLine(argsText5);
                                        sResult = "";
                                        break;
                                    }

                                case "beep":
                                case "bell":
                                    {
                                        if (ConfigSettings.Instance.PlaySounds == true)
                                        {
                                            Interaction.Beep();
                                        }

                                        sResult = "";
                                        break;
                                    }

                                case "play":
                                case "playwave":
                                case "playsound":
                                    {
                                        if (ConfigSettings.Instance.PlaySounds == true)
                                        {
                                            string sSound = GetArgumentString(sRow);
                                            if ((sSound.ToLower() ?? "") == "stop")
                                            {
                                                EventCallBacks.OnPlaySoundRequested(string.Empty);
                                            }
                                            else if (sSound.Length > 0)
                                            {
                                                EventCallBacks.OnPlaySoundRequested(sSound);
                                            }
                                        }

                                        break;
                                    }

                                case "playsystem":
                                    {
                                        if (ConfigSettings.Instance.PlaySounds == true)
                                        {
                                            string sSound = GetArgumentString(sRow);
                                            if (sSound.Length > 0)
                                            {
                                                EventCallBacks.OnPlaySoundRequested(sSound);
                                            }
                                        }

                                        break;
                                    }

                                case "macro":
                                case "macros":
                                    {
                                        if (oArgs.Count < 3)
                                        {
                                            if (oArgs.Count < 2)
                                            {
                                                ListMacros("");
                                            }
                                            else // 2 Arguments
                                            {
                                                var switchExpr9 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr9)
                                                {
                                                    case "load":
                                                        {
                                                            EchoText("Macros Loaded" + System.Environment.NewLine);
                                                            Macros.Instance.Load();
                                                            break;
                                                        }

                                                    case "save":
                                                        {
                                                            EchoText("Macros Saved" + System.Environment.NewLine);
                                                            Macros.Instance.Save();
                                                            break;
                                                        }

                                                    case "clear":
                                                        {
                                                            EchoText("Macros Cleared" + System.Environment.NewLine);
                                                            Macros.Instance.Clear();
                                                            break;
                                                        }

                                                    case "edit":
                                                        {
                                                            Interaction.Shell("\"" + ConfigSettings.Instance.Editor + "\" \"" + ConfigSettings.Instance.ConfigProfileDir + @"\macros.cfg""", 0, false);
                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            ListMacros(Utility.ArrayToString(oArgs, 1));
                                                            break;
                                                        }
                                                }
                                            }
                                        }
                                        // Add
                                        else if (Macros.Instance.Add(oArgs[1].ToString(), oArgs[2].ToString()) == false)
                                        {
                                            EchoText("Unknown key combination: " + oArgs[1].ToString() + System.Environment.NewLine);
                                        }

                                        break;
                                    }

                                case "unmacro":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            if (Macros.Instance.Remove(oArgs[1].ToString()) == -1)
                                            {
                                                EchoText("Unknown key combination: " + oArgs[1].ToString() + System.Environment.NewLine);
                                            }
                                        }

                                        break;
                                    }

                                case "keys":
                                    {
                                        ListKeys();
                                        break;
                                    }

                                case "sub":
                                case "subs":
                                case "substitute":
                                    {
                                        if (oArgs.Count < 3)
                                        {
                                            if (oArgs.Count < 2)
                                            {
                                                ListSubstitutes("");
                                            }
                                            else // 2 Arguments
                                            {
                                                var switchExpr10 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr10)
                                                {
                                                    case "load":
                                                        {
                                                            EchoText("Substitutes Loaded" + System.Environment.NewLine);
                                                            SubstituteRegExp.Instance.Load();
                                                            break;
                                                        }

                                                    case "save":
                                                        {
                                                            EchoText("Substitutes Saved" + System.Environment.NewLine);
                                                            SubstituteRegExp.Instance.Save();
                                                            break;
                                                        }

                                                    case "clear":
                                                        {
                                                            EchoText("Substitutes Cleared" + System.Environment.NewLine);
                                                            SubstituteRegExp.Instance.Clear();
                                                            break;
                                                        }

                                                    case "edit":
                                                        {
                                                            Interaction.Shell("\"" + ConfigSettings.Instance.Editor + "\" \"" + ConfigSettings.Instance.ConfigDir + @"\substitutes.cfg""", 0, false);
                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            ListSubstitutes(Utility.ArrayToString(oArgs, 1));
                                                            break;
                                                        }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Add
                                            string sClass = string.Empty;
                                            if (oArgs.Count > 3)
                                            {
                                                sClass = Globals.ParseGlobalVars(oArgs[3].ToString());
                                            }

                                            string argsText6 = Globals.ParseGlobalVars(oArgs[1].ToString());
                                            string argReplaceBy = Globals.ParseGlobalVars(oArgs[2].ToString());
                                            if (SubstituteRegExp.Instance.Add(argsText6, argReplaceBy, false, sClass, true) == false)
                                            {
                                                EchoText("Invalid regexp in substitute: " + Globals.ParseGlobalVars(oArgs[1].ToString()) + System.Environment.NewLine);
                                            }
                                        }

                                        break;
                                    }

                                case "unsub":
                                case "unsubs":
                                case "unsubstitute":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            string argText = Globals.ParseGlobalVars(oArgs[1].ToString());
                                            SubstituteRegExp.Instance.Remove(argText);
                                        }

                                        break;
                                    }

                                case "gag":
                                case "gags":
                                case "squelch":
                                case "ignore":
                                case "ignores":
                                    {
                                        if (oArgs.Count < 2)
                                        {
                                            ListGags("");
                                        }
                                        else // 1 Arguments
                                        {
                                            var switchExpr11 = oArgs[1].ToString().ToLower();
                                            switch (switchExpr11)
                                            {
                                                case "load":
                                                    {
                                                        EchoText("Gags Loaded" + System.Environment.NewLine);
                                                        GagRegExp.Instance.Load();
                                                        break;
                                                    }

                                                case "save":
                                                    {
                                                        EchoText("Gags Saved" + System.Environment.NewLine);
                                                        GagRegExp.Instance.Save();
                                                        break;
                                                    }

                                                case "clear":
                                                    {
                                                        EchoText("Gags Cleared" + System.Environment.NewLine);
                                                        GagRegExp.Instance.Clear();
                                                        break;
                                                    }

                                                case "edit":
                                                    {
                                                        Interaction.Shell("\"" + ConfigSettings.Instance.Editor + "\" \"" + ConfigSettings.Instance.ConfigProfileDir + @"\gags.cfg""", 0, false);
                                                        break;
                                                    }

                                                default:
                                                    {
                                                        // Add
                                                        string argsText7 = Globals.ParseGlobalVars(oArgs[1].ToString());
                                                        string className = oArgs.Count > 2 ? oArgs[2].ToString() : string.Empty;
                                                        bool caseSensitive = oArgs.Count > 3 ? oArgs[3].ToString().ToUpper() == "TRUE" : false;

                                                        if (GagRegExp.Instance.Add(argsText7, caseSensitive, className) == false)
                                                        {
                                                            EchoText("Invalid regexp in gag: " + Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 1)) + System.Environment.NewLine);
                                                        }

                                                        break;
                                                    }
                                            }
                                        }

                                        break;
                                    }

                                case "preset":
                                case "presets":
                                    {
                                        if (oArgs.Count < 3)
                                        {
                                            if (oArgs.Count < 2)
                                            {
                                                ListPresets("");
                                            }
                                            else // 2 Arguments
                                            {
                                                var switchExpr12 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr12)
                                                {
                                                    case "load":
                                                        {
                                                            EchoText("Presets Loaded" + System.Environment.NewLine);
                                                            Presets.Instance.Load();
                                                            var loadVar = "all";
                                                            EventPresetChanged?.Invoke(loadVar);
                                                            break;
                                                        }

                                                    case "save":
                                                        {
                                                            EchoText("Presets Saved" + System.Environment.NewLine);
                                                            Presets.Instance.Save();
                                                            break;
                                                        }

                                                    case "clear":
                                                        {
                                                            EchoText("Presets Cleared" + System.Environment.NewLine);
                                                            Presets.Instance.Clear();
                                                            var clearVar = "all";
                                                            EventPresetChanged?.Invoke(clearVar);
                                                            break;
                                                        }

                                                    case "edit":
                                                        {
                                                            Interaction.Shell("\"" + ConfigSettings.Instance.Editor + "\" \"" + ConfigSettings.Instance.ConfigDir + @"\presets.cfg""", 0, false);
                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            ListPresets(Utility.ArrayToString(oArgs, 1));
                                                            break;
                                                        }
                                                }
                                            }
                                        }
                                        else if (Presets.Instance.ContainsKey(oArgs[1].ToString().ToLower()))
                                        {
                                            Presets.Instance.Add(oArgs[1].ToString().ToLower(), oArgs[2].ToString());
                                            EventPresetChanged?.Invoke(oArgs[1].ToString().ToLower());
                                        }
                                        else
                                        {
                                            EchoText("Invalid #preset keyword.");
                                        }

                                        break;
                                    }

                                case "ungag":
                                case "unsquelch":
                                case "unignore":
                                case "unignores":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            string argText1 = Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 1));
                                            GagRegExp.Instance.Remove(argText1);
                                        }

                                        break;
                                    }

                                case "highlight":
                                case "highlights":
                                    {
                                        if (oArgs.Count < 2)
                                        {
                                            ListHighlights("");
                                        }
                                        else // 1 Arguments
                                        {
                                            var switchExpr13 = oArgs[1].ToString().ToLower();
                                            switch (switchExpr13)
                                            {
                                                case "load":
                                                    {
                                                        EchoText("Highlights Loaded" + System.Environment.NewLine);
                                                        HighlightBase.LoadHighlights();
                                                        break;
                                                    }

                                                case "save":
                                                    {
                                                        EchoText("Highlights Saved" + System.Environment.NewLine);
                                                        HighlightBase.SaveHighlights();
                                                        break;
                                                    }

                                                case "clear":
                                                    {
                                                        EchoText("Highlights Cleared" + System.Environment.NewLine);
                                                        HighlightsList.Instance.Clear();
                                                        HighlightsRegExpList.Instance.Clear();
                                                        HighlightsBeginWithList.Instance.Clear();
                                                        HighlightsList.Instance.RebuildLineIndex();
                                                        break;
                                                    }

                                                case "edit":
                                                    {
                                                        Interaction.Shell("\"" + ConfigSettings.Instance.Editor + "\" \"" + ConfigSettings.Instance.ConfigDir + @"\highlights.cfg""", 0, false);
                                                        break;
                                                    }

                                                case "line":
                                                case "lines":
                                                    {
                                                        if (oArgs.Count > 3)
                                                        {
                                                            string argsKey = Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 3));
                                                            bool highlightWholeRow = true;
                                                            string color = Globals.ParseGlobalVars(oArgs[2].ToString());
                                                            bool caseSensitive = oArgs.Count > 4 ? oArgs[4].ToString().ToUpper() == "TRUE" : false;
                                                            string soundFile = oArgs.Count > 5 ? oArgs[5].ToString() : string.Empty;
                                                            string className = oArgs.Count > 6 ? oArgs[6].ToString() : string.Empty;
                                                            bool isActive = oArgs.Count > 7 ? oArgs[7].ToString().ToUpper() == "TRUE" : true;
                                                           HighlightsList.Instance.Add(argsKey, highlightWholeRow, color, caseSensitive, soundFile, className, isActive);
                                                            HighlightsList.Instance.RebuildLineIndex();
                                                        }

                                                        break;
                                                    }

                                                case "string":
                                                case "strings":
                                                    {
                                                        if (oArgs.Count > 3)
                                                        {
                                                            string highlightText = Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 3));
                                                            bool highlightWholeRow = false;
                                                            string color = Globals.ParseGlobalVars(oArgs[2].ToString());
                                                            bool caseSensitive = oArgs.Count > 4 ? oArgs[4].ToString().ToUpper() == "TRUE" : false;
                                                            string soundFile = oArgs.Count > 5 ? oArgs[5].ToString() : string.Empty;
                                                            string className = oArgs.Count > 6 ? oArgs[6].ToString() : string.Empty;
                                                            bool isActive = oArgs.Count > 7 ? oArgs[7].ToString().ToUpper() == "TRUE" : true;
                                                           HighlightsList.Instance.Add(highlightText, highlightWholeRow, color , caseSensitive, soundFile, className, isActive);
                                                            HighlightsList.Instance.RebuildStringIndex();
                                                        }

                                                        break;
                                                    }

                                                case "beginswith":
                                                    {
                                                        if (oArgs.Count > 3)
                                                        {
                                                            string beginsWithText = Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 3));
                                                            string color = Globals.ParseGlobalVars(oArgs[2].ToString());
                                                            bool caseSensitive = oArgs.Count > 4 ? oArgs[4].ToString().ToUpper() == "TRUE" : false;
                                                            string soundFile = oArgs.Count > 5 ? oArgs[5].ToString() : string.Empty;
                                                            string className = oArgs.Count > 6 ? oArgs[6].ToString() : string.Empty;
                                                            bool isActive = oArgs.Count > 7 ? oArgs[7].ToString().ToUpper() == "TRUE" : true;
                                                            HighlightsBeginWithList.Instance.Add(beginsWithText, color, caseSensitive, soundFile, className, isActive);
                                                        }

                                                        break;
                                                    }

                                                case "regexp":
                                                case "regex":
                                                    {
                                                        if (oArgs.Count > 3)
                                                        {
                                                            string argsRegExp = Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 3));
                                                            if (Utility.ValidateRegExp(argsRegExp) == true)
                                                            {
                                                                string regexPattern = Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 3));
                                                                string color = Globals.ParseGlobalVars(oArgs[2].ToString());
                                                                bool caseSensitive = oArgs.Count > 4 ? oArgs[4].ToString().ToUpper() == "TRUE" : false;
                                                                string soundFile = oArgs.Count > 5 ? oArgs[5].ToString() : string.Empty;
                                                                string className = oArgs.Count > 6 ? oArgs[6].ToString() : string.Empty;
                                                                bool isActive = oArgs.Count > 7 ? oArgs[7].ToString().ToUpper() == "TRUE" : true;
                                                                HighlightsRegExpList.Instance.Add(regexPattern, color, caseSensitive, soundFile, className, isActive);
                                                            }
                                                            else
                                                            {
                                                                EchoText("Invalid RegExp in highlight: " + Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 3)) + System.Environment.NewLine);
                                                            }
                                                        }

                                                        break;
                                                    }

                                                default:
                                                    {
                                                        ListHighlights(Utility.ArrayToString(oArgs, 1));
                                                        break;
                                                    }
                                            }
                                        }

                                        break;
                                    }

                                case "colors":
                                case "colours":
                                    {
                                        ListColors();
                                        break;
                                    }

                                case "name":
                                case "names":
                                    {
                                        if (oArgs.Count < 3)
                                        {
                                            if (oArgs.Count < 2)
                                            {
                                                ListNames("");
                                            }
                                            else // 2 Arguments
                                            {
                                                var switchExpr14 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr14)
                                                {
                                                    case "load":
                                                        {
                                                            EchoText("Names Loaded" + System.Environment.NewLine);
                                                            Names.Instance.Load();
                                                            break;
                                                        }

                                                    case "save":
                                                        {
                                                            EchoText("Names Saved" + System.Environment.NewLine);
                                                            Names.Instance.Save();
                                                            break;
                                                        }

                                                    case "clear":
                                                        {
                                                            EchoText("Names Cleared" + System.Environment.NewLine);
                                                            Names.Instance.Clear();
                                                            break;
                                                        }

                                                    case "edit":
                                                        {
                                                            Interaction.Shell("\"" + ConfigSettings.Instance.Editor + "\" \"" + ConfigSettings.Instance.ConfigDir + @"\names.cfg""", 0, false);
                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            ListNames(Utility.ArrayToString(oArgs, 1));
                                                            break;
                                                        }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Add
                                            for (int I = 2, loopTo2 = oArgs.Count - 1; I <= loopTo2; I++)
                                                Names.Instance.Add(oArgs[I].ToString(), oArgs[1].ToString());
                                            Names.Instance.RebuildRegExIndex(); // Index is for the regex used to find colors
                                        }

                                        break;
                                    }

                                case "unname":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            for (int I = 1, loopTo3 = oArgs.Count - 1; I <= loopTo3; I++)
                                                Names.Instance.Remove(oArgs[I].ToString());
                                        }

                                        break;
                                    }

                                case "edit":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            string sFile = Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 1));
                                            if (sFile.ToLower().EndsWith($".{ConfigSettings.Instance.ScriptExtension}") == false & sFile.ToLower().EndsWith(".js") == false)
                                            {
                                                sFile += $".{ConfigSettings.Instance.ScriptExtension}";
                                            }

                                            if (sFile.IndexOf(@"\") == -1)
                                            {
                                                string sLocation = ConfigSettings.Instance.ScriptDir;
                                                if (sLocation.EndsWith(@"\"))
                                                {
                                                    sFile = sLocation + sFile;
                                                }
                                                else
                                                {
                                                    sFile = sLocation + @"\" + sFile;
                                                }
                                            }

                                            Interaction.Shell("\"" + ConfigSettings.Instance.Editor + "\" \"" + sFile + "\"", 0, false);
                                        }

                                        break;
                                    }

                                case "help":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            if ((oArgs[1].ToString().ToLower() ?? "") == "edit")	// New topic
                                            {
                                                string sTemp = "index.txt";
                                                if (oArgs.Count > 2)
                                                {
                                                    sTemp = Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 2));
                                                }

                                                if (sTemp.ToLower().EndsWith(".txt") == false)
                                                {
                                                    sTemp += ".txt";
                                                }

                                                if (sTemp.IndexOf(@"\") == -1)
                                                {
                                                    sTemp = AppGlobals.LocalDirectoryPath + @"\Help\" + sTemp;
                                                }

                                                Interaction.Shell("\"" + ConfigSettings.Instance.Editor + "\" \"" + sTemp + "\"", 0, false);
                                            }
                                            else
                                            {
                                                ShowHelp(Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 1)));
                                            }
                                        }
                                        else
                                        {
                                            ShowHelp();
                                        }

                                        break;
                                    }

                                case "flash":
                                    {
                                        EventFlashWindow?.Invoke();
                                        break;
                                    }

                                case "script":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            var switchExpr15 = oArgs[1].ToString().ToLower();
                                            switch (switchExpr15)
                                            {
                                                case "abort":
                                                    {
                                                        EventScriptAbort?.Invoke(Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 2)));
                                                        break;
                                                    }

                                                case "pause":
                                                    {
                                                        EventScriptPause?.Invoke(Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 2)));
                                                        break;
                                                    }

                                                case "pauseorresume":
                                                    {
                                                        EventScriptPauseOrResume?.Invoke(Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 2)));
                                                        break;
                                                    }

                                                case "reload":
                                                    {
                                                        EventScriptReload?.Invoke(Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 2)));
                                                        break;
                                                    }

                                                case "resume":
                                                    {
                                                        EventScriptResume?.Invoke(Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 2)));
                                                        break;
                                                    }

                                                case "trace":
                                                    {
                                                        EventScriptTrace?.Invoke(Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 2)));
                                                        break;
                                                    }

                                                case "vars":
                                                case "variables":
                                                    {
                                                        if (oArgs.Count > 3)
                                                        {
                                                            EventScriptVariables?.Invoke(oArgs[2].ToString(), Globals.ParseGlobalVars(oArgs[3].ToString()));
                                                        }
                                                        else if (oArgs.Count > 2)
                                                        {
                                                            EventScriptVariables?.Invoke(oArgs[2].ToString(), "");
                                                        }
                                                        else
                                                        {
                                                            EventScriptVariables?.Invoke("", "");
                                                        }

                                                        break;
                                                    }

                                                case "debug":
                                                case "debuglevel":
                                                    {
                                                        if (oArgs.Count > 2)
                                                        {
                                                            EventScriptDebug?.Invoke(Conversions.ToInteger(Utility.StringToDouble(oArgs[2].ToString())), Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 3)));
                                                        }

                                                        break;
                                                    }

                                                case "explorer":
                                                    {
                                                        EventShowScriptExplorer?.Invoke();
                                                        break;
                                                    }

                                                default:
                                                    {
                                                        EventListScripts?.Invoke(Globals.ParseGlobalVars(Utility.ArrayToString(oArgs, 2)));
                                                        break;
                                                    }
                                            }
                                        }
                                        else
                                        {
                                            EventListScripts?.Invoke("");
                                        }

                                        if (oArgs.Count < 2)
                                        {
                                        }
                                        // ListScripts("")
                                        else
                                        {
                                        } // 2 Arguments

                                        break;
                                    }

                                case "status":
                                case "statusbar":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            var switchExpr16 = oArgs[1].ToString().ToLower();
                                            switch (switchExpr16)
                                            {
                                                case "1":
                                                case "2":
                                                case "3":
                                                case "4":
                                                case "5":
                                                case "6":
                                                case "7":
                                                case "8":
                                                case "9":
                                                case "10":
                                                    {
                                                        StatusBar(Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2)), int.Parse(oArgs[1].ToString()));
                                                        break;
                                                    }

                                                default:
                                                    {
                                                        StatusBar(Globals.ParseGlobalVars(ParseAllArgs(oArgs, 1)));
                                                        break;
                                                    }
                                            }
                                        }

                                        break;
                                    }

                                case "layout":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            var switchExpr17 = oArgs[1].ToString().ToLower();
                                            switch (switchExpr17)
                                            {
                                                case "load":
                                                    {
                                                        EventLoadLayout?.Invoke(Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                        break;
                                                    }

                                                case "save":
                                                    {
                                                        EventSaveLayout?.Invoke(Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                        break;
                                                    }
                                            }
                                        }
                                        else
                                        {
                                            EventLoadLayout?.Invoke("@windowsize@");
                                        }

                                        break;
                                    }

                                case "window":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            var switchExpr18 = oArgs[1].ToString().ToLower();
                                            switch (switchExpr18)
                                            {
                                                case "add":
                                                case "show":
                                                    {
                                                        int sWidth = 300;
                                                        int sHeight = 200;
                                                        int sTop = 10;
                                                        int sLeft = 10;
                                                        EventAddWindow?.Invoke(Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2)), sWidth, sHeight, sTop, sLeft);
                                                        break;
                                                    }
                                                case "position":
                                                    {
                                                        try
                                                        {
                                                            if (oArgs.Count < 4) throw new Exception("Syntax Error: Window Name, and one of (Width, Height, Top, or Left) are required.");
                                                            if (oArgs.Count > 7) throw new Exception("Error in command: " + sRow + ": " + (oArgs.Count - 7) + " too many Args in Position command." + Interaction.IIf((!int.TryParse(oArgs[3].ToString(), out _)), " Window names with spaces need to be \"enclosed\" in double quotes", ""));
                                                            int? sWidth = null;
                                                            int? sHeight = null;
                                                            if ((!int.TryParse(oArgs[3].ToString(), out int width)) && (oArgs[3].ToString() != "") || (!int.TryParse(oArgs[4].ToString(), out int height)) && (oArgs[4].ToString() != ""))
                                                            {
                                                                throw new Exception($"Syntax Error: Width ({oArgs[3].ToString()}) and/or Height ({oArgs[4].ToString()}) are not formatted correctly.");
                                                            }
                                                            if (width != 0) sWidth = width;
                                                            if (height != 0) sHeight = height;
                                                            int? sTop = null;
                                                            int? sLeft = null;
                                                            if (oArgs.Count > 5 && int.TryParse(oArgs[5].ToString(), out int top)) sTop = top;
                                                            if (oArgs.Count > 6 && int.TryParse(oArgs[6].ToString(), out int left)) sLeft = left;

                                                            EventPositionWindow?.Invoke(Globals.ParseGlobalVars(oArgs[2].ToString()), sWidth, sHeight, sTop, sLeft);
                                                        }
                                                        catch(Exception ex)
                                                        {
                                                            EchoColorText(ex.Message + System.Environment.NewLine, Presets.Instance["scriptecho"].FgColor, Presets.Instance["scriptecho"].BgColor, "");
                                                        }
                                                        break;
                                                    }


                                                case "remove":
                                                    {
                                                        EventRemoveWindow?.Invoke(Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                        break;
                                                    }

                                                case "close":
                                                case "hide":
                                                    {
                                                        EventCloseWindow?.Invoke(Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                        break;
                                                    }

                                                case "load":
                                                    {
                                                        EventLoadLayout?.Invoke(Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                        break;
                                                    }

                                                case "save":
                                                    {
                                                        EventLoadLayout?.Invoke(Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                        break;
                                                    }
                                            }
                                        }

                                        break;
                                    }

                                case "comment":
                                    {
                                        if (oArgs.Count > 2)
                                        {
                                            EventChangeWindowTitle?.Invoke(oArgs[1].ToString(), Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                        }

                                        break;
                                    }

                                case "profile":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            var switchExpr19 = oArgs[1].ToString().ToLower();
                                            switch (switchExpr19)
                                            {
                                                case "load":
                                                    {
                                                        EventLoadProfile?.Invoke();
                                                        break;
                                                    }

                                                case "save":
                                                    {
                                                        EventSaveProfile?.Invoke();
                                                        break;
                                                    }
                                            }
                                        }

                                        break;
                                    }

                                case "goto":
                                case "go":
                                case "g":
                                case "walk":
                                case "walkto":
                                case "path":
                                    {
                                        EventMapperCommand?.Invoke(sRow.Replace("#", "#mapper "));
                                        break;
                                    }

                                case "mapper":
                                case "map":
                                case "m":
                                    {
                                        EventMapperCommand?.Invoke(sRow);
                                        break;
                                    }

                                case "plugin":
                                    {
                                        if (oArgs.Count > 1)
                                        {
                                            var switchExpr20 = oArgs[1].ToString().ToLower();
                                            switch (switchExpr20)
                                            {
                                                case "enable":
                                                    {
                                                        EnablePlugin?.Invoke(Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                        break;
                                                    }

                                                case "disable":
                                                    {
                                                        DisablePlugin?.Invoke(Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                        break;
                                                    }

                                                case "load":
                                                    {
                                                        LoadPlugin?.Invoke(Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                        break;
                                                    }

                                                case "unload":
                                                    {
                                                        UnloadPlugin?.Invoke(Globals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                        break;
                                                    }

                                                case "reload":
                                                    {
                                                        ReloadPlugins?.Invoke();
                                                        break;
                                                    }
                                            }
                                        }
                                        else
                                        {
                                            ListPlugins?.Invoke();
                                        }

                                        break;
                                    }

                                case "xml":
                                    {
                                        EventRawToggle?.Invoke(Utility.ArrayToString(oArgs, 1));
                                        break;
                                    }

                                case "raw":
                                    {
                                        EventSendRaw?.Invoke(Utility.ArrayToString(oArgs, 1) + Constants.vbLf);
                                        break;
                                    }

                                default:
                                    {
                                        if (Conversions.ToString(oArgs[0]).Trim().StartsWith("#") == true && 
                                            ColorCode.IsHexString(Conversions.ToString(oArgs[0]).Trim()) == true)
                                        {
                                            // Hex Code
                                            sResult = sRow;
                                        }
                                        else
                                        {
                                            EchoText("Unknown command: " + sRow + System.Environment.NewLine);
                                        }

                                        break;
                                    }
                            }
                        }
                    }
                }
                else if (sRow.StartsWith(Conversions.ToString(ConfigSettings.Instance.ScriptChar)))
                {
                    RunScript(sRow);
                }
                else
                {
                    if (bSendToGame == true)
                    {
                        // Send Text To Game
                        string argsText8 = Globals.ParseGlobalVars(sRow);
                        SendTextToGame(argsText8, bUserInput, sOrigin);
                    }

                    if (sResult.Length == 0)
                    {
                        sResult = sRow;
                    }
                }
            }
        }

        /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
        return sResult;
    }
 
    private void Do(ArrayList oArgs)
    {
        if (oArgs.Count > 1)
        {
            string s = Globals.ParseGlobalVars(ParseAllArgs(oArgs, 1));
            if ((s.ToLower() ?? "") == "clear")
            {
                m_oGlobals.CommandQueue.Clear();
            }
            else
            {
                double dPauseTime = 0;
                string sNumber = string.Empty;
                foreach (char c in s.ToCharArray())
                {
                    if (Information.IsNumeric(c) | c == '.')
                    {
                        sNumber += Conversions.ToString(c);
                    }
                    else
                    {
                        break;
                    }
                }

                if (sNumber.Length > 0 & (sNumber ?? "") != ".")
                {
                    s = s.Substring(sNumber.Length).Trim();
                    dPauseTime = double.Parse(sNumber);
                }

                if (s.Trim().Length > 0)
                {
                    // Put it in queue
                    m_oGlobals.CommandQueue.AddToQueue(dPauseTime, s, true, true, true);
                }
            }
        }
    }
    private void Send(ArrayList oArgs)
    {
        if (oArgs.Count > 1)
        {
            string s = Globals.ParseGlobalVars(ParseAllArgs(oArgs, 1));
            if ((s.ToLower() ?? "") == "clear")
            {
                m_oGlobals.CommandQueue.Clear();
            }
            else
            {
                double dPauseTime = 0;
                string sNumber = string.Empty;
                foreach (char c in s.ToCharArray())
                {
                    if (Information.IsNumeric(c) | c == '.')
                    {
                        sNumber += Conversions.ToString(c);
                    }
                    else
                    {
                        break;
                    }
                }

                if (sNumber.Length > 0 & (sNumber ?? "") != ".")
                {
                    s = s.Substring(sNumber.Length).Trim();
                    dPauseTime = double.Parse(sNumber);
                }

                if (s.Trim().Length > 0)
                {
                    // Put it in queue
                    m_oGlobals.CommandQueue.AddToQueue(dPauseTime, s, true, false, false);
                }
            }
        }
    }
    public void Connect(ArrayList args, bool isLich = false)
    {
        if (args.Count == 1)
        {
            GameConnection.Instance.Reconnect();
        }
        else if (args.Count == 5)
        {
            CharacterProfile profile = new CharacterProfile()
            {
                Account = args[1].ToString(),
                EncryptedPassword = args[2].ToString(),
                Character = args[3].ToString(),
                Game = args[4].ToString()
            };
            GameConnection.Instance.Connect(profile, isLich);
        }
        else
        {
            EchoText("Invalid number of arguments in #connect command. Syntax: #connect account password character game" + Constants.vbNewLine);
        }

    }

    public string Eval(string sText)
    {
        string s = m_oEval.EvalString(sText, m_oGlobals);
        return s;
    }
    
    private void DisplayImage(string filename, string window, int width, int height)
    {
        EventAddImage?.Invoke(filename, window, width, height);
    }

    public void EchoText(string sText, string sWindow = "")
    {
        EventEchoText?.Invoke(sText, sWindow);
    }

    private void EchoColorText(string sText, Color oColor, Color oBgColor, string sWindow = "")
    {
        EventEchoColorText?.Invoke(sText, oColor, oBgColor, sWindow);
    }

    private void SendText(string sText, [Optional, DefaultParameterValue(false)] bool bUserInput, [Optional, DefaultParameterValue("")] string sOrigin)
    {
        EventSendText?.Invoke(sText, bUserInput, sOrigin);
    }

    private void RunScript(string sText)
    {
        EventRunScript?.Invoke(sText);
    }

    private void ClearWindow(string sWindow = "")
    {
        EventClearWindow?.Invoke(sWindow);
    }

    private void VariableChanged(string sVariable)
    {
        EventVariableChanged?.Invoke(sVariable);
    }

    private void ParseLine(string sText)
    {
        EventParseLine?.Invoke(sText);
    }

    private void StatusBar(string sText, int iIndex = 1)
    {
        EventStatusBar?.Invoke(sText, iIndex);
    }

    private string GetKeywordString(string strRow)
    {
        strRow = strRow.Trim();
        if (strRow.IndexOf(" ") > -1)
        {
            return strRow.Substring(0, strRow.IndexOf(" "));
        }
        else
        {
            return strRow;
        }
    }

    private string GetArgumentString(string strRow)
    {
        strRow = strRow.Trim();
        if (strRow.IndexOf(" ") > -1)
        {
            return strRow.Substring(strRow.IndexOf(" ") + 1);
        }
        else
        {
            return string.Empty;
        }
    }

    private string ParseAlias(string sText)
    {
        string sResult = "";
        var oArgs = new ArrayList();
        oArgs = Utility.ParseArgs(sText);
        string sKey = GetKeywordString(sText);
        if (Aliases.Instance.ContainsKey(sKey) == true)
        {
            sResult = Conversions.ToString(Aliases.Instance[sKey]);
            if (sResult.Contains("$") == true)
            {
                sResult = sResult.Replace("$0", GetArgumentString(sText).Replace("\"", ""));
                for (int i = 1, loopTo = ConfigSettings.Instance.ArgumentCount - 1; i <= loopTo; i++)
                {
                    if (i > oArgs.Count - 1)
                    {
                        sResult = sResult.Replace("$" + i.ToString(), "");
                    }
                    else
                    {
                        sResult = sResult.Replace("$" + i.ToString(), oArgs[i].ToString().Replace("\"", ""));
                    }
                }
            }
            else
            {
                sResult += " " + GetArgumentString(sText);
            }
        }

        return sResult;
    }

    private void ShowHelp(string sFile = "index.txt")
    {
        if (sFile.IndexOf(@"\") == -1)
        {
            if (sFile.ToLower().EndsWith(".txt") == false)
            {
                sFile += ".txt";
            }

            sFile = AppGlobals.LocalDirectoryPath + @"\Help\" + sFile.Replace(" ", "_"); // Replace with "_" for sub categories
        }

        try
        {
            var fi = new FileInfo(sFile);
            if (fi.Exists & fi.Length > 0)
            {
                var objFile = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var objReader = new StreamReader(objFile);
                int I = 0;
                string strLine = string.Empty;
                while (objReader.Peek() > -1)
                {
                    I += 1;
                    strLine = objReader.ReadLine().TrimStart();
                    if (strLine.Length > 0)
                    {
                        EchoText(strLine + System.Environment.NewLine);
                    }
                }

                objReader.Close();
                objFile.Close();
            }
        }
#pragma warning disable CS0168
        catch (FileNotFoundException ex)
#pragma warning restore CS0168
        {
            EchoText("Topic does not exist.");
        }
        catch (FileLoadException ex)
        {
            EchoText("File Load Exception: " + ex.Message);
        }
    }

    private void SendTextToGame(string sText, [Optional, DefaultParameterValue(false)] bool bUserInput, [Optional, DefaultParameterValue("")] string sOrigin)
    {
        if (sText.Contains(@"\"))
        {
            sText = sText.Replace(@"\\", "¤");
            sText = sText.Replace(@"\x", "¤x");
            sText = sText.Replace(@"\@", "¤@");
            sText = sText.Replace(@"\", "");
            sText = sText.Replace("¤", @"\");
        }

        // EchoText("Send: " & sText & vbNewLine)
        SendText(sText, bUserInput, sOrigin);
    }

    private string ParseAllArgs(ArrayList oList, int iStartIndex = 1, bool bParseQuickSend = true)
    {
        string sResult = string.Empty;
        string sCommand = string.Empty;
        for (int i = iStartIndex, loopTo = oList.Count - 1; i <= loopTo; i++)
        {
            if (!Information.IsNothing(oList[i]))
            {
                sCommand += " " + oList[i].ToString();
            }
        }

        if (sCommand.Length > 0)
        {
            if (sCommand.StartsWith(" .") == false)
            {
                sResult = ParseCommand(sCommand.Substring(1), false, false, "", bParseQuickSend).Result; // Remove first space
            }
            else
            {
                sResult = sCommand.Substring(1);
            }	// Remove first space
        }

        return sResult;
    }

    private bool EvalIf(string sText)
    {
        return m_oEval.DoEval(sText, m_oGlobals);
    }

    private string EvalMath(string sText)
    {
        if ((System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator ?? "") != ".")
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        }

        double d = m_oMathEval.Evaluate(sText);
        return d.ToString();
    }

    private void ListColors()
    {
        EchoText("Available colors: " + System.Environment.NewLine);
        KnownColor c;
        foreach (string s in Enum.GetNames(typeof(KnownColor)))
        {
            c = (KnownColor)Enum.Parse(typeof(KnownColor), s);
            if (c > KnownColor.Transparent & c < KnownColor.ButtonFace)
            {
                string argsText = s + System.Environment.NewLine;
                var argoColor = Color.FromKnownColor(c);
                var argoBgColor = Color.Transparent;
                EchoColorText(argsText, argoColor, argoBgColor);
            }
        }
    }

    private void ListKeys()
    {
        EchoText("Available keycodes: " + System.Environment.NewLine);
        foreach (string s in Enum.GetNames(typeof(Keys)))
            EchoText(s + System.Environment.NewLine);
    }

    public string ListSettings(string sPattern = "")
    {
        string allVars = ConfigSettings.Instance.ListAll(sPattern);
        if (!string.IsNullOrEmpty(allVars)) EchoText(allVars);
        return allVars;
    }
    public string ListPresets(string sPattern)
    {
        string allVars = Presets.Instance.ListAll(sPattern);
        if (!string.IsNullOrEmpty(allVars)) EchoText(allVars);
        return allVars;
    }
    public string ListAliases(string sPattern)
    {
        string allVars = Aliases.Instance.ListAll(sPattern);
        if (!string.IsNullOrEmpty(allVars)) EchoText(allVars);
        return allVars;
    }
    public string ListNames(string sPattern)
    {
        string allVars = Names.Instance.ListAll(sPattern);
        if (!string.IsNullOrEmpty(allVars)) EchoText(allVars);
        return allVars;
    }
    public string ListMacros(string sPattern)
    {
        string allVars = Macros.Instance.ListAll(sPattern);
        if (!string.IsNullOrEmpty(allVars)) EchoText(allVars);
        return allVars;
    }
    public string ListClasses(string sPattern)
    {
        string allVars = Classes.Instance.ListAll(sPattern);
        if (!string.IsNullOrEmpty(allVars)) EchoText(allVars);
        return allVars;
    }
    public string ListTriggers(string sPattern)
    {
        string allVars = Triggers.Instance.ListAll(sPattern);
        if (!string.IsNullOrEmpty(allVars)) EchoText(allVars);
        return allVars;
    }
    public string ListGags(string sPattern)
    {
        string allVars = GagRegExp.Instance.ListAll(sPattern);
        if (!string.IsNullOrEmpty(allVars)) EchoText(allVars);
        return allVars;
    }
    public string ListEvents(string sPattern)
    {
        string allVars = QueueList.Instance.ListAll(sPattern);
        if (!string.IsNullOrEmpty(allVars)) EchoText(allVars);
        return allVars;
    }
    public string ListSubstitutes(string sPattern)
    {
        string allVars = SubstituteRegExp.Instance.ListAll(sPattern);
        if (!string.IsNullOrEmpty(allVars)) EchoText(allVars);
        return allVars;
    }
    public string ListVariables(string sPattern)
    {
        string allVars = Variables.Instance.ListAll(sPattern);
        if (!string.IsNullOrEmpty(allVars)) EchoText(allVars);
        return allVars;
    }
    public string ListHighlights(string sPattern)
    {
        StringBuilder sb = new();
        string allVars = HighlightsList.Instance.ListAll(sPattern);
        sb.Append(allVars);
        allVars = HighlightsBeginWithList.Instance.ListAll(sPattern);
        sb.Append(allVars);
        allVars = HighlightsRegExpList.Instance.ListAll(sPattern);
        sb.Append(allVars);
        return sb.ToString();
    }

    public void TriggerAction(string sAction, ArrayList oArgs)
    {
        if (Command.Instance.TriggersEnabled == true)
        {
            if (sAction.Contains("$") == true)
            {
                for (int i = 0, loopTo = ConfigSettings.Instance.ArgumentCount - 1; i <= loopTo; i++)
                {
                    if (i > oArgs.Count - 1)
                    {
                        sAction = sAction.Replace("$" + (i + 1).ToString(), "");
                    }
                    else
                    {
                        sAction = sAction.Replace("$" + (i + 1).ToString(), oArgs[i].ToString().Replace("\"", ""));
                    }
                }

                if (oArgs.Count > 0)
                {
                    sAction = sAction.Replace("$0", oArgs[0].ToString().Replace("\"", ""));
                }
                else
                {
                    sAction = sAction.Replace("$0", string.Empty);
                }
            }

            // sAction = oGlobals.ParseGlobalVars(sAction)

            try
            {
                ParseCommand(sAction, true, false, "Trigger");
            }
#pragma warning disable CS0168
            catch (Exception ex)
#pragma warning restore CS0168
            {
                string argsText = "Trigger action failed: " + sAction;
                EchoText(argsText,"Log");
            }
        }
    }

    private void ListCommandQueue(string sPattern)
    {
        ListEvents(sPattern);
//        EchoText("(" + ((QueueList.EventItem)QueueList.EventList.get_Item(I)).Delay + ") " + ((QueueList.EventItem)QueueList.EventList.get_Item(I)).Action + System.Environment.NewLine);
    }

    internal void TriggerVariableChanged(string sVariableName)
    {
        if (TriggersEnabled)
        {
            if (Triggers.Instance.AcquireReaderLock())
            {
                try
                {
                    foreach (Triggers.Trigger oTrigger in Triggers.Instance.Values)
                    {
                        if (oTrigger.IsActive)
                        {
                            if (oTrigger.bIsEvalTrigger)
                            {
                                if (oTrigger.sTrigger.Contains(sVariableName))
                                {
                                    string s = "1";
                                    // If the command isn't an eval. Simply trigger it without checking.
                                    if ((oTrigger.sTrigger ?? "") != (sVariableName ?? ""))
                                    {
                                        string argsText = Globals.ParseGlobalVars(oTrigger.sTrigger);
                                        s = Eval(argsText);
                                    }

                                    if (s.Length > 0 & (s ?? "") != "0")
                                    {
                                        TriggerAction(oTrigger.sAction, new ArrayList());
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                   Triggers.Instance.ReleaseReaderLock();
                }
            }
            else
            {
                EchoText("TriggerList: Unable to acquire reader lock.","Log");
            }

            if (ScriptList.Instance.AcquireReaderLock())
            {
                try
                {
                    foreach (Script oScript in ScriptList.Instance)
                        oScript.TriggerVariableChanged(sVariableName);
                }
                catch (Exception ex)
                {
                    EchoText("Error in TriggerVariableChange", "Debug");
                    EchoText("---------------------", "Debug");
                    EchoText(ex.Message, "Debug");
                    EchoText("---------------------", "Debug");
                    EchoText(ex.ToString(), "Debug");
                    EchoText("---------------------", "Debug");
                }
                finally
                {
                    ScriptList.Instance.ReleaseReaderLock();
                }
            }
            else
            {
                EchoText("TriggerVariableChanged: Unable to acquire reader lock.","Log");
            }
        }
    }
}
