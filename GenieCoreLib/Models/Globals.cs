using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace GenieCoreLib;

public interface IGlobals
    {
    void UpdateMonsterListRegEx();
}

public class Globals : IGlobals, INotifyPropertyChanged
{
    public static Globals GetInstance() => _m_oGlobals ?? new Globals();
    public static Globals Instance => GetInstance();
    private static Globals _m_oGlobals;

    public List<VolatileHighlight> VolatileHighlights = new List<VolatileHighlight>();
    public List<VolatileHighlight> RoomObjects = new List<VolatileHighlight>();

    #region GameTime
    private int gameTime = 0;
    public int GameTime
    {
        get => gameTime;
        set
        {
            if (gameTime != value) { gameTime = value; NotifyPropertyChanged(); NotifyPropertyChanged("All"); }
        }
    }
    #endregion GameTime

    #region CastTime
    private int castTimeStart = 0;
    public int CastTimeStart //"spellstarttime
    {
        get => castTimeStart;
        set
        {
            if (castTimeStart != value) { castTimeStart = value; NotifyPropertyChanged(); }
        }
    }
    private int castTimeEnd = 0;
    public int CastTimeEnd // "casttime"
    {
        get => castTimeEnd;
        set
        {
            if (castTimeEnd != value) { castTimeEnd = value; NotifyPropertyChanged(); NotifyPropertyChanged("CastTimeLeft"); }
        }
    }
    public int CastTimeLeft
    {
        get => CastTimeEnd > gameTime ? CastTimeEnd - gameTime : 0;
        set
        {
            if (value == 0 && gameTime != CastTimeEnd)
            {
                castTimeStart = 0;
                castTimeEnd = 0;
            }
            NotifyPropertyChanged("CastTimeLeft");
        }
    }
    public int CastTimeLapsed => GameTime - CastTimeStart;
    #endregion CastTime

    #region GameRT
    private int gameRTStart = 0;
    public int GameRTStart
    {
        get => gameRTStart;
        set
        {
            if (gameRTStart != value) { gameRTStart = value; NotifyPropertyChanged("gameRTLeft"); }
        }
    }

    private int gameRTEnd = 0;
    public int GameRTEnd
    {
        get => gameRTEnd;
        set
        {
            if (gameRTEnd != value) { gameRTEnd = value; NotifyPropertyChanged("gameRTLeft"); }
        }
    }
    public int GameRTLeft
    {
        get => GameRTStart > 0? GameTime - GameRTStart : 0;
        set
        {
            if (value == 0 && gameTime != gameRTEnd)
            {
                gameRTStart = 0;
                gameRTEnd = 0;
            }
            NotifyPropertyChanged("gamertleft");
        }
    }
    public int GameRTTLapsed => GameTime - GameRTStart;
    #endregion GameRT



    private string _characterName = string.Empty;
    public string CharacterName
    {
        get => _characterName;
        set
        {
            _characterName = value ?? string.Empty;
        }
    }
    private string _gameName = string.Empty;
    public string GameName
    {
        get => _gameName;
        set
        {
            _gameName = value ?? string.Empty;
        }
    }
    public string CurrentProfileName
    {
        get => $"{CharacterName}{GameName}.xml";
    }

    public Globals()
    {
        _m_oGlobals = this;
        FileDirectory.CheckUserDirectory();
        //m_oLegacyPluginHost = new LegacyPluginHost(this, ref _m_oGlobals);
        //m_oPluginHost = new PluginHost(this, ref _m_oGlobals);
        //m_PluginDialog = new FormPlugins(ref _m_oGlobals.PluginList);
        // This call is required by the Windows Form Designer.
        //MapperSettings = new FormMapperSettings(ref _m_oGlobals) { MdiParent = this };
        //MapperSettings.EventVariableChanged += ClassCommand_EventVariableChanged;
        //MapperSettings.EventClassChange += Command_EventClassChange;

        // Add any initialization after the InitializeComponent() call.
    }

    public CommandQueue CommandQueue = new CommandQueue();
    public ArrayList PluginList = new ArrayList();
    public bool PluginsEnabled = true;
    public Hashtable PluginVerifiedKeyList = new Hashtable();
    public Hashtable PluginPremiumKeyList = new Hashtable();
    
    public List<string> MonsterList = new List<string>();
    public Regex MonsterListRegEx;

    public void UpdateMonsterListRegEx()
    {
        var sb = new StringBuilder();
        MonsterList.Sort();
        MonsterList.Reverse();
        foreach (string s in MonsterList)
        {
            if (sb.Length > 0)
                sb.Append("|");
            sb.Append(Regex.Escape(s));
        }

        if (MonsterList.Count == 0)
        {
            MonsterListRegEx = null;
        }
        else
        {
            MonsterListRegEx = new Regex(@"\b(" + sb.ToString() + @")[\ \,\.]", RegexOptions.IgnoreCase);
            Debug.WriteLine(MonsterListRegEx.ToString());
        }
    }

    public static string ParseGlobalVars(object sVar)
    {
        var sText = sVar.ToString();
        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(sText.Contains("$"), true, false)))
        {
            sText = sText.Replace(@"\$", @"\¤");
            if (Variables.Instance.AcquireReaderLock())
            {
                try
                {
                    // ' Replace global variables
                    // For Each de As DictionaryEntry In Variables.Instance
                    // sText = sText.Replace("$" & de.Key.ToString, CType(de.Value, Variables.Variable).sValue)
                    // Next

                    int p = Conversions.ToInteger(sText.Length - 1);
                    while (p >= 0)
                    {
                        var switchExpr = sText.Substring(p, 1);
                        switch (switchExpr)
                        {
                            case var @case when @case == "$":
                                {
                                    if (p <= 0 || Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(sText.Substring(p - 1, 1), @"\", false)))
                                    {
                                        sText = sText.Substring(0, p) + ParseVariable(Conversions.ToString(sText.Substring(p)));
                                    }

                                    break;
                                }
                        }

                        p -= 1;
                    }
                }
                finally
                {
                    Variables.Instance.ReleaseReaderLock();
                }
            }
            else
            {
                throw new Exception("Unable to aquire reader lock.");
            }

            sText = sText.Replace(@"\¤", "$");
        }

        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(sText.Contains("@"), true, false))) // On the fly variables
        {
            sText = ParseSpecialVariables(Conversions.ToString(sText));
        }

        return Conversions.ToString(sText);
    }

    public static string ParseSpecialVariables(string sText)
    {
        int spelltime = Globals.Instance.GameTime - Globals.Instance.CastTimeStart;
        if (spelltime > 0 & Globals.Instance.CastTimeStart != 0)
        {
            // Spelltime is the amount of time that has elapsed since the spell started
            sText = sText.Replace("@spelltime@", spelltime.ToString());

            // Casttime is game time when spell will be complete
            // Spellstartime is game time when spell started
            int castTime = int.Parse(Variables.Instance["casttime"].ToString());
            int spellStartTime = int.Parse(Variables.Instance["spellstarttime"].ToString());
            int spellpreptime =(castTime = spellStartTime);

            // The amount of time needs to cast
            sText = sText.Replace("@spellpreptime@", spellpreptime.ToString());
            int casttimeremaining = spellpreptime - spelltime;
            sText = sText.Replace("@casttimeremaining@", casttimeremaining > 0 ? casttimeremaining.ToString() : "0");
        }
        else
        {
            sText = sText.Replace("@spelltime@", "0");
            sText = sText.Replace("@casttimeremaining@", "0");
        }
        
        sText = sText.Replace("@time@", DateTime.Now.ToString("hh:mm:ss tt").Trim());
        sText = sText.Replace("@time24@", DateTime.Now.ToString("HH:mm:ss tt").Trim());
        sText = sText.Replace("@date@", DateTime.Now.ToString("M/d/yyyy").Trim());
        sText = sText.Replace("@datetime@", DateTime.Now.ToString("M/d/yyyy hh:mm:ss tt").Trim());
        sText = sText.Replace("@datetime24@", DateTime.Now.ToString("M/d/yyyy HH:mm:ss tt").Trim());
        sText = sText.Replace("@militarytime@", DateTime.Now.ToString("HHmm").Trim());
        sText = sText.Replace("@unixtime@", DateTimeOffset.Now.ToUnixTimeSeconds().ToString());

        sText = sText.Replace("@year@", DateTime.Now.ToString("yyyy").Trim());
        sText = sText.Replace("@month@", DateTime.Now.ToString("mm").Trim());
        sText = sText.Replace("@dayofmonth@", DateTime.Now.ToString("dd").Trim());
        sText = sText.Replace("@dayofyear@", DateTime.Now.DayOfYear.ToString().Trim());
        
        

        return sText;
    }

    public static string ParseVariable(string Line)
    {
        if (Line.Length <= 1)
            return Line;

        // Remove first char ($)
        string sVar = Line.Substring(1);

        // Trim down string if there is a space
        int I = sVar.IndexOf(' ');
        if (I > -1)
        {
            sVar = sVar.Substring(0, I);
        }

        // Check if it's an array
        I = sVar.IndexOf('(');
        if (I > -1 && sVar.IndexOf(')') > -1)
        {
            if (Variables.Instance.ContainsKey(sVar.Substring(0, I)))
            {
                int idx = 0;
                if (int.TryParse(sVar.Substring(I + 1, sVar.IndexOf(')') - I - 1), out idx))
                {
                    string sValue = Conversions.ToString(Variables.Instance[sVar.Substring(0, I)]);
                    var oArr = sValue.Split('|');
                    if (idx >= 0 && Information.UBound(oArr) >= idx)
                    {
                        return oArr[idx] + Line.Substring(Line.IndexOf(')') + 1);
                    }
                    else
                    {
                        // Invalid index - Return empty result and remainder string
                        // Return Line.Substring(Line.IndexOf(")"c) + 1)
                    }
                }
            }
        }

        // Loop trough hashtable of variables starting with the longer words
        int p = sVar.Length;
        string sCurrent = string.Empty;
        while (p > 0)
        {
            sCurrent = sVar.Substring(0, p);
            if (sCurrent.Contains(Conversions.ToString('.')))
            {
                if (sCurrent.ToLower().EndsWith(".length"))
                {
                    if (Variables.Instance.ContainsKey(sVar.Substring(0, p - 7)))
                    {
                        return (Utility.Count(Conversions.ToString(Variables.Instance[sVar.Substring(0, p - 7)]), "|") + 1).ToString() + Line.Substring(p + 1);
                    }
                }
            }

            if (Variables.Instance.ContainsKey(sCurrent))
            {
                return Conversions.ToString(Variables.Instance[sCurrent] + Line.Substring(p + 1));
            }

            p -= 1;
        }

        return Line;
    }

    public class DescendingComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            return Comparer.Default.Compare(y, x);
        }
    }
    #region Property Changed Notification
    public event PropertyChangedEventHandler PropertyChanged;

    // This method is called by the Set accessor of each property.
    // The CallerMemberName attribute that is applied to the optional propertyName
    // parameter causes the property name of the caller to be substituted as an argument.
    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        if (propertyName.Equals("All"))
        {
            NotifyPropertyChanged("CastTimeLeft");
            // When game time updates we have to check everything
            if (CastTimeLeft == 0)
            {
                castTimeEnd = 0;
                castTimeStart = 0;
            }
            NotifyPropertyChanged("GameRTLeft");
            if (GameRTLeft == 0)
            {
                gameRTEnd = 0;
                gameRTStart = 0;
            }
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Property Changed Notification
}