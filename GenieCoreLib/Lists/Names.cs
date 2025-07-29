using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace GenieCoreLib;

public class Names : SortedList
{
    public static Names Instance => m_Names ??= new Names();
    private static Names m_Names;

    public Names()
    {
        m_Names = this;
    }

    private Regex m_oRegexNames = null;
    public Regex RegexNames
    {
        get
        {
            return m_oRegexNames;
        }

        set
        {
            m_oRegexNames = value;
        }
    }

    public class Name
    {
        public Color FgColor;
        public Color BgColor;
        public string ColorName;

        public Name(Color oColor, Color oBgColor, string sColorName = "")
        {
            FgColor = oColor;
            BgColor = oBgColor;
            ColorName = sColorName;
        }
    }

    public bool Add(string sKey, string sColorName)
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
                base[sKey] = new Name(oColor, oBgcolor, sColorName);
            }
            else
            {
                object argvalue = new Name(oColor, oBgcolor, sColorName);
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

    public void RebuildRegExIndex()
    {
        var al = new ArrayList();
        foreach (string s in base.Keys)
            al.Add(s);
        al.Sort();
        string sList = string.Empty;
        foreach (string s in al)
        {
            if (sList.Length > 0)
            {
                sList += "|";
            }

            sList += s;
        }

        if (sList.Length > 0)
        {
            sList = @"\b(" + sList + @")\b";
        }

        m_oRegexNames = new Regex(sList, MyRegexOptions.options);
    }
    public bool Load()
    {
        return Load(Path.Combine(ConfigSettings.Instance.ConfigDir, "names.cfg"));
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
                    if (strLine.StartsWith("#name {") && strLine.EndsWith("}"))
                    {
                        LoadRow(strLine);
                    }
                }
                RebuildRegExIndex();
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception Err)
        {
            return false;
        }
    }
    private void LoadRow(string sText)
    {
        var oArgs = Utility.ParseArgs(sText);
        if (oArgs.Count == 3)
        {
            Add(oArgs[2].ToString(), oArgs[1].ToString());
        }
    }

    public bool Save()
    {
        return Save(Path.Combine(ConfigSettings.Instance.ConfigDir, "names.cfg"));
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
            foreach (string key in base.Keys)
            {
                string sColorName = ((Name)base[key]).ColorName;
                if (sColorName.Length == 0)
                {
                    sColorName = ColorCode.ColorToHex(((Name)base[key]).FgColor) + "," + ColorCode.ColorToHex(((Name)base[key]).BgColor);
                }
                sb.AppendLine("#name {" + sColorName + "} {" + key + "}");
            }

            File.WriteAllText(sFileName, sb.ToString());
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}