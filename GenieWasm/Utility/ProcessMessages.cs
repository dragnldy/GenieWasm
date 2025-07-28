using GenieCoreLib;
using System.Threading.Tasks;

namespace GenieWasm;

public static class ProcessQueuedMessages
{
    // This routine needs to be async as it waits for messages to show up in a concurrent queue
    public static async Task ProcessMessagesAsync()
    {
        while (true)
        {
            if (TextFunctions.ConcurrentTextMessageQueue.TryDequeue(out var message))
            {
                if (message is ExceptionMessage exceptionMessage)
                {
                    // Handle exception messages specifically with a special dialog
                    GenieException.HandleGenieException(exceptionMessage.Section, exceptionMessage.Text, exceptionMessage.Description);
                }
                else if (message is FormattedTextMessage formattedMessage)
                {
                    // Handle formatted text messages

                    //GameWindow gameWindow = GenieWasm.Views.MainView.GetGameWindow();
                    //if (gameWindow != null)
                    //{
                    //    gameWindow.AddFormattedText(formattedMessage.Text, formattedMessage.ForegroundColor, formattedMessage.BackgroundColor, formattedMessage.IsMono, formattedMessage.IsBold, formattedMessage.IsItalic);
                    //}
                }
                else
                {
                    // Just normal text- don't need to format
                    // Handle plain text messages
                    //GameWindow gameWindow = GenieWasm.Views.MainView.GetGameWindow();
                    //if (gameWindow != null)
                    //{
                    //    gameWindow.AddText(message.Text);
                    //}
                }
                {
                    TextFunctions.SendTextMessage(message);
                }
            }
            await Task.Delay(100); // Adjust the delay as needed
        }
    }

}
