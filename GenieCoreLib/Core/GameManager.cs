using System.Data;
using System.Drawing;

namespace GenieCoreLib.Core
{
    public static class GameManager
    {
        public static void SetThingsUp()
        {
            // This method can be used to set up any initial game state or configurations
            // For now, it does nothing but can be expanded later

            TextFunctions.EchoText("Loading Settings...", AppGlobals.MainWindow);
            ConfigSettings.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, AppGlobals.MainWindow);
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Presets...", AppGlobals.MainWindow);
            Presets.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, AppGlobals.MainWindow);
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Global Variables...", AppGlobals.MainWindow);
            Variables.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, AppGlobals.MainWindow);
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Highlights...", AppGlobals.MainWindow);
            HighlightsList.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, AppGlobals.MainWindow);
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Names...", AppGlobals.MainWindow);
            Names.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, AppGlobals.MainWindow);
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Macros...", AppGlobals.MainWindow);
            Macros.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, AppGlobals.MainWindow);
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Aliases...", AppGlobals.MainWindow);
            Aliases.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, AppGlobals.MainWindow);
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Substitutes...", AppGlobals.MainWindow);
            SubstituteRegExp.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, AppGlobals.MainWindow);
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Gags...", AppGlobals.MainWindow);
            GagRegExp.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, AppGlobals.MainWindow);
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Triggers...", AppGlobals.MainWindow);
            Triggers.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, AppGlobals.MainWindow);
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Classes...", AppGlobals.MainWindow);
            Classes.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, AppGlobals.MainWindow);
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Plugins...", AppGlobals.MainWindow);
            //Plugins.Instance.Load();
            TextFunctions.EchoText("Just Kidding!" + Environment.NewLine, AppGlobals.MainWindow);
            Thread.Sleep(100); // Give time for the settings to load
        }

    }
}
