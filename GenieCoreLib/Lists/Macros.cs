using System.IO;
using System.Text;

namespace GenieCoreLib;

public class Macros : SortedList
{
    public static Macros Instance => m_Macros ??= new Macros();
    private static Macros m_Macros;

    public Macros()
    {
        m_Macros = this;
    }

    public class Macro
    {
        public string sKey = string.Empty;
        public string sAction = string.Empty;

        public Macro(string sKey, string sMacro)
        {
            this.sKey = sKey;
            sAction = sMacro;
        }
        public string ToFormattedString(string sValuePattern = "")
        {
            return $"Key: {sKey} Action: {sAction}";
        }
    }

    public bool Add(string sKey, string sMacro)
    {
        Keys oKey;
        oKey = KeyCode.StringToKey(sKey);
        if (oKey == GenieCoreLib.Keys.None)
        {
            return false;
        }
        else
        {
            if (base.ContainsKey(oKey) == true)
            {
                base[oKey] = new Macro(sKey, sMacro);
            }
            else
            {
                object argvalue = new Macro(sKey, sMacro);
                Add(oKey, argvalue);
            }

            return true;
        }
    }

    public int Remove(string sKey)
    {
        Keys oKey;
        oKey = KeyCode.StringToKey(sKey);
        if (oKey == GenieCoreLib.Keys.None)
        {
            return -1;
        }
        else if (base.ContainsKey(oKey) == true)
        {
            Remove(oKey);
            return 1;
        }
        else
        {
            return 0;
        }
    }
    public bool Load()
    {
        return Load(Path.Combine(ConfigSettings.Instance.ConfigDir, "macros.cfg"));
    }
    public bool Load(string sFileName)
    {
        try
        {
            if (sFileName.IndexOf(@"\") == -1)
            {
                sFileName = Path.Combine(ConfigSettings.Instance.ConfigDir, sFileName);
            }

            if (File.Exists(sFileName) == true)
            {
                string[] lines = File.ReadAllLines(sFileName);
                foreach (string strLine in lines)
                {
                    if (strLine.StartsWith("#macro {") && strLine.EndsWith("}"))
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
            GenieCoreLib.Keys oKey = KeyCode.StringToKey(oArgs[1].ToString());
            if (oKey != GenieCoreLib.Keys.None)
            {
                string argsKey = oKey.ToString();
                Add(argsKey, oArgs[2].ToString());
            }
        }
    }

    public bool Save()
    {
        return Save(Path.Combine(ConfigSettings.Instance.ConfigDir, "macros.cfg"));
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
            foreach (object key in base.Keys)
            {
                sb.AppendLine("#macro {" + ((Keys)key).ToString() + "} {" + ((Macro)base[key]).sAction + "}");
            }
            File.WriteAllText(sFileName, sb.ToString());
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    public string ListSubset(string keyPattern = "", string valuePattern = "")
    {
        return ListArray("Macros", keyPattern, valuePattern);
    }
    public string ListAll(string keyPattern)
    {
        return ListSubset(keyPattern);
    }

}