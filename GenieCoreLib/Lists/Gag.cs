using System.Text;
using System.Text.RegularExpressions;

namespace GenieCoreLib;

public class GagRegExp : ArrayList
{
    public static GagRegExp Instance => m_GagRegExp ??= new GagRegExp();
    private static GagRegExp m_GagRegExp;

    public GagRegExp()
    {
        m_GagRegExp = this;
    }

    public class Gag
    {
        public string Text = string.Empty;
        public bool bIgnoreCase = false;
        public Regex RegexGag = null;
        public string ClassName = string.Empty;
        public bool IsActive = true;

        public Gag(string text, bool ignoreCase = false, string className = "", bool isActive = true)
        {
            Text = text;
            bIgnoreCase = ignoreCase;
            ClassName = className;
            IsActive = isActive;
            if (ignoreCase)
            {
                RegexGag = new Regex(Text, bIgnoreCase ? (MyRegexOptions.options | RegexOptions.IgnoreCase) : MyRegexOptions.options);
            }
        }
    }

    public bool Add(string sText, bool IgnoreCase = false, string ClassName = "", bool IsActive = true)
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

        if (Utility.ValidateRegExp(sText) == false)
        {
            return false;
        }

        Gag newGag = new Gag(sText, IgnoreCase, ClassName, IsActive);

        int I = Contains(sText);
        if (I > -1)
        {
            set_Item(I, newGag);
        }
        else
        {
            Add(newGag);
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
                    if ((((Gag)get_Item(I)).ClassName ?? "") == (ClassName ?? ""))
                    {
                        ((Gag)get_Item(I)).IsActive = Value;
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
            if ((((Gag)get_Item(I)).Text ?? "") == (Text ?? ""))
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
            if ((((Gag)get_Item(I)).Text ?? "") == (Text ?? ""))
            {
                return I;
            }
        }

        return -1;
    }

    public bool Load()
    {
        return Load(Path.Combine(ConfigSettings.Instance.ConfigDir, "gags.cfg"));
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
                    if (strLine.StartsWith("#gag {") && strLine.EndsWith("}"))
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
        if (oArgs.Count > 1)
        {
            string sClass = string.Empty;
            if (oArgs.Count > 2)
            {
                sClass = oArgs[2].ToString();
            }

            var arg1 = oArgs[0].ToString();
            var arg2 = oArgs[1].ToString();
            Add(arg2, false, sClass);

        }
    }

    public bool Save()
    {
        return Save(Path.Combine(ConfigSettings.Instance.ConfigDir, "gags.cfg"));
    }
    public bool Save(string sFileName)
    {
        try
        {
            if (sFileName.IndexOf(@"\") == -1 && sFileName.IndexOf(@"/") == -1)
            {
                sFileName = Path.Combine(ConfigSettings.Instance.ConfigDir, sFileName);
            }

            if (AcquireReaderLock())
            {
                try
                {
                    StringBuilder sb = new();

                    for (int I = 0, loopTo = base.Count - 1; I <= loopTo; I++)
                    {
                        Gag os = (Gag)get_Item(I);
                        string sKey = os.Text;
                        if (os.bIgnoreCase == true)
                        {
                            sKey = "/" + sKey + "/i";
                        }

                        string sLine = "#gag {" + sKey + "}";
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
