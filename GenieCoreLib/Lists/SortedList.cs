using System.Collections;
using System.Text;

namespace GenieCoreLib;

public class SortedList : System.Collections.SortedList
{
    private ReaderWriterLockSlim m_oRWLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

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
            if (m_oRWLock.IsWriteLockHeld)
            {
                return false;
            }
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
            m_oRWLock.ExitReadLock();
            return true;
        }
        catch 
        {
            return false;
        }
    }

    public SortedList() : base()
    {
    }

    public SortedList(IComparer comparer) : base(comparer)
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

    public new void Add(object key, object value)
    {
        if (AcquireWriterLock())
        {
            try
            {
                if (base.ContainsKey(key))
                    base[key] = value;
                else
                    base.Add(key, value);
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

    public new object this[object key]
    {
        get
        {
            return base[key];
        }

        set
        {
            if (AcquireWriterLock())
            {
                try
                {
                    base[key] = value;
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
                return (sb.ToString());
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
    }

    private string FormatVariable(object? value, string itemName, string sValuePattern)
    {
        return ArrayList.FormatVariable(value, itemName, sValuePattern);
    }
}