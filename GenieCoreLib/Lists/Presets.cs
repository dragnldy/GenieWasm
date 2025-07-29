using System.Drawing;
using System.Text;

namespace GenieCoreLib;

public class Presets : SortedList
{
    public static Presets Instance => m_Presets ??= new Presets();
    private static Presets m_Presets;

    public Presets()
    {
        m_Presets = this;
        SetDefaultPresets();
    }

    public class Preset
    {
        public string sKey;
        public Color FgColor;
        public Color BgColor;
        public string sColorName;
        public bool bHighlightLine = false;
        public bool bSaveToFile = true;

        public Preset(string sKey, Color oColor, Color oBgColor, string sColorName, string _bSaveToFile, bool highlightLine)
        {
            this.sKey = sKey;
            FgColor = oColor;
            BgColor = oBgColor;
            this.sColorName = sColorName;
            bSaveToFile = Conversions.ToBoolean(_bSaveToFile);
            bHighlightLine = highlightLine;
        }
    }

    public void Add(string sKey, string sColorName, bool bSaveToFile = true, bool highlightLine = false)
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

        string arg_bSaveToFile = Conversions.ToString(bSaveToFile);
        var oVar = new Preset(sKey, oColor, oBgcolor, sColorName, arg_bSaveToFile, highlightLine);
        if (base.ContainsKey(sKey.ToLower()) == true)
        {
            base[sKey.ToLower()] = oVar;
        }
        else
        {
            object argvalue = oVar;
            Add(sKey.ToLower(), argvalue);
        }
    }

    // Do not remove built in variables
    public void Remove(string key)
    {
        if (base.ContainsKey(key) == true)
        {
            if (((Preset)base[key]).bSaveToFile == true)
            {
                base.Remove(key);
            }
        }
    }

    public new void Clear(bool reloadDefaults = true)
    {
        base.Clear();
        if (reloadDefaults)
            SetDefaultPresets();
    }

    public Preset this[string key]
    {
        get
        {
            if (base.ContainsKey(key) == true)
            {
                return (Preset)base[key];
            }
            else
            {
                return null;
            }
        }

        set
        {
            base[key] = value;
        }
    }

    public bool Load()
    {
        return Load(Path.Combine(ConfigSettings.Instance.ConfigDir, "presets.cfg"));
    }
    public bool Load(string sFileName)
    {
        try
        {
            if (sFileName.IndexOf(@"\") == -1 && sFileName.IndexOf(@"/") == -1)
            {
                sFileName = Path.Combine(ConfigSettings.Instance.ConfigDir,sFileName);
            }

            if (File.Exists(sFileName) == true)
            {
                string[] lines = File.ReadAllLines(sFileName);
                foreach (string strLine in lines)
                {
                    if (strLine.StartsWith("#preset {") && strLine.EndsWith("}"))
                    {
                        LoadRow(strLine);
                    }
                }
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

    private void LoadRow(string sText)
    {
        var oArgs = Utility.ParseArgs(sText);
        if (oArgs.Count == 3)
        {
            //preserve this for loading configs which predate new parameters
            Add(oArgs[1].ToString(), oArgs[2].ToString(), true, false);
        }
        else if (oArgs.Count == 4)
        {
            Add(oArgs[1].ToString(), oArgs[2].ToString(), true, oArgs[3].ToString().ToLower() == "true");
        }
    }

    public bool Save()
    {
        return Save(Path.Combine(ConfigSettings.Instance.ConfigDir, "presets.cfg"));
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
            foreach (Preset ov in base.Values)
            {
                if (ov.bSaveToFile == true)
                {
                    sb.AppendLine("#preset {" + ov.sKey + "} {" + ov.sColorName + "} {" + ov.bHighlightLine + "}");
                }
            }
            File.WriteAllText(sFileName, sb.ToString());
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public void SetDefaultPresets()
    {
        Add("automapper.panel", "Black, PaleGoldenrod");
        Add("automapper.line", "Black, White");
        Add("automapper.linestump", "Cyan, White");
        Add("automapper.lineclimb", "Green, White");
        Add("automapper.linego", "Blue, White");
        Add("automapper.node", "White, White");
        Add("automapper.path", "Green, LightGreen");
        Add("castbar", "Magenta");
        Add("concentration", "Navy");
        Add("creatures", "Cyan");
        Add("familiar", "PaleGreen");
        Add("health", "Maroon");
        Add("inputuser", "Yellow");
        Add("inputother", "GreenYellow");
        Add("mana", "Navy");
        Add("roomdesc", "Silver");
        Add("roomname", "Yellow,DarkBlue");
        Add("roundtime", "MediumBlue");
        Add("scriptecho", "Cyan");
        Add("speech", "Yellow");
        Add("spirit", "Purple");
        Add("stamina", "Green");
        Add("thoughts", "Cyan");
        Add("ui.menu", "Black, #EEEEEE");
        Add("ui.menu.checked", "LightBlue");
        Add("ui.menu.highlight", "LightBlue");
        Add("ui.window", "Black, #EEEEEE");
        Add("ui.status", "Black, #EEEEEE");
        Add("ui.textbox", "Black, White");
        Add("ui.button", "Black, Silver");
        Add("whispers", "Magenta");
    }
}

