using Avalonia.Controls;
using Avalonia.Threading;
using GenieCoreLib;
using GenieWasm.Views;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GenieWasm.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ConfigSettings ConfigSettings { get; set; }

    private Avalonia.Input.Key _lastKey = Avalonia.Input.Key.None;
    public Avalonia.Input.Key LastKey
    {
        get => _lastKey;
        set
        {
            _lastKey = value;
        }
    }
    public bool LastKeyWasCommand()
    {
        switch (_lastKey)
        {
            case Avalonia.Input.Key.Enter:
            case Avalonia.Input.Key.Escape:
            case Avalonia.Input.Key.Tab:
            case Avalonia.Input.Key.Down:
            case Avalonia.Input.Key.Up:
            case Avalonia.Input.Key.PageDown:
            case Avalonia.Input.Key.PageUp:
                return true;
            default:
                break;
        }
        return false;
    }

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
        if (AppGlobals.IsLocalServer())
        {
            GameConnection.Instance.Profile = new CharacterProfile
            {
                Account = "dragnldy",
                Character = "GeniePlayer",
                EncryptedPassword = "encrypted_password_here", // Replace with actual encrypted password
                Game = "DR",
                Layout = "Default"
            };
            ConnectionRequest connectionRequest = new ConnectionRequest
            {
                Account = GameConnection.Instance.Profile.Account,
                Character = GameConnection.Instance.Profile.Character,
                Password = GameConnection.Instance.Profile.EncryptedPassword,
                Game = GameConnection.Instance.Profile.Game,
                SaveAccount = false, // Assuming you want to save the account
                SavePassword = false, // Assuming you want to save the character
                Host = AppGlobals.Host, // Local server
                Port = AppGlobals.Port, // Default port for local server
                IsSecure = false, // Assuming local server is not secure
                IsValid = true // Assuming the connection request is valid for local server
            };
            Connection.Instance.TestConnect();
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new ConnectView();
                var result = await dialog.ShowDialog<ConnectionRequest>((Window)m_MainView.GetVisualRoot());
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
    }


    private string _gameWindowText = "This is some plain text, followed by \r\n    <Bold>bold text</Bold>, \r\n    <Italic>italic text</Italic>, \r\n    and even a <Span Foreground=\"Green\">custom green span</Span>.\r\n    You can also use a <Run FontSize=\"24\">Run with a different font size</Run>.";
    public string GameWindowText
    {
        get => "Testing";
        set { _gameWindowText = value; NotifyPropertyChanged(); }
    }

    private string _userInput = "";
    public string UserInput
    {
        get => _userInput;
        set
        {
            if (_userInput != value)
            {
                _userInput = value;
                NotifyPropertyChanged();
            }
        }
    }

    public MainViewModel(IConfigSettings configsettings)
    {
        ConnectCommand = new RelayCommand(OnConnectCommandExecuted);
        ViewManager.Instance.SetMainVewModel(this);
        ConfigSettings = (configsettings as ConfigSettings);
        // Initialize the GameWindowText with some default text
        GameWindowText = "Welcome to the Genie Game!";
        ViewManager.Instance.PropertyChanged += ViewManager_PropertyChanged;
    }

    public MainView m_MainView = null;
    public void SetMainView(MainView mainView)
    {
        m_MainView = mainView;
    }

    public void ViewManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Handle property changes from ViewManager if needed
        NotifyPropertyChanged(e.PropertyName);
    }

    #region Property Changed Notification
    public event PropertyChangedEventHandler PropertyChanged;

    // This method is called by the Set accessor of each property.
    // The CallerMemberName attribute that is applied to the optional propertyName
    // parameter causes the property name of the caller to be substituted as an argument.
    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        if (propertyName.Equals(nameof(UserInput)))
        {
            if (LastKeyWasCommand())
            {
                // Do something here

                UserInput = "";
                // Need to handle the command input
            }
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Property Changed Notification
}
