using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace GenieCoreLib;


public class HighlightsList : SortedList
{
    public static HighlightsList Instance => m_Highlights ??= new HighlightsList();
    private static HighlightsList m_Highlights;

    public HighlightsList()
    {
        m_Highlights = this;
    }

    public class Highlight : HighlightBase
    {
        public Highlight(Color oColor, string sColorName, Color oBgColor, bool bHighlightWholeRow, bool CaseSensitive = true, string SoundFile = "", string ClassName = "", bool IsActive = true)
        {
            HighlightWholeRow = bHighlightWholeRow;
            FgColor = oColor;
            BgColor = oBgColor;
            ColorName = sColorName;
            this.CaseSensitive = CaseSensitive;
            this.SoundFile = SoundFile;
            this.ClassName = ClassName;
            this.IsActive = IsActive;
        }
        public string ToFormattedString(string sValuePattern)
        {
            return $"Regular Expression: {base.ToFormattedString(sValuePattern)}";
        }
        public string ToFileString(string sKey)
        {
            string sColorName = ColorName;
            string sText = sKey;
            if (CaseSensitive == false)
            {
                sText = "/" + sText + "/i";
            }

            string sLine = string.Empty;
            if (HighlightWholeRow == true)
            {
                sLine = "#highlight {line} {" + sColorName + "} {" + sText + "}";
            }
            else
            {
                sLine = "#highlight {string} {" + sColorName + "} {" + sText + "}";
            }
            if (ClassName.Length > 0 || SoundFile.Length > 0)
            {
                sLine += " {" + ClassName + "}";
            }

            if (SoundFile.Length > 0)
            {
                sLine += " {" + SoundFile + "}";
            }
            return sLine;
        }
    }


    private Regex m_oRegexString = null;
    private Regex m_oRegexLine = null;

    public Regex RegexString
    {
        get
        {
            return m_oRegexString;
        }

        set
        {
            m_oRegexString = value;
        }
    }

    public Regex RegexLine
    {
        get
        {
            return m_oRegexLine;
        }

        set
        {
            m_oRegexLine = value;
        }
    }

    public void ToggleClass(string ClassName, bool Value)
    {
        if (AcquireReaderLock())
        {
            var al = new ArrayList();
            try
            {
                foreach (string s in base.Keys)
                    al.Add(s);
            }
            finally
            {
                ReleaseReaderLock();
                foreach (string s in al)
                {
                    Highlight hl = (Highlight)base[s];
                    if ((hl.ClassName.ToLower() ?? "") == (ClassName.ToLower() ?? ""))
                    {
                        hl.IsActive = Value;
                    }
                }
            }
        }
        else
        {
            throw new Exception("Unable to aquire reader lock.");
        }
    }

    public bool Add(string sKey, bool bHighlightWholeRow, string sColorName, bool bCaseSensitive = true, string SoundFile = "", string ClassName = "", bool IsActive = true)
    {
        if (sKey.Length == 0)
        {
            return false;
        }
        else
        {
            Color oColor;
            Color oBgcolor;
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

            if (base.ContainsKey(sKey) == true)
            {
                base[sKey] = new Highlight(oColor, sColorName, oBgcolor, bHighlightWholeRow, bCaseSensitive, SoundFile, ClassName, IsActive);
            }
            else
            {
                object argvalue = new Highlight(oColor, sColorName, oBgcolor, bHighlightWholeRow, bCaseSensitive, SoundFile, ClassName, IsActive);
                Add(sKey, argvalue);
            }

            return true;
        }
    }

    public int Remove(string sKey)
    {
        if (base.ContainsKey(sKey) == true)
        {
            base.Remove(sKey);
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public void RebuildStringIndex()
    {
        if (AcquireReaderLock())
        {
            try
            {
                var al = new ArrayList();
                foreach (string s in base.Keys)
                {
                    if (((Highlight)base[s]).IsActive == true)
                    {
                        if (((Highlight)base[s]).HighlightWholeRow == false)
                        {
                            al.Add(s);
                        }
                    }
                }

                al.Sort();
                string sList = string.Empty;
                foreach (string s in al)
                {
                    if (sList.Length > 0)
                    {
                        sList += "|";
                    }

                    sList += Regex.Replace(s, @"([^A-Za-z0-9\s])", @"\$1");
                }

                if (sList.Length > 0)
                {
                    sList = "(" + sList + ")";
                }

                RegexString = new Regex(sList);
            }
            finally
            {
                ReleaseReaderLock();
            }
        }
        else
        {
            throw new Exception("Unable to aquire writer lock.");
        }
    }

    public void RebuildLineIndex()
    {
        if (AcquireReaderLock())
        {
            try
            {
                var al = new ArrayList();
                foreach (string s in base.Keys)
                {
                    if (((Highlight)base[s]).IsActive == true)
                    {
                        if (((Highlight)base[s]).HighlightWholeRow == true)
                        {
                            al.Add(s);
                        }
                    }
                }

                al.Sort();
                string sList = string.Empty;
                foreach (string s in al)
                {
                    if (sList.Length > 0)
                    {
                        sList += "|";
                    }

                    sList += Regex.Replace(s, @"([^A-Za-z0-9\s])", @"\$1");
                }

                if (sList.Length > 0)
                {
                    sList = "(" + sList + ")";
                }

                RegexLine = new Regex(sList);
            }
            finally
            {
                ReleaseReaderLock();
            }
        }
        else
        {
            throw new Exception("Unable to aquire writer lock.");
        }
    }
    public bool Load()
    {
        return Load(Path.Combine(ConfigSettings.Instance.ConfigDir, "highlights.cfg"));
    }
    public bool Load(string sFileName)
    {
        try
        {
            if (sFileName.IndexOf(@"\") == -1 && sFileName.IndexOf(@"/") == -1)
            {
                sFileName = Path.Combine(ConfigSettings.Instance.ConfigDir, sFileName);
            }

            if (File.Exists(sFileName) == true)
            {
                string[] lines = File.ReadAllLines(sFileName);
                foreach (string strLine in lines)
                {
                    if (strLine.StartsWith("#highlight {") && strLine.EndsWith("}"))
                    {
                        AddHighlight(strLine);
                    }
                }
                RebuildLineIndex();
                RebuildStringIndex();

                return true;
            }
            else
            {
                return false;
            }
        }
#pragma warning disable CS0168
        catch (Exception Err)
#pragma warning restore CS0168
        {
            return false;
        }
    }
    private void AddHighlight(string sLine)
    {
        var oArgs = new ArrayList();
        oArgs = Utility.ParseArgs(sLine);
        if (oArgs.Count > 0)
        {
            if (Conversions.ToString(oArgs[0]).Length > 0)
            {
                string sClass = string.Empty;
                string sSound = string.Empty;
                string sHighlight = oArgs[3].ToString();
                if (sLine.Contains("{"))
                {
                    if (oArgs.Count > 4)
                        sClass = oArgs[4].ToString();
                    if (oArgs.Count > 5)
                        sSound = oArgs[5].ToString();
                }
                else // Add all args after 3 for highlights added without {} args.
                {
                    sHighlight = Utility.ArrayToString(oArgs, 3);
                }

                var switchExpr = Conversions.ToString(oArgs[0]).Substring(1);
                switch (switchExpr)
                {
                    case "highlight":
                    case "highlights":
                        {
                            var switchExpr1 = oArgs[1].ToString().ToLower();
                            switch (switchExpr1)
                            {
                                case "line":
                                case "lines":
                                    {
                                        if (oArgs.Count > 3)
                                        {
                                            bool argbHighlightWholeRow = true;
                                            var arg1 = oArgs[1].ToString();
                                            var arg2 = oArgs[2].ToString();
                                            var arg3 = oArgs[3].ToString();
                                            HighlightsList.Instance.Add(arg3, argbHighlightWholeRow, arg2, true, sSound, sClass);
                                        }

                                        break;
                                    }

                                case "string":
                                case "strings":
                                    {
                                        if (oArgs.Count > 3)
                                        {
                                            bool argbHighlightWholeRow1 = false;
                                            var arg1 = oArgs[1].ToString();
                                            var arg2 = oArgs[2].ToString();
                                            var arg3 = oArgs[3].ToString();
                                            HighlightsList.Instance.Add(arg3, argbHighlightWholeRow1, arg2, true, sSound, sClass);
                                        }

                                        break;
                                    }

                                case "beginswith":
                                    {
                                        if (oArgs.Count > 3)
                                        {
                                            var arg1 = oArgs[1].ToString();
                                            var arg2 = oArgs[2].ToString();
                                            var arg3 = oArgs[3].ToString();
                                            HighlightsBeginWithList.Instance.Add(arg3, arg2, true, sSound, sClass);
                                        }

                                        break;
                                    }

                                case "regexp":
                                case "regex":
                                    {
                                        if (oArgs.Count > 3)
                                        {
                                            string argsRegExp = Globals.ParseGlobalVars(oArgs[3].ToString());
                                            if (Utility.ValidateRegExp(argsRegExp) == true)
                                            {
                                                var arg1 = oArgs[1].ToString();
                                                var arg2 = oArgs[2].ToString();
                                                var arg3 = oArgs[3].ToString();
                                                HighlightsRegExpList.Instance.Add(arg3, arg2, true, sSound, sClass);
                                            }
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                }
            }
        }
    }

    public bool Save()
    {
        return Save(Path.Combine(ConfigSettings.Instance.ConfigDir, "highlights.cfg"));
    }
    public bool Save(string sFileName)
    {
        try
        {
            if (sFileName.IndexOf(@"\") == -1 && sFileName.IndexOf(@"/") == -1)
            {
                sFileName = Path.Combine(ConfigSettings.Instance.ConfigDir, sFileName);
            }
            StringBuilder sb = new();
            foreach (string sKey in HighlightsList.Instance.Keys)
            {
                HighlightsList.Highlight oHighlight = (HighlightsList.Highlight)HighlightsList.Instance[sKey];
                sb.AppendLine(oHighlight.ToFileString(sKey));
            }
            foreach (string sKey in HighlightsBeginWithList.Instance.Keys)
            {
                HighlightsBeginWithList.Highlight oHighlight = (HighlightsBeginWithList.Highlight)HighlightsBeginWithList.Instance[sKey];
                sb.AppendLine(oHighlight.ToFileString(sKey));
            }
            foreach (string sKey in HighlightsRegExpList.Instance.Keys)
            {
                HighlightsRegExpList.Highlight oHighlight = (HighlightsRegExpList.Highlight)HighlightsRegExpList.Instance[sKey];
                sb.AppendLine(oHighlight.ToFileString(sKey));
            }
            File.WriteAllText(sFileName, sb.ToString());
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    public string ListSubset(string keyPattern)
    {
        return ListArray("Highlights", keyPattern);
    }
    public string ListAll(string keyPattern = "")
    {
        return ListSubset(keyPattern);
    }
}
