using System.Collections;
using System.IO;
using System.Text;
using static GenieCoreLib.Macros;

namespace GenieCoreLib;

public class Classes : SortedList
{
    public static Classes Instance => m_Classes ??= new Classes();
    private static Classes m_Classes;

    public Classes()
    {
        m_Classes = this;
    }

    public void ActivateAll()
    {
        var oList = new ArrayList();
        if (AcquireReaderLock())
        {
            try
            {
                foreach (object key in base.Keys)
                    oList.Add(key);
            }
            finally
            {
                ReleaseReaderLock();
                foreach (object key in oList)
                    base[key] = true;
            }
        }
        else
        {
            throw new Exception("Unable to aquire reader lock.");
        }
    }

    public void InActivateAll()
    {
        var oList = new ArrayList();
        if (AcquireReaderLock())
        {
            try
            {
                foreach (object key in base.Keys)
                    oList.Add(key);
            }
            finally
            {
                ReleaseReaderLock();
                foreach (object key in oList)
                {
                    if ((key.ToString() ?? "") != "default")
                    {
                        base[key] = false;
                    }
                }
            }
        }
        else
        {
            throw new Exception("Unable to aquire reader lock.");
        }
    }

    public bool GetValue(string sKey)
    {
        if (sKey.Length == 0) // Default
        {
            return Conversions.ToBoolean(base["default"]);
        }
        else if (base.ContainsKey(sKey.ToLower()))
        {
            return Conversions.ToBoolean(base[sKey.ToLower()]);
        }
        else
        {
            object argvalue = true;
            Add(sKey.ToLower(), argvalue);
            return true;
        }
    }

    public new void Clear()
    {
        base.Clear();
        object argvalue = "True";
        Add("default", argvalue);
    }

    public bool Add(string sKey, string sValue)
    {
        bool bActive = false;
        var switchExpr = sValue.ToLower();
        switch (switchExpr)
        {
            case "true":
            case "on":
            case "1":
                {
                    bActive = true;
                    break;
                }

            case "false":
            case "off":
            case "0":
                {
                    bActive = false;
                    break;
                }
        }

        if (base.ContainsKey(sKey) == true)
        {
            base[sKey] = bActive;
        }
        else
        {
            object argvalue = bActive;
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
        return Load(Path.Combine(ConfigSettings.Instance.ConfigDir, "classes.cfg"));
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
                string argsKey = "default";
                string argsValue = "True";
                Add(argsKey, argsValue);
                string[] lines = File.ReadAllLines(sFileName);
                foreach (string strLine in lines)
                {
                    if (strLine.StartsWith("#class {") && strLine.EndsWith("}"))
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
            Add(oArgs[1].ToString(), oArgs[2].ToString().ToLower());
        }
    }

    public bool Save()
    {
        return Save(Path.Combine(ConfigSettings.Instance.ConfigDir, "classes.cfg"));
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
                sb.AppendLine("#class {" + Conversions.ToString(key).ToString() + "} {" + Conversions.ToBoolean(base[key]).ToString() + "}");
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
        return ListArray("Classes", keyPattern, valuePattern);
    }
    public string ListAll(string keyPattern)
    {
        return ListSubset(keyPattern);
    }

}