using System.Drawing;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace GenieCoreLib;
public class HighlightBase
{
    public Color FgColor;
    public Color BgColor;
    public string ColorName = string.Empty;
    public bool HighlightWholeRow = false;
    public bool CaseSensitive = true;
    public string ClassName = string.Empty;
    public bool IsActive = true;
    public string SoundFile = string.Empty;
    public string ToFormattedString(string sValuePattern)
    {
        return $"Color: {ColorName}, FgColor: {FgColor}, BgColor: {BgColor}, CaseSensitive: {CaseSensitive}, HighlightWholeRow: {HighlightWholeRow}" +
            $"SoundFile: {SoundFile}, ClassName: {ClassName}, IsActive: {IsActive}";
    }

    public static bool SaveHighlights()
    {
        return SaveHighlights(Path.Combine(ConfigSettings.Instance.ConfigDir, "highlights.cfg"));
    }
    public static bool SaveHighlights(string sFileName)
    {
        if (sFileName.IndexOf(@"\") == -1 && sFileName.IndexOf(@"/") == -1)
        {
            sFileName = Path.Combine(ConfigSettings.Instance.ConfigDir, sFileName);
        }

        StringBuilder sb = new();

        foreach (string sKey in HighlightsList.Instance.Keys)
        {
            HighlightsList.Highlight oHighlight = (HighlightsList.Highlight)HighlightsList.Instance[sKey];
            string sColorName = oHighlight.ColorName;
            string sText = sKey;
            if (oHighlight.CaseSensitive == false)
            {
                sText = "/" + sText + "/i";
            }

            string sLine = string.Empty;
            if (oHighlight.HighlightWholeRow == true)
            {
                sLine = "#highlight {line} {" + sColorName + "} {" + sText + "}";
            }
            else
            {
                sLine = "#highlight {string} {" + sColorName + "} {" + sText + "}";
            }

            if (oHighlight.ClassName.Length > 0 | oHighlight.SoundFile.Length > 0)
            {
                sLine += " {" + oHighlight.ClassName + "}";
            }

            if (oHighlight.SoundFile.Length > 0)
            {
                sLine += " {" + oHighlight.SoundFile + "}";
            }

            sb.AppendLine(sLine);
        }

        foreach (string sKey in HighlightBeginsWithList.Instance.Keys)
        {
            HighlightBeginsWithList.Highlight oHighlight = (HighlightBeginsWithList.Highlight)HighlightBeginsWithList.Instance[sKey];
            string sColorName = oHighlight.ColorName;
            string sText = oHighlight.Text;
            if (oHighlight.CaseSensitive == false)
            {
                sText = "/" + sText + "/i";
            }

            string sLine = "#highlight {beginswith} {" + sColorName + "} {" + sText + "}";
            if (oHighlight.ClassName.Length > 0 | oHighlight.SoundFile.Length > 0)
            {
                sLine += " {" + oHighlight.ClassName + "}";
            }

            if (oHighlight.SoundFile.Length > 0)
            {
                sLine += " {" + oHighlight.SoundFile + "}";
            }

            sb.AppendLine(sLine);
        }

        foreach (string sKey in HighlightRegExpList.Instance.Keys)
        {
            HighlightRegExpList.Highlight oHighlight = (HighlightRegExpList.Highlight)HighlightRegExpList.Instance[sKey];
            string sColorName = oHighlight.ColorName;
            string sText = oHighlight.Text;
            if (oHighlight.CaseSensitive == false)
            {
                sText = "/" + sText + "/i";
            }

            string sLine = "#highlight {regexp} {" + sColorName + "} {" + sText + "}";
            if (oHighlight.ClassName.Length > 0 | oHighlight.SoundFile.Length > 0)
            {
                sLine += " {" + oHighlight.ClassName + "}";
            }

            if (oHighlight.SoundFile.Length > 0)
            {
                sLine += " {" + oHighlight.SoundFile + "}";
            }

            sb.AppendLine(sLine);
        }
        return true;
    }

    public static bool LoadHighlights()
    {
        return LoadHighlights(Path.Combine(ConfigSettings.Instance.ConfigDir, "highlights.cfg"));
    }
    public static bool LoadHighlights(string sFileName)
    {
        if (sFileName.IndexOf(@"\") == -1 && sFileName.IndexOf(@"/") == -1)
        {
            sFileName = Path.Combine(ConfigSettings.Instance.ConfigDir, sFileName);
        }
        try
        {
            if (File.Exists(sFileName) == true)
            {
                string[] lines = File.ReadAllLines(sFileName);
                foreach (string strLine in lines)
                {
                    var oArgs = Utility.ParseArgs(strLine);
                    if (oArgs.Count > 3)
                    {
                        AddHighlight(strLine);
                    }
                }
                HighlightsList.Instance.RebuildLineIndex();
                HighlightsList.Instance.RebuildStringIndex();
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
    private static void AddHighlight(string sLine)
    {
        var oArgs = new ArrayList();
        oArgs = Utility.ParseArgs(sLine);
        if (oArgs.Count > 0)
        {
            if (Conversions.ToString(oArgs[0]).Length > 0)
            {
                string sClass = string.Empty;
                string sSound = string.Empty;
                string sHighlight = oArgs[3].ToString();
                if (sLine.Contains("{"))
                {
                    if (oArgs.Count > 4)
                        sClass = oArgs[4].ToString();
                    if (oArgs.Count > 5)
                        sSound = oArgs[5].ToString();
                }
                else // Add all args after 3 for highlights added without {} args.
                {
                    sHighlight = Utility.ArrayToString(oArgs, 3);
                }

                var switchExpr = Conversions.ToString(oArgs[0]).Substring(1);
                switch (switchExpr)
                {
                    case "highlight":
                    case "highlights":
                        {
                            var switchExpr1 = oArgs[1].ToString().ToLower();
                            switch (switchExpr1)
                            {
                                case "line":
                                case "lines":
                                    {
                                        if (oArgs.Count > 3)
                                        {
                                            bool argbHighlightWholeRow = true;
                                            var arg1 = oArgs[1].ToString();
                                            var arg2 = oArgs[2].ToString();
                                            var arg3 = oArgs[3].ToString();
                                            HighlightsList.Instance.Add(arg3, argbHighlightWholeRow, arg2, true, sSound, sClass);
                                        }

                                        break;
                                    }

                                case "string":
                                case "strings":
                                    {
                                        if (oArgs.Count > 3)
                                        {
                                            bool argbHighlightWholeRow1 = false;
                                            var arg1 = oArgs[1].ToString();
                                            var arg2 = oArgs[2].ToString();
                                            var arg3 = oArgs[3].ToString();
                                            HighlightsList.Instance.Add(arg3, argbHighlightWholeRow1, arg2, true, sSound, sClass);
                                        }

                                        break;
                                    }

                                case "beginswith":
                                    {
                                        if (oArgs.Count > 3)
                                        {
                                            var arg1 = oArgs[1].ToString();
                                            var arg2 = oArgs[2].ToString();
                                            var arg3 = oArgs[3].ToString();
                                            HighlightBeginsWithList.Instance.Add(arg3, arg2, true, sSound, sClass);
                                        }

                                        break;
                                    }

                                case "regexp":
                                case "regex":
                                    {
                                        if (oArgs.Count > 3)
                                        {
                                            string argsRegExp = Globals.ParseGlobalVars(oArgs[3].ToString());
                                            if (Utility.ValidateRegExp(argsRegExp) == true)
                                            {
                                                var arg1 = oArgs[1].ToString();
                                                var arg2 = oArgs[2].ToString();
                                                var arg3 = oArgs[3].ToString();
                                                HighlightRegExpList.Instance.Add(arg3, arg2, true, sSound, sClass);
                                            }
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                }
            }
        }
    }
}
