using System.Text;
using System.Text.RegularExpressions;

namespace GenieCoreLib;

public class SubstituteRegExp : ArrayList
{
    public static SubstituteRegExp Instance => m_SubstituteRegExp ??= new SubstituteRegExp();
    private static SubstituteRegExp m_SubstituteRegExp;

    public SubstituteRegExp()
    {
        m_SubstituteRegExp = this;
    }

    public class Substitute
    {
        public string sText = string.Empty;
        public string sReplaceBy = string.Empty;
        public bool bIgnoreCase = false;
        public Regex SubstituteRegex = null;
        public string ClassName = string.Empty;
        public bool IsActive = true;

        public Substitute(string Text, string ReplaceBy, bool IgnoreCase = false, string ClassName = "", bool IsActive = true)
        {
            sText = Text;
            sReplaceBy = ReplaceBy;
            bIgnoreCase = IgnoreCase;
            if (IgnoreCase == true)
            {
                SubstituteRegex = new Regex(Text, MyRegexOptions.options | RegexOptions.IgnoreCase);
            }
            else
            {
                SubstituteRegex = new Regex(Text, MyRegexOptions.options);
            }

            this.ClassName = ClassName;
            this.IsActive = IsActive;
        }
    }

    public bool Add(string sText, string ReplaceBy, bool IgnoreCase = false, string ClassName = "", bool IsActive = true)
    {
        if (sText.StartsWith("/") == true)
        {
            sText = sText.Substring(1);
        }

        if (sText.EndsWith("/i") == true)
        {
            IgnoreCase = true;
            sText = sText.Substring(0, sText.Length - 2);
        }
        else if (sText.EndsWith("/") == true)
        {
            sText = sText.Substring(0, sText.Length - 1);
        }

        if (!Utility.ValidateRegExp(sText))
        {
            return false;
        }

        Substitute substitute = new Substitute(sText, ReplaceBy, IgnoreCase, ClassName, IsActive);

        int I = Contains(sText);
        if (I > -1)
        {
            set_Item(I, substitute);
        }
        else
        {
            Add(substitute);
        }

        return true;
    }

    public void ToggleClass(string ClassName, bool Value)
    {
        if (AcquireReaderLock())
        {
            var al = new ArrayList();
            try
            {
                for (int I = 0, loopTo = base.Count - 1; I <= loopTo; I++)
                {
                    if ((((Substitute)get_Item(I)).ClassName ?? "") == (ClassName ?? ""))
                    {
                        ((Substitute)get_Item(I)).IsActive = Value;
                    }
                }
            }
            finally
            {
                ReleaseReaderLock();
            }
        }
        else
        {
            throw new Exception("Unable to aquire reader lock.");
        }
    }

    public void Remove(string Text)
    {
        for (int I = 0, loopTo = base.Count - 1; I <= loopTo; I++)
        {
            if ((((Substitute)get_Item(I)).sText ?? "") == (Text ?? ""))
            {
                RemoveAt(I);
                Remove(Text); // Recursively remove all
                return;
            }
        }
    }

    public int Contains(string Text)
    {
        for (int I = 0, loopTo = base.Count - 1; I <= loopTo; I++)
        {
            if ((((Substitute)get_Item(I)).sText ?? "") == (Text ?? ""))
            {
                return I;
            }
        }

        return -1;
    }

    public bool Load()
    {
        return Load(GetFileName("substitutes.cfg"));
    }
    public bool Load(string sFileName)
    {
        try
        {
            if (sFileName.IndexOf(@"\") == -1 && sFileName.IndexOf(@"/") == -1)
            {
                sFileName = GetFileName(sFileName);
            }

            if (File.Exists(sFileName))
            {
                string[] lines = File.ReadAllLines(sFileName);
                foreach (string strLine in lines)
                {
                    if (strLine.StartsWith("#subs {") && strLine.EndsWith("}"))
                    {
                        LoadRow(strLine);
                    }
                }
                return true;
            }
            return false;
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
        if (oArgs.Count > 2)
        {
            string sClass = string.Empty;
            if (oArgs.Count > 3)
            {
                sClass = oArgs[3].ToString();
            }

            var arg1 = oArgs[1].ToString();
            var arg2 = oArgs[2].ToString();
            Add(arg1, arg2, false, sClass);
        }
    }
    private string GetFileName(string sFileName)
    {
        if (sFileName.IndexOf(@"\") == -1 && sFileName.IndexOf(@"/") == -1)
        {
            sFileName = Path.Combine(ConfigSettings.Instance.ConfigDir, sFileName);
        }
        return sFileName;
    }
    public bool Save()
    {
        return Save(GetFileName("substitutes.cfg"));
    }
    public bool Save(string sFileName)
    {
        try
        {
            if (sFileName.IndexOf(@"\") == -1)
            {
                sFileName = GetFileName(sFileName);
            }

            if (AcquireReaderLock())
            {
                try
                {
                    StringBuilder sb = new();
                    for (int I = 0, loopTo = base.Count - 1; I <= loopTo; I++)
                    {
                        Substitute os = (Substitute)get_Item(I);
                        string sKey = os.sText;
                        if (os.bIgnoreCase)
                        {
                            sKey = "/" + sKey + "/i";
                        }

                        string sLine = "#subs {" + sKey + "} {" + os.sReplaceBy + "}";
                        if (os.ClassName.Length > 0)
                        {
                            sLine += " {" + os.ClassName + "}";
                        }
                        sb.AppendLine(sLine);
                    }
                    File.WriteAllText(sFileName, sb.ToString());
                }
                finally
                {
                    ReleaseReaderLock();
                }
            }
            else
            {
                throw new Exception("Unable to aquire reader lock.");
            }

            return true;
        }
#pragma warning disable CS0168
        catch (Exception ex)
#pragma warning restore CS0168
        {
            return false;
        }
    }
}
