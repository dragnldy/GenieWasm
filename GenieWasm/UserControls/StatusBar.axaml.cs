using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using GenieCoreLib;
using GenieWasm.Utility;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using Avalonia.Utilities;
using DynamicData.Kernel;

namespace GenieWasm.UserControls;

public partial class StatusBar : UserControl,INotifyPropertyChanged
{
    // Hold the list of variables this control will display dynamically
    private List<string> DynamicVariables = new();
    StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
    string[] directions = "compass north northeast east southeast south southwest west northwest up down out".Split(' ');
    string[] positions = "dead standing kneeling sitting prone".Split(' ');
    string[] conditions = "stunned bleeding invisible hidden joined webbed".Split(' ');

    public StatusBar()
    {
        DynamicVariables.AddRange(directions);
        DynamicVariables.AddRange(positions);
        DynamicVariables.AddRange(conditions);

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
            return;
        }
        string pname = e.PropertyName.TrimStart('$').ToLower();
        if (DynamicVariables.Contains(pname))
        {
            int iActive = 0;
            object status = Variables.Instance[pname]?.ToString();
            if (status is not null && int.TryParse(status.ToString(), out iActive))
            {
                if (positions.Contains(pname))
                    UpdatePositionIcon(pname, iActive);

                if (conditions.Contains(pname))
                    UpdateConditionIcon(pname, iActive);

                if (directions.Contains(pname))
                    UpdateDirectionIcons(pname, iActive);
                return;
            }
        }
        NotifyPropertyChanged(e.PropertyName);
    }
    private void UpdateDirectionIcons(string propertyName,int iActive)
    {
        // Update the compass icon
        if (propertyName.Equals("compass", StringComparison.OrdinalIgnoreCase))
        {
            IconCompass = ImageHelper.LoadFromResource(new Uri($"avares://GenieWasm/Assets/Icons/{Variables.Instance["compass"]}.png"));
        }
    }

    private void UpdateConditionIcon(string propertyName, int iActive)
    {
        if (iActive == 1)
        {
            _iconConditionName = propertyName.ToLower();
            NotifyPropertyChanged(nameof(IconCondition));
        }
    }

    private void UpdatePositionIcon(string propertyName, int iActive)
    {
        if (iActive == 1)
        {
            _iconPositionName = propertyName.ToLower();
            NotifyPropertyChanged(nameof(IconPosition));
        }
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

    private string _iconPositionName = "sitting";

    private string _iconCurrentPosition = null;
    private Bitmap? _iconPosition = null;
    public Bitmap? IconPosition
    {
        get
        {
            // Don't refresh the icon if it is already set and the current position matches
            if (_iconPosition != null && !string.IsNullOrEmpty(_iconCurrentPosition))
            {
                if (_iconCurrentPosition.Equals(_iconPositionName, StringComparison.OrdinalIgnoreCase))
                    return _iconPosition;
            }
            if (string.IsNullOrEmpty(_iconPositionName))
            {
                _iconPositionName = "standing"; // Default position
            }
            _iconCurrentPosition = _iconPositionName; // Update current position
            var res = $"avares://{tempassembly}/Assets/Icons/{_iconPositionName}.png";
            if (res == null)
            {
                res = $"avares://{tempassembly}/Assets/Icons/standing.png"; // Default to standing if not found
            }
            _iconPosition = ImageHelper.LoadFromResource(new Uri(res));
            return _iconPosition;
        }
//        set { if (value != _iconPosition) { _iconPosition = value; NotifyPropertyChanged(); } }
    }

    private string _iconConditionName = "";
    private string _iconCurrentCondition = "";
    private Bitmap? _iconCondition = null;
    public Bitmap? IconCondition
    {
        get
        {
            // Don't refresh the icon if it is already set and the current position matches
            if (_iconCondition != null && !string.IsNullOrEmpty(_iconCurrentCondition))
            {
                if (_iconCurrentCondition.Equals(_iconConditionName, StringComparison.OrdinalIgnoreCase))
                    return _iconCondition;
            }
            if (string.IsNullOrEmpty(_iconConditionName))
            {
                _iconCondition = null;
                _iconCurrentCondition = "";
                return null;
            }
            var res = $"avares://{tempassembly}/Assets/Icons/{_iconConditionName}.png";
            if (res == null)
            {
                _iconCondition = null;
                return null;
            }
            _iconCurrentCondition = _iconConditionName; // Update current condition
            _iconCondition = ImageHelper.LoadFromResource(new Uri(res));
            return _iconCondition; 
        }
//        set { if (value != _iconCondition) { _iconCondition = value; NotifyPropertyChanged(); } }
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
