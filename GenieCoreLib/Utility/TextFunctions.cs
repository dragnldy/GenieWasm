using System.Collections.Concurrent;
using System.Drawing;

namespace GenieCoreLib;

public class TextMessage
{
    public string Text { get; set; } = string.Empty;
    public string TargetPanel { get; set; } = AppGlobals.MainWindow;
    public bool IsMono { get; set; } = false; // Indicates if the text should be displayed in monospace font
    public bool IsException { get; set; } = false; // Indicates if this is an exception message
    public bool IsError { get; set; } = false; // Indicates if this is an error message

    public TextMessage(string text, string targetPanel="game") 
    {
        Text = text ?? "Empty Message Text";
        TargetPanel = targetPanel;
    }
}

public class FormattedTextMessage : TextMessage
{
    public Color ForegroundColor { get; set; }
    public Color BackgroundColor { get; set; }
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public FormattedTextMessage(string text, string targetPanel,
        bool isMono = false, bool isBold = false, bool isItalic=false): 
        base(text,targetPanel)
    {
        ForegroundColor = Color.WhiteSmoke;
        BackgroundColor = Color.Transparent;
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
    public static void EchoBoldText(string sText, string sWindow = "")
    {
        TextMessage textMessage = new FormattedTextMessage(sText, sWindow)
        {
            IsBold = true
        };
        SendTextMessage(textMessage);
    }
    public static void EchoException(string sText, string sWindow = "")
    {
        TextMessage exceptionMessage = new TextMessage(sText, sWindow)
        {
            IsException = true
        };
        SendTextMessage(exceptionMessage);
    }
    public static void EchoError(string sText, string sWindow = "")
    {
        TextMessage errorMessage = new TextMessage(sText, sWindow)
        {
            IsError = true
        };
        SendTextMessage(errorMessage);
    }
    public static void EchoFormattedText(string sText, string sWindow = "", Color? fgColor = null, Color? bgColor = null, bool isMono = false,
        bool isItalic = false, bool isBold=false)
    {
        FormattedTextMessage formattedTextMessage = new FormattedTextMessage(sText, sWindow, isMono, isBold, isItalic)
        {
            ForegroundColor = fgColor ?? Color.WhiteSmoke,
            BackgroundColor = bgColor ?? Color.Transparent
        };
        SendTextMessage(formattedTextMessage);
    }

    public static void EchoText(string sText, string sWindow = "")
    {
        TextMessage textMessage = new TextMessage(sText, sWindow);
        SendTextMessage(textMessage);

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
public class ExceptionMessage : TextMessage
{
    public string Description { get; set; }
    public string Section { get; set; }
    public ExceptionMessage(string text, string targetPanel, string section, string description = null) : base(text, targetPanel)
    {
        Description = description;
        Section = section;
    }
}
public static class GenieException
{
    public static void HandleGenieException(string section, string message, string description = null)
    {
        TextFunctions.SendTextMessage(new ExceptionMessage(message, "exception", section, description));
    }
}

