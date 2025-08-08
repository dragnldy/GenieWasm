namespace GenieCoreLib
{
    public class ScriptManager
    {
        public static ScriptManager Instance => m_ScriptManager ??= new ScriptManager();
        private static ScriptManager m_ScriptManager;
        private ScriptManager()
        {
            m_ScriptManager = this;
            Game.Instance.EventTriggerMove += EventTriggerMove;
            Game.Instance.EventRoundTime += SetRoundTime;
        }
        public void Game_EventTriggerPrompt()
        {
            try
            {
                if (ScriptList.Instance.AcquireReaderLock())
                {
                    try
                    {
                        foreach (Script oScript in ScriptList.Instance)
                            oScript.TriggerPrompt();
                    }
                    finally
                    {
                        ScriptList.Instance.ReleaseReaderLock();
                    }
                }
                else
                {
                    EchoText("TriggerPrompt: Unable to acquire reader lock.","Log");
                }
            }
            /* TODO ERROR: Skipped IfDirectiveTrivia */
            catch (Exception ex)
            {
                EchoText($"TriggerPrompt: {ex.Message} {ex.ToString()}", "Log");
            }
        }
        private void EchoText(string message, string logType)
        {
            TextFunctions.EchoText(message, logType);
        }
        public void EventTriggerMove()
        {
            try
            {
                if (ScriptList.Instance.AcquireReaderLock())
                {
                    try
                    {
                        foreach (Script oScript in ScriptList.Instance)
                            oScript.TriggerMove();
                    }
                    finally
                    {
                        ScriptList.Instance.ReleaseReaderLock();
                    }
                }
                else
                {
                    TextFunctions.EchoError("TriggerMove: Unable to acquire reader lock.","Log");
                }
            }
            catch (Exception ex)
            {
                GenieException.HandleGenieException($"TriggerMove: {ex.Message} {ex.ToString()}", "Log");
            }
        }
        public void SetRoundTime(int iTime)
        {
            if (ScriptList.Instance.AcquireReaderLock())
            {
                try
                {
                    foreach (Script oScript in ScriptList.Instance)
                        oScript.SetRoundTime(iTime);
                }
                finally
                {
                    ScriptList.Instance.ReleaseReaderLock();
                }
            }
            else
            {
                EchoText($"SetRoundTime: Unable to acquire reader lock.", "Log");
            }
        }

        public void EventEndUpdate()
        {
            if (ScriptList.Instance.AcquireReaderLock())
            {
                try
                {
                    foreach (Script oScript in ScriptList.Instance)
                        oScript.SetBufferEnd();
                }
                catch (Exception ex)
                {
                    GenieException.HandleGenieException("ScriptEndUpdate", ex.Message, ex.ToString());
                }
                finally
                {
                    ScriptList.Instance.ReleaseReaderLock();
                }
            }
            else
            {
                TextFunctions.EchoError("EndUpdate: Unable to acquire reader lock.", "Log");
            }
        }
    }
}
