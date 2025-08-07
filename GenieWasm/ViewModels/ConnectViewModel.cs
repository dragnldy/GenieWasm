using Avalonia.Interactivity;
using GenieCoreLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GenieWasm.ViewModels;

public class ConnectViewModel: INotifyPropertyChanged
{
    #region Commands
    public ICommand CancelCommand { get; }
    public ICommand ConnectCommand { get; }
    public ICommand ListCharactersCommand { get; }
    private void CancelRequest()
    {
        // Logic to handle cancellation of the connection request
        ConnectionRequest.IsValid = false;
        DialogResult = "Cancelled";
    }
    private void ConnectRequest()
    {
        CharacterProfile profile = new CharacterProfile
        {
            Account = AccountName,
            Character = CharacterName,
            EncryptedPassword = CharacterProfile.GetEncryptedPassword(AccountName, Password),
            Game = Game,
            Layout = string.Empty
        };

        if (!profile.CheckValid())
        {
            // Send error message to the user
            return;
        }

        if (GameConnection.Instance.IsConnectedToGame)
        {
            // If already connected, disconnect first
            GameConnection.Instance.Disconnect();
        }
        
        if (AppGlobals.IsLocalServer())
            Connection.Instance.TestConnect();
        else
            GameConnection.Instance.Connect(profile, false, false);

        if (GameConnection.Instance.IsConnectedToGame)
        {
            // Save the potentially new or updated profile
            profiles = (new CharacterProfiles(useLegacy: false)).Profiles;
            if (profiles.FirstOrDefault(p => p.Account == profile.Account && p.Character == profile.Character && p.Game == profile.Game) is null)
            {
                profiles.Add(profile);
            }
            else
            {
                // Update existing profile
                var existingProfile = profiles.First(p => p.Account == profile.Account && p.Character == profile.Character && p.Game == profile.Game);
                existingProfile.EncryptedPassword = profile.EncryptedPassword;
                existingProfile.Layout = profile.Layout;
                CharacterProfiles.SaveProfiles(profiles);
            }

            ConnectionRequest.IsValid = true;
            DialogResult = "Connected";
        }
        else
        {
            /// Handle connection failure
        }
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

    private ConnectionRequest? _connectionRequest = new ConnectionRequest();
    public ConnectionRequest? ConnectionRequest
    {
        get => _connectionRequest;
        set
        {
            if (_connectionRequest != value)
            {
                _connectionRequest = value;
                OnPropertyChanged();
            }
        }
    }

    private ObservableCollection<string> availableCharacters = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableCharacters
    {
        get => availableCharacters;
        set
        {
            if (availableCharacters != value)
            {
                availableCharacters = value;
                OnPropertyChanged();
            }
        }
    }

    private ObservableCollection<string> availableAccounts = new ObservableCollection<string>();
    public ObservableCollection<string> AvailableAccounts
    {
        get => availableAccounts;
        set
        {
            if (availableAccounts != value)
            {
                availableAccounts = value;
                OnPropertyChanged();
            }
        }
    }
    private string _selectedCharacter = string.Empty;
    public string SelectedCharacter
    {
        get => _selectedCharacter;
        set
        {
            if (_selectedCharacter != value)
            {
                _selectedCharacter = value;
                CharacterName = _selectedCharacter;
                OnPropertyChanged();
            }
        }
    }
    public void CharacterCompleteBox_LostFocus(object? sender, RoutedEventArgs e)
    {
        // Handle the LostFocus event to set the character selection state
        if (sender is Avalonia.Controls.AutoCompleteBox autoCompleteBox)
        {
            _characterEntered = false; // Reset character selection state
            string value = autoCompleteBox.Text;
            if (!string.IsNullOrEmpty(autoCompleteBox.Text))
            {
                // If the selected character entry/selection is complete
                if (!string.IsNullOrEmpty(value) && value.EndsWith(")"))
                {
                    // Extract character name and game from the selected item
                    var parts = value.Split('(');
                    if (parts.Length == 2)
                    {
                        string characterName = parts[0].Trim();
                        string game = parts[1].TrimEnd(')');
                        CharacterProfile profile = profiles.FirstOrDefault(p => p.Character == characterName && p.Game == game);
                        if (profile is null) SelectedProfile = null;
                        if (profile != null)
                        {
                            SelectedProfile = profile;
                            // Update the connection request with the selected character's details
                            CharacterName = profile.Character;
                            SelectedGame = profile.Game; // Update the selected game
                            AccountName = profile.Account;
                            Password = Decrypt(profile.Account, profile.EncryptedPassword); // Assuming DecryptedPassword is a property that returns the decrypted password
                            RememberAccount = true; // Set to true if you want to remember account
                            RememberPassword = !string.IsNullOrEmpty(Password); // Set to true if you want to remember password
                            _characterEntered = true; // Set to true if a character is selected
                        }
                    }
                }
            }
            OnPropertyChanged(nameof(IsCharacterSelected));
        }
    }

    CharacterProfile SelectedProfile = null;

    private bool _characterEntered   = true;
    public bool IsCharacterSelected
    {
        get => true; // always allow edits in order to correct mistakes or make updates
        set
        {
            if (_characterEntered != value)
            {
                _characterEntered = value;
                OnPropertyChanged();
            }
        }
    }

    private string Decrypt(string account, string encryptedPassword)
    {
        return CharacterProfile.GetDecryptedPassword(account, encryptedPassword);
    }
    public string AccountName
    {
        get => ConnectionRequest?.Account;
        set
        {
            if (ConnectionRequest.Account != value)
            {
                ConnectionRequest.Account = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAccountAssigned));
            }
        }
    }
    public string Password
    {
        get => ConnectionRequest.Password;
        set
        {
            if (ConnectionRequest.Password != value)
            {
                ConnectionRequest.Password = value;
                OnPropertyChanged(nameof(IsAccountAssigned));
                OnPropertyChanged();
            }
        }
    }
    public string CharacterName
    {
        get => ConnectionRequest.Character;
        set
        {
            if (ConnectionRequest.Character != value)
            {
                ConnectionRequest.Character = value;
                OnPropertyChanged();
            }
        }
    }
    public string Game
    {
        get => ConnectionRequest.Game;
        set
        {
            if (ConnectionRequest.Game != value)
            {
                ConnectionRequest.Game = value;
                OnPropertyChanged(nameof(IsAccountAssigned));
                OnPropertyChanged();
            }
        }
    }
    public bool RememberPassword
    {
        get => ConnectionRequest.SavePassword;
        set
        {
            if (ConnectionRequest.SavePassword != value)
            {
                ConnectionRequest.SavePassword = value;
                OnPropertyChanged();
            }
        }
    }
    public bool RememberAccount
    {
        get => ConnectionRequest.SaveAccount;
        set
        {
            if (ConnectionRequest.SaveAccount != value)
            {
                ConnectionRequest.SaveAccount = value;
                OnPropertyChanged();
            }
        }
    }
    public bool IsAccountAssigned { 
        get =>  !string.IsNullOrEmpty(AccountName) &&
                !string.IsNullOrEmpty(Password) &&
                !string.IsNullOrEmpty(Game); }

    public bool isInfoComplete = false;
    public bool IsInfoComplete
    {
        get => isInfoComplete;
        set
        {
            if (isInfoComplete != value)
            {
                isInfoComplete = value;
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

    public string SelectedGame
    {
        get => Game;
        set { if (value != Game) { Game = value; OnPropertyChanged(); } }
    }

    private bool CharacterListRequest()
    {
        // Fill the autocomplete box for characters based on the selected account and game
        if (!IsAccountAssigned) return false;

        AvailableCharacters.Clear();
        profiles.ForEach(p => AvailableCharacters.Add($"{p.Character}({p.Game})"));
        return true;
    }

    private List<CharacterProfile> profiles;
    public ConnectViewModel()
    {
        CancelCommand = new RelayCommand(_ => CancelRequest());
        ConnectCommand = new RelayCommand(_ => ConnectRequest());
        ListCharactersCommand = new RelayCommand(_ => CharacterListRequest());
        // Get all the existing characters from the profiles
        profiles = (new CharacterProfiles(useLegacy: false)).Profiles;
        profiles.ForEach(p => AvailableCharacters.Add($"{p.Character}({p.Game})"));
        List<string> accounts = profiles.Select(p => p.Account).Distinct().ToList();
        accounts.ForEach(a => AvailableAccounts.Add(a));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (PropertyChanged == null) return;
        if (!propertyName.Equals(nameof(IsInfoComplete)))
        {
            IsInfoComplete = !string.IsNullOrEmpty(AccountName) &&
                              !string.IsNullOrEmpty(Password) &&
                              !string.IsNullOrEmpty(CharacterName) &&
                              !string.IsNullOrEmpty(Game);
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}