using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GenieWasm;

public partial class ProgressBars : UserControl
{
    public ProgressBars()
    {
        InitializeComponent();
        DataContext = this;
    }
    private int _health = 0;
    public int Health
    {
        get => _health;
        set { if (value != _health) { _health = value; NotifyPropertyChanged(); } }
    }
    private int _mana = 0;
    public int Mana
    {
        get => _mana;
        set { if (value != _mana) { _mana = value; NotifyPropertyChanged(); } }
    }
    private int _stamina = 0;
    public int Stamina
    {
        get => _stamina;
        set { if (value != _stamina) { _stamina = value; NotifyPropertyChanged(); } }
    }
    private int _spirit = 0;
    public int Spirit
    {
        get => _spirit;
        set { if (value != _spirit) { _spirit = value; NotifyPropertyChanged(); } }
    }
    private int _concentration = 0;
    public int Concentration
    {
        get => _concentration;
        set { if (value != _concentration) { _concentration = value; NotifyPropertyChanged(); } }
    }

    private string _gameStatus = "Game Status: Not Connected";
    public string GameStatus
    {
        get => _gameStatus;
        set { if (value != _gameStatus) { _gameStatus = value; NotifyPropertyChanged(); } }
    }

    #region Property Changed Notification
    public event PropertyChangedEventHandler PropertyChanged;

    // This method is called by the Set accessor of each property.
    // The CallerMemberName attribute that is applied to the optional propertyName
    // parameter causes the property name of the caller to be substituted as an argument.
    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Property Changed Notification

}