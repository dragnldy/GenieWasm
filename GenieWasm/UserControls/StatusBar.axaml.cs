using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using GenieCoreLib;
using GenieWasm.Utility;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GenieCoreLib;

namespace GenieWasm.UserControls;

public partial class StatusBar : UserControl
{
    public StatusBar()
    {
        InitializeComponent();
        DataContext = this;
        ViewManager.Instance.PropertyChanged += ViewManager_PropertyChanged;
    }
            
    private void ViewManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Handle property changes from ViewManager if needed
        if (e.PropertyName.Equals("StatusBar",StringComparison.OrdinalIgnoreCase))
        {
            SetStatusBarLabels();
        }
        NotifyPropertyChanged(e.PropertyName);
    }

    private string _labelRHC = "Empty";
    public string LabelRHC
    {
        get => _labelRHC;
        set
        {
            if (_labelRHC != value)
            {
                if (string.IsNullOrEmpty(value)) value = "Empty";
                _labelRHC = value;
                NotifyPropertyChanged();
            }
        }
    }
    private string _labelLHC = "Empty";
    public string LabelLHC
    {
        get => _labelLHC;
        set
        {
            if (_labelLHC != value)
            {
                if (string.IsNullOrEmpty(value)) value = "Empty";
                _labelLHC = value;
                NotifyPropertyChanged();
            }
        }
    }
    private string _labelSpellC = "None";
    public string LabelSpellC
    {
        get => _labelSpellC;
        set
        {
            if (_labelSpellC != value)
            {
                if (string.IsNullOrEmpty(value)) value = "None";
                _labelSpellC = value;
                NotifyPropertyChanged();
            }
        }
    }

    private int _maxRT = 0;
    public int MaxRT
    {
        get => _maxRT;
        set { if (value != _maxRT) { _maxRT = value; NotifyPropertyChanged(); } }
    }
    private int _valueRT = 0;
    public int ValueRT
    {
        get => _valueRT;
        set { if (value != _valueRT) { _valueRT = value; NotifyPropertyChanged(); } }
    }

    private void SetStatusBarLabels()
    {
        Dispatcher.UIThread.Post(() =>
        {
            LabelLHC = Variables.Instance["lefthand"]?.ToString();
            LabelRHC = Variables.Instance["righthand"]?.ToString();

            if (ConfigSettings.Instance.ShowSpellTimer && Globals.Instance.SpellTimeStart != DateTime.MinValue)
            {
                var argoDateEnd = DateTime.Now;
                LabelSpellC = Conversions.ToString("(" + GenieCoreLib.Utility.GetTimeDiffInSeconds(Globals.Instance.SpellTimeStart, argoDateEnd) + ") " + Variables.Instance["preparedspell"]);
            }
            else
            {
                LabelSpellC = Conversions.ToString(Variables.Instance["preparedspell"]);
            }
        });
    }

    private string tempassembly = "GenieWasm";
    private Bitmap? _iconCompass = null;
    public Bitmap? IconCompass
    {
        get
        {
            if (_iconCompass != null) return _iconCompass;
            var res = $"avares://{tempassembly}/Assets/Icons/compass.png";
            return ImageHelper.LoadFromResource(new Uri(res));
        }
        set { if (value != _iconCompass) { _iconCompass = value; NotifyPropertyChanged(); } }
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
