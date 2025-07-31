using System.Drawing;

namespace GenieCoreLib;

public class HighlightBeginsWithList : SortedList
{
    public static HighlightBeginsWithList Instance => m_HighlightLineBeginsWith ??= new HighlightBeginsWithList();
    private static HighlightBeginsWithList m_HighlightLineBeginsWith;

    public HighlightBeginsWithList()
    {
        m_HighlightLineBeginsWith = this;
    }

    public class Highlight: HighlightBase
    {
        public string Text = string.Empty;

        public Highlight(string Text, string ColorName, Color FgColor, Color BgColor, bool CaseSensitive = true, string SoundFile = "", string ClassName = "", bool IsActive = true)
        {
            this.Text = Text;
            this.ColorName = ColorName;
            this.FgColor = FgColor;
            this.BgColor = BgColor;
            this.CaseSensitive = CaseSensitive;
            this.SoundFile = SoundFile;
            this.ClassName = ClassName;
            this.IsActive = IsActive;
        }
        public string ToFormattedString(string sValuePattern)
        {
            return $"Line Begins With: {Text} {base.ToFormattedString(sValuePattern)}";
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

    public bool Add(string sKey, string sColorName, bool CaseSensitive = true, string SoundFile = "", string ClassName = "", bool IsActive = true)
    {
        if (sKey.Length == 0)
        {
            return false;
        }
        else
        {
            if (sKey.StartsWith("/") == true)
            {
                sKey = sKey.Substring(1);
            }

            if (sKey.EndsWith("/i") == true)
            {
                CaseSensitive = false;
                sKey = sKey.Substring(0, sKey.Length - 2);
            }
            else if (sKey.EndsWith("/") == true)
            {
                sKey = sKey.Substring(0, sKey.Length - 1);
            }

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
                base[sKey] = new Highlight(sKey, sColorName, oColor, oBgcolor, CaseSensitive, SoundFile, ClassName, IsActive);
            }
            else
            {
                object argvalue = new Highlight(sKey, sColorName, oColor, oBgcolor, CaseSensitive, SoundFile, ClassName, IsActive);
                Add(sKey, argvalue);
            }

            return true;
        }
    }
}


