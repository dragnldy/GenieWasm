using Avalonia.Controls;
using Avalonia.Threading;
using GenieCoreLib;
using GenieWasm.UserControls;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
public class ViewManager
{
    public static ViewManager Instance => _m_oViewManager ??= new ViewManager();
    private static ViewManager _m_oViewManager;
    
    public ViewManager()
    {
        _m_oViewManager = this;
        // Initialize any required components or settings here
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
        return testing? GameWindows.Values : null;
    }

    private GameWindow CreateNewWindow(string windowname, int location, bool testing = false)
    {
        GameWindow newWindow = new GameWindow()
            {GameWindowName = windowname, BodyContent = "", WindowLocation = location};
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
            if (GameWindows.ContainsKey(id))  UnregisterWindow(gameWindow,testing);

            GameWindows.Add(id,gameWindow);
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

    private void ClassCommand_EchoText(string sText, string sWindow)
    {
        //try
        //{
        //    FormSkin oFormSkin = null;
        //    if (sWindow.Length > 0)
        //    {
        //        if ((sWindow.ToLower() ?? "") != "game" & (sWindow.ToLower() ?? "") != "main")
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
        //        var argoColor = Color.WhiteSmoke;
        //        var argoBgColor = Color.Transparent;
        //        AddText(sText, argoColor, argoBgColor, oFormSkin, true, bMono);
        //    }
        //    else if (sWindow.Length == 0)
        //    {
        //        var argoColor1 = Color.WhiteSmoke;
        //        var argoBgColor1 = Color.Transparent;
        //        string argsTargetWindow = "";
        //        AddText(sText, argoColor1, argoBgColor1, Genie.Game.WindowTarget.Main, argsTargetWindow, true, bMono);
        //    }
        //}
        ///* TODO ERROR: Skipped IfDirectiveTrivia */
        //catch (Exception ex)
        //{
        //    HandleGenieException("EchoText", ex.Message, ex.ToString());
        //    /* TODO ERROR: Skipped ElseDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
        //}
    }

    private void ClassCommand_LinkText(string sText, string sLink, string sWindow)
    {
        //try
        //{
        //    FormSkin oFormSkin = null;
        //    if (sWindow.Length > 0)
        //    {
        //        if ((sWindow.ToLower() ?? "") != "game" & (sWindow.ToLower() ?? "") != "main")
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
        //        if ((sWindow.ToLower() ?? "") != "game" & (sWindow.ToLower() ?? "") != "main")
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
}
