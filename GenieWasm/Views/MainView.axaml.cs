using Avalonia.Controls;
using Avalonia.VisualTree;
using GenieCoreLib;
using GenieCoreLib.Core;
using GenieWasm.UserControls;
using GenieWasm.ViewModels;
using System;
using System.Linq;
using System.Threading;

namespace GenieWasm.Views;

public partial class MainView : UserControl
{
    public int HandlePluginException { get; private set; }


    public MainView()
    {

        InitializeComponent();
        Loaded += MainView_Loaded;
        DoHookups();
    }
    public object GetVisualRoot()
    {
        return this.VisualRoot;
    }
    private void DoHookups()
    {
        Game.Instance.EventParseXML += Plugin_ParsePluginXML;
       
        Game.Instance.EventVariableChanged += ViewManager.Instance.EventVariableChanged; // Viewmanager will do UI updates
        Game.Instance.EventClearWindow += ViewManager.Instance.Command_EventClearWindow;
        Game.Instance.EventAddImage += ViewManager.Instance.AddImage;
        Game.Instance.EventStreamWindow += ViewManager.Instance.EventStreamWindow;
        //GenieError.EventGeniePluginError += ViewManager.Instance.HandlePluginException;
        Game.Instance.EventTriggerParse += Game.Instance.Game_EventTriggerParse;
        Game.Instance.EventStatusBarUpdate += ViewManager.Instance.Game_EventStatusBarUpdate;
        Game.Instance.EventClearSpellTime += ViewManager.Instance.Game_EventClearSpellTime;
        Game.Instance.EventSpellTime += Game_EventSpellTime;
        Game.Instance.EventCastTime += Game_EventCastTime;
        Game.Instance.EventRoundTime += Game_EventRoundtime;
        Game.Instance.EventTriggerPrompt += Game_EventTriggerPrompt;
        Game.Instance.EventGlobalVariableChanged += ViewManager.Instance.GlobalVariableChanged;

        //// Hook up the PlaySoundCommand to the button in the UI
        //var playSoundButton = this.FindControl<Button>("PlaySoundButton");
        //if (playSoundButton is not null)
        //{
        //    playSoundButton.Command = new RelayCommand(PlaySoundCommand);
        //}
    }

    private void Game_EventTriggerPrompt()
    {
        throw new NotImplementedException();
    }

    private void Game_EventRoundtime(int time)
    {
        throw new NotImplementedException();
    }

    private void Game_EventCastTime()
    {
        throw new NotImplementedException();
    }

    private void Game_EventSpellTime()
    {
        throw new NotImplementedException();
    }

    private void Plugin_ParsePluginXML(string xml)
    {
        //TODO: Implement
    }

    private MainViewModel m_MainViewModel = null;
    private void MainView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Loaded -= MainView_Loaded; // Unsubscribe after first load
        m_MainViewModel = this.DataContext as MainViewModel;
        m_MainViewModel.SetMainView(this);
        // Set the initial text for the GameWindow
        m_MainViewModel.GameWindowText = "Welcome to the Genie Game!";
        // Optionally, you can set the text block style here if needed
        // SetCustomStyles();

        var grids = this.GetVisualDescendants().OfType<Grid>().ToList();
        // Find the scrollviewer controls that will be used to house the game windows
        var stackPanels = this.GetVisualDescendants().OfType<StackPanel>().ToList();
        // Clear existing children of the stackpanels that hold the game windows
        foreach (StackPanel panel in stackPanels.Where(sp => !string.IsNullOrEmpty(sp.Name) && sp.Name.Contains("StackPanel")))
        {
            if (panel.Name.Equals("StackPanel1") || panel.Name.Equals("StackPanel2") || panel.Name.Equals("StackPanel3"))
                panel.Children.Clear();
        }
        // Get the width of each of the three panels

        // Register all the default windows with the ViewManager
        // Load all the default windows

        ViewManager.Instance.InitializeDefaultWindows();
        foreach (GameWindow gameWindow in ViewManager.Instance.GameWindows.Values)
        {
            int location = gameWindow.WindowLocation;
            int panel = (int)(location / 100); // use the 100's digit to pick 1 of 3 panels (left to right)
            int order = (int)(location % 100); // use the 1's and 10's digit to order the windows in the panel
            // Attach the game windows to the appropriate scrollviewer
            var panelName = $"StackPanel{panel}";
            var target = stackPanels.FirstOrDefault(sv => !string.IsNullOrEmpty(sv.Name) && sv.Name.Equals(panelName));
            if (target is null)
            {
                throw new Exception($"ScrollViewer with name Stack{panel} not found.");
            }
            if (gameWindow.GameWindowName.Equals("Game"))
                gameWindow.BodyContent = m_MainViewModel.GameWindowText + Environment.NewLine;
            target.Children.Add(gameWindow);
        }
        ViewManager.Instance.StartMessagePump();
        GameManager.SetThingsUp();
        Thread.Sleep(500); // Give time for the settings to load
        TextFunctions.EchoText("Initialization Complete", "Game");
    }
    private static bool SoundIsOn = false;
    public bool PlaySoundCommand()
    {
        if (!SoundIsOn)
        {
            SoundIsOn = true;
            EventCallBacks.OnPlaySoundRequested("https://github.com/rafaelreis-hotmart/Audio-Sample-files/raw/refs/heads/master/sample.mp3");
        }
        else
        {
            SoundIsOn = false;
            EventCallBacks.OnPlaySoundRequested(string.Empty);
        }
        return SoundIsOn;
    }

    private void UserInputTextBox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        m_MainViewModel.LastKey = e.Key;
        if (e.Key == Avalonia.Input.Key.Enter)
        {
            // Handle the Enter key press
            m_MainViewModel.UserInput = this.UserInputTextBox.Text;
            e.Handled = true; // Mark the event as handled to prevent further processing
        }
    }
}