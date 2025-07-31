using System.Collections;
using static GenieCoreLib.CommandQueue;

namespace GenieCoreLib;

public class QueueList : ArrayList
{
    // Add this for compatibility with existing code
    public static QueueList EventList => Instance;
    public static QueueList Instance => m_Queue ??= new QueueList();
    private static QueueList m_Queue;
    private object m_oThreadLock = new object(); // Thread safety

    public QueueList()
    {
        m_Queue = this;
    }

    public class EventItem
    {
        public DateTime DoDate;
        public double Delay;
        public string Action;
        public CommandRestrictions Restrictions;

        public EventItem(DateTime oInDate, string sInAction)
        {
            DoDate = oInDate;
            Action = sInAction;
        }
        public EventItem(double InDelay, string InAction, CommandRestrictions InRestrictions)
        {
            DoDate = DateTime.Now.AddSeconds(InDelay);
            Delay = InDelay;
            Action = InAction;
            Restrictions = InRestrictions;
        }

        public bool IsRestricted(bool InRoundtime, bool IsWebbed, bool IsStunned)
        {
            return ((Restrictions.WaitForRoundtime && InRoundtime) |
                    (Restrictions.WaitForStunned && IsStunned) |
                    (Restrictions.WaitForWebbed && IsWebbed));
        }
        public string ToFormattedString(string sValuePattern = "")
        {
            string sFormatted = $"Delay: {Delay}, Action: {Action} Time: {DoDate}";
            if (!string.IsNullOrEmpty(sValuePattern))
            {
                string sRestrictions = string.Empty +
                    (Restrictions.WaitForStunned ? "Stunned" : string.Empty) +
                    (Restrictions.WaitForWebbed ? "Webbed" : string.Empty) +
                    (Restrictions.WaitForRoundtime ? "Roundtime" : string.Empty);

                sFormatted += $"{sFormatted} Restrictions: {sRestrictions}";
            }
            return sFormatted;
        }
    }
    public class EventComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            return Comparer.Default.Compare(((EventItem)x).DoDate, ((EventItem)y).DoDate);
        }
    }
    private IComparer argic = new EventComparer();

    public int Add(double dSeconds, string sAction)
    {
        object argvalue = new EventItem(DateTime.Now.AddMilliseconds(Utility.EvalDoubleTime(dSeconds.ToString(), 0)), sAction);
        Add(argvalue);
        Sort(argic);
        return default;
    }

    public bool Add(double Delay, string Action, CommandRestrictions Restrictions)
    {
        object argvalue = new EventItem(Delay, Action, Restrictions);
        Add(argvalue);
        Sort(argic);
        return true;
    }
    public bool Add(double dDelay, string sAction, bool bWaitForRoundtime = false, bool WaitForWebbed = false, bool WaitForStunned = false)
    {
        CommandRestrictions restrictions = new CommandRestrictions();
        return Add(dDelay, sAction, restrictions);
    }
    public int AddToQueue(double dSeconds, string sAction)
    {
        if (Monitor.TryEnter(m_oThreadLock, 250))
        {
            try
            {
                Add(dSeconds, sAction);
            }
            finally
            {
                Monitor.Exit(m_oThreadLock);
            }
        }
        else
        {
            throw new Exception("Unable to aquire commandqueue thread lock.");
        }

        return default;
    }

    // Called on regular intervals to see if our Queue has anything ready to be checked out
    public string Poll()
    {
        string sReturn = string.Empty;
        if (Monitor.TryEnter(m_oThreadLock))
        {
            try
            {
                if (EventList.Count > 0)
                {
                    if (DateTime.Now >= ((QueueList.EventItem)EventList.get_Item(0)).DoDate)
                    {
                        sReturn = ((QueueList.EventItem)EventList.get_Item(0)).Action;
                        EventList.RemoveAt(0);
                    }
                }
            }
            finally
            {
                Monitor.Exit(m_oThreadLock);
            }
            // Else
            // Throw New Exception("Unable to aquire commandqueue thread lock.")
        }

        return sReturn;
    }

    public string ListSubset(string keyPattern = "", string valuePattern = "")
    {
        return ListArray("Events", keyPattern, valuePattern);
    }
    public string ListAll(string keyPattern)
    {
        return ListSubset(keyPattern);
    }

}

