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
    private int _health = 100;
    public int Health
    {
        get => _health;
        set { if (value != _health) { _health = value; NotifyPropertyChanged(); } }
    }
    public string HealthPercent => $"Health ({Health}%)";
    private int _mana = 50;
    public int Mana
    {
        get => _mana;
        set { if (value != _mana) { _mana = value; NotifyPropertyChanged(); } }
    }
    public string ManaPercent => $"Mana ({Mana}%)";
    private int _stamina = 100;
    public int Stamina
    {
        get => _stamina;
        set { if (value != _stamina) { _stamina = value; NotifyPropertyChanged(); } }
    }
    public string StaminaPercent => $"Stamina ({Stamina}%)";

    private int _spirit = 100;
    public int Spirit
    {
        get => _spirit;
        set { if (value != _spirit) { _spirit = value; NotifyPropertyChanged(); } }
    }
    public string SpiritPercent => $"Mana ({Spirit}%)";

    private int _concentration = 100;
    public int Concentration
    {
        get => _concentration;
        set { if (value != _concentration) { _concentration = value; NotifyPropertyChanged(); } }
    }
    public string ConcentrationPercent => $"Concentration ({Concentration}%)";

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
        switch (propertyName)
        {
            case nameof(Health):
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HealthPercent)));
                break;
            case nameof(Mana):
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ManaPercent)));
                break;
            case nameof(Stamina):
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StaminaPercent)));
                break;
            case nameof(Spirit):
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpiritPercent)));
                break;
            case nameof(Concentration):
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConcentrationPercent)));
                break;
        }
    }
    #endregion Property Changed Notification

}