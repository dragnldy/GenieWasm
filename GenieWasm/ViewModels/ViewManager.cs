using Avalonia.Threading;
using GenieCoreLib;
using GenieWasm.UserControls;
using GenieWasm.ViewModels;
using GenieWasm.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GenieCoreLib.Names;

namespace GenieWasm;

public enum DefaultWindows
{
    // 100-199 Left panel 150+ not shown by default
    // 200-299 Center Panel 250+ not shown by default
    // 300-399 Right Panel 350+ not shown by default
    // Displayed top to bottom in numerical order for each panel
    Arrivals = 101,
    Deaths = 102,
    Moons = 103,
    Mobs = 104,
    Objects = 105,
    Players = 106,
    Room = 201,
    Game = 202,
    Thoughts = 203,
    Combat = 250,
    Experience = 301,
    ActiveSpells = 302,
    Conversations = 303,
    Whispers = 304,
    Familiar = 350,
    Log = 351,
    Debug = 352,
    Inventory = 399,
}
// Manages information flow between UI windows and core processes
// Previous logic was based on Windows Forms framework
public class ViewManager: INotifyPropertyChanged
{
    public static ViewManager Instance => _m_oViewManager ??= new ViewManager();
    private static ViewManager _m_oViewManager;

    public ViewManager()
    {
        _m_oViewManager = this;
        Globals.Instance.PropertyChanged += Global_PropertyChanged;
    }

    private void Global_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName.Equals(nameof(Globals.Instance.GameRTLeft)))
        {
            Variables.Instance.Add("gamertleft", Globals.Instance.GameRTLeft.ToString());
            EventVariableChanged("gamertleft");
        }

        if (e.PropertyName.Equals(nameof(Globals.Instance.CastTimeLeft)))
        {
            Variables.Instance.Add("casttimeleft", Globals.Instance.CastTimeLeft.ToString());
            EventVariableChanged("casttimelft");
        }
    }

    public MainViewModel m_MainViewModel = null;
    public void SetMainVewModel(MainViewModel mainViewModel)
    {
        m_MainViewModel = mainViewModel;
    }

    private Dictionary<string, GameWindow> _gameWindows = new();

    public Dictionary<string, GameWindow> GameWindows
    {
        get => _gameWindows;
        set => _gameWindows = value;
    }

    // Add all the default windows until we can get to the user windows
    public IEnumerable<GameWindow> InitializeDefaultWindows(bool testing = false)
    {
        List<DefaultWindows> defaultWindows = Enum.GetValues(typeof(DefaultWindows)).Cast<DefaultWindows>().ToList();
        foreach (DefaultWindows defaultWindow in defaultWindows.Order())
        {
            int order = (int)((int)defaultWindow % 100); // use the 1's and 10's digit to order the windows in the panel
            if (order > 50) continue; // These panels are hidden on startup
            var gameWindow = CreateNewWindow(defaultWindow.ToString(), (int)defaultWindow, testing);
            RegisterWindow(gameWindow, testing);
        }
        return testing ? GameWindows.Values : null;
    }

    private GameWindow CreateNewWindow(string windowname, int location, bool testing = false)
    {
        GameWindow newWindow = new GameWindow()
        { GameWindowName = windowname, BodyContent = "", WindowLocation = location };
        return newWindow;
    }

    public bool RegisterWindow(GameWindow gameWindow, bool testing = false)
    {
        if (gameWindow is null || string.IsNullOrEmpty(gameWindow.GameWindowName))
        {
            throw new ArgumentNullException(nameof(gameWindow), "GameWindow cannot be null.");
        }

        var id = gameWindow.GameWindowName;
        try
        {
            if (GameWindows.ContainsKey(id)) UnregisterWindow(gameWindow, testing);

            GameWindows.Add(id, gameWindow);
            return true; // Successfully added the window
        }
        catch (Exception ex)
        {
            // Handle exceptions, e.g., log them or show a message
            Console.WriteLine($"Error adding or replacing game window: {ex.Message}");
            return false; // Failed to add the window
        }
    }

    public GameWindow? GetGameWindow(string id)
    {
        if (GameWindows.ContainsKey(id)) return GameWindows[id];
        return null;
    }
    private bool isRunning = false;
    public void StartMessagePump()
    {
        isRunning = true;
        Task.Run(() =>
        {
            StringBuilder sb = new();
            string targetPanel = string.Empty;
            while (isRunning)
            {
                while (TextFunctions.ConcurrentTextMessageQueue.Count > 0)
                {
                    if (TextFunctions.ConcurrentTextMessageQueue.TryDequeue(out TextMessage textMessage))
                    {
                        if (textMessage is ExceptionMessage)
                        {

                        }
                        if (textMessage is null || string.IsNullOrEmpty(textMessage.Text)) continue;
                        if (!string.IsNullOrEmpty(targetPanel) || textMessage.TargetPanel == targetPanel)
                        {
                            targetPanel = textMessage.TargetPanel;
                            sb.Append(textMessage.Text);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(targetPanel))
                            {
                                // Send the accumulated text to the previous target panel
                                SendText(sb.ToString(), targetPanel);
                            }
                            // Clear the StringBuilder and set the new target panel
                            sb.Clear();
                            targetPanel = textMessage.TargetPanel;
                            sb.Append(textMessage.Text);
                        }
                    }
                }
                if (sb.Length > 0)
                {
                    SendText(sb.ToString(), targetPanel);
                    sb.Clear();
                }
                Thread.Sleep(100); // Wait a second before checking the queue again
            }
        });

    }

    StringBuilder sb = new();
    public void SendText(string message, string destWindow)
    {
        GameWindow? game = GetGameWindow(destWindow);
        if (game is null) return;

        sb.Append(message);
        Dispatcher.UIThread.Post(() =>
        {
            // Update the UI element (e.g., a TextBlock)
            game.BodyContent += message;
            game.NotifyPropertyChanged("BodyContent");
        });
    }
    public void StopMessagePump()
    {
        isRunning = false;
    }
    private void SetCustomStyles()
    {
        //// Set custom styles for the GameWindow text block
        //GameWindow gameWindow = this.FindControl<GameWindow>("mainGame");
        //if (gameWindow is not null)
        //{
        //    gameWindow.SetTextBlockStyle("GameWindowTextBlock", "Courier New", 16, "WhiteSmoke");
        //}
    }

    private bool OutputFormNameExists(string sID)
    {
        return GameWindows.ContainsKey(sID);
    }

    // Remove Disposed Objects from FormList
    public void UnregisterWindow(GameWindow gameWindow, bool testing = false)
    {
        if (gameWindow is null || string.IsNullOrEmpty(gameWindow.GameWindowName)) return;
        var id = gameWindow.GameWindowName;

        // If the window already exists and is not disposed, replace it
        if (GameWindows[id] != null)
        {
            //// Remove it from the UI first
            //if (GameWindows[id].IsDisposed)
            //{
            //    // If the existing window is disposed, we can safely remove it
            //    GameWindows[id].Dispose();
            //}
            //else
            {
                // If the existing window is not disposed, we can close it so long as we aren't testing since there is no UI thread running
                if (!testing) GameWindows[id].Close();
            }
            // Then remove it from our management list
            GameWindows.Remove(id); // Remove disposed window
        }
    }


    public bool InvokeRequired { get; set; } = false; // Simulates whether invoke is required, for example in a WebAssembly context
    public int HandlePluginException { get; internal set; }

    public delegate GameWindow CreateOutputFormDelegate(string sID, string sName, string sIfClosed, int iWidth, int iHeight, int iTop, int iLeft, bool bIsVisible, string fontStyle, string sColorName, bool UpdateFormList);

    public GameWindow SafeCreateOutputForm(string sID, string sName, string sIfClosed, int iWidth, int iHeight, int iTop, int iLeft, bool bIsVisible, string fontStyle = null, string sColorName = "", bool UpdateFormList = false)
    {
        if (InvokeRequired == true)
        {
            var parameters = new object[] { sID, sName, sIfClosed, iWidth, iHeight, iTop, iLeft, bIsVisible, fontStyle, sColorName, UpdateFormList };
            //            return (FormSkin)Invoke(new CreateOutputFormDelegate(CreateOutputForm), parameters);
        }
        else
        {
            return CreateOutputForm(sID, sName, sIfClosed, iWidth, iHeight, iTop, iLeft, bIsVisible, fontStyle, sColorName, UpdateFormList);
        }
        return new GameWindow();
    }

    public GameWindow CreateOutputForm(string sID, string sName, string sIfClosed, int iWidth, int iHeight, int iTop, int iLeft, bool bIsVisible, string fontStyle = null, string sColorName = "", bool UpdateFormList = false)
    {
        GameWindow oForm = null;
        //var oEnumerator = m_oFormList.GetEnumerator();
        //while (oEnumerator.MoveNext())
        //{
        //    if ((((FormSkin)oEnumerator.Current).ID ?? "") == (sID ?? ""))
        //    {
        //        oForm = (FormSkin)oEnumerator.Current;
        //    }
        //}

        //if (Information.IsNothing(oForm))
        //{
        //    var argoGlobal = m_oGlobals;
        //    oForm = new FormSkin(sID, sName, ref _m_oGlobals);
        //    oForm.EventLinkClicked += FormSkin_LinkClicked;
        //    oForm.MdiParent = this;
        //    m_oFormList.Add(oForm);
        //}

        //oForm.Name = "FormSkin" + sID;
        //oForm.Text = sName;
        //oForm.Title = sName.ToLower() == "percwindow" ? "Active Spells" : sName;
        //oForm.ID = sID;
        //oForm.IfClosed = sIfClosed;
        //if (!Information.IsNothing(oFont))
        //{
        //    oForm.TextFont = oFont;
        //}

        //oForm.RichTextBoxOutput.MonoFont = m_oGlobals.Config.MonoFont;
        //oForm.Width = iWidth;
        //oForm.Height = iHeight;
        //oForm.Top = iTop;
        //oForm.Left = iLeft;
        //oForm.Tag = bIsVisible;
        //if (sColorName.Length > 0)
        //{
        //    if (sColorName.Contains(",") == true && sColorName.EndsWith(",") == false)
        //    {
        //        string sColor = sColorName.Substring(0, sColorName.IndexOf(",")).Trim();
        //        string sBgColor = sColorName.Substring(sColorName.IndexOf(",") + 1).Trim();
        //        oForm.RichTextBoxOutput.ForeColor = Genie.ColorCode.StringToColor(sColor);
        //        oForm.RichTextBoxOutput.BackColor = Genie.ColorCode.StringToColor(sBgColor);
        //    }
        //    else
        //    {
        //        oForm.RichTextBoxOutput.ForeColor = Genie.ColorCode.StringToColor(sColorName);
        //    }
        //}

        //switch (sID)
        //{
        //    case "inv":
        //    case "inventory":
        //        {
        //            m_oOutputInv = oForm;
        //            oForm.UserForm = false;
        //            break;
        //        }

        //    case "familiar":
        //        {
        //            m_oOutputFamiliar = oForm;
        //            oForm.UserForm = false;
        //            break;
        //        }

        //    case "thoughts":
        //        {
        //            m_oOutputThoughts = oForm;
        //            oForm.UserForm = false;
        //            break;
        //        }

        //    case "logons":
        //    case "arrivals":
        //        {
        //            m_oOutputLogons = oForm;
        //            oForm.UserForm = false;
        //            break;
        //        }

        //    case "deaths":
        //    case "death":
        //        {
        //            m_oOutputDeath = oForm;
        //            oForm.UserForm = false;
        //            break;
        //        }

        //    case "room":
        //        {
        //            m_oOutputRoom = oForm;
        //            oForm.UserForm = false;
        //            break;
        //        }

        //    case "log":
        //        {
        //            m_oOutputLog = oForm;
        //            oForm.UserForm = false;
        //            break;
        //        }

        //    case "debug":
        //        {
        //            m_oOutputDebug = oForm;
        //            oForm.UserForm = false;
        //            break;
        //        }
        //    case "percWindow":
        //    case "percwindow":
        //        {
        //            m_oOutputActiveSpells = oForm;
        //            oForm.UserForm = false;
        //            break;
        //        }

        //    case "combat":
        //        {
        //            m_oOutputCombat = oForm;
        //            oForm.UserForm = false;
        //            break;
        //        }

        //    case "portrait":
        //        {
        //            m_oOutputPortrait = oForm;
        //            oForm.UserForm = false;
        //            break;
        //        }
        //}

        //if (UpdateFormList)
        //    UpdateWindowMenuList();
        return oForm;
    }


    private void TextBoxInput_SendText(string sText)
    {
        //try
        //{
        //    m_CommandSent = true;

        //    string argsText = "";
        //    var argoColor = Color.Transparent;
        //    var argoBgColor = Color.Transparent;
        //    Genie.Game.WindowTarget argoTargetWindow = Genie.Game.WindowTarget.Main;
        //    string argsTargetWindow = "";
        //    m_oCommand.ParseCommand(sText, true, true);
        //    AddText(argsText, argoColor, argoBgColor, oTargetWindow: argoTargetWindow, sTargetWindow: argsTargetWindow);

        //    EndUpdate();
        //}
        ///* TODO ERROR: Skipped IfDirectiveTrivia */
        //catch (Exception ex)
        //{
        //    HandleGenieException("SendText", ex.Message, ex.ToString());
        //    /* TODO ERROR: Skipped ElseDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
        //}
    }
    public void HandleGenieException(string section, string message, string description = null)
    {
        GenieException.HandleGenieException(section, message, description);
        //if (InvokeRequired == true)
        //{
        //    var parameters = new[] { section, message, description };
        //    Invoke(new PrintDialogExceptionDelegate(ShowDialogException), parameters);
        //}
        //else
        //{
        //    ShowDialogException(section, message, description);
        //}
    }


    private void ClassCommand_LinkText(string sText, string sLink, string sWindow)
    {
        //try
        //{
        //    FormSkin oFormSkin = null;
        //    if (sWindow.Length > 0)
        //    {
        //        if ((sWindow.ToLower() ?? "") != "game" & (sWindow.ToLower() ?? "") != MainWindow)
        //        {
        //            var oEnumerator = m_oFormList.GetEnumerator();
        //            while (oEnumerator.MoveNext())
        //            {
        //                if ((((FormSkin)oEnumerator.Current).ID ?? "") == (sWindow.ToLower() ?? ""))
        //                {
        //                    oFormSkin = (FormSkin)oEnumerator.Current;
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    if (Information.IsNothing(oFormSkin))
        //        oFormSkin = m_oOutputMain;
        //    SafeLinkText(sText, sLink, oFormSkin);
        //}
        ///* TODO ERROR: Skipped IfDirectiveTrivia */
        //catch (Exception ex)
        //{
        //    HandleGenieException("EchoText", ex.Message, ex.ToString());
        //    /* TODO ERROR: Skipped ElseDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
        //}
    }

    public delegate void LinkTextDelegate(string sText, string sLink, GameWindow oTargetWindow);

    private void SafeLinkText(string sText, string sLink, GameWindow oTargetWindow)
    {
        //if (InvokeRequired == true)
        //{
        //    var parameters = new object[] { sText, sLink, oTargetWindow };
        //    Invoke(new LinkTextDelegate(LinkText), parameters);
        //}
        //else
        //{
        //    LinkText(sText, sLink, oTargetWindow);
        //}
    }

    private void LinkText(string sText, string sLink, GameWindow oTargetWindow)
    {
        //        oTargetWindow.DataContext.InsertLink(sText, sLink);
    }

    private void ClassCommand_EchoColorText(string sText, Color oColor, Color oBgColor, string sWindow)
    {
        //try
        //{
        //    FormSkin oFormSkin = null;
        //    if (sWindow.Length > 0)
        //    {
        //        if ((sWindow.ToLower() ?? "") != "game" & (sWindow.ToLower() ?? "") != MainWindow)
        //        {
        //            var oEnumerator = m_oFormList.GetEnumerator();
        //            while (oEnumerator.MoveNext())
        //            {
        //                if ((((FormSkin)oEnumerator.Current).ID ?? "") == (sWindow.ToLower() ?? ""))
        //                {
        //                    oFormSkin = (FormSkin)oEnumerator.Current;
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    bool bMono = false;
        //    if (sText.ToLower().StartsWith("mono "))
        //    {
        //        sText = sText.Substring(5);
        //        bMono = true;
        //    }

        //    if (!Information.IsNothing(oFormSkin))
        //    {
        //        AddText(sText, oColor, oBgColor, oFormSkin, true, bMono);
        //    }
        //    else if (sWindow.Length == 0)
        //    {
        //        string argsTargetWindow = "";
        //        AddText(sText, oColor, oBgColor, Genie.Game.WindowTarget.Main, argsTargetWindow, true, bMono);
        //    }
        //}
        ///* TODO ERROR: Skipped IfDirectiveTrivia */
        //catch (Exception ex)
        //{
        //    HandleGenieException("EchoColorText", ex.Message, ex.ToString());
        //    /* TODO ERROR: Skipped ElseDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
        //}
    }

    private void SendTextInputBox(string sText)
    {
        //if (sText.Contains(@"\x") | sText.Contains("@"))
        //{
        //    SafeParseInputBox(sText);
        //}
    }

    private void ClassCommand_SendRaw(string sText)
    {
        //        m_oGame.SendRaw(sText);
    }

    private void ClassCommand_SendText(string sText, bool bUserInput, string sOrigin)
    {
        //    try
        //    {
        //        sText = sText.Replace(@"\@", "¤");
        //        if (sText.Contains(@"\x") | sText.Contains("@"))
        //        {
        //            SendTextInputBox(sText);
        //        }
        //        else
        //        {
        //            sText = sText.Replace("¤", "@");
        //            sText = SafeParsePluginInput(sText);
        //            if (sText.Length > 0)
        //            {
        //                m_CommandSent = true;
        //                m_oGame.SendText(sText, bUserInput, sOrigin);
        //                if (m_oGlobals.Config.bTriggerOnInput == true)
        //                {
        //                    ParseTriggers(sText);
        //                }
        //                //lastrow 
        //            }
        //        }
        //    }
        //    /* TODO ERROR: Skipped IfDirectiveTrivia */
        //    catch (Exception ex)
        //    {
        //        HandleGenieException("SendText", ex.Message, ex.ToString());
        //        /* TODO ERROR: Skipped ElseDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
        //    }
        //}

    }

    public void ReconnectToGame()
    {
        try
        {
            if (GameConnection.Instance.Profile.CheckValid())
            {
                GameConnection.Instance.Reconnect();
            }
        }
        /* TODO ERROR: Skipped IfDirectiveTrivia */
        catch (Exception ex)
        {
            HandleGenieException("ReconnectToGame", ex.Message, ex.ToString());
            /* TODO ERROR: Skipped ElseDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
        }
    }

    private void ConnectToGame(CharacterProfile profile, bool isLich = false)
    {
        if (profile is null || !profile.CheckValid()) { Game.Instance.PrintError("Character profile must be supplied."); return; }
        try
        {
            GameConnection.Instance.Connect(profile, isLich);
        }
        catch (Exception ex)
        {
            HandleGenieException("ConnectToGame", ex.Message, ex.ToString());
        }
    }

    private void DisconnectFromGame()
    {
        GameConnection.Instance.Disconnect();
    }

    private void DisconnectAndExit()
    {
        GameConnection.Instance.Disconnect(true);
    }

    public void Command_EventClearWindow(string sWindow)
    {
        GameWindow? oFormSkin = GetGameWindow(sWindow);
        if (oFormSkin is not null)
            SafeClearWindow(oFormSkin);
    }
    public void SafeClearWindow(GameWindow oFormSkin)
    { 
        try
        {   if (InvokeRequired == true)
            {
//                Invoke(new Action(() => ClearWindow(oFormSkin)));
            }
            else
            {
                oFormSkin.BodyContent = string.Empty; // Clear the content of the window
            }
        }
        catch (Exception ex)
        {
            HandleGenieException("ClearWindow", ex.Message, ex.ToString());
        }
    }


    //private void HandlePluginException(GeniePlugin.Plugins.IPlugin plugin, string section, Exception ex)
    //{
    //    if (InvokeRequired == true)
    //    {
    //        var parameters = new object[] { plugin, section, ex };
    //        Invoke(new PrintDialogPluginExceptionDelegate(ShowDialogPluginException), parameters);
    //    }
    //    else
    //    {
    //        ShowDialogPluginException(plugin, section, ex);
    //    }
    //}

    private GameWindow? FindGameWindow(WindowTarget? eTargetWindow = WindowTarget.Main, string sTargetWindow="")
    {
        if (string.IsNullOrEmpty(sTargetWindow)) sTargetWindow = eTargetWindow?.ToString();
        GameWindow? oFormTarget = GetGameWindow(sTargetWindow);
        oFormTarget = oFormTarget ??= GetGameWindow(AppGlobals.MainWindow);

        if (oFormTarget is null) return null;
        return oFormTarget;
    }

    public void AddImage(string sImageFileName, GameWindow? oTargetWindow, int width, int height)
    {
        oTargetWindow ??= GetGameWindow(AppGlobals.MainWindow);
        if (InvokeRequired == true)
        {
            var parameters = new object[] { sImageFileName, oTargetWindow, width, height };
//            Invoke(new AddImageDelegate(InvokeAddImage), parameters);
        }
        else
        {
//            InvokeAddImage(sImageFileName, oTargetWindow, width, height);
        }
    }

    public void AddImage(string sImageFileName, string sTargetWindow, int width, int height)
    {
        GameWindow? oTargetWindow = FindGameWindow(WindowTarget.Portrait, sTargetWindow);
        AddImage(sImageFileName, oTargetWindow, width, height);
    }
    //public void AddImage(string sImageFileName, WindowTarget? oTargetWindow = WindowTarget.Portrait,
    //    string sTargetWindow, int width, int height)
    //{
    //    GameWindow? oFormTarget = FindGameWindow(oTargetWindow, sTargetWindow);
    //    AddImage(sImageFileName, oFormTarget, width, height);
    //}
    public int GetWindowLocation(string sWindow)
    {
        int maxLocation = 0;
        foreach (var windowLoc in Enum.GetValues(typeof(WindowTarget)))
        {
            if (windowLoc.ToString().Equals(sWindow, StringComparison.OrdinalIgnoreCase))
            {
                return (int)windowLoc;
            }
            maxLocation = Math.Max(maxLocation, (int)windowLoc);
        }
        // If we fall through, we need to create a new location for the window
        // Get the maximum location for a new window based on existing windows
        maxLocation = Math.Max(GameWindows.Values.Max(w => w.WindowLocation),maxLocation);
        return maxLocation + 1; // Increment to get the next available location
    }
    public void EventStreamWindow(object sTitle, object sIfClosed, bool testing = false)
    {
        sTitle ??= "";
        // The main window is always open
        if (string.IsNullOrEmpty(sTitle.ToString()) || sTitle.ToString().Equals(AppGlobals.MainWindow, StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            Dispatcher.UIThread.Post(() =>
            {
                GameWindow? fo = GetGameWindow(sTitle.ToString());
                // Update the UI element (e.g., a TextBlock)
                if (fo is null)
                {
                    fo = CreateNewWindow(sTitle.ToString(), GetWindowLocation(sTitle.ToString()), testing);
                    RegisterWindow(fo, testing);
                    // width=300 , height=200, top=10, left=10, isVisible=false

                    string argsText = $"Created new window: {sTitle.ToString()} {System.Environment.NewLine}";
                    WindowTarget argoTargetWindow = WindowTarget.Main;
                    TextFunctions.EchoText(argsText, "Game");
                }
                else if (Information.IsNothing(fo.IfClosed) & !Information.IsNothing(sIfClosed))
                {
                    fo.IfClosed = (bool)sIfClosed;
                    string argsText1 = Conversions.ToString("Altered window: " + sTitle + System.Environment.NewLine);
                    TextFunctions.EchoText(argsText1, "Game");
                }
            });
        }
        catch (Exception ex)
        {
            HandleGenieException("EventStreamWindow", ex.Message, ex.ToString());
        }
    }

    #region Variables Changed that affect UI
    public void EventVariableChanged(string sVariableName)
    {
        switch (sVariableName)
        {
            case "$health":
            case "$mana":
            case "$concentration":
            case "$stamina":
            case "$spirit":
            case "compass":
            case "$north":
            case "$northeast":
            case "$east":
            case "$southeast":
            case "$south":
            case "$southwest":
            case "$west":
            case "$northwest":
            case "$up":
            case "$down":
            case "$out":
            case "$dead":
            case "$standing":
            case "$kneeling":
            case "$sitting":
            case "$prone":
            case "$stunned":
            case "$bleeding":
            case "$invisible":
            case "$hidden":
            case "$joined":
            case "$webbed":
                NotifyPropertyChanged(sVariableName.TrimStart('$'));
                break;

            case "$preparedspell":
            case "casttimeend":
            case "casttimeleft":
            case "gamertleft":
            case "gamertend":
            case "$roundtime":
            case "$casttime":
            case "$lefthand":
            case "$righthand":
                NotifyPropertyChanged(sVariableName.TrimStart('$'));
                break;

            case "$charactername":
            case "$gamename":
            case "$prompt": // Use prompt as a heartbeat for progress bar updates
                NotifyPropertyChanged(sVariableName.TrimStart('$'));
                break;

            case "$connected":
                {
                    //string argsValue = Conversions.ToString(m_oGlobals.VariableList["connected"]);
                    //bool bConnected = Utility.StringToBoolean(argsValue);
                    //ComponentBarsHealth.IsConnected = bConnected;
                    //ComponentBarsMana.IsConnected = bConnected;
                    //ComponentBarsFatigue.IsConnected = bConnected;
                    //ComponentBarsSpirit.IsConnected = bConnected;
                    //ComponentBarsConc.IsConnected = bConnected;
                    //IconBar.IsConnected = bConnected;
                    //oRTControl.IsConnected = bConnected;
                    //Castbar.IsConnected = bConnected;
                    //m_CommandSent = false;
                    //m_oGlobals.VariableList["charactername"] = m_oGame.AccountCharacter;
                    //m_oGlobals.VariableList["game"] = m_oGame.AccountGame;
                    //m_oGlobals.VariableList["gamename"] = m_oGame.AccountGame;
                    //m_oAutoMapper.CharacterName = m_oGame.AccountCharacter;
                    //m_sCurrentProfileName = m_oGame.AccountCharacter + m_oGame.AccountGame + ".xml";
                    //m_oGame.ResetIndicators();
                    //IconBar.UpdateStatusBox();
                    //IconBar.UpdateStunned();
                    //IconBar.UpdateBleeding();
                    //IconBar.UpdateInvisible();
                    //IconBar.UpdateHidden();
                    //IconBar.UpdateJoined();
                    //IconBar.UpdateWebbed();
                    //if (m_oGame.IsConnectedToGame)
                    //{
                    //    if (!string.IsNullOrWhiteSpace(m_oGlobals.Config.ConnectScript)) ClassCommand_SendText(m_oGlobals.Config.ScriptChar + m_oGlobals.Config.ConnectScript, false, "Connected");
                    //    if (m_oGlobals.VariableList.ContainsKey("connectscript")) ClassCommand_SendText(m_oGlobals.Config.ScriptChar + m_oGlobals.Config.ConnectScript, false, "Connected");
                    //}
                    //SafeUpdateMainWindowTitle();
                    break;
                }


        }
    }
    #endregion Variables Changed that affect UI


    #region Property Changed Notification

    // This method is called by the Set accessor of each property.
    // The CallerMemberName attribute that is applied to the optional propertyName
    // parameter causes the property name of the caller to be substituted as an argument.
    public event PropertyChangedEventHandler PropertyChanged;

    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Property Changed Notification

}