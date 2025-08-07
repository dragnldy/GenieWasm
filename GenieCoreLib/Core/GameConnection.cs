namespace GenieCoreLib;

public enum ConnectStates
{
    Disconnected,
    ConnectingKeyServer,
    ConnectingGameServer,
    ConnectedKey,
    ConnectedGameHandshake,
    ConnectedGame
}

public class GameConnection
{
    public static GameConnection Instance => _instance ??= new GameConnection();
    private static GameConnection? _instance;
    private string m_sConnectHost = string.Empty;
    private int m_sConnectPort = 0;
    private string m_sConnectKey = string.Empty;


    public GameConnection()
    {
        _instance = this;
        HookupEvents();
    }
    private void HookupEvents()
    {
        Connection.Instance.EventConnected += GameSocket_EventConnected;
        Connection.Instance.EventDisconnected += GameSocket_EventDisconnected;
        Connection.Instance.EventConnectionLost += GameSocket_EventConnectionLost;
    }

    public bool IsConnected
    {
        get => Connection.Instance.IsConnected;
    }

    public bool IsConnectedToGame
    {
        get => m_oConnectState == ConnectStates.ConnectedGame || m_oConnectState == ConnectStates.ConnectedGameHandshake;
    }

    private CharacterProfile? characterProfile = null;
    public CharacterProfile? Profile
    {
        get => characterProfile;
        set => characterProfile = value;
    }

    private bool isLich = false;
    public bool IsLich
    {
        get => isLich;
        set => isLich = value;
    }

    private ConnectStates m_oConnectState = ConnectStates.Disconnected;
    public ConnectStates ConnectState
    {
        get => m_oConnectState;
        set => m_oConnectState = value;
    }

    public DateTime LastServerActivity
    {
        get => Connection.Instance.LastServerActivity;
    }

    private DateTime m_oLastUserActivity = DateTime.Now;
    public DateTime LastUserActivity
    {
        get => m_oLastUserActivity;
        set => m_oLastUserActivity = value;
    }

    private DateTime m_oReconnectTime = default;
    public DateTime ReconnectTime
    {
        get => m_oReconnectTime;
        set => m_oReconnectTime = value;
    }

    private int m_iConnectAttempts = 0;
    public int ConnectAttempts
    {
        get => m_iConnectAttempts;
        set => m_iConnectAttempts = value;
    }

    private bool m_bManualDisconnect = false;
    public bool ManualDisconnect
    {
        get => m_bManualDisconnect;
        set => m_bManualDisconnect = value;
    }

    private string m_sLoginKey = string.Empty;
    public string LoginKey
    {
        get => m_sLoginKey;
        set => m_sLoginKey = value;
    }

    private ArrayList characterList = new ArrayList();
    public ArrayList CharacterList
    {
        get => characterList;
    }

    public void Reconnect()
    {
        if (m_oReconnectTime == default || DateTime.Now >= m_oReconnectTime)
        {
            if (!Profile.CheckValid()) return;

            ManualDisconnect = false;
            ConnectAttempts = 0;
            if (IsConnected)
                Disconnect();
            DoConnect(m_sConnectHost, m_sConnectPort);
        }
    }

    public void Connect(bool isLich = false, bool isTesting = false)
    {
        if (Profile is null)
        {
            ConnectionError("Profile is not set. Cannot connect to key server.");
            return;
        }
        LastUserActivity = DateTime.Now;
        Variables.Instance["charactername"] = Profile.Character;
        Variables.Instance["game"] = Profile.Game;
        Variables.Instance["account"] = Profile.Account.ToUpper();
        IsLich = isLich;
        if (isTesting)
            DoConnect("127.0.0.1", 8888, isTesting);
        else
            DoConnect(AppGlobals.Host, AppGlobals.Port);
    }
    private void DoConnect(string sHostName, int iPort, bool isTesting = false)
    {
        Connection.IsTesting = isTesting;
        m_oConnectState = ConnectStates.ConnectingKeyServer;
        Connection.Instance.ConnectAndAuthenticate(sHostName, iPort);
    }
    public void Connect(CharacterProfile profile, bool isLich = false, bool isTesting=false)
    {
        Profile = profile;
        Connect(isLich,isTesting);
    }

    private void ConnectionError(string errorMessage, string window="")
    {
        TextFunctions.EchoError("Connection Error: " + errorMessage,window);
    }

    public void Disconnect(bool ExitOnDisconnect = false)
    {
        if (Connection.Instance.IsConnected)
        {
            Connection.Instance.Disconnect(ExitOnDisconnect);
            m_oConnectState = ConnectStates.Disconnected;
        }
    }

    private void GameSocket_EventConnected()
    {
        var switchExpr = m_oConnectState;
        switch (switchExpr)
        {
            case ConnectStates.ConnectingKeyServer:
                {
                    try
                    {
                        if (Profile is not null)
                        {
                            m_oConnectState = ConnectStates.ConnectedKey;
                            Connection.Instance.Authenticate(Profile.Account, CharacterProfile.GetDecryptedPassword(Profile.Account, Profile.EncryptedPassword));
                            ParseKeyRow(Connection.Instance.GetLoginKey(Profile.Game, Profile.Character));
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ConnectionError("Error connecting to key server: " + ex.Message);
                        m_oConnectState = ConnectStates.Disconnected;
                        m_iConnectAttempts++;
                        m_bManualDisconnect = false;
                        m_oReconnectTime = default;
                    }
                    ConnectionError("Profile is not set. Cannot connect to key server.");
                    break;
                }

            case ConnectStates.ConnectingGameServer:
                {
                    m_oConnectState = ConnectStates.ConnectedGameHandshake;
                    m_iConnectAttempts = 0;
                    m_bManualDisconnect = false;
                    m_oReconnectTime = default;
                    Connection.Instance.Send(m_sConnectKey + Constants.vbLf + "FE:WRAYTH /VERSION:1.0.1.22 /P:WIN_UNKNOWN /XML" + Constants.vbLf);
                    string argkey = "connected";
                    Variables.Instance["connected"] = Connection.Instance.IsConnected ? "1" : "0";
                    Game.Instance.VariableChanged("$connected");
                    Variables.Instance["account"] = Profile.Account;
                    Game.Instance.VariableChanged("$account");
                    Game.Instance.StatusPromptEnabled = false;
                    break;
                }
        }
    }

    private void GameSocket_EventDisconnected()
    {
        if (m_oConnectState == ConnectStates.ConnectedGame)
        {
            string argkey = "connected";
            string argvalue = Connection.Instance.IsConnected ? "1" : "0";
            Variables.Instance.Add(argkey, argvalue, Variables.VariablesType.Reserved);
            string argsVariable = "$connected";
            Game.Instance.VariableChanged(argsVariable);
            Game.Instance.StatusPromptEnabled = false;
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
                ConnectionError(argtext);
            }
            else if (m_iConnectAttempts > 10) // After 10 attempts wait 30 seconds
            {
                m_oReconnectTime = DateTime.Now.AddSeconds(30);
                string argtext3 = Utility.GetTimeStamp() + " Attempting to reconnect in 30 seconds.";
                ConnectionError(argtext3);
            }
            else if (m_iConnectAttempts > 5) // After 5 attempts wait 15 seconds
            {
                m_oReconnectTime = DateTime.Now.AddSeconds(15);
                string argtext2 = Utility.GetTimeStamp() + " Attempting to reconnect in 15 seconds.";
                ConnectionError(argtext2);
            }
            else if (m_iConnectAttempts > 0) // After first attempt wait 5 seconds
            {
                m_oReconnectTime = DateTime.Now.AddSeconds(5);
                string argtext1 = Utility.GetTimeStamp() + " Attempting to reconnect in 5 seconds.";
                ConnectionError(argtext1);
            }
        }
        m_bManualDisconnect = false;
    }


    public void ParseKeyRow(string sText)
    {
        if (Connection.IsTesting || AppGlobals.IsLocalServer())
        {
            m_oConnectState = ConnectStates.ConnectedGame;
            GameSocket_EventConnected();
            return;
        }
        if (sText.Length == 32)
        {
            // Commented out code that was not used in the original context.
            //Connection.Instance.Send("A" + Constants.vbTab + Profile.Account.ToUpper() + Constants.vbTab);
            //Connection.Instance.Send(CharacterProfile.GetDecryptedPassword(Profile.Account, Profile.EncryptedPassword));
            //Connection.Instance.Send(System.Environment.NewLine);
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
                            ConnectionError(argtext);
                            Connection.Instance.Disconnect();
                            break;
                        }

                    // This code block was commented out since doesn't appear to be used.

                    //case "A":
                    //    {
                    //        var switchExpr1 = oData[2];
                    //        switch (switchExpr1)
                    //        {
                    //            case "KEY":
                    //                {
                    //                    m_sLoginKey = Conversions.ToString(oData[3]);
                    //                    accountOwner = Conversions.ToString(oData[4]);
                    //                    Connection.Instance.Send("G" + Constants.vbTab + .ToUpper() + System.Environment.NewLine);
                    //                    break;
                    //                }

                    //            case "NORECORD":
                    //                {
                    //                    string argtext1 = "Account does not exist.";
                    //                    ConnectionError(argtext1);
                    //                    Connection.Instance.Disconnect();
                    //                    break;
                    //                }

                    //            case "PASSWORD":
                    //                {
                    //                    string argtext2 = "Invalid password.";
                    //                    ConnectionError(argtext2);
                    //                    Connection.Instance.Disconnect();
                    //                    break;
                    //                }

                    //            case "REJECT":
                    //                {
                    //                    string argtext3 = "Access rejected.";
                    //                    ConnectionError(argtext3);
                    //                    Connection.Instance.Disconnect();
                    //                    break;
                    //                }
                    //        }

                    //        break;
                    //    }

                    case "G":
                        {
                            Connection.Instance.Send("C" + System.Environment.NewLine);
                            break;
                        }

                    case "C":
                        {
                            // See if we are asking for a list of characters
                            if (Profile.Character.Trim().Length == 0)
                            {
                                string argtext4 = "Listing characters:";
                                ConnectionError(argtext4);
                                string strUserKey = string.Empty;
                                for (int i = 5, loopTo = oData.Count - 1; i <= loopTo; i++)
                                {
                                    if (i % 2 == 0)
                                    {
                                        characterList.Clear();
                                        characterList.Add(oData[i].ToString());
                                        var temp = oData[i].ToString();
                                        ConnectionError(temp);
                                    }
                                    else
                                    {
                                        strUserKey = oData[i].ToString();
                                    }
                                }
                                Connection.Instance.Disconnect();
                            }
                            else
                            {
                                string strUserKey = string.Empty;
                                string strUserKeyTemp = string.Empty;
                                bool blnFoundMatch = false;
                                for (int i = 5, loopTo1 = oData.Count - 1; i <= loopTo1; i++)
                                {
                                    if (i % 2 == 0)
                                    {
                                        string sChar = oData[i].ToString();
                                        if (sChar.Contains(" "))
                                            sChar = sChar.Substring(0, sChar.IndexOf(' '));
                                        if (sChar.ToUpper().Equals(Profile.Character.ToUpper()))
                                        {
                                            blnFoundMatch = true;
                                            strUserKey = strUserKeyTemp;
                                        }

                                        if (blnFoundMatch == false)
                                        {
                                            if (sChar.ToUpper().StartsWith(Profile.Character.ToUpper()))
                                            {
                                                blnFoundMatch = true;
                                                strUserKey = strUserKeyTemp;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        strUserKeyTemp = oData[i].ToString();                  }
                                }

                                if (blnFoundMatch)
                                {
                                    Connection.Instance.Send("L" + Constants.vbTab + strUserKey + Constants.vbTab + "STORM" + Constants.vbLf);
                                }

                                if (blnFoundMatch == false)
                                {
                                    string argtext5 = "Character not found.";
                                    ConnectionError(argtext5);
                                    Connection.Instance.Disconnect();
                                }
                            }

                            break;
                        }
                    case "E": //Indicates an Error Message
                        {
                            string[] errorStrings = sText.Split("\t");
                            for (int i = 1; i < errorStrings.Length; i++)
                            {
                                ConnectionError(errorStrings[i]);
                            }
                            Connection.Instance.Disconnect();
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
                                    Connection.Instance.Disconnect();
                                    m_oConnectState = ConnectStates.ConnectingGameServer;
                                    Connection.Instance.Connect(m_sConnectHost, m_sConnectPort);
                                }
                            }
                            else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oData[1], "PROBLEM", false)))
                            {
                                string argtext6 = "There is a problem with your account. Log in to play.net website for more information.";
                                ConnectionError(argtext6);
                                Connection.Instance.Disconnect();
                            }

                            break;
                        }
                }
            }
        }
    }
}
