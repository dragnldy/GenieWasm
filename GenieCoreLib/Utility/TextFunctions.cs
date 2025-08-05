using System.Collections.Concurrent;
using System.Drawing;

namespace GenieCoreLib;

public class TextMessage
{
    public string Text { get; set; } = string.Empty;
    public string TargetPanel { get; set; } = "Game";
    public TextMessage() { }
    public TextMessage(string text, string targetPanel)
    {
        Text = text;
        TargetPanel = targetPanel;
    }
}

public class FormattedTextMessage : TextMessage
{
    public Color ForegroundColor { get; set; }
    public Color BackgroundColor { get; set; }
    public bool IsMono { get; set; }
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public FormattedTextMessage(string text, string targetPanel, Color foregroundColor, Color backgroundColor, bool isMono, bool isBold, bool isItalic) : base(text,targetPanel)
    {
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
        IsMono = isMono;
        IsBold = isBold;
        IsItalic = isItalic;
    }
}
public static class TextFunctions
{
    public static ConcurrentQueue<TextMessage> ConcurrentTextMessageQueue = new ConcurrentQueue<TextMessage>();
    public static void EchoNewLine(string sText, string sWindow = "")
    {
        EchoText(Environment.NewLine + sText, sWindow);
    }
    public static void EchoText(string sText, string sWindow = "")
    {
        try
        {
            if (string.IsNullOrEmpty(sText))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(sText));
            }
            if (string.IsNullOrEmpty(sWindow))
            {
                sWindow = "Game"; // Default to GameWindow if no window is specified
            }

            bool bMono = false;
            if (sText.ToLower().StartsWith("mono "))
            {
                sText = sText.Substring(5);
                bMono = true;
            }
            var argoColor = Color.WhiteSmoke;
            var argoBgColor = Color.Transparent;
            SendTextMessage(new FormattedTextMessage(sText, sWindow, argoColor, argoBgColor, isMono: bMono, isBold: false, isItalic: false));
        }
        catch (Exception ex)
        {
            GenieException.HandleGenieException("EchoText", ex.Message, ex.ToString());
        }
    }
    public static void SendTextMessage(TextMessage message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Message cannot be null.");
        }
        ConcurrentTextMessageQueue.Enqueue(message);
    }
}
