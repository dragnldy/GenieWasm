using System.Diagnostics;

namespace GenieCoreLib;

public class CommandQueue
{
    public record struct CommandRestrictions 
    {
        public bool WaitForRoundtime = false;
        public bool WaitForStunned = false;
        public bool WaitForWebbed = false;
        public CommandRestrictions() { }
    }

    private object m_oThreadLock = new object(); // Thread safety
    private DateTime? m_oNextTime;


    public int AddToQueue(double Delay, string Action, bool WaitForRoundtime, bool WaitForWebbed, bool WaitForStunned)
    {
        if (Monitor.TryEnter(m_oThreadLock, 250))
        {
            try
            {
                QueueList.EventList.Add(Delay, Action, WaitForRoundtime, WaitForWebbed, WaitForStunned);
                if (QueueList.EventList.Count == 1) // Only item in list. Set the timer!
                {
                    SetNextTime(Delay);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error adding to command queue: " + ex.Message);
                return -1; // Indicate failure
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
    public void Clear()
    {
        if (Monitor.TryEnter(m_oThreadLock, 250))
        {
            try
            {
                QueueList.EventList.Clear();
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
    }

    // Called on regular intervals to see if our QueueList has anything ready to be checked out
    public string Poll(bool InRoundtime, bool IsWebbed, bool IsStunned)
    {
        string sReturn = string.Empty;
        if (Monitor.TryEnter(m_oThreadLock))
        {
            try
            {
                if (QueueList.EventList.Count > 0)
                {
                    if (m_oNextTime.HasValue && DateTime.Now >= m_oNextTime.Value)
                    {
                        QueueList.EventItem ei = (QueueList.EventItem)QueueList.EventList.get_Item(0);
                        if (!ei.IsRestricted(InRoundtime, IsWebbed, IsStunned))
                        {
                            sReturn = ei.Action;
                            Debug.WriteLine("Now: " + DateTime.Now);
                            Debug.WriteLine("Send: " + sReturn);
                            double i1 = ei.Delay;
                            QueueList.EventList.RemoveAt(0);
                            if (QueueList.EventList.Count > 0)
                            {
                                SetNextTime(((QueueList.EventItem)QueueList.EventList.get_Item(0)).Delay);
                            }
                        }
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



    private void SetNextTime(double dDelay)
    {
        if (QueueList.EventList.Count > 0)
        {
            m_oNextTime = DateTime.Now.AddSeconds(dDelay);
            Debug.WriteLine("Now: " + DateTime.Now);
            Debug.WriteLine("Set Delay: " + dDelay.ToString());
            Debug.WriteLine("Set Next: " + m_oNextTime.ToString());
        }
        else
        {
            m_oNextTime = null;
        }
    }
}