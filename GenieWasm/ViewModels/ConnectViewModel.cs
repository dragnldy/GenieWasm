using Avalonia.Controls;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GenieWasm.ViewModels;

public class ConnectViewModel: INotifyPropertyChanged
{
    public Interaction<string, string> CloseDialogInteraction { get; }

    #region Commands
    public ICommand CancelCommand { get; }
    public ICommand ConnectCommand { get; }

    private void CancelRequest()
    {
        // Logic to handle cancellation of the connection request
        // This could involve resetting fields or closing a dialog
        AccountName = string.Empty;
        Password = string.Empty;
        CharacterName = string.Empty;
        Game = string.Empty;
        RememberPassword = false;
        RememberAccount = false;
        DialogResult = "Cancelled";
    }
    private void ConnectRequest()
    {
        DialogResult = "Connected";
    }

    #endregion

    #region Properties

    private string _dialogResult = string.Empty;
    public string DialogResult
    {
        get => _dialogResult;
        set
        {
            if (_dialogResult != value)
            {
                _dialogResult = value;
                OnPropertyChanged();
            }
        }
    }

    private string _accountName = string.Empty;
    public string AccountName
    {
        get => _accountName;
        set
        {
            if (_accountName != value)
            {
                _accountName = value;
                OnPropertyChanged();
            }
        }
    }
    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set
        {
            if (_password != value)
            {
                _password = value;
                OnPropertyChanged();
            }
        }
    }
    private string _characterName = string.Empty;
    public string CharacterName
    {
        get => _characterName;
        set
        {
            if (_characterName != value)
            {
                _characterName = value;
                OnPropertyChanged();
            }
        }
    }
    private string _game = string.Empty;
    public string Game
    {
        get => _game;
        set
        {
            if (_game != value)
            {
                _game = value;
                OnPropertyChanged();
            }
        }
    }
    private bool _rememberPassword = false;
    public bool RememberPassword
    {
        get => _rememberPassword;
        set
        {
            if (_rememberPassword != value)
            {
                _rememberPassword = value;
                OnPropertyChanged();
            }
        }
    }
    private bool _rememberAccount = false;
    public bool RememberAccount
    {
        get => _rememberAccount;
        set
        {
            if (_rememberAccount != value)
            {
                _rememberAccount = value;
                OnPropertyChanged();
            }
        }
    }
    #endregion

    private ObservableCollection<string> _games =
        new ObservableCollection<string>
        {
            "DR",
            "DRX",
            "DRF",
            "DRT"
        };

public ObservableCollection<string> Games
    {
        get => _games;
        set => _games = value; // Use SetProperty for INotifyPropertyChanged
    }

    private string _selectedGame = "DRT";
    public string SelectedGame
    {
        get => _selectedGame;
        set { if (value != _selectedGame) { _selectedGame = value; OnPropertyChanged(); } }
    }
    private string _selectedValue;

    public ConnectViewModel()
    {
        CancelCommand = new RelayCommand(_ => CancelRequest());
        ConnectCommand = new RelayCommand(_ => ConnectRequest());
        CloseDialogInteraction = new Interaction<string, string>();
    }

     public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}