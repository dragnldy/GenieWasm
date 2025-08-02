﻿using System.Collections;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace GenieCoreLib;

public interface IGame
{
    void Connect(string sGenieKey, string sAccountName, string sPassword, string sCharacter, string sGame);
    void DirectConnect(string Character, string Game, string Host, int Port, string Key);
    void Disconnect(bool ExitOnDisconnect = false);
    void SendText(string sText, bool bUserInput = false, string sOrigin = "");
    void SendRaw(string text);
    void ParseGameRow(string sText);
    void SetBufferEnd();
}
public class Game : IGame
{
    public static Game Instance => _m_oGame ?? new Game();
    private static Game _m_oGame;

    public Game()
    {
        _m_oGame = this;
        m_oSocket = Connection.Instance;
    }

    public event EventAddImageEventHandler EventAddImage;
    public delegate void EventAddImageEventHandler(string filename, string window, int width, int height);

    public event EventPrintTextEventHandler EventPrintText;
    public delegate void EventPrintTextEventHandler(string text, Color color, Color bgcolor, WindowTarget targetwindow, string targetwindowstring, bool mono, bool isprompt, bool isinput);

    public event EventPrintErrorEventHandler EventPrintError;
    public delegate void EventPrintErrorEventHandler(string text);

    public event EventClearWindowEventHandler EventClearWindow;
    public delegate void EventClearWindowEventHandler(string sWindow);

    public event EventDataRecieveEndEventHandler EventDataRecieveEnd;
    public delegate void EventDataRecieveEndEventHandler();

    public event EventRoundTimeEventHandler EventRoundTime;
    public delegate void EventRoundTimeEventHandler(int time);

    public event EventCastTimeEventHandler EventCastTime;
    public delegate void EventCastTimeEventHandler();

    public event EventSpellTimeEventHandler EventSpellTime;
    public delegate void EventSpellTimeEventHandler();

    public event EventClearSpellTimeEventHandler EventClearSpellTime;
    public delegate void EventClearSpellTimeEventHandler();

    public event EventTriggerParseEventHandler EventTriggerParse;
    public delegate void EventTriggerParseEventHandler(string text);

    public event EventTriggerMoveEventHandler EventTriggerMove;
    public delegate void EventTriggerMoveEventHandler();

    public event EventTriggerPromptEventHandler EventTriggerPrompt;
    public delegate void EventTriggerPromptEventHandler();

    public event EventStatusBarUpdateEventHandler EventStatusBarUpdate;
    public delegate void EventStatusBarUpdateEventHandler();

    public event EventVariableChangedEventHandler EventVariableChanged;
    public delegate void EventVariableChangedEventHandler(string sVariable);

    public event EventParseXMLEventHandler EventParseXML;
    public delegate void EventParseXMLEventHandler(string xml);

    public event EventStreamWindowEventHandler EventStreamWindow;
    public delegate void EventStreamWindowEventHandler(object sID, object sTitle, object sIfClosed);


    private Connection _m_oSocket;
    private Connection m_oSocket
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            return _m_oSocket;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            _m_oSocket = value;
            if (_m_oSocket != null)
            {
                _m_oSocket.EventConnected += GameSocket_EventConnected;
                _m_oSocket.EventDisconnected += GameSocket_EventDisconnected;
                _m_oSocket.EventConnectionLost += GameSocket_EventConnectionLost;
                _m_oSocket.EventParseRow += GameSocket_EventParseRow;
                _m_oSocket.EventParsePartialRow += GameSocket_EventParsePartialRow;
                _m_oSocket.EventDataRecieveEnd += GameSocket_EventDataRecieveEnd;
                _m_oSocket.EventPrintText += GameSocket_EventPrintText;
                _m_oSocket.EventPrintError += GameSocket_EventPrintError;
            }
        }
    }

    private bool m_bShowRawOutput = false;
    private string m_sEncryptionKey = string.Empty;
    private string m_sAccountOwner = string.Empty;
    private string m_sLoginKey = string.Empty;
    private string m_sAccountName = string.Empty;
    private string m_sAccountPassword = string.Empty;
    private string m_sAccountCharacter = string.Empty;
    private string m_sAccountGame = "DR"; // DR, DRX, DRF
    private string m_sConnectHost = string.Empty;
    private int m_sConnectPort = 0;
    private string m_sConnectKey = string.Empty;
    private string m_sGenieKey = string.Empty;
    private bool m_bLastRowWasBlank = false;
    private bool m_bBold = false;
    private string m_sStyle = string.Empty;
    private string m_sRoomDesc = string.Empty;
    private string m_sRoomObjs = string.Empty;
    private string m_sRoomPlayers = string.Empty;
    private string m_sRoomExits = string.Empty;
    private int m_iHealth = 100;
    private int m_iMana = 100;
    private int m_iSpirit = 100;
    private int m_iStamina = 100;
    private int m_iConcentration = 100;
    private int m_iEncumbrance = 0;
    private string m_sCharacterName = string.Empty;
    private string m_sGameName = string.Empty;
    private int m_iRoundTime = 0;
    private int m_iSpellTime = 0;
    private int m_iCastTime = 0;
    private int m_iGameTime = 0;
    private string m_sTriggerBuffer = string.Empty;
    private bool m_bLastRowWasPrompt = false;
    private bool m_bUpdatingRoom = false;
    private bool m_bUpdateRoomOnStreamEnd = false;
    private string m_sRoomTitle = string.Empty;
    private string m_sRoomUid = string.Empty;
    // private Match m_oRegMatch;
    private Hashtable m_oIndicatorHash = new Hashtable();
    private Hashtable m_oCompassHash = new Hashtable();
    private WindowTarget m_oTargetWindow = WindowTarget.Main;
    private string m_sTargetWindow = string.Empty;
    private bool m_bIgnoreXMLDepth = false;
    private ConnectStates m_oConnectState;
    private object m_oThreadLock = new object(); // Thread safety
    private bool m_bFamiliarLineParse = false;
    public bool IsLich = false;

    /* TODO ERROR: Skipped RegionDirectiveTrivia */
    public enum WindowTarget
    {
        Unknown,
        Combat,
        Portrait,
        Main,
        Inv,
        Familiar,
        Thoughts,
        Logons,
        Death,
        Room,
        Log,
        Raw,
        Debug,
        ActiveSpells,
        Other
    }

    private enum ConnectStates
    {
        Disconnected,
        ConnectingKeyServer,
        ConnectingGameServer,
        ConnectedKey,
        ConnectedGameHandshake,
        ConnectedGame
    }

    private enum Indicator
    {
        Kneeling,
        Prone,
        Sitting,
        Standing,
        Stunned,
        Hidden,
        Invisible,
        Dead,
        Webbed,
        Joined,
        Bleeding
    }

    private enum Direction
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest,
        Up,
        Down,
        Out
    }

    /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    private Hashtable m_oBanned = new Hashtable();

    public bool ShowRawOutput
    {
        get
        {
            return m_bShowRawOutput;
        }

        set
        {
            m_bShowRawOutput = value;
        }
    }

    public bool IsConnected
    {
        get
        {
            if (Information.IsNothing(m_oSocket))
            {
                return false;
            }
            else
            {
                return m_oSocket.IsConnected;
            }
        }
    }

    public bool IsConnectedToGame
    {
        get
        {
            return m_oConnectState == ConnectStates.ConnectedGame;
        }
    }
    public bool LastRowWasPrompt
    {
        get
        {
            return m_bLastRowWasPrompt;
        }

        set
        {
            m_bLastRowWasPrompt = value;
        }
    }

    public DateTime LastServerActivity
    {
        get
        {
            return m_oSocket.LastServerActivity;
        }
    }

    private DateTime m_oLastUserActivity = DateTime.Now;

    public DateTime LastUserActivity
    {
        get
        {
            return m_oLastUserActivity;
        }
    }

    public string AccountName
    {
        get
        {
            return m_sAccountName;
        }

        set
        {
            m_sAccountName = value;
        }
    }

    public string AccountPassword
    {
        get
        {
            return m_sAccountPassword;
        }

        set
        {
            m_sAccountPassword = value;
        }
    }

    public string AccountCharacter
    {
        get
        {
            return m_sAccountCharacter;
        }

        set
        {
            m_sAccountCharacter = value;
        }
    }

    public string AccountGame
    {
        get
        {
            return m_sAccountGame;
        }

        set
        {
            m_sAccountGame = value;
        }
    }

    public void Connect(string sGenieKeyx, string sAccountName, string sPassword, string sCharacter, string sGame)
    {
        m_sAccountName = sAccountName;
        m_sAccountPassword = sPassword;
        m_sAccountCharacter = sCharacter;
        m_sAccountGame = sGame;
        m_oLastUserActivity = DateTime.Now;
        var accountName = m_sAccountName.ToUpper();

        Variables.Instance["charactername"] = sCharacter;
        Variables.Instance["game"] = sGame;
        Variables.Instance["account"] = sAccountName;

        DoConnect("eaccess.play.net", 7910);
    }

    public void DirectConnect(string Character, string Game, string Host, int Port, string Key)
    {
        m_sConnectKey = Key;
        DirectConnect(Character, Game, Host, Port);
    }
    public void DirectConnect(string Character, string Game, string Host, int Port)
    {
        m_oLastUserActivity = DateTime.Now;
        Variables.Instance["charactername"] = Character;
        Variables.Instance["game"] = Game;
        Variables.Instance["account"] = "Unknown";

        m_sEncryptionKey = string.Empty;
        m_oConnectState = ConnectStates.ConnectingGameServer;
        m_oSocket.Connect(Host, Port);
    }

    public void Disconnect(bool ExitOnDisconnect = false)
    {
        if (m_oSocket.IsConnected)
        {
            m_oSocket.Disconnect(ExitOnDisconnect);
        }
    }

    public void SendText(string sText, bool bUserInput = false, string sOrigin = "")
    {
        string sShowText = sText;
        if (!m_oSocket.IsConnected)
        {
            if (!sText.StartsWith(ConfigSettings.Instance.MyCommandChar.ToString()))
            {
                sShowText = "(" + sShowText + ")";
            }
        }
        else if (sText.StartsWith("qui", StringComparison.CurrentCultureIgnoreCase) | sText.StartsWith("exi", StringComparison.CurrentCultureIgnoreCase))
        {
            m_oReconnectTime = default;
            m_bManualDisconnect = true;
        }
        else if ((sText.ToLower() ?? "") == "set !statusprompt")
        {
            m_bStatusPromptEnabled = false;
        }

        bool bHideOutput = false;
        if (sOrigin.Length > 0)
        {
            // Gag List
            if (GagRegExp.Instance.AcquireReaderLock())
            {
                try
                {
                    foreach (GagRegExp.Gag sl in GagRegExp.Instance)
                    {
                        if (!Information.IsNothing(sl.RegexGag))
                        {
                            if (sl.RegexGag.Match(sOrigin).Success == true)
                            {
                                bHideOutput = true;
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    GagRegExp.Instance.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("SendText", "Unable to aquire reader lock.");
            }

            sShowText = "[" + sOrigin + "]: " + sShowText;
        }

        // Maybe not needed
        if (sOrigin.Equals("") && sShowText.Equals(""))
            sShowText = sText;

        if (bHideOutput == false)
        {
            Color color;
            Color bgcolor;
            if (bUserInput == true)
            {
                color = Presets.Instance["inputuser"].FgColor;
                bgcolor = Presets.Instance["inputuser"].BgColor;
                if (!sText.StartsWith(ConfigSettings.Instance.MyCommandChar.ToString())) // Skip user commands
                {
                    Variables.Instance["lastinput"] = sText;
                    var lastinputVar = "lastinput";
                    EventVariableChanged?.Invoke(lastinputVar);
                }

            }
            else
            {
                color = Presets.Instance["inputother"].FgColor;
                bgcolor = Presets.Instance["inputother"].BgColor;
            }

            string argsText = sShowText + System.Environment.NewLine;
            PrintInputText(argsText, color, bgcolor);
        }

        if (!sText.StartsWith(ConfigSettings.Instance.MyCommandChar.ToString())) // Skip user commands
        {
            m_oLastUserActivity = DateTime.Now;
            m_oSocket.Send(sText + Constants.vbCrLf);
            Variables.Instance["lastcommand"] = sText;
            var lastCommandVar = "lastcommand";
            EventVariableChanged?.Invoke(lastCommandVar);
        }

        if (ConfigSettings.Instance.AutoLog == true)
        {
            Log.LogText(sShowText + System.Environment.NewLine, Conversions.ToString(Variables.Instance["charactername"]), Conversions.ToString(Variables.Instance["game"]));
        }
    }

    public void SendRaw(string text)
    {
        m_oSocket.Send(text);
    }

    private StringBuilder m_oXMLBuffer = new StringBuilder();

    public void ParseGameRow(string sText)
    {
        var oXMLBuffer = new StringBuilder();
        bool hasXML = false;
        int iInsideXML = 0;
        bool bEndTagFound = false;
        bool bInsideHTMLTag = false;
        string sHTMLBuffer = string.Empty;
        string sTextBuffer = string.Empty;
        string sBoldBuffer = string.Empty;
        int iBoldIndex = 0;
        char cPreviousChar = Conversions.ToChar("");
        bool bCombatRow = false;
        bool bPromptRow = false;

        // Fix for DR html encoding problems
        if (sText.StartsWith("< "))
        {
            sText = sText.Replace("< ", "&lt; ");
        }

        if (sText.StartsWith("> "))
        {
            sText = sText.Replace("> ", "&gt; ");
        }

        if (m_bShowRawOutput == true)
        {
            PrintTextToWindow(sText, Color.LightGray, Color.Black, WindowTarget.Raw);
        }

        foreach (char c in sText)
        {
            switch (c)
            {
                case '<':
                    {
                        iInsideXML += 1;
                        hasXML = true;
                        oXMLBuffer.Append(c);
                        break;
                    }

                case '>':
                    {
                        if (Conversions.ToString(cPreviousChar) == "/")	// End tag in same statement
                        {
                            iInsideXML -= 1;
                        }
                        else if (bEndTagFound == true)	// Jump two steps back if we found end tag
                        {
                            iInsideXML -= 2;
                            bEndTagFound = false;
                        }

                        oXMLBuffer.Append(c);
                        if (iInsideXML == 0)
                        {
                            m_oXMLBuffer.Append(oXMLBuffer);
                            string buffer = m_oXMLBuffer.ToString();
                            string sTmp = ProcessXML(buffer);
                            if (buffer.EndsWith("</preset>"))
                            {
                                XmlDocument presetXML = new XmlDocument();
                                presetXML.LoadXml(buffer);

                                string presetLabel = GetAttributeData(presetXML.FirstChild, "id").ToLower();
                                switch(presetLabel)
                                {
                                    case "whisper":
                                        presetLabel = "whispers";
                                        break;

                                    case "thought":
                                        presetLabel = "thoughts";
                                        break;

                                    default:
                                        break;
                                }
                                Globals.Instance.VolatileHighlights.Add(new VolatileHighlight(sTmp, presetLabel, sTextBuffer.Length));
                                if(presetLabel == "roomdesc")
                                {
                                    PrintTextWithParse(sTmp, bIsPrompt: false, oWindowTarget: 0);
                                    sTmp = string.Empty;
                                }
                            }
                            if (buffer.EndsWith(@"<pushBold/>"))
                            {
                                sBoldBuffer = string.Empty;
                                iBoldIndex = sTextBuffer.Length; //do not subtract 1 because our start index isn't added yet
                            }
                            if (buffer.EndsWith(@"<popBold/>"))
                            {
                                if (!string.IsNullOrWhiteSpace(sBoldBuffer))
                                {
                                    sBoldBuffer = ParseSubstitutions(sBoldBuffer);
                                    Globals.Instance.VolatileHighlights.Add(new VolatileHighlight(sBoldBuffer, "creatures", iBoldIndex));
                                }
                            }
                            if (m_bBold & !ConfigSettings.Instance.Condensed)
                            {
                                if (sTextBuffer.StartsWith("< ") | sTextBuffer.StartsWith("> ") | sTextBuffer.StartsWith("* "))
                                {
                                    m_bBold = false;
                                    string argsText = sTextBuffer + System.Environment.NewLine;
                                    bool argbIsPrompt = false;
                                    WindowTarget argoWindowTarget = 0;
                                    PrintTextWithParse(argsText, bIsPrompt: argbIsPrompt, oWindowTarget: argoWindowTarget);
                                    m_bBold = true;
                                    sTextBuffer = string.Empty;
                                    iBoldIndex = sTextBuffer.Length;
                                    bCombatRow = true;
                                }
                            }

                            sTextBuffer += sTmp;

                            m_oXMLBuffer.Clear();
                            oXMLBuffer.Clear();
                        }

                        break;
                    }

                case '/':
                    {
                        if (Conversions.ToString(cPreviousChar) == "<")	// End tag found
                        {
                            bEndTagFound = true;
                        }

                        if (iInsideXML > 0)
                        {
                            oXMLBuffer.Append(c);
                        }
                        else
                        {
                            sTextBuffer += Conversions.ToString(c);
                            if (m_bBold)
                            {
                                sBoldBuffer += c;
                            }
                        }

                        break;
                    }

                case '&':
                    {
                        bInsideHTMLTag = true;
                        sHTMLBuffer += Conversions.ToString(c);
                        break;
                    }

                case ';':
                    {
                        if (bInsideHTMLTag == true)
                        {
                            sHTMLBuffer += Conversions.ToString(c);
                            if (iInsideXML > 0)
                            {
                                oXMLBuffer.Append(sHTMLBuffer);
                            }
                            else
                            {
                                if (m_bBold)
                                {
                                    sBoldBuffer += c;
                                }
                                sTextBuffer += Utility.TranslateHTMLChar(sHTMLBuffer);
                            }

                            sHTMLBuffer = string.Empty;
                            bInsideHTMLTag = false;
                        }

                        break;
                    }

                case (char)28: // GSL. Skip rest of line.
                    {
                        break;
                    }

                default:
                    {
                        if (bInsideHTMLTag == true)
                        {
                            sHTMLBuffer += Conversions.ToString(c);
                            if (sHTMLBuffer.Length > 6) // Abort
                            {
                                if (iInsideXML > 0)
                                {
                                    oXMLBuffer.Append(sHTMLBuffer.Replace("&", "&amp;"));
                                }
                                else
                                {
                                    if (m_bBold)
                                    {
                                        sBoldBuffer += sHTMLBuffer;
                                    }
                                    sTextBuffer += sHTMLBuffer;
                                }

                                sHTMLBuffer = string.Empty;
                                bInsideHTMLTag = false;
                            }
                        }
                        else if (iInsideXML > 0)
                        {
                            oXMLBuffer.Append(c);
                        }
                        else
                        {
                            if (m_bBold)
                            {
                                sBoldBuffer += c;
                            }
                            sTextBuffer += Conversions.ToString(c);
                        }

                        break;
                    }
            }

            cPreviousChar = c;
        }

        if (oXMLBuffer.Length > 0)
        {
            if (iInsideXML > 0)
            {
                m_oXMLBuffer = oXMLBuffer;
            }
            else
            {
                m_oXMLBuffer.Append(oXMLBuffer);
                string buffer = m_oXMLBuffer.ToString();
                sTextBuffer += ProcessXML(buffer);
                m_oXMLBuffer.Clear();
                oXMLBuffer.Clear();
            }
        }

        if (sTextBuffer.Length > 0)
        {
            
            if (bCombatRow == true)
            {
                m_bBold = true;
            }
            else if (!string.IsNullOrWhiteSpace(sBoldBuffer))
            {
                if (sBoldBuffer.EndsWith("\r\n")) sBoldBuffer = sBoldBuffer.Substring(0, sBoldBuffer.Length - "\r\n".Length);
                sBoldBuffer = ParseSubstitutions(sBoldBuffer);
                Globals.Instance.VolatileHighlights.Add(new VolatileHighlight(sBoldBuffer, "creatures", iBoldIndex)); //trim because excessive whitespace seems to be breaking this
                sBoldBuffer = string.Empty;
            }

            // Fix for broke familiar XML
            if (m_bFamiliarLineParse)
            {
                if (m_oTargetWindow == WindowTarget.Other)
                {
                    sTextBuffer = "";
                }
                else
                {
                    if (m_oTargetWindow == WindowTarget.Main)
                    {
                        m_oTargetWindow = WindowTarget.Familiar;
                    }

                    m_bFamiliarLineParse = false;
                }
            }

            if (!(sTextBuffer == "\r\n" && hasXML))
            {
                bool isRoomOutput = sText.Contains(@"<preset id='roomDesc'>");
                PrintTextWithParse(sTextBuffer, default, default, default, default, isRoomOutput);
            }
            
            
            if (bCombatRow == true)
            {
                m_bBold = false;
            }
        }
    }

    private bool m_bIsParsingSettings = false;
    private bool m_bIsParsingSettingsStart = false;

    public string ProcessXML(string sXML)
    {
        string sReturn = string.Empty;
        if (sXML.Length == 0)
        {
            return sReturn;
        }

        if (m_bIsParsingSettings)
        {
            if (sXML.Contains("<sentSettings/>"))
            {
                m_bIsParsingSettings = false;
            }

            if (sXML.Contains("<vars>"))
            {
                int I = sXML.IndexOf("<vars>") + 6;
                int J = sXML.IndexOf("</vars>");
                sXML = sXML.Substring(I, J - I);
            }
            else
            {
                return sReturn;
            }
        }
        else if (m_bIsParsingSettingsStart)
        {
            if (sXML.Contains("<settings "))
            {
                m_bIsParsingSettingsStart = false;
                m_bIsParsingSettings = true;
                if (sXML.Contains("<vars>"))
                {
                    int I = sXML.IndexOf("<vars>");
                    int J = sXML.IndexOf("</vars>") + 7;
                    sXML = sXML.Substring(I, J - I);
                    if (sXML.Length == 0)
                        return sReturn;
                }
                else
                {
                    return sReturn;
                }
            }
        }

        var oDocument = new XmlDocument();
        try
        {
            oDocument.LoadXml("<data>" + sXML + "</data>");
        }
#pragma warning disable CS0168
        catch (XmlException ex)
#pragma warning restore CS0168
        {
            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
            return sReturn;
        }

        XmlNode argoXmlNode = oDocument.DocumentElement;
        sReturn = ProcessXMLData(argoXmlNode);
        EventParseXML?.Invoke(sXML);
        return sReturn;
    }

    public void SetBufferEnd()
    {
        if (Monitor.TryEnter(m_oThreadLock))
        {
            try
            {
                if (m_bUpdateRoomOnStreamEnd == true)
                {
                    m_bUpdateRoomOnStreamEnd = false;
                    m_bUpdatingRoom = false;
                    UpdateRoom();
                }
                Globals.Instance.VolatileHighlights.Clear();
            }
            finally
            {
                Monitor.Exit(m_oThreadLock);
            }
        }
        else
        {
            GenieError.Error("SetBufferEnd", "Unable to aquire game thread lock.");
        }
    }

    public void UpdateRoom()
    {
        if (Monitor.TryEnter(m_oThreadLock))
        {
            try
            {
                if (m_bUpdatingRoom == false)
                {
                    // ClearWindow(WindowTarget.Room)
                    WindowTarget targetRoom = WindowTarget.Room;
                    PrintTextToWindow("@suspend@", Color.Transparent, Color.Transparent, targetRoom, false, true);
                    if (Strings.Len(m_sRoomTitle) > 0)
                    {
                        string argsText = "[" + m_sRoomTitle + "]" + Constants.vbCrLf;
                        bool argbIsRoomOutput = true;
                        PrintTextWithParse(argsText, Presets.Instance["roomname"].FgColor, Presets.Instance["roomname"].BgColor, false, targetRoom, argbIsRoomOutput);
                    }
                    else
                    {
                        string argsText1 = "[Unknown Room]" + Constants.vbCrLf;
                        bool argbIsRoomOutput1 = true;
                        PrintTextWithParse(argsText1, Presets.Instance["roomname"].FgColor, Presets.Instance["roomname"].BgColor, false, targetRoom, argbIsRoomOutput1);
                    }

                    if (Strings.Len(m_sRoomDesc) > 0)
                    {
                        string argsText2 = m_sRoomDesc + System.Environment.NewLine;
                        bool argbIsRoomOutput2 = true;
                        PrintTextWithParse(argsText2, Presets.Instance["roomdesc"].FgColor, Presets.Instance["roomdesc"].BgColor, false, WindowTarget.Room, argbIsRoomOutput2);
                    }

                    if (Strings.Len(m_sRoomObjs) > 0)
                    {
                        string argsText3 = m_sRoomObjs + System.Environment.NewLine;
                        bool argbIsRoomOutput3 = true;
                        PrintTextWithParse(argsText3, default, default, false, targetRoom, argbIsRoomOutput3);
                    }

                    if (Strings.Len(m_sRoomPlayers) > 0)
                    {
                        string argsText4 = m_sRoomPlayers + System.Environment.NewLine;
                        bool argbIsRoomOutput4 = true;
                        PrintTextWithParse(argsText4, default, default, false, targetRoom, argbIsRoomOutput4);
                    }

                    if (Strings.Len(m_sRoomExits) > 0)
                    {
                        if (m_sRoomExits.Trim().EndsWith(":"))
                        {
                            m_sRoomExits = m_sRoomExits.Trim() + " none.";
                        }

                        if (!m_sRoomExits.EndsWith("."))
                        {
                            m_sRoomExits += ".";
                        }

                        string argsText5 = m_sRoomExits + System.Environment.NewLine;
                        bool argbIsRoomOutput5 = true;
                        PrintTextWithParse(argsText5, Color.Transparent, Color.Transparent, false, targetRoom, argbIsRoomOutput5);
                    }

                    PrintTextToWindow("@resume@", Color.Transparent, Color.Transparent, WindowTarget.Room, false, true);
                }
                else
                {
                    m_bUpdateRoomOnStreamEnd = true;
                }
            }
            finally
            {
                Monitor.Exit(m_oThreadLock);
            }
        }
        else
        {
            GenieError.Error("UpdateRoom", "Unable to aquire game thread lock.");
        }
    }

    private async void ParseRow(string sText)
    {
        var switchExpr = m_oConnectState;
        switch (switchExpr)
        {
            case ConnectStates.ConnectedKey:
                {
                    ParseKeyRow(sText);
                    break;
                }

            case ConnectStates.ConnectedGame:
                {
                    ParseGameRow(sText);
                    break;
                }

            case ConnectStates.ConnectedGameHandshake:
                {
                    m_oConnectState = ConnectStates.ConnectedGame;
                    await Task.Delay(1000);
                    m_oSocket.Send(Constants.vbLf + Constants.vbLf);
                    break;
                }
        }
    }

    private ArrayList _CharacterList = new ArrayList();

    public ArrayList CharacterList
    {
        get
        {
            return _CharacterList;
        }
    }

    private void ParseKeyRow(string sText)
    {
        if (sText.Length == 32 & m_sEncryptionKey.Length == 0)
        {

            m_sEncryptionKey = sText;
            m_oSocket.Send("A" + Constants.vbTab + m_sAccountName.ToUpper() + Constants.vbTab);
            m_oSocket.Send(Utility.EncryptText(m_sEncryptionKey, m_sAccountPassword));
            m_oSocket.Send(System.Environment.NewLine);
        }
        else
        {
            var oData = new ArrayList();
        foreach (string strLine in sText.Split(Conversions.ToChar(Constants.vbTab)))
            oData.Add(strLine);
        if (oData.Count > 0)
        {
            var switchExpr = oData[0];
            switch (switchExpr)
            {
                case "?":
                    {
                        string argtext = "Unable to get login key.";
                        PrintError(argtext);
                        m_oSocket.Disconnect();
                        break;
                    }

                case "A":
                    {
                        var switchExpr1 = oData[2];
                        switch (switchExpr1)
                        {
                            case "KEY":
                                {
                                    m_sLoginKey = Conversions.ToString(oData[3]);
                                    m_sAccountOwner = Conversions.ToString(oData[4]);
                                    m_oSocket.Send("G" + Constants.vbTab + m_sAccountGame.ToUpper() + System.Environment.NewLine);
                                    break;
                                }

                            case "NORECORD":
                                {
                                    string argtext1 = "Account does not exist.";
                                    PrintError(argtext1);
                                    m_oSocket.Disconnect();
                                    break;
                                }

                            case "PASSWORD":
                                {
                                    string argtext2 = "Invalid password.";
                                    PrintError(argtext2);
                                    m_oSocket.Disconnect();
                                    break;
                                }

                            case "REJECT":
                                {
                                    string argtext3 = "Access rejected.";
                                    PrintError(argtext3);
                                    m_oSocket.Disconnect();
                                    break;
                                }
                        }

                        break;
                    }

                case "G":
                    {
                        m_oSocket.Send("C" + System.Environment.NewLine);
                        break;
                    }

                case "C":
                    {
                        if (m_sAccountCharacter.Trim().Length == 0)
                        {
                            string argtext4 = "Listing characters:";
                            PrintError(argtext4);
                            string strUserKey = string.Empty;
                            // bool blnFoundMatch = false;
                            for (int i = 5, loopTo = oData.Count - 1; i <= loopTo; i++)
                            {
                                if (i % 2 == 0)
                                {
                                    _CharacterList.Clear();
                                    _CharacterList.Add(oData[i].ToString());
                                    var temp = oData[i].ToString();
                                    PrintError(temp);
                                }
                                else
                                {
                                    strUserKey = Conversions.ToString(oData[i]);
                                }
                            }

                            m_oSocket.Disconnect();
                        }
                        else
                        {
                            string strUserKey = string.Empty;
                            string strUserKeyTemp = string.Empty;
                            bool blnFoundMatch = false;
                            bool bFoundBanned = false;
                            for (int i = 5, loopTo1 = oData.Count - 1; i <= loopTo1; i++)
                            {
                                if (i % 2 == 0)
                                {
                                    string sChar = oData[i].ToString();
                                    if (sChar.Contains(" "))
                                        sChar = sChar.Substring(0, sChar.IndexOf(' '));
                                    if (m_oBanned.ContainsKey(Utility.GenerateHashSHA256(sChar)))
                                        bFoundBanned = true;
                                    if (sChar.ToUpper().Equals(m_sAccountCharacter.ToUpper()))
                                    {
                                        blnFoundMatch = true;
                                        strUserKey = strUserKeyTemp;
                                    }

                                    if (blnFoundMatch == false)
                                    {
                                        if (sChar.ToUpper().StartsWith(m_sAccountCharacter.ToUpper()))
                                        {
                                            blnFoundMatch = true;
                                            strUserKey = strUserKeyTemp;
                                        }
                                    }
                                }
                                else
                                {
                                    strUserKeyTemp = Conversions.ToString(oData[i]);
                                }
                            }

                            if (bFoundBanned)
                            {
                                m_oSocket.Disconnect();
                                return;
                            }

                            if (blnFoundMatch)
                            {
                                m_oSocket.Send("L" + Constants.vbTab + strUserKey + Constants.vbTab + "STORM" + Constants.vbLf);
                            }

                            if (blnFoundMatch == false)
                            {
                                string argtext5 = "Character not found.";
                                PrintError(argtext5);
                                m_oSocket.Disconnect();
                            }
                        }

                        break;
                    }
                case "E": //Indicates an Error Message
                    {
                        string[] errorStrings = sText.Split("\t");
                        for(int i = 1;i < errorStrings.Length;i++)
                        {
                            PrintError(errorStrings[i]);
                        }
                        m_oSocket.Disconnect();
                        break;
                    }

                case "L":
                    {
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oData[1], "OK", false)))
                        {
                            foreach (string strRow in oData)
                            {
                                if (strRow.IndexOf("GAMEHOST=") > -1)
                                {
                                        m_sConnectHost = IsLich ? ConfigSettings.Instance.LichServer : strRow.Substring(9);

                                    }
                                else if (strRow.IndexOf("GAMEPORT=") > -1)
                                {
                                        m_sConnectPort = IsLich ? ConfigSettings.Instance.LichPort : int.Parse(strRow.Substring(9));
                                    }
                                else if (strRow.IndexOf("KEY=") > -1)
                                {
                                    m_sConnectKey = strRow.Substring(4).TrimEnd('\0');
                                }
                            }

                            if (m_sConnectKey.Length > 0)
                            {
                                m_oSocket.Disconnect();
                                m_oConnectState = ConnectStates.ConnectingGameServer;
                                m_oSocket.Connect(m_sConnectHost, m_sConnectPort);
                            }
                        }
                        else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oData[1], "PROBLEM", false)))
                        {
                            string argtext6 = "There is a problem with your account. Log in to play.net website for more information.";
                            PrintError(argtext6);
                            m_oSocket.Disconnect();
                        }

                        break;
                    }
            }
        }
    }
    }

    private bool m_bMonoOutput = false;
    private bool m_bPresetSpeechOutput = false;
    private bool m_bPresetWhisperOutput = false;
    private bool m_bPresetThoughtOutput = false;
    private bool m_bStatusPromptEnabled = false;

    private static Regex m_RoomNameRegex = new Regex(@"\[(?<roomname>[^\]]+)\](?: \((?<roomuid>\d+|[*]{2})\))?");

    private string ProcessXMLNodeElement(XmlNode oXmlNode)
    {
        string sReturn = string.Empty;
       // Debug.WriteLine(oXmlNode.Name);
        if (oXmlNode.NodeType == XmlNodeType.Element)
        {
            var switchExpr = oXmlNode.Name;
            switch (switchExpr)
            {
                case "a":
                    {
                        // Dim sText As String = "{{" & GetTextFromXML(oXmlNode) & "}}"
                        // Dim sNoun As String = GetAttributeData(oXmlNode, "noun")
                        // If sNoun.Length > 0 Then
                        // sText = sText.Replace(sNoun, "[[" & sNoun & "]]")
                        // End If
                        // sReturn &= sText

                        sReturn += GetTextFromXML(oXmlNode);
                        break;
                    }

                case "d":
                    {
                        if ((oXmlNode.ParentNode.Name ?? "") != "component")
                        {
                            string sText = GetTextFromXML(oXmlNode);
                            if (ConfigSettings.Instance.ShowLinks)
                            {
                                string argstrAttributeName = "cmd";
                                string sCmd = GetAttributeData(oXmlNode, argstrAttributeName);
                                if (sCmd.Length == 0)
                                    sCmd = sText;
                                sReturn += "{" + sText + ":" + sCmd + "}";
                            }
                            else
                            {
                                sReturn += sText;
                            }
                        }

                        break;
                    }

                case "k":
                    {
                        if ((oXmlNode.ParentNode.Name ?? "") == "vars")
                        {
                            string argstrAttributeName1 = "name";
                            string sName = GetAttributeData(oXmlNode, argstrAttributeName1);
                            string argstrAttributeName2 = "value";
                            string sVal = GetAttributeData(oXmlNode, argstrAttributeName2);
                            if (sName.Length > 0)
                            {
                                Variables.Instance.Add(sName, sVal, Variables.VariablesType.Server);
                                string argsVariable = "$" + sName;
                                VariableChanged(argsVariable);
                                /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                            }
                        }

                        break;
                    }

                case "output":
                    {
                        string argstrAttributeName3 = "class";
                        var switchExpr1 = GetAttributeData(oXmlNode, argstrAttributeName3);
                        switch (switchExpr1)
                        {
                            case "mono":
                                {
                                    m_bMonoOutput = true;
                                    break;
                                }

                            default:
                                {
                                    m_bMonoOutput = false;
                                    break;
                                }
                        }

                        break;
                    }
                case "resource":
                    {
                        if (!ConfigSettings.Instance.ShowImages) break;
                        var attribute = GetAttributeData(oXmlNode, "picture");
                        if (!string.IsNullOrEmpty(attribute) && attribute != "0") 
                        {
                            attribute += ".jpg";
                            string gamecode = "DR"; //default DR
                            if (AccountGame.StartsWith("GS")) gamecode = "GS";
                            // ToDo: Check if the image exists in the art directory
                            // if (FileHandler.FetchImage(attribute, ConfigSettings.Instance.ArtDir, gamecode).Result) AddImage(Path.Combine(gamecode, attribute), "portrait");
                        }
                        break;
                    }
                case "streamWindow":	// Window Names
                    {
                        string argstrAttributeName5 = "id";
                        var switchExpr2 = GetAttributeData(oXmlNode, argstrAttributeName5);
                        switch (switchExpr2)
                        {
                            case "main":
                                {
                                    break;
                                }

                            case "inv":
                                {
                                    break;
                                }

                            case "familiar":
                                {
                                    break;
                                }

                            case "thoughts":
                                {
                                    break;
                                }

                            case "logons":
                                {
                                    break;
                                }

                            case "death":
                                {
                                    break;
                                }

                            case "whispers":
                                {
                                    break;
                                }

                            case "assess":
                                {
                                    break;
                                }

                            case "room":
                                {
                                    m_sRoomUid = "0";

                                    string argstrAttributeName4 = "subtitle";
                                    m_sRoomTitle = GetAttributeData(oXmlNode, argstrAttributeName4);

                                    // If flag showroomid is off then roomtitle is presented as [xxxx,xxxx]
                                    // if flag showroomid is on and room has a DR applied id number, then roomtitle is presented as [xxxx,xxxx] (ddddd)
                                    // if flag showroomid is on and room has NOT DR applied id number, then roomtitle is presented as [xxxx,xxxx] (**)

                                    System.Text.RegularExpressions.Match o_Match = m_RoomNameRegex.Match(m_sRoomTitle);
                                    if (o_Match.Success)
                                    {
                                        m_sRoomTitle = o_Match.Groups["roomname"].Value;
                                        if (o_Match.Groups["roomuid"].Success && !o_Match.Groups["roomuid"].Value.Equals("**"))
                                        {
                                            m_sRoomUid = o_Match.Groups["roomuid"].Value;
                                        }
                                    }
                                    else
                                    {
                                        if (m_sRoomTitle.StartsWith(" - "))
                                        {
                                            m_sRoomTitle = m_sRoomTitle.Substring(3);
                                        }

                                        if (m_sRoomTitle.StartsWith("["))
                                        {
                                            m_sRoomTitle = m_sRoomTitle.Substring(1, m_sRoomTitle.Length - 2);
                                        }

                                        m_sRoomTitle = m_sRoomTitle.Trim();
                                    }
                                    string argkey1 = "roomname";
                                    Variables.Instance.Add(argkey1, m_sRoomTitle, Variables.VariablesType.Reserved);
                                    string argsVariable1 = "$roomname";
                                    VariableChanged(argsVariable1);

                                    string argkey2 = "uid";
                                    Variables.Instance.Add(argkey2, m_sRoomUid, Variables.VariablesType.Reserved);
                                    string argsVariable2 = "$uid";
                                    VariableChanged(argsVariable2);

                                    m_bUpdatingRoom = true;
                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                                /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                        }

                        string argstrAttributeName10 = "target";
                        if (!HasAttribute(oXmlNode, argstrAttributeName10))
                        {
                            string argstrAttributeName6 = "id";
                            string sID = GetAttributeData(oXmlNode, argstrAttributeName6);
                            string argstrAttributeName7 = "title";
                            string sTitle = GetAttributeData(oXmlNode, argstrAttributeName7);
                            // ifClosed = '' means ignore
                            // no ifClosed means send to main
                            string sIfClosed = null;
                            string argstrAttributeName9 = "ifClosed";
                            if (HasAttribute(oXmlNode, argstrAttributeName9))
                            {
                                string argstrAttributeName8 = "ifClosed";
                                sIfClosed = GetAttributeData(oXmlNode, argstrAttributeName8);
                            }

                            EventStreamWindow?.Invoke(sID, sTitle, sIfClosed);
                        }

                        break;
                    }

                case "clearStream": // Clear Window
                    {
                        string argstrAttributeName11 = "id";
                        string sWindow = GetAttributeData(oXmlNode, argstrAttributeName11);
                        ClearWindow(sWindow);
                        break;
                    }

                case "pushStream": // Output to Window
                    {
                        m_sTargetWindow = string.Empty;
                        string argstrAttributeName13 = "id";
                        var switchExpr3 = GetAttributeData(oXmlNode, argstrAttributeName13);
                        switch (switchExpr3)
                        {
                            case "combat":
                                {
                                    m_oTargetWindow = WindowTarget.Combat;
                                    break;
                                }
                            case "main":
                                {
                                    m_oTargetWindow = WindowTarget.Main;
                                    break;
                                }

                            case "inv":
                                {
                                    m_oTargetWindow = WindowTarget.Inv;
                                    break;
                                }

                            case "familiar":
                                {
                                    m_oTargetWindow = WindowTarget.Familiar;
                                    m_bFamiliarLineParse = true;
                                    break;
                                }

                            case "thoughts":
                                {
                                    m_oTargetWindow = WindowTarget.Thoughts;
                                    break;
                                }

                            case "logons":
                                {
                                    m_oTargetWindow = WindowTarget.Logons;
                                    break;
                                }

                            case "death":
                                {
                                    m_oTargetWindow = WindowTarget.Death;
                                    break;
                                }

                            case "room":
                                {
                                    m_oTargetWindow = WindowTarget.Room;
                                    break;
                                }
                            case "debug":
                                {
                                    m_oTargetWindow = WindowTarget.Debug;
                                    break;
                                }

                            case "percWindow":
                                {
                                    m_oTargetWindow = WindowTarget.ActiveSpells;
                                    break;
                                }

                            default:
                                {
                                    m_oTargetWindow = WindowTarget.Other;
                                    string argstrAttributeName12 = "id";
                                    m_sTargetWindow = GetAttributeData(oXmlNode, argstrAttributeName12);
                                    break;
                                }
                        }

                        break;
                    }

                case "popStream": // Output to Default Window
                    {
                        if (m_oTargetWindow == WindowTarget.Inv)
                        {
                            PrintTextToWindow("@resume@", default, default, WindowTarget.Inv);
                        }

                        m_oTargetWindow = WindowTarget.Main;
                        break;
                    }

                case "stream":
                    {
                        string argstrAttributeName14 = "id";
                        var switchExpr4 = GetAttributeData(oXmlNode, argstrAttributeName14);
                        switch (switchExpr4)
                        {
                            case "main":
                                {
                                    break;
                                }

                            case "inv":
                                {
                                    break;
                                }

                            case "familiar":
                                {
                                    break;
                                }

                            case "thoughts":
                                {
                                    string argsText = GetTextFromXML(oXmlNode) + System.Environment.NewLine;
                                    bool argbIsRoomOutput = false;
                                    WindowTarget windowTarget = WindowTarget.Thoughts;
                                    PrintTextWithParse(argsText, Presets.Instance["thoughts"].FgColor, Presets.Instance["thoughts"].BgColor, false, windowTarget, bIsRoomOutput: argbIsRoomOutput);
                                    break;
                                }

                            case "logons":
                                {
                                    break;
                                }

                            case "death":
                                {
                                    break;
                                }

                            case "debug":
                                {
                                    break;
                                }

                            case "room":
                                {
                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                                /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                        }

                        break;
                    }

                case "preset":
                    {
                        string argstrAttributeName16 = "id";
                        var switchExpr5 = GetAttributeData(oXmlNode, argstrAttributeName16);
                        switch (switchExpr5)
                        {
                            case "speech":
                                {
                                    m_bPresetSpeechOutput = true;
                                    sReturn += GetTextFromXML(oXmlNode);
                                    break;
                                }

                            case "whisper":
                                {
                                    m_bPresetWhisperOutput = true;
                                    sReturn += GetTextFromXML(oXmlNode);
                                    break;
                                }

                            case "thought":
                                {
                                    m_bPresetThoughtOutput = true;
                                    sReturn += GetTextFromXML(oXmlNode);
                                    break;
                                }

                            default:
                                {
                                    string argstrAttributeName15 = "id";
                                    m_sStyle = GetAttributeData(oXmlNode, argstrAttributeName15);
                                    sReturn += GetTextFromXML(oXmlNode);
                                    break;
                                }
                        }

                        break;
                    }

                case "compDef":
                case "component":
                    {
                        string argstrAttributeName17 = "id";
                        var switchExpr6 = GetAttributeData(oXmlNode, argstrAttributeName17);
                        switch (switchExpr6)
                        {
                            case "room desc":
                                {
                                    EventTriggerMove?.Invoke();
                                    m_sRoomDesc = GetTextFromXML(oXmlNode);
                                    string argkey1 = "roomdesc";
                                    string argvalue = m_sRoomDesc.Replace(Conversions.ToString('"'), "");
                                    Variables.Instance.Add(argkey1, argvalue, Variables.VariablesType.Reserved);
                                    string argsVariable2 = "$roomdesc";
                                    VariableChanged(argsVariable2);
                                    string argkey2 = "roomobjs";
                                    string argvalue1 = "";
                                    Variables.Instance.Add(argkey2, argvalue1, Variables.VariablesType.Reserved);
                                    string argkey3 = "roomplayers";
                                    string argvalue2 = "";
                                    Variables.Instance.Add(argkey3, argvalue2, Variables.VariablesType.Reserved);
                                    string argkey4 = "roomexits";
                                    string argvalue3 = "";
                                    Variables.Instance.Add(argkey4, argvalue3, Variables.VariablesType.Reserved);
                                    UpdateRoom();
                                    break;
                                }

                            case "room objs":
                                {
                                    m_sRoomObjs = GetTextFromXML(oXmlNode).TrimStart();
                                    SetRoomObjects(oXmlNode);
                                    string argkey5 = "monstercount";
                                    string argvalue4 = CountMonsters(oXmlNode).ToString();
                                    Variables.Instance.Add(argkey5, argvalue4, Variables.VariablesType.Reserved); // $monstercount
                                    string argsVariable3 = "$monstercount";
                                    VariableChanged(argsVariable3);
                                    string argkey6 = "roomobjs";
                                    var roomobjs = m_sRoomObjs.Replace(Conversions.ToString('"'), "").TrimStart();
                                    Variables.Instance.Add(argkey6, roomobjs, Variables.VariablesType.Reserved);
                                    string argsVariable4 = "$roomobjs";
                                    VariableChanged(argsVariable4);
                                    UpdateRoom();
                                    break;
                                }

                            case "room players":
                                {
                                    m_sRoomPlayers = GetTextFromXML(oXmlNode);
                                    string argkey7 = "roomplayers";
                                    string argvalue5 = m_sRoomPlayers.Replace(Conversions.ToString('"'), "");
                                    Variables.Instance.Add(argkey7, argvalue5, Variables.VariablesType.Reserved);
                                    string argsVariable5 = "$roomplayers";
                                    VariableChanged(argsVariable5);
                                    UpdateRoom();
                                    break;
                                }

                            case "room exits":
                                {
                                    m_sRoomExits = GetTextFromXML(oXmlNode);
                                    string argkey8 = "roomexits";
                                    string argvalue6 = m_sRoomExits.Replace(Conversions.ToString('"'), "");
                                    Variables.Instance.Add(argkey8, argvalue6, Variables.VariablesType.Reserved);
                                    string argsVariable6 = "$roomexits";
                                    VariableChanged(argsVariable6);
                                    UpdateRoom();
                                    break;
                                }

                            default:
                                {
                                    m_bIgnoreXMLDepth = true; // Skip any elements inside an unknown component or compDef
                                    break;
                                }
                                /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                        }

                        break;
                    }

                case "progressBar":
                    {
                        int barValue = int.Parse(GetAttributeData(oXmlNode, "value"));
                        string barName = GetAttributeData(oXmlNode, "id");
                        string barTextBase = GetAttributeData(oXmlNode, "text");
                        string barText = string.Empty;
                        char previousCharacter = ' ';
                        for (int i = 0; i < barTextBase.Length; i++)
                        {
                            barText += previousCharacter == ' ' ? barTextBase[i].ToString().ToUpper() : barTextBase[i];
                            previousCharacter = barTextBase[i];
                        }
                        switch (barName)
                        {
                            case "health":
                                {
                                    Variables.Instance.Add("health", barValue.ToString(), Variables.VariablesType.Reserved);
                                    Variables.Instance.Add("healthBarText", barText, Variables.VariablesType.Reserved);
                                    VariableChanged("$health");
                                    break;
                                }

                            case "mana":
                                {
                                    Variables.Instance.Add("mana", barValue.ToString(), Variables.VariablesType.Reserved);
                                    Variables.Instance.Add("manaBarText", barText, Variables.VariablesType.Reserved);
                                    VariableChanged("$mana");
                                    break;
                                }

                            case "spirit":
                                {
                                    Variables.Instance.Add("spirit", barValue.ToString(), Variables.VariablesType.Reserved);
                                    Variables.Instance.Add("spiritBarText", barText, Variables.VariablesType.Reserved);
                                    VariableChanged("$spirit");
                                    break;
                                }

                            case "stamina":
                                {
                                    Variables.Instance.Add("stamina", barValue.ToString(), Variables.VariablesType.Reserved);
                                    Variables.Instance.Add("staminaBarText", barText, Variables.VariablesType.Reserved);
                                    VariableChanged("$stamina");
                                    break;
                                }

                            case "conclevel":
                            case "concentration":
                                {
                                    Variables.Instance.Add("concentration", barValue.ToString(), Variables.VariablesType.Reserved);
                                    Variables.Instance.Add("concentrationBarText", barText, Variables.VariablesType.Reserved);
                                    VariableChanged("$concentration");
                                    break;
                                }

                            case "encumlevel":
                            case "encumblevel":
                            case "encumbrance":
                                {
                                    m_iEncumbrance = barValue;
                                    string argkey14 = "encumbrance";
                                    var encumbVar = m_iEncumbrance.ToString();
                                    Variables.Instance.Add(argkey14, encumbVar, Variables.VariablesType.Reserved);
                                    string argsVariable12 = "$encumbrance";
                                    VariableChanged(argsVariable12);
                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                                /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                        }

                        break;
                    }

                case "spell":
                    {
                        string sSpellName = GetTextFromXML(oXmlNode);
                        if ((sSpellName ?? "") == "ScriptNumber")
                        {
                            sSpellName = "Unknown";
                        }

                        if ((sSpellName ?? "") == "None")
                        {
                            ClearSpellTime();
                        }
                        else
                        {
                            SetSpellTime();
                        }

                        string argkey15 = "preparedspell";
                        Variables.Instance.Add(argkey15, sSpellName, Variables.VariablesType.Reserved);
                        string argsVariable13 = "$preparedspell";
                        VariableChanged(argsVariable13);
                        StatusBarUpdate();
                        break;
                    }

                case "left":
                    {
                        string argkey16 = "lefthand";
                        string argvalue7 = GetTextFromXML(oXmlNode);
                        Variables.Instance.Add(argkey16, argvalue7, Variables.VariablesType.Reserved);
                        string lefthandnoun = GetAttributeData(oXmlNode, "noun");
                        string lefthandkey = "lefthandnoun";
                        Variables.Instance.Add(lefthandkey, lefthandnoun, Variables.VariablesType.Reserved);
                        string lefthandid = GetAttributeData(oXmlNode, "exist");
                        string lefthandidkey = "lefthandid";
                        Variables.Instance.Add(lefthandidkey, lefthandid, Variables.VariablesType.Reserved);
                        string argsVariable14 = "$lefthand";
                        VariableChanged(argsVariable14);
                        string argsVariable15 = "$lefthandnoun";
                        VariableChanged(argsVariable15);
                        string argsVariable16 = "$lefthandid";
                        VariableChanged(argsVariable16);
                        StatusBarUpdate();
                        break;
                    }

                case "right":
                    {
                        string argkey19 = "righthand";
                        string argvalue10 = GetTextFromXML(oXmlNode);
                        Variables.Instance.Add(argkey19, argvalue10, Variables.VariablesType.Reserved);
                        string righthandnoun = GetAttributeData(oXmlNode, "noun");
                        string righthandkey = "righthandnoun";
                        Variables.Instance.Add(righthandkey, righthandnoun, Variables.VariablesType.Reserved);
                        string righthandid = GetAttributeData(oXmlNode, "exist");
                        string righthandidkey = "righthandid";
                        Variables.Instance.Add(righthandidkey, righthandid, Variables.VariablesType.Reserved);

                        string argsVariable17 = "$righthand";
                        VariableChanged(argsVariable17);
                        string argsVariable18 = "$righthandnoun";
                        VariableChanged(argsVariable18);
                        string argsVariable19 = "$righthandid";
                        VariableChanged(argsVariable19);
                        StatusBarUpdate();
                        break;
                    }

                case "app":
                    {
                        string argstrAttributeName20 = "char";
                        string sTemp = GetAttributeData(oXmlNode, argstrAttributeName20);
                        if (sTemp.Length > 0)
                        {
                            m_sCharacterName = sTemp;
                            string argkey22 = "charactername";
                            Variables.Instance.Add(argkey22, m_sCharacterName, Variables.VariablesType.Reserved);
                            string argsVariable20 = "$charactername";
                            VariableChanged(argsVariable20);
                            if (m_oBanned.ContainsKey(Utility.GenerateHashSHA256(m_sCharacterName)))
                            {
                                m_oSocket.Disconnect();
                                m_bManualDisconnect = true;
                            }

                            string argstrAttributeName21 = "game";
                            m_sGameName = GetAttributeData(oXmlNode, argstrAttributeName21);
                            m_sGameName = m_sGameName.Replace(":", "").Replace(" ", "");
                            string argkey23 = "gamename";
                            Variables.Instance.Add(argkey23, m_sGameName, Variables.VariablesType.Reserved);
                            string argsVariable21 = "$gamename";
                            VariableChanged(argsVariable21);
                        }

                        break;
                    }

                case "indicator":
                    {
                        bool blnActive = false;
                        string argstrAttributeName22 = "visible";
                        if ((GetAttributeData(oXmlNode, argstrAttributeName22) ?? "") == "y")
                        {
                            blnActive = true;
                        }

                        string argstrAttributeName23 = "id";
                        var switchExpr8 = GetAttributeData(oXmlNode, argstrAttributeName23);
                        switch (switchExpr8)
                        {
                            case "IconKNEELING":
                                {
                                    m_oIndicatorHash[Indicator.Kneeling] = blnActive;
                                    string argkey24 = "kneeling";
                                    var kneelingVar = Utility.BooleanToInteger(blnActive).ToString();
                                    Variables.Instance.Add(argkey24, kneelingVar, Variables.VariablesType.Reserved);
                                    string argsVariable22 = "$kneeling";
                                    VariableChanged(argsVariable22);
                                    break;
                                }

                            case "IconPRONE":
                                {
                                    m_oIndicatorHash[Indicator.Prone] = blnActive;
                                    string argkey25 = "prone";
                                    var proneVar = Utility.BooleanToInteger(blnActive).ToString();
                                    Variables.Instance.Add(argkey25, proneVar, Variables.VariablesType.Reserved);
                                    string argsVariable23 = "$prone";
                                    VariableChanged(argsVariable23);
                                    break;
                                }

                            case "IconSITTING":
                                {
                                    m_oIndicatorHash[Indicator.Sitting] = blnActive;
                                    string argkey26 = "sitting";
                                    var sittingVar = Utility.BooleanToInteger(blnActive).ToString();
                                    Variables.Instance.Add(argkey26, sittingVar, Variables.VariablesType.Reserved);
                                    string argsVariable24 = "$sitting";
                                    VariableChanged(argsVariable24);
                                    break;
                                }

                            case "IconSTANDING":
                                {
                                    m_oIndicatorHash[Indicator.Standing] = blnActive;
                                    string argkey27 = "standing";
                                    var standingVar = Utility.BooleanToInteger(blnActive).ToString();
                                    Variables.Instance.Add(argkey27, standingVar, Variables.VariablesType.Reserved);
                                    string argsVariable25 = "$standing";
                                    VariableChanged(argsVariable25);
                                    break;
                                }

                            case "IconSTUNNED":
                                {
                                    m_oIndicatorHash[Indicator.Stunned] = blnActive;
                                    string argkey28 = "stunned";
                                    var stunnedVar = Utility.BooleanToInteger(blnActive).ToString();
                                    Variables.Instance.Add(argkey28, stunnedVar, Variables.VariablesType.Reserved);
                                    string argsVariable26 = "$stunned";
                                    VariableChanged(argsVariable26);
                                    break;
                                }

                            case "IconHIDDEN":
                                {
                                    m_oIndicatorHash[Indicator.Hidden] = blnActive;
                                    string argkey29 = "hidden";
                                    var hiddenVar = Utility.BooleanToInteger(blnActive).ToString();
                                    Variables.Instance.Add(argkey29, hiddenVar, Variables.VariablesType.Reserved);
                                    string argsVariable27 = "$hidden";
                                    VariableChanged(argsVariable27);
                                    break;
                                }

                            case "IconINVISIBLE":
                                {
                                    m_oIndicatorHash[Indicator.Invisible] = blnActive;
                                    string argkey30 = "invisible";
                                    var invisibleVar = Utility.BooleanToInteger(blnActive).ToString();
                                    Variables.Instance.Add(argkey30, invisibleVar, Variables.VariablesType.Reserved);
                                    string argsVariable28 = "$invisible";
                                    VariableChanged(argsVariable28);
                                    break;
                                }

                            case "IconDEAD":
                                {
                                    m_oIndicatorHash[Indicator.Dead] = blnActive;
                                    string argkey31 = "dead";
                                    var deadVar = Utility.BooleanToInteger(blnActive).ToString();
                                    Variables.Instance.Add(argkey31, deadVar, Variables.VariablesType.Reserved);
                                    string argsVariable29 = "$dead";
                                    VariableChanged(argsVariable29);
                                    break;
                                }

                            case "IconWEBBED":
                                {
                                    m_oIndicatorHash[Indicator.Webbed] = blnActive;
                                    string argkey32 = "webbed";
                                    var webbedVar = Utility.BooleanToInteger(blnActive).ToString();
                                    Variables.Instance.Add(argkey32, webbedVar, Variables.VariablesType.Reserved);
                                    string argsVariable30 = "$webbed";
                                    VariableChanged(argsVariable30);
                                    break;
                                }

                            case "IconJOINED":
                                {
                                    m_oIndicatorHash[Indicator.Joined] = blnActive;
                                    string argkey33 = "joined";
                                    var joinedVar = Utility.BooleanToInteger(blnActive).ToString();
                                    Variables.Instance.Add(argkey33, joinedVar, Variables.VariablesType.Reserved);
                                    string argsVariable31 = "$joined";
                                    VariableChanged(argsVariable31);
                                    break;
                                }

                            case "IconBLEEDING":
                                {
                                    m_oIndicatorHash[Indicator.Bleeding] = blnActive;
                                    string argkey34 = "bleeding";
                                    var bleedingVar = Utility.BooleanToInteger(blnActive).ToString();
                                    Variables.Instance.Add(argkey34, bleedingVar, Variables.VariablesType.Reserved);
                                    string argsVariable32 = "$bleeding";
                                    VariableChanged(argsVariable32);
                                    break;
                                }

                            case "IconPOISONED":
                                {
                                    string argkey35 = "poisoned";
                                    var poisonedVar = Utility.BooleanToInteger(blnActive).ToString();
                                    Variables.Instance.Add(argkey35, poisonedVar, Variables.VariablesType.Reserved);
                                    string argsVariable33 = "$poisoned";
                                    VariableChanged(argsVariable33);
                                    break;
                                }

                            case "IconDISEASED":
                                {
                                    string argkey36 = "diseased";
                                    var diseasedVar = Utility.BooleanToInteger(blnActive).ToString();
                                    Variables.Instance.Add(argkey36, diseasedVar, Variables.VariablesType.Reserved);
                                    string argsVariable34 = "$diseased";
                                    VariableChanged(argsVariable34);
                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                                /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                        }

                        break;
                    }

                case var @case when @case == "spell":
                    {
                        break;
                    }

                case var case1 when case1 == "left":
                    {
                        break;
                    }

                case var case2 when case2 == "right":
                    {
                        break;
                    }

                case "roundTime":
                    {
                        string argstrAttributeName24 = "value";
                        m_iRoundTime = int.Parse(GetAttributeData(oXmlNode, argstrAttributeName24));
                        break;
                    }

                case "castTime":
                    {
                        if (Variables.Instance.Contains("casttime"))
                        {
                            Variables.Instance["casttime"] = GetAttributeData(oXmlNode, "value");
                        }
                        else
                        {
                            Variables.Instance.Add("casttime", GetAttributeData(oXmlNode, "value"));
                        }
                        VariableChanged("$casttime");
                        m_iCastTime = int.Parse(GetAttributeData(oXmlNode, "value"));
                        break;
                    }
                case "spelltime":
                    {
                        if(Variables.Instance["preparedspell"].ToString() == "None")
                        {
                            if (Variables.Instance.Contains("spellstarttime"))
                            {
                                Variables.Instance["spellstarttime"] = "0";
                            }
                            else
                            {
                                Variables.Instance.Add("spellstarttime", "0");

                            }
                        }
                        else
                        {
                            if (Variables.Instance.Contains("spellstarttime"))
                            {
                                Variables.Instance["spellstarttime"] = GetAttributeData(oXmlNode, "value");
                            }
                            else
                            {
                                Variables.Instance.Add("spellstarttime", GetAttributeData(oXmlNode, "value"));

                            }
                        }
                        VariableChanged("$spellstarttime");
                        break;
                    }
                case "prompt":
                    {
                        string strBuffer = GetTextFromXML(oXmlNode);
                        if (m_bStatusPromptEnabled == false)
                        {
                            if ((strBuffer ?? "") != ">")
                            {
                                m_bStatusPromptEnabled = true;

                                // Fix for Joined and Bleeding
                                if (strBuffer.Contains("J") == false)
                                {
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(Variables.Instance["joined"], "1", false)))
                                    {
                                        string argkey37 = "joined";
                                        string argvalue13 = "0";
                                        Variables.Instance.Add(argkey37, argvalue13, Variables.VariablesType.Reserved);
                                        string argsVariable35 = "$joined";
                                        VariableChanged(argsVariable35);
                                    }
                                }

                                if (strBuffer.Contains("!") == false)
                                {
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(Variables.Instance["bleeding"], "1", false)))
                                    {
                                        string argkey38 = "bleeding";
                                        string argvalue14 = "0";
                                        Variables.Instance.Add(argkey38, argvalue14, Variables.VariablesType.Reserved);
                                        string argsVariable36 = "$bleeding";
                                        VariableChanged(argsVariable36);
                                    }
                                }
                            }
                            else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(m_oIndicatorHash[Indicator.Dead], true, false)))
                            {
                                strBuffer += "DEAD";
                            }
                            else if (ConfigSettings.Instance.PromptForce == true)
                            {
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(m_oIndicatorHash[Indicator.Kneeling], true, false)))
                                {
                                    strBuffer += "K";
                                }

                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(m_oIndicatorHash[Indicator.Sitting], true, false)))
                                {
                                    strBuffer += "s";
                                }

                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(m_oIndicatorHash[Indicator.Prone], true, false)))
                                {
                                    strBuffer += "P";
                                }

                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(m_oIndicatorHash[Indicator.Stunned], true, false)))
                                {
                                    strBuffer += "S";
                                }

                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(m_oIndicatorHash[Indicator.Hidden], true, false)))
                                {
                                    strBuffer += "H";
                                }

                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(m_oIndicatorHash[Indicator.Invisible], true, false)))
                                {
                                    strBuffer += "I";
                                }

                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(m_oIndicatorHash[Indicator.Webbed], true, false)))
                                {
                                    strBuffer += "W";
                                }

                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(m_oIndicatorHash[Indicator.Bleeding], true, false)))
                                {
                                    strBuffer += "!";
                                }

                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(m_oIndicatorHash[Indicator.Joined], true, false)))
                                {
                                    strBuffer += "J";
                                }
                            }
                        }
                        else
                        {
                            // Fix for Joined and Bleeding
                            if (strBuffer.Contains("J") == false)
                            {
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(Variables.Instance["joined"], "1", false)))
                                {
                                    string argkey39 = "joined";
                                    string argvalue15 = "0";
                                    Variables.Instance.Add(argkey39, argvalue15, Variables.VariablesType.Reserved);
                                    string argsVariable37 = "$joined";
                                    VariableChanged(argsVariable37);
                                }
                            }

                            if (strBuffer.Contains("!") == false)
                            {
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(Variables.Instance["bleeding"], "1", false)))
                                {
                                    string argkey40 = "bleeding";
                                    string argvalue16 = "0";
                                    Variables.Instance.Add(argkey40, argvalue16, Variables.VariablesType.Reserved);
                                    string argsVariable38 = "$bleeding";
                                    VariableChanged(argsVariable38);
                                }
                            }
                        }

                        // Dim strBuffer As String = String.Empty

                        string argstrAttributeName25 = "time";
                        if (int.TryParse(GetAttributeData(oXmlNode, argstrAttributeName25), out m_iGameTime))
                        {
                            string argkey41 = "gametime";
                            string argvalue17 = m_iGameTime.ToString();
                            Variables.Instance.Add(argkey41, argvalue17, Variables.VariablesType.Reserved);
                            string argsVariable39 = "$gametime";
                            VariableChanged(argsVariable39);
                            int rt = m_iRoundTime - m_iGameTime;
                            if (rt > 0)
                            {
                                SetRoundTime(rt);
                                if (m_bStatusPromptEnabled == false && (ConfigSettings.Instance.PromptForce == true))
                                    strBuffer += "R";
                                rt += Convert.ToInt32(ConfigSettings.Instance.RTOffset);
                                var rtString = rt.ToString();
                                string argkey42 = "roundtime";
                                Variables.Instance.Add(argkey42, rtString, Variables.VariablesType.Reserved);
                                m_iRoundTime = 0;
                            }
                            else
                            {
                                string argkey43 = "roundtime";
                                string argvalue18 = "0";
                                Variables.Instance.Add(argkey43, argvalue18, Variables.VariablesType.Reserved);
                            }
                            string argsVariable40 = "$roundtime";
                            VariableChanged(argsVariable40);

                            if (m_iCastTime > 0)
                            {
                                EventCastTime?.Invoke();
                                m_iCastTime = 0;
                            }

                            if (ConfigSettings.Instance.Prompt.Length > 0 && !m_bLastRowWasPrompt)
                            {
                                strBuffer = strBuffer.Replace(ConfigSettings.Instance.Prompt.Trim(), "");
                                strBuffer += ConfigSettings.Instance.Prompt;
                                bool argbIsPrompt = true;
                                WindowTarget argoWindowTarget = 0;
                                //Status prompt set from the XML printing to main/game 
                                PrintTextWithParse(strBuffer, argbIsPrompt, oWindowTarget: argoWindowTarget);
                            }

                            string argkey44 = "prompt";
                            Variables.Instance.Add(argkey44, strBuffer, Variables.VariablesType.Reserved);
                            string argsVariable41 = "$prompt";
                            VariableChanged(argsVariable41);
                            EventTriggerPrompt?.Invoke();
                        }

                        break;
                    }

                case "style":
                    {
                        string argstrAttributeName26 = "id";
                        string tmpString = GetAttributeData(oXmlNode, argstrAttributeName26);
                        m_sStyle = tmpString;
                        break;
                    }

                case "compass":
                    {
                        m_oCompassHash[Direction.North] = false;
                        m_oCompassHash[Direction.NorthEast] = false;
                        m_oCompassHash[Direction.East] = false;
                        m_oCompassHash[Direction.SouthEast] = false;
                        m_oCompassHash[Direction.South] = false;
                        m_oCompassHash[Direction.SouthWest] = false;
                        m_oCompassHash[Direction.West] = false;
                        m_oCompassHash[Direction.NorthWest] = false;
                        m_oCompassHash[Direction.Up] = false;
                        m_oCompassHash[Direction.Down] = false;
                        m_oCompassHash[Direction.Out] = false;
                        string argkey45 = "north";
                        string argvalue19 = "0";
                        Variables.Instance.Add(argkey45, argvalue19, Variables.VariablesType.Reserved);
                        string argkey46 = "northeast";
                        string argvalue20 = "0";
                        Variables.Instance.Add(argkey46, argvalue20, Variables.VariablesType.Reserved);
                        string argkey47 = "east";
                        string argvalue21 = "0";
                        Variables.Instance.Add(argkey47, argvalue21, Variables.VariablesType.Reserved);
                        string argkey48 = "southeast";
                        string argvalue22 = "0";
                        Variables.Instance.Add(argkey48, argvalue22, Variables.VariablesType.Reserved);
                        string argkey49 = "south";
                        string argvalue23 = "0";
                        Variables.Instance.Add(argkey49, argvalue23, Variables.VariablesType.Reserved);
                        string argkey50 = "southwest";
                        string argvalue24 = "0";
                        Variables.Instance.Add(argkey50, argvalue24, Variables.VariablesType.Reserved);
                        string argkey51 = "west";
                        string argvalue25 = "0";
                        Variables.Instance.Add(argkey51, argvalue25, Variables.VariablesType.Reserved);
                        string argkey52 = "northwest";
                        string argvalue26 = "0";
                        Variables.Instance.Add(argkey52, argvalue26, Variables.VariablesType.Reserved);
                        string argkey53 = "up";
                        string argvalue27 = "0";
                        Variables.Instance.Add(argkey53, argvalue27, Variables.VariablesType.Reserved);
                        string argkey54 = "down";
                        string argvalue28 = "0";
                        Variables.Instance.Add(argkey54, argvalue28, Variables.VariablesType.Reserved);
                        string argkey55 = "out";
                        string argvalue29 = "0";
                        Variables.Instance.Add(argkey55, argvalue29, Variables.VariablesType.Reserved);
                        string argsVariable42 = "compass";
                        VariableChanged(argsVariable42);
                        break;
                    }

                case "dir":
                    {
                        if ((oXmlNode.ParentNode.Name ?? "") == "compass")
                        {
                            string argstrAttributeName27 = "value";
                            var switchExpr9 = GetAttributeData(oXmlNode, argstrAttributeName27);
                            switch (switchExpr9)
                            {
                                case "n":
                                    {
                                        m_oCompassHash[Direction.North] = true;
                                        string argkey56 = "north";
                                        string argvalue30 = "1";
                                        Variables.Instance.Add(argkey56, argvalue30, Variables.VariablesType.Reserved);
                                        break;
                                    }

                                case "ne":
                                    {
                                        m_oCompassHash[Direction.NorthEast] = true;
                                        string argkey57 = "northeast";
                                        string argvalue31 = "1";
                                        Variables.Instance.Add(argkey57, argvalue31, Variables.VariablesType.Reserved);
                                        break;
                                    }

                                case "e":
                                    {
                                        m_oCompassHash[Direction.East] = true;
                                        string argkey58 = "east";
                                        string argvalue32 = "1";
                                        Variables.Instance.Add(argkey58, argvalue32, Variables.VariablesType.Reserved);
                                        break;
                                    }

                                case "se":
                                    {
                                        m_oCompassHash[Direction.SouthEast] = true;
                                        string argkey59 = "southeast";
                                        string argvalue33 = "1";
                                        Variables.Instance.Add(argkey59, argvalue33, Variables.VariablesType.Reserved);
                                        break;
                                    }

                                case "s":
                                    {
                                        m_oCompassHash[Direction.South] = true;
                                        string argkey60 = "south";
                                        string argvalue34 = "1";
                                        Variables.Instance.Add(argkey60, argvalue34, Variables.VariablesType.Reserved);
                                        break;
                                    }

                                case "sw":
                                    {
                                        m_oCompassHash[Direction.SouthWest] = true;
                                        string argkey61 = "southwest";
                                        string argvalue35 = "1";
                                        Variables.Instance.Add(argkey61, argvalue35, Variables.VariablesType.Reserved);
                                        break;
                                    }

                                case "w":
                                    {
                                        m_oCompassHash[Direction.West] = true;
                                        string argkey62 = "west";
                                        string argvalue36 = "1";
                                        Variables.Instance.Add(argkey62, argvalue36, Variables.VariablesType.Reserved);
                                        break;
                                    }

                                case "nw":
                                    {
                                        m_oCompassHash[Direction.NorthWest] = true;
                                        string argkey63 = "northwest";
                                        string argvalue37 = "1";
                                        Variables.Instance.Add(argkey63, argvalue37, Variables.VariablesType.Reserved);
                                        break;
                                    }

                                case "up":
                                    {
                                        m_oCompassHash[Direction.Up] = true;
                                        string argkey64 = "up";
                                        string argvalue38 = "1";
                                        Variables.Instance.Add(argkey64, argvalue38, Variables.VariablesType.Reserved);
                                        break;
                                    }

                                case "down":
                                    {
                                        m_oCompassHash[Direction.Down] = true;
                                        string argkey65 = "down";
                                        string argvalue39 = "1";
                                        Variables.Instance.Add(argkey65, argvalue39, Variables.VariablesType.Reserved);
                                        break;
                                    }

                                case "out":
                                    {
                                        m_oCompassHash[Direction.Out] = true;
                                        string argkey66 = "out";
                                        string argvalue40 = "1";
                                        Variables.Instance.Add(argkey66, argvalue40, Variables.VariablesType.Reserved);
                                        break;
                                    }

                                default:
                                    {
                                        break;
                                    }
                                    /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                            }

                            string argsVariable43 = "compass";
                            VariableChanged(argsVariable43);
                        }

                        break;
                    }

                case "pushBold":
                    {
                        m_bBold = true;
                        break;
                    }

                case "popBold":
                    {
                        m_bBold = false;
                        break;
                    }

                case "b":
                    {
                        sReturn += oXmlNode.InnerText;
                        break;
                    }

                case "settingsInfo":
                    {
                        if (m_bIsParsingSettings == false)
                        {
                            m_bIsParsingSettingsStart = true;
                            SendRaw("<sendSettings/>" + Constants.vbLf);
                        }

                        break;
                    }
                    /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
            }
        }

        return sReturn;
    }

     public void ResetIndicators()
    {
        m_oIndicatorHash[Indicator.Bleeding] = false;
        m_oIndicatorHash[Indicator.Dead] = false;
        m_oIndicatorHash[Indicator.Hidden] = false;
        m_oIndicatorHash[Indicator.Invisible] = false;
        m_oIndicatorHash[Indicator.Joined] = false;
        m_oIndicatorHash[Indicator.Kneeling] = false;
        m_oIndicatorHash[Indicator.Prone] = false;
        m_oIndicatorHash[Indicator.Sitting] = false;
        m_oIndicatorHash[Indicator.Standing] = false;
        m_oIndicatorHash[Indicator.Stunned] = false;
        m_oIndicatorHash[Indicator.Webbed] = false;
    }

    private Regex m_MonsterRegex = new Regex("<pushBold />([^<]*)<popBold />([^,.]*)", MyRegexOptions.options);
    private Regex m_RoomObjectsRegex = new Regex("<pushBold />([^<]*)<popBold />");
    private static int tagOffset = "<pushBold /><popBold />".Length;
    private void SetRoomObjects(XmlNode oXmlNode)
    {
        Globals.Instance.RoomObjects.Clear();
        foreach (Match roomObject in m_RoomObjectsRegex.Matches(oXmlNode.InnerXml))
        {
            int position = roomObject.Index - (tagOffset * Globals.Instance.RoomObjects.Count);
            VolatileHighlight highlight = new VolatileHighlight(ParseSubstitutions(roomObject.Groups[1].Value), "creatures", position);
            Globals.Instance.RoomObjects.Add(highlight);
        }
    }
    private int CountMonsters(XmlNode oXmlNode)
    {
        int iMonsterCount = 0;
        string sMonsterList = string.Empty;
        Globals.Instance.MonsterList.Clear();
        foreach (Match m in m_MonsterRegex.Matches(oXmlNode.InnerXml.Replace(" and ", ", ").Replace(" and <pushBold />", ", <pushBold />")))
        {
            var sValue = m.Groups[1].Value + m.Groups[2].Value;
            // PrintText(sValue & vbNewLine)

            bool bIgnore = false;
            foreach (string sIgnore in ConfigSettings.Instance.IgnoreMonsterList.Split('|'))
            {
                if (Conversions.ToBoolean(sValue.Contains(sIgnore)))
                {
                    bIgnore = true;
                    break;
                }
            }

            if (bIgnore == false)
            {
                iMonsterCount += 1;
                if (sMonsterList.Length > 0)
                {
                    sMonsterList += ", ";
                }

                sMonsterList += sValue.ToString().Trim();
            }

            if (!Globals.Instance.MonsterList.Contains(sValue.ToString().Trim()))
            {
                Globals.Instance.MonsterList.Add(sValue.ToString().Trim());
            }
        }

        Globals.Instance.UpdateMonsterListRegEx();
        string argkey = "monsterlist";
        Variables.Instance.Add(argkey, sMonsterList, Variables.VariablesType.Reserved);
        string argsVariable = "monsterlist";
        VariableChanged(argsVariable);
        return iMonsterCount;
    }

    private string GetTextFromXML(XmlNode oXmlNode)
    {
        return oXmlNode.InnerText;
    }

    private string GetAttributeData(XmlNode oXmlNode, string strAttributeName)
    {
        XmlNode oXmlAttribute;
        oXmlAttribute = oXmlNode.Attributes.GetNamedItem(strAttributeName);
        if (Information.IsNothing(oXmlAttribute) == false)
        {
            return oXmlAttribute.Value.ToString();
        }
        else
        {
            return "";
        }
    }

    private bool HasAttribute(XmlNode oXmlNode, string strAttributeName)
    {
        XmlNode oXmlAttribute;
        oXmlAttribute = oXmlNode.Attributes.GetNamedItem(strAttributeName);
        return !Information.IsNothing(oXmlAttribute);
    }

    private string ProcessXMLData(XmlNode oXmlNode)
    {
        string sReturn = string.Empty;
        if (Information.IsNothing(oXmlNode))
        {
            return sReturn;
        }

        if (oXmlNode.HasChildNodes == false)
        {
            return sReturn;
        }

        foreach (XmlNode oNode in oXmlNode.ChildNodes)
        {
            if (oNode.NodeType == XmlNodeType.Element)
            {
                var tmpNode = oNode;
                sReturn += ProcessXMLNodeElement(tmpNode);
            }

            if (!Information.IsNothing(oNode))
            {
                // Row below is for stream (thoughts)
                if (oNode.NodeType != XmlNodeType.Element | (oNode.Name ?? "") != "stream")
                {
                    if (oNode.HasChildNodes == true)
                    {
                        if (m_bIgnoreXMLDepth == false)
                        {
                            var tmpNode = oNode;
                            sReturn += ProcessXMLData(tmpNode);
                        }
                        else
                        {
                            m_bIgnoreXMLDepth = false;
                        }
                    }
                }
            }
        }

        return sReturn;
    }

    // Confuse decompilers and reverse engineers by having this method in the middle of everything and no string names in it
    private void DoConnect(string sHostName, int iPort)
    {

        m_sEncryptionKey = string.Empty;
        m_oConnectState = ConnectStates.ConnectingKeyServer;
        m_oSocket.ConnectAndAuthenticate(sHostName, iPort);
        
    }

    private MatchCollection m_oMatchCollection;

    public void PrintTextWithParse(string sText, [Optional, DefaultParameterValue(false)] bool bIsPrompt, [Optional, DefaultParameterValue(WindowTarget.Unknown)] WindowTarget oWindowTarget)
    {
        bool argbIsRoomOutput = false;
        PrintTextWithParse(sText, default, default, bIsPrompt, oWindowTarget, bIsRoomOutput: argbIsRoomOutput);
    }

    public void PrintTextWithParse(string sText, Color color, Color bgcolor, bool bIsPrompt = false, WindowTarget oWindowTarget = WindowTarget.Unknown, bool bIsRoomOutput = false)
    {
        
        if (sText.Trim().Length > 0)
        {
            if (sText.StartsWith("  You also see"))
            {
                PrintTextToWindow(Environment.NewLine, color, bgcolor, oWindowTarget, bIsPrompt, true);
                sText = sText.TrimStart();
            }

            if (m_sStyle.Length > 0)
            {
                var switchExpr = m_sStyle;
                switch (switchExpr)
                {
                    case "roomName":
                        {
                            color = Presets.Instance["roomname"].FgColor;
                            bgcolor = Presets.Instance["roomname"].BgColor;
                            m_oLastFgColor = color;
                            break;
                        }

                    case "roomDesc":
                        {
                            color = Presets.Instance["roomdesc"].FgColor;
                            bgcolor = Presets.Instance["roomdesc"].BgColor;
                            m_oLastFgColor = color;
                            break;
                        }

                    default:
                        {
                            break;
                        }
                        /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                }

                m_sStyle = string.Empty;
            }

            if (m_bPresetSpeechOutput) m_bPresetSpeechOutput = false;
            if (m_bPresetWhisperOutput) m_bPresetWhisperOutput = false;
            if (m_bPresetThoughtOutput) m_bPresetThoughtOutput = false;

            //if (m_bBold == true)
            //{
            //}

            // Line begins with
            if (HighlightsBeginWithList.Instance.AcquireReaderLock())
            {
                try
                {
                    foreach (DictionaryEntry de in HighlightsBeginWithList.Instance)
                    {
                        HighlightsBeginWithList.Highlight o = (HighlightsBeginWithList.Highlight)de.Value;
                        if (o.IsActive)
                        {
                            if (sText.StartsWith(o.Text, !o.CaseSensitive, null) == true)
                            {
                                color = o.FgColor;
                                bgcolor = o.BgColor;
                                m_oLastFgColor = color;
                                // ToDo: Figure out way to call back to platform specific sound player
                                //if (o.SoundFile.Length > 0 && ConfigSettings.Instance.PlaySounds)
                                //    Sound.PlayWaveFile(o.SoundFile);
                            }
                        }
                    }
                }
                finally
                {
                    HighlightsBeginWithList.Instance.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("PrintTextWithParse", "Unable to aquire reader lock.");
            }

            // Line contains
            if (!Information.IsNothing(HighlightsList.Instance.RegexLine) && !string.IsNullOrWhiteSpace(HighlightsList.Instance.RegexLine.ToString()))
            {
                m_oMatchCollection =HighlightsList.Instance.RegexLine.Matches(sText);
                HighlightsList.Highlight oHighlightString;
                foreach (Match oMatch in m_oMatchCollection)
                {
                    if (HighlightsList.Instance.Contains(oMatch.Value))
                    {
                        oHighlightString = (HighlightsList.Highlight)HighlightsList.Instance[oMatch.Value];
                        color = oHighlightString.FgColor;
                        bgcolor = oHighlightString.BgColor;
                        m_oLastFgColor = color;
                        // ToDo: Figure out way to call back to platform specific sound player
                        //if (oHighlightString.SoundFile.Length > 0 && ConfigSettings.Instance.PlaySounds)
                        //    Sound.PlayWaveFile(oHighlightString.SoundFile);
                    }
                }
            }
        }

        if (oWindowTarget == WindowTarget.Unknown)
        {
            oWindowTarget = m_oTargetWindow;
        }
        PrintTextToWindow(sText, color, bgcolor, oWindowTarget, bIsPrompt, bIsRoomOutput);
        
    }

    private Color m_oLastFgColor = default;
    private Color m_oEmptyColor = default;

    private void PrintTextToWindow(string text, Color color, Color bgcolor, WindowTarget targetwindow = WindowTarget.Main, bool isprompt = false, bool isroomoutput = false)
    {
        if (text.Length == 0 || (!isroomoutput && ConfigSettings.Instance.Condensed && text.Trim().Length == 0))
        {
            return;
        }

        string sTargetWindowString = string.Empty;
        var switchExpr = targetwindow;
        switch (switchExpr)
        {
            case WindowTarget.Main:
                {
                    sTargetWindowString = "main";
                    break;
                }

            case WindowTarget.Death:
                {
                    sTargetWindowString = "death";
                    break;
                }

            case WindowTarget.Combat:
                {
                    sTargetWindowString = "combat";
                    break;
                }
            case WindowTarget.Portrait:
                {
                    sTargetWindowString = "portrait";
                    break;
                }
            case WindowTarget.Familiar:
                {
                    sTargetWindowString = "familiar";
                    break;
                }

            case WindowTarget.Inv:
                {
                    sTargetWindowString = "inv";
                    break;
                }

            case WindowTarget.Log:
                {
                    sTargetWindowString = "log";
                    break;
                }

            case WindowTarget.Logons:
                {
                    sTargetWindowString = "logons";
                    break;
                }

            case WindowTarget.Room:
                {
                    sTargetWindowString = "room";
                    break;
                }

            case WindowTarget.Thoughts:
                {
                    sTargetWindowString = "thoughts";
                    break;
                }

            case WindowTarget.Raw:
                {
                    sTargetWindowString = "raw";
                    m_sTargetWindow = "raw";
                    targetwindow = WindowTarget.Other;
                    break;
                }
            case WindowTarget.Debug:
                {
                    sTargetWindowString = "debug";
                    break;
                }

            case WindowTarget.ActiveSpells:
                {
                    sTargetWindowString = "percwindow";
                    break;
                }

            case WindowTarget.Other:
                {
                   // Debug.Write("Target Window is " + targetwindow.ToString());
                    sTargetWindowString = m_sTargetWindow.ToLower();
                    break;
                }
        }

        text = ParsePluginText(text, sTargetWindowString);
        if (text.Length == 0)
        {
            return;
        }

        if (targetwindow != WindowTarget.Room & targetwindow != WindowTarget.Inv & targetwindow != WindowTarget.Log & text.Trim().Length > 0)
        {
            if (ConfigSettings.Instance.ParseGameOnly == false | targetwindow == WindowTarget.Main)
            {
                string argsText = Utility.Trim(text);
                TriggerParse(argsText);
            }
        }

        if (targetwindow == WindowTarget.Room & isroomoutput == false) // Skip all other text to room window
        {
            return;
        }

        if (ConfigSettings.Instance.GagsEnabled == true && targetwindow != WindowTarget.Thoughts)
        {
            // Gag List
            if (GagRegExp.Instance.AcquireReaderLock())
            {
                try
                {
                    foreach (GagRegExp.Gag sl in GagRegExp.Instance)
                    {
                        if (sl.IsActive && !Information.IsNothing(sl.RegexGag))
                        {
                            if (sl.RegexGag.Match(Utility.Trim(text)).Success == true)
                            {
                                return;
                            }
                        }
                    }
                }
                finally
                {
                    GagRegExp.Instance.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("PrintTextToWindow", "Unable to aquire reader lock.");
            }
        }

        text = ParseSubstitutions(text);
        if (0 == 1)//(text.Trim().Length > 0)
        {
            // Substitute Lists Switch this to text = ParseSubstrings(text) so theres only one place subs are processed at
            if (SubstituteRegExp.Instance.AcquireReaderLock())
            {
                try
                {
                    foreach (SubstituteRegExp.Substitute sl in SubstituteRegExp.Instance)
                    {
                        if (sl.IsActive && !Information.IsNothing(sl.SubstituteRegex))
                        {
                            if (sl.SubstituteRegex.Match(Utility.Trim(text)).Success)
                            {
                                bool bNewLineStart = text.StartsWith(System.Environment.NewLine);
                                bool bNewLineEnd = text.EndsWith(System.Environment.NewLine);
                                text = sl.SubstituteRegex.Replace(Utility.Trim(text), sl.sReplaceBy.ToString());
                                if (bNewLineStart == true)
                                {
                                    text = System.Environment.NewLine + text;
                                }

                                if (bNewLineEnd == true)
                                {
                                    text += System.Environment.NewLine;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    SubstituteRegExp.Instance.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("PrintTextToWindow", "Unable to aquire reader lock.");
            }
        }

        if (targetwindow == WindowTarget.Main)
        {
            if (text.Trim().Length == 0)
            {
                if (m_bLastRowWasBlank == true | m_bLastRowWasPrompt == true)
                {
                    return;
                }

                m_bLastRowWasBlank = true;
            }
            else
            {
                m_bLastRowWasBlank = false;
            }
        }

        if (targetwindow == WindowTarget.Main | targetwindow == WindowTarget.Thoughts | targetwindow == WindowTarget.Combat)
        {
            if (ConfigSettings.Instance.AutoLog == true)
            {
                Log.LogText(text, Conversions.ToString(Variables.Instance["charactername"]), Conversions.ToString(Variables.Instance["game"]));
                //if (m_bLastRowWasPrompt == true)
                //{
                //    Globals.Instance.Log?.LogText(text + System.Environment.NewLine, Conversions.ToString(Variables.Instance["charactername"]), Conversions.ToString(Variables.Instance["game"]));
                //}

                //     Globals.Instance.Log.LogText(text, Conversions.ToString(Variables.Instance["charactername"]), Conversions.ToString(Variables.Instance["game"]));
            }
        }

        if (text.Trim().StartsWith("Invalid login key."))
        {
            m_oReconnectTime = default;
            m_bManualDisconnect = true;
        }

        if (color == m_oEmptyColor | color == Color.Transparent)
        {
            if (m_oLastFgColor != m_oEmptyColor)
            {
                color = m_oLastFgColor;
            }
        }

        if (text.EndsWith(System.Environment.NewLine) | text.StartsWith(System.Environment.NewLine))
        {
            m_oLastFgColor = default;
        }

        string targetwindowstring = string.Empty;
        if (targetwindow == WindowTarget.Other)
        {
            targetwindowstring = m_sTargetWindow;
        }

        if (targetwindow == WindowTarget.Familiar)
        {
            color = Presets.Instance["familiar"].FgColor;
            bgcolor = Presets.Instance["familiar"].BgColor;
        }

        var tempVar = false;
        EventPrintText?.Invoke(text, color, bgcolor, targetwindow, targetwindowstring, m_bMonoOutput, isprompt, tempVar);
    }
    private String ParseSubstitutions(string text)
    {
        if (text.Trim().Length > 0)
        {
            // Substitute Lists
            if (SubstituteRegExp.Instance.AcquireReaderLock())
            {
                try
                {
                    foreach (SubstituteRegExp.Substitute sl in SubstituteRegExp.Instance)
                    {
                        if (sl.IsActive && !Information.IsNothing(sl.SubstituteRegex))
                        {
                            if (sl.SubstituteRegex.Match(Utility.Trim(text)).Success)
                            {
                                bool bNewLineStart = text.StartsWith(System.Environment.NewLine);
                                bool bNewLineEnd = text.EndsWith(System.Environment.NewLine);
                                text = sl.SubstituteRegex.Replace(Utility.Trim(text), Globals.ParseGlobalVars(sl.sReplaceBy).ToString());
                                if (bNewLineStart == true)
                                {
                                    text = System.Environment.NewLine + text;
                                }

                                if (bNewLineEnd == true)
                                {
                                    text += System.Environment.NewLine;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    SubstituteRegExp.Instance.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("PrintTextToWindow", "Unable to aquire reader lock.");
            }
        }

        return text;
    }
    private void VariableChanged(string sVariable)
    {
        EventVariableChanged?.Invoke(sVariable);
    }

    private void TriggerParse(string sText)
    {
        if (sText.Trim().Length > 0)
        {
            EventTriggerParse?.Invoke(sText);
        }
    }

    // Skip all blank line/prompt checks and just print it
    private void PrintInputText(string sText, Color oColor, Color oBgColor)
    {
        if (sText.Length == 0)
        {
            return;
        }

        var windowVar = WindowTarget.Main;
        var emptyVar = "";
        var trueVar = true;
        var falseVar = false;

     //   EventPrintText?.Invoke(sText, oColor, oBgColor, windowVar, emptyVar, m_bMonoOutput, trueVar, falseVar);
        EventPrintText?.Invoke(sText, oColor, oBgColor, windowVar, emptyVar, m_bMonoOutput, falseVar, trueVar);
    }

    private void AddImage(string filename, string window = "")
    {
        EventAddImage?.Invoke(filename, window, 0, 0);
    }
    private void ClearWindow(string sWindow)
    {
        EventClearWindow?.Invoke(sWindow);
    }

    // Round Time
    private void SetRoundTime(int iTime)
    {
        EventRoundTime?.Invoke(iTime);
    }

    // Reset Spell Time
    private void SetSpellTime()
    {
        EventSpellTime?.Invoke();
    }

    // Clear Spell Time
    private void ClearSpellTime()
    {
        EventClearSpellTime?.Invoke();
    }

    private void StatusBarUpdate()
    {
        EventStatusBarUpdate?.Invoke();
    }

    private void PrintError(string text)
    {
        // Honor prompt
        if (m_bLastRowWasPrompt)
        {
            m_bLastRowWasPrompt = false;
            var rowVar = System.Environment.NewLine + text;
            EventPrintError?.Invoke(rowVar);
        }
        else
        {
            EventPrintError?.Invoke(text);
        }
    }

    private void HandleGenieException(string section, string message, string description = null)
    {
        GenieError.Error(section, message, description);
    }

    private void GameSocket_EventConnected()
    {
        var switchExpr = m_oConnectState;
        switch (switchExpr)
        {
            case ConnectStates.ConnectingKeyServer:
                {
                    m_oConnectState = ConnectStates.ConnectedKey;
                    m_oSocket.Authenticate(AccountName, AccountPassword);
                    ParseKeyRow(m_oSocket.GetLoginKey(AccountGame, AccountCharacter));
                    break;
                }

            case ConnectStates.ConnectingGameServer:
                {
                    m_oConnectState = ConnectStates.ConnectedGameHandshake;
                    m_iConnectAttempts = 0;
                    m_bManualDisconnect = false;
                    m_oReconnectTime = default;
                    m_oSocket.Send(m_sConnectKey + Constants.vbLf + "FE:WRAYTH /VERSION:1.0.1.22 /P:WIN_UNKNOWN /XML" + Constants.vbLf);    // TEMP
                    string argkey = "connected";
                    Variables.Instance["connected"] = m_oSocket.IsConnected ? "1" : "0";
                    VariableChanged("$connected");
                    Variables.Instance["account"] = AccountName;
                    VariableChanged("$account");
                    m_bStatusPromptEnabled = false;                        
                    break;
                }
        }
    }

    private void GameSocket_EventDisconnected()
    {
        if (m_oConnectState == ConnectStates.ConnectedGame)
        {
            string argkey = "connected";
            string argvalue = m_oSocket.IsConnected ? "1" : "0";
            Variables.Instance.Add(argkey, argvalue, Variables.VariablesType.Reserved);
            string argsVariable = "$connected";
            VariableChanged(argsVariable);
            m_bStatusPromptEnabled = false;
        }
    }

    private void GameSocket_EventExit()
    {
        Disconnect(true);
    }
    private void GameSocket_EventParseRow(StringBuilder row)
    {
        var rowVar = row.ToString();
        ParseRow(rowVar);
    }

    private string ParsePluginText(string sText, string sWindow)
    {
        // Logic removed to Unsupported.Txt
        return sText;
    }

    private void GameSocket_EventParsePartialRow(string row)
    {
        if (m_oConnectState == ConnectStates.ConnectedKey | m_oConnectState == ConnectStates.ConnectedGameHandshake)
        {
            ParseRow(row);
        }
    }

    private void GameSocket_EventDataRecieveEnd()
    {
        EventDataRecieveEnd?.Invoke();
    }

    private void GameSocket_EventPrintText(string text)
    {
        WindowTarget argoWindowTarget = 0;
        bool argbIsRoomOutput = false;
        PrintTextWithParse(text, Color.White, Color.Transparent, oWindowTarget: argoWindowTarget, bIsRoomOutput: argbIsRoomOutput);
    }

    private void GameSocket_EventPrintError(string text)
    {
        PrintTextToWindow(text, Color.Red, Color.Transparent);
    }

    private bool m_bManualDisconnect = false;
    private DateTime m_oReconnectTime = default;
    private int m_iConnectAttempts = 0;

    public DateTime ReconnectTime
    {
        get
        {
            return m_oReconnectTime;
        }

        set
        {
            m_oReconnectTime = value;
        }
    }

    public int ConnectAttempts
    {
        get
        {
            return m_iConnectAttempts;
        }

        set
        {
            m_iConnectAttempts = value;
        }
    }

    private void GameSocket_EventConnectionLost()
    {
        if (ConfigSettings.Instance.Reconnect == true & m_bManualDisconnect == false)
        {
            if (m_iConnectAttempts == 0) // Attempt to connect right away
            {
                m_oReconnectTime = DateTime.Now;
                string argtext = Utility.GetTimeStamp() + " Attempting to reconnect.";
                PrintError(argtext);
            }
            else if (m_iConnectAttempts > 10) // After 10 attempts wait 30 seconds
            {
                m_oReconnectTime = DateTime.Now.AddSeconds(30);
                string argtext3 = Utility.GetTimeStamp() + " Attempting to reconnect in 30 seconds.";
                PrintError(argtext3);
            }
            else if (m_iConnectAttempts > 5) // After 5 attempts wait 15 seconds
            {
                m_oReconnectTime = DateTime.Now.AddSeconds(15);
                string argtext2 = Utility.GetTimeStamp() + " Attempting to reconnect in 15 seconds.";
                PrintError(argtext2);
            }
            else if (m_iConnectAttempts > 0) // After first attempt wait 5 seconds
            {
                m_oReconnectTime = DateTime.Now.AddSeconds(5);
                string argtext1 = Utility.GetTimeStamp() + " Attempting to reconnect in 5 seconds.";
                PrintError(argtext1);
            }
        }

        m_bManualDisconnect = false;
    }
}