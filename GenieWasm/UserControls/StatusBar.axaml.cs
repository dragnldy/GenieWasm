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
    string[] baritems = "roundtime gamertend gamertleft lefthand righthand".Split(' ');
    string[] spells = "preparedspell casttime casttimeend casttimeleft".Split(' ');

    public StatusBar()
    {
        DynamicVariables.AddRange(directions);
        DynamicVariables.AddRange(positions);
        DynamicVariables.AddRange(conditions);
        DynamicVariables.AddRange(baritems);
        DynamicVariables.AddRange(spells);

        InitializeComponent();
        DataContext = this;
        ViewManager.Instance.PropertyChanged += ViewManager_PropertyChanged;
    }
            
    private void ViewManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Handle property changes from ViewManager if needed
        string pname = e.PropertyName.TrimStart('$').ToLower();
        if (pname == "prompt")
        {
            // Use this as a heartbeat to update the progress bars
            UpdatePreparedSpellLabel("prompt");
            UpdateSpellCastBar("prompt");
            UpdateRoundTime("prompt");
        }
        if (DynamicVariables.Contains(pname))
        {
            if (baritems.Contains(pname))
            {
                UpdateBarItemLabels(pname);
                return;
            }
            if (spells.Contains(pname))
            {
                UpdateSpells(pname);
                return; // No need to update icons for prepared spell
            }
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

    private void UpdateSpellCastBar(string pname)
    {
        if (Globals.Instance.CastTimeStart == 0)
            return;

        var spell = Variables.Instance["preparedspell"]?.ToString() ?? "None";
        if (spell.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            // If the spell is None, reset the cast time
            Globals.Instance.CastTimeLeft = 0; // Reset the cast time left
            return;
        }
        CastRT = Globals.Instance.GameTime - Globals.Instance.CastTimeStart;
        UpdateSpellLabel();
    }

    private void UpdateSpells(string pname)
    {
        if (pname.Equals("preparedspell", StringComparison.OrdinalIgnoreCase))
        {
            UpdatePreparedSpellLabel(pname);
            return;
        }

        if (pname.Equals("casttime", StringComparison.OrdinalIgnoreCase) ||
                pname.Equals("casttimeend", StringComparison.OrdinalIgnoreCase))
        {
            MaxCastRT = Globals.Instance.CastTimeEnd - Globals.Instance.CastTimeStart;
            CastRT = MaxCastRT;
        } 
        else if (pname.Equals("casttimeleft"))
        {
            CastRT = Globals.Instance.CastTimeLeft;
            // Need to update the title spell has been held
            UpdatePreparedSpellLabel(pname);
        }
    }

    private void UpdatePreparedSpellLabel(string pname)
    {
        int timeSpellHeld = 0;
        LabelSpellC = Variables.Instance["preparedspell"]?.ToString() ?? "None";
        if (LabelSpellC.Equals("None"))
        {
            Globals.Instance.CastTimeStart = 0; // Reset the cast time start
            Globals.Instance.CastTimeEnd = 0; // Reset the cast time end
        }
        else
        {
            if (Globals.Instance.CastTimeStart == 0)
                Globals.Instance.CastTimeStart = Globals.Instance.GameTime; // Reset the cast time start

            UpdateSpellLabel();
            timeSpellHeld = Globals.Instance.GameTime - Globals.Instance.CastTimeStart;
            if (ConfigSettings.Instance.ShowSpellTimer)
                LabelSpellC = $"({timeSpellHeld}) {Variables.Instance["preparedspell"].ToString()}";
        }
    }

    private void UpdateSpellLabel()
    {
        int timeSpellHeld = Globals.Instance.GameTime - Globals.Instance.CastTimeStart;
        if (ConfigSettings.Instance.ShowSpellTimer)
            LabelSpellC = $"({timeSpellHeld}) {Variables.Instance["preparedspell"].ToString()}";
    }

    private void UpdateRoundTime(string pname)
    {
        if (pname.Equals("roundtime", StringComparison.OrdinalIgnoreCase) ||
                pname.Equals("gamertend", StringComparison.OrdinalIgnoreCase))
        {
            if (Globals.Instance.GameRTStart == 0)
            {
                MaxRT = 0;
                RT = 0;
            }
            else
            {
                MaxRT = Globals.Instance.GameRTEnd - Globals.Instance.GameRTStart;
                RT = MaxRT;
            }
        }
        else if (pname.Equals("prompt"))
        {
            if (RT != Globals.Instance.GameRTLeft)
            {
                // Update the round time if it has changed
                RT = Globals.Instance.GameRTLeft;
            }
        }
    }
    private void UpdateBarItemLabels(string pname)
    {
        if (pname.Equals("roundtime") || pname.Equals("gamertend") || pname.Equals("gamertleft"))
        {
            UpdateRoundTime(pname);
            return;
        }
        var results = Variables.Instance[pname]?.ToString() ?? "Empty";
        if (pname.Equals("lefthand", StringComparison.OrdinalIgnoreCase))
        {
            LabelLHC = results;
        }
        else if (pname.Equals("righthand", StringComparison.OrdinalIgnoreCase))
        {
            LabelRHC = results;
        }
        return;
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
    private int _currentRT = 0;
    public int RT
    {
        get => _currentRT;
        set { if (value != _currentRT) { _currentRT = value; NotifyPropertyChanged(); } }
    }

    private int _castRT = 0;
    public int CastRT
    {
        get => _castRT;
        set { if (value != _castRT) { _castRT = value; NotifyPropertyChanged(); } }
    }
    private int _maxCastRT = 0;
    public int MaxCastRT
    {
        get => _maxCastRT;
        set { if (value != _maxCastRT) { _maxCastRT = value; NotifyPropertyChanged(); } }
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
