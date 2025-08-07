using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Xml.Linq;
using static GenieCoreLib.GagRegExp;
using static System.Net.Mime.MediaTypeNames;

namespace GenieCoreLib;

public class ArrayList : System.Collections.ArrayList
{
    private ReaderWriterLockSlim m_oRWLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private const int m_iDefaultTimeout = 250;

public bool AcquireWriterLock()
    {
        try
        {
            if(m_oRWLock.IsWriteLockHeld | m_oRWLock.IsReadLockHeld)
            {
                return false;
            }
            return m_oRWLock.TryEnterWriteLock(500);
        }
        catch 
        {
            return false;
        }
    }

    public bool AcquireReaderLock()
    {
        try
        {
            if (m_oRWLock.IsWriteLockHeld) return false;
            return m_oRWLock.TryEnterReadLock(500);
        }
        catch 
        {
            return false;
        }
    }
    public bool ReleaseWriterLock()
    {
        try
        {
            m_oRWLock.ExitWriteLock();
            return true;
        }
        catch 
        {
            return false;
        }
    }

    public bool ReleaseReaderLock()
    {
        try
        {
            if(m_oRWLock.IsReadLockHeld)
                m_oRWLock.ExitReadLock();
            return true;
        }
        catch 
        {
            return false;
        }
    }

    public ArrayList() : base()
    {
    }

    public ArrayList(IComparer comparer) : base((ICollection)comparer)
    {
    }

    public new void Clear()
    {
        if (AcquireWriterLock())
        {
            try
            {
                base.Clear();
            }
            finally
            {
                ReleaseWriterLock();
            }
        }
        else
        {
            throw new Exception("Unable to aquire writer lock.");
        }
    }

    public new int Add(object value)
    {
        if (AcquireWriterLock())
        {
            try
            {
                return base.Add(value);
            }
            finally
            {
                ReleaseWriterLock();
            }
        }
        else
        {
            throw new Exception("Unable to aquire writer lock.");
        }

        return -1;
    }

    public new void Remove(object key)
    {
        if (AcquireWriterLock())
        {
            try
            {
                base.Remove(key);
            }
            finally
            {
                ReleaseWriterLock();
            }
        }
        else
        {
            throw new Exception("Unable to aquire writer lock.");
        }
    }

    public new void RemoveAt(int index)
    {
        if (AcquireWriterLock())
        {
            try
            {
                base.RemoveAt(index);
            }
            finally
            {
                ReleaseWriterLock();
            }
        }
        else
        {
            throw new Exception("Unable to aquire writer lock.");
        }
    }

    public object get_Item(int index)
    {
        return base[index];
    }

    public void set_Item(int index, object value)
    {
        if (AcquireWriterLock())
        {
            try
            {
                base[index] = value;
            }
            finally
            {
                ReleaseWriterLock();
            }
        }
        else
        {
            throw new Exception("Unable to aquire writer lock.");
        }
    }

    public new void Sort()
    {
        if (AcquireWriterLock())
        {
            try
            {
                base.Sort();
            }
            finally
            {
                ReleaseWriterLock();
            }
        }
        else
        {
            throw new Exception("Unable to aquire writer lock.");
        }
    }

    public new void Sort(IComparer ic)
    {
        if (AcquireWriterLock())
        {
            try
            {
                base.Sort(ic);
            }
            finally
            {
                ReleaseWriterLock();
            }
        }
        else
        {
            throw new Exception("Unable to aquire writer lock.");
        }
    }
    public string ListArray(string itemName, string sKeyPattern = "", string sValuePattern = "")
    {
        StringBuilder sb = new();
        sb.AppendLine(System.Environment.NewLine + $"Active {itemName}: ");
        bool bUsePattern = false;
        if (sKeyPattern.Length > 0)
        {
            bUsePattern = true;
            sb.AppendLine("Filter: " + sKeyPattern);
        }

        if (AcquireReaderLock())
        {
            try
            {
                int i = 0;
                foreach (DictionaryEntry de in this)
                {
                    if (bUsePattern == false | de.Key.ToString().Contains(sKeyPattern,StringComparison.OrdinalIgnoreCase))
                    {
                        string text = FormatVariable(de.Value, itemName, sValuePattern);
                        if (!string.IsNullOrEmpty(text))
                        {
                            string stext = $"{itemName}${de.Key.ToString()}={text}";
                            sb.AppendLine(stext); i++;
                        }
                    }
                }

                if (i == 0)
                {
                    sb.AppendLine("None.");
                }
                ReleaseReaderLock();
                return (sb.ToString());
            }
            catch(Exception exc)
            {
                try
                {
                    int i = 0;
                    for (int I = 0, loopTo = base.Count - 1; I <= loopTo; I++)
                    {
                        object obj = get_Item(I);
                        string text = FormatVariable(obj, itemName, sKeyPattern);
                        if (string.IsNullOrEmpty(text)) continue;
                        string stext = $"{itemName}${text}";
                        sb.AppendLine(stext); i++;
                    }
                    if (i == 0)
                    {
                        sb.AppendLine("None.");
                    }
                    ReleaseReaderLock();
                    return (sb.ToString());
                }
                catch (Exception exc2)
                {

                }
                finally
                {
                    ReleaseReaderLock();
                }
            }
            finally
            {
                ReleaseReaderLock();
            }
        }
        else
        {
            Game.Instance.SendGenieError("ListVariables", "Unable to aquire reader lock.");
            return string.Empty;
        }
        return string.Empty;
    }

    public static string FormatVariable(object? value, string itemName, string sValuePattern)
    {
        if (value is null || string.IsNullOrEmpty(itemName)) return string.Empty;

        switch (itemName)
        {
            case "Presets":
                Presets.Preset preset = value as Presets.Preset;
                if (preset is null) return string.Empty;
                return preset.ToFormattedString(sValuePattern);

            case "Variables":
                Variables.Variable variable = value as Variables.Variable;
                if (variable is null) return string.Empty;
                if (string.IsNullOrEmpty(sValuePattern) || variable.oType.ToString().Equals(sValuePattern))
                    return variable.ToFormattedString(sValuePattern);
                return string.Empty;

            case "Events":
                QueueList.EventItem eventItem = value as QueueList.EventItem;
                if (eventItem is null) return string.Empty;
                if (string.IsNullOrEmpty(sValuePattern) || eventItem.Action.Contains(sValuePattern, StringComparison.OrdinalIgnoreCase))
                {
                    return eventItem.ToFormattedString();
                }
                return string.Empty;

            case "Gags":
                GagRegExp.Gag gag = value as GagRegExp.Gag;
                if (gag is null) return string.Empty;
                if (string.IsNullOrEmpty(sValuePattern) || gag.Text.Contains(sValuePattern, StringComparison.OrdinalIgnoreCase))
                {
                    return gag.ToFormattedString();
                }
                return string.Empty;

            case "Names":
                Names.Name name = value as Names.Name;
                if (name is null) return string.Empty;
                return name.ToFormattedString(sValuePattern);

            case "Substitutes":
                SubstituteRegExp.Substitute subs = value as SubstituteRegExp.Substitute;
                if (subs is null) return string.Empty;
                if (string.IsNullOrEmpty(sValuePattern) || subs.sText.Contains(sValuePattern,StringComparison.OrdinalIgnoreCase))
                    return subs.ToFormattedString();
                return string.Empty;

            case "Macros":
                Macros.Macro macro = value as Macros.Macro;
                if (macro is null) return string.Empty;
                return macro.ToFormattedString(sValuePattern);  

            case "Aliases":
            case "Classes":
                return value is null ? string.Empty : value.ToString();

            case "Triggers":
                Triggers.Trigger trigger = value as Triggers.Trigger;
                if (trigger is null) return string.Empty;
                return trigger.ToFormattedString(sValuePattern);

            case "HighlightsBeginWith":
                HighlightsBeginWithList.Highlight beginsWithList = value as HighlightsBeginWithList.Highlight;
                if (beginsWithList is null) return string.Empty;
                return beginsWithList.ToFormattedString(sValuePattern);
            case "HighlightsRegExp":
                HighlightsRegExpList.Highlight regExpList = value as HighlightsRegExpList.Highlight;
                if (regExpList is null) return string.Empty;
                return regExpList.ToFormattedString(sValuePattern);
            case "Highlights":
                HighlightsList.Highlight highlightList = value as HighlightsList.Highlight;
                if (highlightList is null) return string.Empty;
                return highlightList.ToFormattedString(sValuePattern);
            default:
                return "Unknown type";
        }
        return string.Empty;
    }
}