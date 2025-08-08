using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace GenieCoreLib;
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

public interface IGame
{
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
        DoHookups();
    }
    private void DoHookups()
    {
        Connection.Instance.EventParseRow += GameSocket_EventParseRow;
        Connection.Instance.EventParsePartialRow += GameSocket_EventParsePartialRow;
        Connection.Instance.EventDataRecieveEnd += GameSocket_EventDataRecieveEnd;
        
    }
    private void GameSocket_EventParseRow(StringBuilder row)
    {
        ParseRow(row.ToString(), GameConnection.Instance.ConnectState);
    }
    private void GameSocket_EventParsePartialRow(string row)
    {
        if (GameConnection.Instance.ConnectState == ConnectStates.ConnectedKey | GameConnection.Instance.ConnectState == ConnectStates.ConnectedGameHandshake)
        {
            ParseRow(row, GameConnection.Instance.ConnectState);
        }
    }

    public ConnectStates ParseRow(string sText, ConnectStates connectState)
    {
        return ParseRowAsync(sText, GameConnection.Instance.ConnectState).Result;
    }
    public async Task<ConnectStates> ParseRowAsync(string sText, ConnectStates connectState)
    {
        switch (connectState)
        {
            case ConnectStates.ConnectedKey:
                {
                    GameConnection.Instance.ParseKeyRow(sText);
                    return ConnectStates.ConnectedKey;
                }

            case ConnectStates.ConnectedGame:
                {
                    ParseGameRow(sText);
                    return ConnectStates.ConnectedGame;
                }

            case ConnectStates.ConnectedGameHandshake:
                {
                    await Task.Delay(1000);
                    Connection.Instance.Send(Constants.vbLf + Constants.vbLf);
                    return ConnectStates.ConnectedGame;
                }
        }
        return connectState;
    }

    public event EventAddImageEventHandler EventAddImage;
    public delegate void EventAddImageEventHandler(string filename, string window, int width, int height);


    public event EventClearWindowEventHandler EventClearWindow;
        public delegate void EventClearWindowEventHandler(string sWindow);

    public event EventDataRecieveEndEventHandler EventDataRecieveEnd;
        public delegate void EventDataRecieveEndEventHandler();

    public event EventRoundTimeEventHandler EventRoundTime;
        public delegate void EventRoundTimeEventHandler(int time);

    public event EventCastTimeEventHandler EventCastTime;
        public delegate void EventCastTimeEventHandler();

    public event EventTriggerParseEventHandler EventTriggerParse;
        public delegate void EventTriggerParseEventHandler(string text);

    public event EventTriggerMoveEventHandler EventTriggerMove;
        public delegate void EventTriggerMoveEventHandler();

    public event EventTriggerPromptEventHandler EventTriggerPrompt;
        public delegate void EventTriggerPromptEventHandler();

    public event EventVariableChangedEventHandler EventVariableChanged;
        public delegate void EventVariableChangedEventHandler(string sVariable);

    public event EventParseXMLEventHandler EventParseXML;
        public delegate void EventParseXMLEventHandler(string xml);

    public event EventStreamWindowEventHandler EventStreamWindow;
        public delegate void EventStreamWindowEventHandler(object sTitle, object sIfClosed, bool testing=false);

    public void VariableChangedxx(string variable, object value = null)
    {
            EventVariableChanged?.Invoke(variable);
    }

    public void SendGenieError(string section, string message, string exmessage = "")
    {
        GenieException.HandleGenieException(section, message, exmessage);
    }

    private bool m_bLastRowWasBlank = false;
    private bool m_bBold = false;
    private string m_sStyle = string.Empty;
    private string m_sRoomDesc = string.Empty;
    private string m_sRoomObjs = string.Empty;
    private string m_sRoomPlayers = string.Empty;
    private string m_sRoomExits = string.Empty;
    private int m_iEncumbrance = 0;
    private string m_sTriggerBuffer = string.Empty;
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
    private object m_oThreadLock = new object(); // Thread safety
    private bool m_bFamiliarLineParse = false;
    public bool IsLich = false;

    /* TODO ERROR: Skipped RegionDirectiveTrivia */
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

    public void EchoText(string sText, string sWindow)
    {
        TextFunctions.EchoText(sText, sWindow);
    }

    private bool m_bShowRawOutput = false;
    public bool ShowRawOutput
    {
        get => m_bShowRawOutput;
        set => m_bShowRawOutput = value;
    }

    private bool m_bLastRowWasPrompt = false;
    public bool LastRowWasPrompt
    {
        get => m_bLastRowWasPrompt;
        set => m_bLastRowWasPrompt = value;
    }

    private bool m_bStatusPromptEnabled = true;
    public bool StatusPromptEnabled
    {
        get => m_bStatusPromptEnabled;
        set => m_bStatusPromptEnabled = value;
    }

    public void SendText(string sText, bool bUserInput = false, string sOrigin = "")
    {
        string sShowText = sText;
        if (!Connection.Instance.IsConnected)
        {
            if (!sText.StartsWith(ConfigSettings.Instance.MyCommandChar.ToString()))
            {
                sShowText = "(" + sShowText + ")";
            }
        }
        else if (sText.StartsWith("qui", StringComparison.CurrentCultureIgnoreCase) | sText.StartsWith("exi", StringComparison.CurrentCultureIgnoreCase))
        {
            GameConnection.Instance.ReconnectTime = default;
            GameConnection.Instance.ManualDisconnect = true;
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
                SendGenieError("SendText", "Unable to aquire reader lock.");
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
            GameConnection.Instance.LastUserActivity = DateTime.Now;
            Connection.Instance.Send(sText + Constants.vbCrLf);
            Variables.Instance["lastcommand"] = sText;
            var lastCommandVar = "lastcommand";
            VariableChanged(lastCommandVar);
        }

        if (ConfigSettings.Instance.AutoLog == true)
        {
            Log.LogText(sShowText + System.Environment.NewLine, Conversions.ToString(Variables.Instance["charactername"]), Conversions.ToString(Variables.Instance["game"]));
        }
    }

    public void SendRaw(string text)
    {
        Connection.Instance.Send(text);
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
            SendGenieError("SetBufferEnd", "Unable to aquire game thread lock.");
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
            SendGenieError("UpdateRoom", "Unable to aquire game thread lock.");
        }
    }

    private bool m_bMonoOutput = false;
    private bool m_bPresetSpeechOutput = false;
    private bool m_bPresetWhisperOutput = false;
    private bool m_bPresetThoughtOutput = false;

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
                            if (Globals.Instance.GameName.StartsWith("GS")) gamecode = "GS";
                            // ToDo: Check if the image exists in the art directory
                            // if (FileHandler.FetchImage(attribute, ConfigSettings.Instance.ArtDir, gamecode).Result) AddImage(Path.Combine(gamecode, attribute), "portrait");
                        }
                        break;
                    }
                case "streamWindow":	// Window Names
                    {
                        var switchExpr2 = GetAttributeData(oXmlNode, "id");
                        switch (switchExpr2)
                        {
                            case "main":
                            case "game":
                            case "inv":
                            case "familiar":
                            case "thoughts":
                            case "logons":
                            case "death":
                            case "whispers":
                            case "assess":
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

                            EventStreamWindow?.Invoke(sTitle, sIfClosed);
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
                            case "game":
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
                            case "game":
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

                #region Done
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
                        if (barName == "conclevel")
                            barName = "concentration";
                        switch (barName)
                        {
                            case "health":
                            case "mana":
                            case "spirit":
                            case "stamina":
                            case "concentration":
                                {
                                    Variables.Instance.Add(barName, barValue.ToString(), Variables.VariablesType.Reserved);
                                    Variables.Instance.Add(barName + "BarText", barText, Variables.VariablesType.Reserved);
                                    VariableChanged($"${barName}");
                                    break;
                                }
                            case "encumlevel":
                            case "encumblevel":
                            case "encumbrance":
                                {
                                    // ToDo: Check if encumbrance is a valid bar
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
                        Variables.Instance.Add("preparedspell", sSpellName, Variables.VariablesType.Reserved);
                        VariableChanged("$preparedspell");
                        break;
                    }

                case "left":
                    {
                        GetHandContents(oXmlNode, "left");
                        break;
                    }

                case "right":
                    {
                        GetHandContents(oXmlNode, "right");
                        break;
                    }
                case "app":
                    {
                        var temp = GetAttributeData(oXmlNode, "char");
                        if (temp.Length > 0)
                        {
                            Variables.Instance.Add("charactername", temp, Variables.VariablesType.Reserved);
                            VariableChanged("$charactername");
                            string gameName = GetAttributeData(oXmlNode, "game");
                            gameName = gameName.Replace(":", "").Replace(" ", "");
                            Variables.Instance.Add("gamename", gameName, Variables.VariablesType.Reserved);
                            VariableChanged("$gamename");
                        }
                        break;
                    }
                case "indicator":
                    {
                        bool blnActive = (GetAttributeData(oXmlNode, "visible") ?? "") == "y";
                        var indicator = GetAttributeData(oXmlNode, "id");
                        ProcessAnIcon(indicator, blnActive);
                        break;
                    }
                #endregion Done

                case "roundTime":
                    {
                        // This is the time roundtime will end
                        int roundtime = int.Parse(GetAttributeData(oXmlNode, "value"));
                        Globals.Instance.GameRTStart = Globals.Instance.GameTime;
                        Variables.Instance.Add("roundtimestart", Globals.Instance.GameTime.ToString());
                        VariableChanged("$roundtimestart");

                        Globals.Instance.GameRTEnd = roundtime;
                        Variables.Instance.Add("roundtime", roundtime.ToString());
                        VariableChanged("$roundtime");
                        SetRoundTime(roundtime);
                        break;
                    }

                case "castTime":
                    {
                        var spell = Variables.Instance["preparedspell"]?.ToString() ?? "None";
                        if (string.IsNullOrEmpty(spell) || spell.Equals("None"))
                        {
                            // this is the official end of the spell
                            Globals.Instance.CastTimeLeft = 0;
                            Variables.Instance.Add("casttime", "0");
                            VariableChanged("$casttime");
                        }
                        else
                        {
                            // This is the gametime when  the spell will be fully prepared
                            Variables.Instance["spellstarttime"] = Globals.Instance.GameTime;
                            VariableChanged("$spellstarttime");
                            string value = GetAttributeData(oXmlNode, "value");
                            Globals.Instance.CastTimeEnd = int.Parse(value);
                            Variables.Instance.Add("casttime", value);
                            VariableChanged("$casttime");
                        }
                        break;
                    }
                case "spelltime":
                    {
                        /// I don't think this is in use
                        // Spelltime is when the spell starts
                        if(Variables.Instance["preparedspell"].ToString() == "None")
                        {
                            // Note that the 'add' logic checks for previous existence
                            Variables.Instance.Add("spellstarttime", "0");
                        }
                        else
                        {
                            Variables.Instance.Add("spellstarttime", GetAttributeData(oXmlNode, "value"));
                        }
                        VariableChanged("$spellstarttime");
                        break;
                    }


                case "prompt":
                    {
                        if (int.TryParse(GetAttributeData(oXmlNode, "time"), out int igameTime))
                        {
                            Globals.Instance.GameTime = igameTime;
                        }
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
                            else if (CheckIndicator(m_oIndicatorHash[Indicator.Dead]))
                            {
                                strBuffer += "DEAD";
                            }
                            else if (ConfigSettings.Instance.PromptForce)
                            {
                                if (CheckIndicator(m_oIndicatorHash[Indicator.Kneeling]))
                                {
                                    strBuffer += "K";
                                }

                                if (CheckIndicator(m_oIndicatorHash[Indicator.Sitting]))
                                {
                                    strBuffer += "s";
                                }

                                if (CheckIndicator(m_oIndicatorHash[Indicator.Prone]))
                                {
                                    strBuffer += "P";
                                }

                                if (CheckIndicator(m_oIndicatorHash[Indicator.Stunned]))
                                {
                                    strBuffer += "S";
                                }

                                if (CheckIndicator(m_oIndicatorHash[Indicator.Hidden]))
                                {
                                    strBuffer += "H";
                                }

                                if (CheckIndicator(m_oIndicatorHash[Indicator.Invisible]))
                                {
                                    strBuffer += "I";
                                }

                                if (CheckIndicator(m_oIndicatorHash[Indicator.Webbed]))
                                {
                                    strBuffer += "W";
                                }

                                if (CheckIndicator(m_oIndicatorHash[Indicator.Bleeding]))
                                {
                                    strBuffer += "!";
                                }

                                if (CheckIndicator(m_oIndicatorHash[Indicator.Joined]))
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
                        int gameTime = 0;
                        if (int.TryParse(GetAttributeData(oXmlNode, "time"), out gameTime))
                        {
                            Globals.Instance.GameTime = gameTime;
                            Variables.Instance.Add("gametime", gameTime.ToString(), Variables.VariablesType.Reserved);
                            VariableChanged("$gametime");
                            int roundTime = Globals.Instance.GameRTEnd;
                            int rt = roundTime - gameTime;
                            if (rt > 0)
                            {
                                if (m_bStatusPromptEnabled == false && (ConfigSettings.Instance.PromptForce == true))
                                    strBuffer += "R";
                                rt += Convert.ToInt32(ConfigSettings.Instance.RTOffset);
                                Variables.Instance.Add("roundtime", rt.ToString(), Variables.VariablesType.Reserved);
                            }
                            else
                            {
                                Variables.Instance.Add("roundtime","0", Variables.VariablesType.Reserved);
                            }
                            VariableChanged("$roundtime");

                            if (ConfigSettings.Instance.Prompt.Length > 0 && !m_bLastRowWasPrompt)
                            {
                                strBuffer = strBuffer.Replace(ConfigSettings.Instance.Prompt.Trim(), "");
                                strBuffer += ConfigSettings.Instance.Prompt;
                                bool argbIsPrompt = true;
                                WindowTarget argoWindowTarget = 0;
                                //Status prompt set from the XML printing to main/game 
                                PrintTextWithParse(strBuffer, argbIsPrompt, oWindowTarget: argoWindowTarget);
                            }

                            Variables.Instance.Add("prompt", strBuffer, Variables.VariablesType.Reserved);
                            VariableChanged("$prompt");
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

    private void GetHandContents(XmlNode oXmlNode, string v)
    {
        string hand = v.ToLower() + "hand";
        Variables.Instance.Add(hand, GetTextFromXML(oXmlNode), Variables.VariablesType.Reserved);
        string noun = GetAttributeData(oXmlNode, "noun");
        Variables.Instance.Add($"{hand}noun", noun, Variables.VariablesType.Reserved);
        string id = GetAttributeData(oXmlNode, "exist");
        Variables.Instance.Add($"{hand}id", id, Variables.VariablesType.Reserved);
        VariableChanged($"${hand}");
        VariableChanged($"${hand}noun");
        VariableChanged($"${hand}id");
    }

    private bool CheckIndicator(object? v)
    {
        return (v is not null && v is bool blnActive && blnActive);
    }

    private void SaveIcon(string iconId, bool isActive)
    {
        var id = iconId.Replace("Icon", "").ToLower();
        Variables.Instance.Add(id, (isActive? "1" : "0"), Variables.VariablesType.Reserved);
        VariableChanged($"${id}");
        return;
    }
    private void ProcessAnIcon(string iconId, bool blnActive)
    {
        switch (iconId)
        {
            case "IconKNEELING":
                {
                    m_oIndicatorHash[Indicator.Kneeling] = blnActive;
                    SaveIcon(iconId, blnActive);
                    break;
                }

            case "IconPRONE":
                {
                    m_oIndicatorHash[Indicator.Prone] = blnActive;
                    SaveIcon(iconId, blnActive);
                    break;
                }

            case "IconSITTING":
                {
                    m_oIndicatorHash[Indicator.Sitting] = blnActive;
                    SaveIcon(iconId, blnActive);
                    break;
                }

            case "IconSTANDING":
                {
                    m_oIndicatorHash[Indicator.Standing] = blnActive;
                    SaveIcon(iconId, blnActive);
                    break;
                }

            case "IconSTUNNED":
                {
                    m_oIndicatorHash[Indicator.Stunned] = blnActive;
                    SaveIcon(iconId, blnActive);
                    break;
                }

            case "IconHIDDEN":
                {
                    m_oIndicatorHash[Indicator.Hidden] = blnActive;
                    SaveIcon(iconId, blnActive);
                    break;
                }

            case "IconINVISIBLE":
                {
                    m_oIndicatorHash[Indicator.Invisible] = blnActive;
                    SaveIcon(iconId, blnActive);
                    break;
                }

            case "IconDEAD":
                {
                    m_oIndicatorHash[Indicator.Dead] = blnActive;
                    SaveIcon(iconId, blnActive);
                    break;
                }

            case "IconWEBBED":
                {
                    m_oIndicatorHash[Indicator.Webbed] = blnActive;
                    SaveIcon(iconId, blnActive);
                    break;
                }

            case "IconJOINED":
                {
                    m_oIndicatorHash[Indicator.Joined] = blnActive;
                    SaveIcon(iconId, blnActive);
                    break;
                }

            case "IconBLEEDING":
                {
                    m_oIndicatorHash[Indicator.Bleeding] = blnActive;
                    SaveIcon(iconId, blnActive);
                    break;
                }

            // I don't think DR supports these, but leaving them here for now
            case "IconPOISONED":
            case "IconDISEASED":
                {
                    SaveIcon(iconId, blnActive);
                    break;
                }

            default:
                {
                    break;
                }
        }
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

    private MatchCollection m_oMatchCollection;

    public void PrintTextWithParse(string sText, bool bIsPrompt = false, WindowTarget oWindowTarget = WindowTarget.Unknown)
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
                SendGenieError("PrintTextWithParse", "Unable to aquire reader lock.");
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

    public void PrintTextToWindow(string text, Color color, Color bgcolor, WindowTarget targetwindow = WindowTarget.Main, bool isprompt = false, bool isroomoutput = false)
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
                    sTargetWindowString = AppGlobals.MainWindow;
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
                SendGenieError("PrintTextToWindow", "Unable to aquire reader lock.");
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
                SendGenieError("PrintTextToWindow", "Unable to aquire reader lock.");
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
            GameConnection.Instance.ReconnectTime = default;
            GameConnection.Instance.ManualDisconnect = true;
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

        TextFunctions.EchoFormattedText(text, targetwindowstring, color, bgcolor, isMono: m_bMonoOutput, isItalic: isprompt);
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
                SendGenieError("PrintTextToWindow", "Unable to aquire reader lock.");
            }
        }

        return text;
    }
    public void VariableChanged(string sVariable)
    {
        switch (sVariable)
        {
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

            case "$prompt": // Safety
                {
                    //                    IconBar.UpdateBleeding();
                    break;
                }
        }
        EventVariableChanged?.Invoke(sVariable);
        Command.Instance.TriggerVariableChanged(sVariable);
    }

    private void TriggerParse(string sText)
    {
        if (sText.Trim().Length > 0)
        {
            EventTriggerParse?.Invoke(sText);
        }
    }

    // Skip all blank line/prompt checks and just print it
    public void PrintInputText(string sText, Color oColor, Color oBgColor)
    {
        TextFunctions.EchoFormattedText(sText, "Game", oColor, oBgColor);
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
        // Have the UI do its thing
        EventRoundTime?.Invoke(iTime);
    }

    private void GameError(string text, string window="")
    {
        TextFunctions.EchoError(text, window);
    }
    public void PrintError(string text)
    {
        // Honor prompt
        if (m_bLastRowWasPrompt)
        {
            m_bLastRowWasPrompt = false;
            var rowVar = System.Environment.NewLine + text;
            GameError(rowVar);
        }
        else
        {
            GameError(text);
        }
    }

    public void HandleGenieException(string section, string message, string description = null)
    {
        SendGenieError(section, message, description);
    }

    public void Game_EventTriggerParse(string sText)
    {
        try
        {
            ParseTriggers(sText);
        }
        /* TODO ERROR: Skipped IfDirectiveTrivia */
        catch (Exception ex)
        {
            HandleGenieException("TriggerParse", ex.Message, ex.ToString());
            /* TODO ERROR: Skipped ElseDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
        }
    }
    // BufferWait is since script always wait for end of buffer before setting off an action... We don't want this with #parse
    private Match oRegMatch;

    private void ParseTriggers(string sText, bool bBufferWait = true)
    {
        if (Command.Instance.TriggersEnabled == true)
        {
            if (sText.Trim().Length > 0)
            {
                if (Triggers.Instance.AcquireReaderLock())
                {
                    try
                    {
                        foreach (Triggers.Trigger oTrigger in Triggers.Instance.Values)
                        {
                            if (oTrigger.IsActive)
                            {
                                if (oTrigger.bIsEvalTrigger == false)
                                {
                                    if (!Information.IsNothing(oTrigger.oRegexTrigger))
                                    {
                                        oRegMatch = oTrigger.oRegexTrigger.Match(sText);
                                        if (oRegMatch.Success == true)
                                        {
                                            var RegExpArg = new ArrayList();
                                            if (oRegMatch.Groups.Count > 0)
                                            {
                                                int J;
                                                var loopTo = oRegMatch.Groups.Count - 1;
                                                for (J = 1; J <= loopTo; J++)
                                                    RegExpArg.Add(oRegMatch.Groups[J].Value);
                                            }

                                            Command.Instance.TriggerAction(oTrigger.sAction, RegExpArg);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    /* TODO ERROR: Skipped IfDirectiveTrivia */
                    catch (Exception ex)
                    {
                        EchoText("Error in TriggerAction", "Debug");
                        EchoText("---------------------", "Debug");
                        EchoText(ex.Message, "Debug");
                        EchoText("---------------------", "Debug");
                        EchoText(ex.ToString(), "Debug");
                        EchoText("---------------------", "Debug");
                    }
                    /* TODO ERROR: Skipped EndIfDirectiveTrivia */
                    finally
                    {
                        Triggers.Instance.ReleaseReaderLock();
                    }
                }
                else
                {
                    EchoText("TriggerList: Unable to acquire reader lock.","Log");
                }

                // Scripts
                if (ScriptList.Instance.AcquireReaderLock())
                {
                    try
                    {
                        foreach (Script oScript in ScriptList.Instance)
                            oScript.TriggerParse(sText, bBufferWait);
                    }
                    /* TODO ERROR: Skipped IfDirectiveTrivia */
                    catch (Exception ex)
                    {
                        EchoText("Error in TriggerParse", "Debug");
                        EchoText("---------------------", "Debug");
                        EchoText(ex.Message, "Debug");
                        EchoText("---------------------", "Debug");
                        EchoText(ex.ToString(), "Debug");
                        EchoText("---------------------", "Debug");

                    }
                    /* TODO ERROR: Skipped EndIfDirectiveTrivia */
                    finally
                    {
                        ScriptList.Instance.ReleaseReaderLock();
                    }
                }
                else
                {
                    EchoText("TriggerParse:Unable to acquire reader lock.","Log");
                }
            }
        }
    }

    private void GameSocket_EventExit()
    {
        GameConnection.Instance.Disconnect(true);
    }
    private void GameSocket_EventDataRecieveEnd()
    {
            TextFunctions.EchoText("EndUpdate: Starting...", "Log");
            EndUpdate();
            SetBufferEnd();
            ScriptManager.Instance.EventEndUpdate();
    }
    private void EndUpdate()
    {
        //FormSkin oFormSkin;
        //var oEnumerator = m_oFormList.GetEnumerator();
        //while (oEnumerator.MoveNext())
        //{
        //    oFormSkin = (FormSkin)oEnumerator.Current;
        //    oFormSkin.RichTextBoxOutput.EndTextUpdate();
        //}
    }


    private string ParsePluginText(string sText, string sWindow)
    {
        // Logic removed to Unsupported.Txt
        return sText;
    }

}