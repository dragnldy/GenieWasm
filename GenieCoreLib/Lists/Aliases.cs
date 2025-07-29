using System.Text;

namespace GenieCoreLib;

public class Aliases : SortedList
{
    public static Aliases Instance => m_Aliases ??= new Aliases();
    private static Aliases m_Aliases;

    public Aliases()
    {
        m_Aliases = this;
    }

    public bool Add(string sKey, string sAlias)
    {
        if (base.ContainsKey(sKey) == true)
        {
            base[sKey] = sAlias;
        }
        else
        {
            object argvalue = sAlias;
            Add(sKey, argvalue);
        }

        return true;
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

    public bool Load()
    {
        return Load(Path.Combine(ConfigSettings.Instance.ConfigDir, "aliases.cfg"));
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
                    if (strLine.StartsWith("#alias {") && strLine.EndsWith("}"))
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
            Add(oArgs[1].ToString(), oArgs[2].ToString());
        }
    }

    public bool Save()
    {
        return Save(Path.Combine(ConfigSettings.Instance.ConfigDir, "aliases.cfg"));
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
                sb.AppendLine("#alias {" + Conversions.ToString(key).ToString() + "} {" + base[key].ToString() + "}");
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