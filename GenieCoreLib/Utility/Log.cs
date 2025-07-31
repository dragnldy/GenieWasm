using System.Text;

namespace GenieCoreLib;

public class Log
{
    public static Log Instance => m_Log ?? new Log();
    private static Log m_Log;
    public static bool IsTesting = false;
    public static StringBuilder LogBuffer = new();

    private static bool m_bIsLogging = true;

    public Log()
    {
        m_Log = this;
        Task.Run(() =>
        {
            m_bIsLogging = true;
            StringBuilder sb = new();
            string targetPanel = string.Empty;
            Thread.Sleep(1000); // Give the app time to start up before logging
            while (m_bIsLogging)
            {
                while (AppGlobals.LogQueue.Count > 0)
                {
                    if (AppGlobals.LogQueue.TryDequeue(out TextMessage textMessage))
                    {
                        if (textMessage.Text is null) continue;
                        if (string.IsNullOrEmpty(targetPanel) || textMessage.TargetPanel == targetPanel)
                        {
                            targetPanel = textMessage.TargetPanel;
                            sb.Append(textMessage.Text);
                        }
                        else
                        {
                            SendText(sb.ToString(), targetPanel);
                            sb.Clear();
                            targetPanel = textMessage.TargetPanel;
                            sb.Append(textMessage.Text);
                        }
                    }
                }
                if (sb.Length > 0)
                {
                    SendText(sb.ToString(), targetPanel);
                    sb.Clear();
                }
                Thread.Sleep(1000); // Wait a second before checking the queue again
            }
        });
    }
    private void SendText(string text, string targetFile)
    {
        if (text.Length > 0)
        {
            if (IsTesting)
            {
                LogBuffer.Append(text);
            }
            else
            {
                File.AppendAllText(targetFile, text);
            }
        }
    }
    public static void StopLogging()
    {
        m_bIsLogging = false;
    }
    public static string GetFileName(string sCharacterName, string sInstanceName)
    {
        string fileBase = Path.Combine(AppGlobals.LocalDirectoryPath, ConfigSettings.Instance.LogDir);
        fileBase = Path.Combine(fileBase, string.IsNullOrEmpty(sCharacterName) ? "Unknown" : sCharacterName);
        return fileBase + sInstanceName + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
    }
    public static bool LogText(string sText, string sCharacterName, string sInstanceName)
    {
        // Send text to file with no additional newline
        if (string.IsNullOrEmpty(sText))
        {
            return false;
        }
        Log logger = Log.Instance; // Make sure logger is open
        string fileName = GetFileName(sCharacterName, sInstanceName);
        AppGlobals.LogQueue.Enqueue(new TextMessage(sText, fileName));
        return true;
    }
    public static bool LogLine(string sText, string sCharacterName, string sInstanceName)
    {
        // Send text to file followed by a newline
        return LogLine(sText, GetFileName(sCharacterName, sInstanceName));
    }
    public static bool LogLine(string sText, string sFileName = "default.log")
    {
        // Send text to file followed by a newline
        Log logger = Log.Instance; // Make sure logger is open
        AppGlobals.LogQueue.Enqueue(new TextMessage(sText + Environment.NewLine, sFileName));
        return true;
    }
}