using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GenieCoreLib;
using GenieCoreLib.Core;
using GenieWasm.UserControls;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GenieWasm.Views;

public partial class MainView : UserControl
{
    private string _GameWindowText = "This is some plain text, followed by \r\n    <Bold>bold text</Bold>, \r\n    <Italic>italic text</Italic>, \r\n    and even a <Span Foreground=\"Green\">custom green span</Span>.\r\n    You can also use a <Run FontSize=\"24\">Run with a different font size</Run>.";
    public string GameWindowText
    {
        get => _GameWindowText;
        set => _GameWindowText = value;
    }
    #region Command Properties
    // Add ICommand properties
    public ICommand ConnectCommand { get; }
    private void OnConnectCommandExecuted(object? parameter)
    {
        Task.Run(async () =>
        {
            try
            {
                await ShowConnectDialogAsync();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during dialog display
                TextFunctions.EchoText($"Error showing connect dialog: {ex.Message}", "Game");
            }
        });
    }
    public async Task ShowConnectDialogAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var dialog = new ConnectView();
            var result = await dialog.ShowDialog<ConnectionRequest>((Window)(Window)this.GetVisualRoot());
            if (result is not null && result.IsValid && GameConnection.Instance.IsConnectedToGame)
            {
                TextFunctions.EchoNewLine("Connected successfully", "Game");
            }
            else
            {
                // Dialog returned false (e.g., "Cancel" was clicked)
                // Handle connection failure logic here
                TextFunctions.EchoNewLine("Connection failed", "Game");
            }
        });
    }
    #endregion Command Properties


    public MainView()
    {
        ConnectCommand = new RelayCommand(OnConnectCommandExecuted);

        InitializeComponent();
        Loaded += MainView_Loaded;
        DataContext = this;
    }

    private void MainView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Loaded -= MainView_Loaded; // Unsubscribe after first load
        // Set the initial text for the GameWindow
        GameWindowText = "Welcome to the Genie Game!";
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
                gameWindow.BodyContent = GameWindowText + Environment.NewLine;
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

}