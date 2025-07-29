using System.Collections;

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
}