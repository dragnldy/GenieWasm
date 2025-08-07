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

            TextFunctions.EchoText("Loading Settings...", "Game");
            ConfigSettings.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, "Game");
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Presets...", "Game");
            Presets.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, "Game");
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Global Variables...", "Game");
            Variables.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, "Game");
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Highlights...", "Game");
            HighlightsList.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, "Game");
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Names...", "Game");
            Names.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, "Game");
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Macros...", "Game");
            Macros.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, "Game");
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Aliases...", "Game");
            Aliases.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, "Game");
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Substitutes...", "Game");
            SubstituteRegExp.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, "Game");
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Gags...", "Game");
            GagRegExp.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, "Game");
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Triggers...", "Game");
            Triggers.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, "Game");
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Classes...", "Game");
            Classes.Instance.Load();
            TextFunctions.EchoText("OK" + Environment.NewLine, "Game");
            Thread.Sleep(100); // Give time for the settings to load

            TextFunctions.EchoText("Loading Plugins...", "Game");
            //Plugins.Instance.Load();
            TextFunctions.EchoText("Just Kidding!" + Environment.NewLine, "Game");
            Thread.Sleep(100); // Give time for the settings to load
        }

    }
}
