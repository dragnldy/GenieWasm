using System.IO;
using System.Text;
using static GenieCoreLib.Presets;
using static GenieCoreLib.Variables;

namespace GenieCoreLib;
public class Variables : SortedList
{
    public enum VariablesType
    {
        SaveToFile,
        Reserved,
        Server,
        Temporary,
        Ignore
    }

    public static Variables Instance => m_Variables ??= new Variables();
    private static Variables m_Variables;

    public Variables()
    {
        m_Variables = this;
        SetDefaultGlobalVars();
    }

    public class Variable
    {
        public string sKey;
        public string sValue;
        public Variables.VariablesType oType;

        public bool bSaveToFile
        {
            get
            {
                if (oType == Variables.VariablesType.SaveToFile)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            set
            {
                if (value == true)
                {
                    oType = Variables.VariablesType.SaveToFile;
                }
                else
                {
                    oType = Variables.VariablesType.Reserved;
                }
            }
        }

        public Variable(string _sKey, string _sValue, Variables.VariablesType _oType)
        {
            sKey = _sKey;
            sValue = _sValue;
            oType = _oType;
        }
        public string ToFormattedString(string sValuePattern)
        {
            return (oType.ToString().Equals(sValuePattern) ? sValue : string.Empty);
        }
    }

    public void Add(string key, string value, Variables.VariablesType oType = Variables.VariablesType.SaveToFile)
    {
        if (oType != Variables.VariablesType.Reserved)
        {

        }
        if (base.ContainsKey(key) == true)
        {
            ((Variable)base[key]).sValue = value;
            ((Variable)base[key]).oType = oType;
        }
        else
        {
            var oVar = new Variable(key, value, oType);
            object argvalue = oVar;
            Add(key, argvalue);
        }
    }

    // Do not remove built in variables
    public new void Remove(object key)
    {
        if (base.ContainsKey(key) == true)
        {
            if (((Variable)base[key]).oType != Variables.VariablesType.Reserved)
            {
                base.Remove(key);
            }
        }
    }

    public new void Clear(bool reloadDefaults = true)
    {
        base.Clear();
        if (reloadDefaults)
            SetDefaultGlobalVars();
    }

    public void ClearUser()
    {
        if (AcquireReaderLock())
        {
            var al = new ArrayList();
            try
            {
                foreach (string s in base.Keys)
                {
                    if (((Variable)base[s]).bSaveToFile)
                    {
                        al.Add(s);
                    }
                }
            }
            finally
            {
                ReleaseReaderLock();
                foreach (string s in al)
                    Remove(s);
            }
        }
        else
        {
            throw new Exception("Unable to aquire reader lock.");
        }
    }

    public new object this[object key]
    {
        get
        {
            if (base.ContainsKey(key) == true)
            {
                return ((Variable)base[key]).sValue;
            }
            else
            {
                return null;
            }
        }

        set
        {
            if (base.ContainsKey(key))
            {
                ((Variable)base[key]).sValue = Conversions.ToString(value);
            }
            else
            {
                string arg_sKey = Conversions.ToString(key);
                string arg_sValue = Conversions.ToString(value);
                var VariablesType = Variables.VariablesType.SaveToFile;
                var oVar = new Variable(arg_sKey, arg_sValue, VariablesType);
                object argvalue = oVar;
                Add(key, argvalue);
            }
        }
    }

    public Variable get_GetVariable(object key)
    {
        if (base.ContainsKey(key) == true)
        {
            return (Variable)base[key];
        }
        else
        {
            return null;
        }
    }

    //public void set_GetVariable(object key, Variable value)
    //{
    //    if (base.ContainsKey(key))
    //    {
    //        base[key] = value;
    //    }
    //    else
    //    {
    //        object argvalue = value;
    //        Add(key, argvalue);
    //    }
    //}

    public bool Load()
    {
        return Load(Path.Combine(ConfigSettings.Instance.ConfigDir, "variables.cfg"));
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
                    if (strLine.StartsWith("#var {") && strLine.EndsWith("}"))
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
            var arg1 = oArgs[1].ToString();
            var arg2 = oArgs[2].ToString();
            Add(arg1, arg2);
        }
    }

    public bool Save()
    {
        return Save(Path.Combine(ConfigSettings.Instance.ConfigDir, "variables.cfg"));
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
            foreach (Variable ov in base.Values)
            {
                if (ov.bSaveToFile)
                {
                    sb.AppendLine("#var {" + ov.sKey + "} {" + ov.sValue + "}");
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
    public void SetDefaultGlobalVars()
    {
        Add("north", "0", Variables.VariablesType.Reserved);
        Add("northeast", "0", Variables.VariablesType.Reserved);
        Add("east", "0", Variables.VariablesType.Reserved);
        Add("southeast", "0", Variables.VariablesType.Reserved);
        Add("south", "0", Variables.VariablesType.Reserved);
        Add("southwest", "0", Variables.VariablesType.Reserved);
        Add("west", "0", Variables.VariablesType.Reserved);
        Add("northwest", "0", Variables.VariablesType.Reserved);
        Add("up", "0", Variables.VariablesType.Reserved);
        Add("down", "0", Variables.VariablesType.Reserved);
        Add("out", "0", Variables.VariablesType.Reserved);

        Add("roomname", "", Variables.VariablesType.Reserved);
        Add("roomdesc", "", Variables.VariablesType.Reserved);
        Add("roomobjs", "", Variables.VariablesType.Reserved);
        Add("roomplayers", "", Variables.VariablesType.Reserved);
        Add("roomexits", "", Variables.VariablesType.Reserved);
        Add("roomnote", "", Variables.VariablesType.Reserved);

        Add("concentration", "100", Variables.VariablesType.Reserved);
        Add("encumbrance", "0", Variables.VariablesType.Reserved);
        Add("health", "100", Variables.VariablesType.Reserved);
        Add("mana", "100", Variables.VariablesType.Reserved);
        Add("spirit", "100", Variables.VariablesType.Reserved);
        Add("stamina", "100", Variables.VariablesType.Reserved);

        Add("charactername", "", Variables.VariablesType.Reserved);
        Add("account", "", Variables.VariablesType.Reserved);
        Add("gamename", "", Variables.VariablesType.Reserved);
        Add("gamehost", "eaccess.play.net", Variables.VariablesType.Reserved);
        Add("gameport", "7910", Variables.VariablesType.Reserved);

        Add("kneeling", "0", Variables.VariablesType.Reserved);
        Add("prone", "0", Variables.VariablesType.Reserved);
        Add("sitting", "0", Variables.VariablesType.Reserved);
        Add("standing", "0", Variables.VariablesType.Reserved);
        Add("stunned", "0", Variables.VariablesType.Reserved);
        Add("hidden", "0", Variables.VariablesType.Reserved);
        Add("invisible", "0", Variables.VariablesType.Reserved);
        Add("dead", "0", Variables.VariablesType.Reserved);
        Add("joined", "0", Variables.VariablesType.Reserved);
        Add("bleeding", "0", Variables.VariablesType.Reserved);
        Add("webbed", "0", Variables.VariablesType.Reserved);
        Add("roundtime", "0", Variables.VariablesType.Reserved);
        Add("preparedspell", "None", Variables.VariablesType.Reserved);
        Add("lefthand", "Empty", Variables.VariablesType.Reserved);
        Add("lefthandnoun", "", Variables.VariablesType.Reserved);
        Add("righthand", "Empty", Variables.VariablesType.Reserved);
        Add("righthandnoun", "", Variables.VariablesType.Reserved);
        Add("gametime", "0", Variables.VariablesType.Reserved);
        Add("poisoned", "0", Variables.VariablesType.Reserved);
        Add("diseased", "0", Variables.VariablesType.Reserved);
        Add("connected", "0", Variables.VariablesType.Reserved);
        Add("client", MyResources.GetApplicationName(), Variables.VariablesType.Reserved);
        Add("version", MyResources.GetApplicationVersion(), Variables.VariablesType.Reserved);
        Add("time", "@time@", Variables.VariablesType.Reserved);
        Add("time24", "@time24@", Variables.VariablesType.Reserved);
        Add("militarytime", "@militarytime@", Variables.VariablesType.Reserved);
        Add("date", "@date@", Variables.VariablesType.Reserved);
        Add("year", "@year@", Variables.VariablesType.Reserved);
        Add("month", "@month@", Variables.VariablesType.Reserved);
        Add("dayofmonth", "@dayofmonth@", Variables.VariablesType.Reserved);
        Add("dayofyear", "@dayofyear@", Variables.VariablesType.Reserved);
        Add("datetime", "@datetime@", Variables.VariablesType.Reserved);
        Add("datetime24", "@datetime24@", Variables.VariablesType.Reserved);
        Add("unixtime", "@unixtime@", Variables.VariablesType.Reserved);
        Add("spelltime", "@spelltime@", Variables.VariablesType.Reserved);
        Add("spellpreptime", "@spellpreptime@", Variables.VariablesType.Reserved);
        Add("spellstarttime", "0", Variables.VariablesType.Reserved);
        Add("casttime", "0", Variables.VariablesType.Reserved);
        Add("casttimeremaining", "@casttimeremaining@", Variables.VariablesType.Reserved);
        Add("monstercount", "0", Variables.VariablesType.Reserved);
        Add("monsterlist", "", Variables.VariablesType.Reserved);
        Add("prompt", "", Variables.VariablesType.Reserved);
        Add("lastcommand", "", Variables.VariablesType.Reserved);
        Add("zoneid", "0", Variables.VariablesType.Reserved);
        Add("zonename", "0", Variables.VariablesType.Reserved);
        Add("scriptlist", "none", Variables.VariablesType.Reserved);
        Add("repeatregex", @"^\.\.\.wait|^Sorry\, you may only type ahead|^You are still stunned|^You can\'t do that while|^You don\'t seem to be able", Variables.VariablesType.Reserved);

    }
    public string ListSubset(string keyPattern = "", string valuePattern = "")
    {
        return ListArray("Variables", keyPattern, valuePattern);
    }
    public string ListAll(string keyPattern = "", string valuePattern = "")
    {
        StringBuilder sb = new();
        sb.Append(ListSubset(keyPattern, "SaveToFile"));
        sb.Append(ListSubset(keyPattern, "Temporary"));
        sb.Append(ListSubset(keyPattern, "Reserved"));
        sb.Append(ListSubset(keyPattern, "Server"));
        return sb.ToString();
    }
}
