using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static GenieCoreLib.Macros;

namespace GenieCoreLib;
public class Triggers : SortedList
{
    public static Triggers Instance => m_Triggers ??= new Triggers();
    private static Triggers m_Triggers;

    public Triggers()
    {
        m_Triggers = this;
    }

    public class Trigger
    {
        public string sTrigger = string.Empty;
        public string sAction = string.Empty;
        public bool bIgnoreCase = false;
        public bool bIsEvalTrigger = false;
        public Regex oRegexTrigger = null;
        public string ClassName = string.Empty;
        public bool IsActive = true;

        public Trigger(string _sTrigger, string _sAction, bool _bIgnoreCase, bool _bIsEvalTrigger, string _ClassName)
        {
            sTrigger = _sTrigger;
            sAction = _sAction;
            bIgnoreCase = _bIgnoreCase;
            bIsEvalTrigger = _bIsEvalTrigger;
            if (_bIsEvalTrigger == false)
            {
                if (_bIgnoreCase == true)
                {
                    oRegexTrigger = new Regex(_sTrigger, MyRegexOptions.options | RegexOptions.IgnoreCase);
                }
                else
                {
                    oRegexTrigger = new Regex(_sTrigger, MyRegexOptions.options);
                }
            }

            ClassName = _ClassName;
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
                    Trigger tr = (Trigger)base[s];
                    if ((tr.ClassName.ToLower() ?? "") == (ClassName.ToLower() ?? ""))
                    {
                        tr.IsActive = Value;
                    }
                }
            }
        }
        else
        {
            throw new Exception("Unable to aquire reader lock.");
        }
    }

    public bool Add(string sTrigger, string sAction, bool bIgnoreCase = false, bool bIsEvalTrigger = false, string ClassName = "")
    {
        if (sTrigger.StartsWith("e/") == true)
        {
            sTrigger = sTrigger.Substring(2);
            bIsEvalTrigger = true;
        }
        else if (sTrigger.ToLower().StartsWith("eval ") == true)
        {
            sTrigger = sTrigger.Substring(5);
            bIsEvalTrigger = true;
        }
        else if (sTrigger.StartsWith("/") == true)
        {
            sTrigger = sTrigger.Substring(1);
        }

        if (sTrigger.EndsWith("/i") == true)
        {
            bIgnoreCase = true;
            sTrigger = sTrigger.Substring(0, sTrigger.Length - 2);
        }
        else if (sTrigger.EndsWith("/") == true)
        {
            sTrigger = sTrigger.Substring(0, sTrigger.Length - 1);
        }

        if (bIsEvalTrigger == false && Utility.ValidateRegExp(sTrigger) == false)
        {
            return false;
        }

        if (base.ContainsKey(sTrigger) == true)
        {
            base[sTrigger] = new Trigger(sTrigger, sAction, bIgnoreCase, bIsEvalTrigger, ClassName);
        }
        else
        {
            object argvalue = new Trigger(sTrigger, sAction, bIgnoreCase, bIsEvalTrigger, ClassName);
            Add(sTrigger, argvalue);
        }

        return true;
    }

    public void Remove(string sTrigger)
    {
        if (sTrigger.StartsWith("e/") == true)
        {
            sTrigger = sTrigger.Substring(2);
        }
        else if (sTrigger.ToLower().StartsWith("eval ") == true)
        {
            sTrigger = sTrigger.Substring(5);
        }
        else if (sTrigger.StartsWith("/") == true)
        {
            sTrigger = sTrigger.Substring(1);
        }

        if (sTrigger.EndsWith("/i") == true)
        {
            sTrigger = sTrigger.Substring(0, sTrigger.Length - 2);
        }
        else if (sTrigger.EndsWith("/") == true)
        {
            sTrigger = sTrigger.Substring(0, sTrigger.Length - 1);
        }

        if (base.ContainsKey(sTrigger) == true)
        {
            base.Remove(sTrigger);
        }
    }

    public bool Load()
    {
        return Load(Path.Combine(ConfigSettings.Instance.ConfigDir, "triggers.cfg"));
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
                    if (strLine.StartsWith("#trigger {") && strLine.EndsWith("}"))
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
        if (oArgs.Count > 2)
        {
            string sClass = string.Empty;
            if (oArgs.Count > 3)
                sClass = oArgs[3].ToString().Trim();

            var arg1 = oArgs[1].ToString();
            var arg2 = oArgs[2].ToString();
            Add(arg1, arg2, false, false, sClass);
        }
    }

    public bool Save()
    {
        return Save(Path.Combine(ConfigSettings.Instance.ConfigDir, "triggers.cfg"));
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
            foreach (Trigger ot in base.Values)
            {
                string sKey = ot.sTrigger;
                if (ot.bIsEvalTrigger == true)
                {
                    sKey = "e/" + sKey + "/";
                }
                else if (ot.bIgnoreCase == true)
                {
                    sKey = "/" + sKey + "/i";
                }

                string sLine = "#trigger {" + sKey + "} {" + ot.sAction + "}";
                if (ot.ClassName.Length > 0)
                {
                    sLine += " {" + ot.ClassName + "}";
                }
                sb.AppendLine(sLine);
            }
            File.WriteAllText(sFileName, sb.ToString());
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    public bool AddTrigger(string sTrigger, string sAction, bool bIgnoreCase = false, bool bIsEvalTrigger = false, string ClassName = "")
    {
        if (sTrigger.StartsWith("e/") == true)
        {
            bIsEvalTrigger = true;
        }
        else if (sTrigger.ToLower().StartsWith("eval ") == true)
        {
            bIsEvalTrigger = true;
        }

        if (bIsEvalTrigger == false)
        {
            sTrigger = Globals.ParseGlobalVars(sTrigger);
        }

        return Add(sTrigger, sAction, bIgnoreCase, bIsEvalTrigger, ClassName);
    }

}
