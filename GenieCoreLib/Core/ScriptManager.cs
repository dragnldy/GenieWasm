using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenieCoreLib
{
    public class ScriptManager
    {
        public static ScriptManager Instance => m_ScriptManager ??= new ScriptManager();
        private static ScriptManager m_ScriptManager;
        private ScriptManager()
        {
            m_ScriptManager = this;
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
            Command.Instance.EchoText(message, logType);
        }
        private void Game_EventTriggerMove()
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
                    EchoText("TriggerMove: Unable to acquire reader lock.","Log");
                }
            }
            catch (Exception ex)
            {
                EchoText($"TriggerMove: {ex.Message} {ex.ToString()}", "Log");
            }
        }

        private bool HasRoundTime
        {
            get
            {
                return DateTime.Now < Globals.Instance.RoundTimeEnd;
            }
        }

        public void SetRoundTime(int iTime)
        {
            if (iTime == 0)
                return;

            Game.Instance.VariableChanged("RoundTime", (int)(iTime + ConfigSettings.Instance.RTOffset));

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

            Globals.Instance.RoundTimeEnd = DateTime.Now.AddMilliseconds(iTime * 1000 + ConfigSettings.Instance.RTOffset * 1000);
        }

    }
}
