using Avalonia.Controls;
using Avalonia.Threading;
using GenieCoreLib;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GenieWasm.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ConfigSettings ConfigSettings { get; set; }

private string _GameWindowText = "This is some plain text, followed by \r\n    <Bold>bold text</Bold>, \r\n    <Italic>italic text</Italic>, \r\n    and even a <Span Foreground=\"Green\">custom green span</Span>.\r\n    You can also use a <Run FontSize=\"24\">Run with a different font size</Run>.";
    public string GameWindowText
    {
        get => "Testing";
        set { _GameWindowText = value; NotifyPropertyChanged(); }
    }
    private string _labelRHC = string.Empty;
    public string LabelRHC
    {
        get => ViewManager.Instance.LabelRHC;
    }
    private string _labelLHC = string.Empty;
    public string LabelLHC
    {
        get => ViewManager.Instance.LabelLHC;
    }
    private string _labelSpellC = string.Empty;
    public string LabelSpellC
    {
        get => ViewManager.Instance.LabelSpellC;
    }

    public MainViewModel(IConfigSettings configsettings)
    {
        ConfigSettings = (configsettings as ConfigSettings);
        // Initialize the GameWindowText with some default text
        GameWindowText = "Welcome to the Genie Game!";
        ViewManager.Instance.PropertyChanged += ViewManager_PropertyChanged;
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
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Property Changed Notification
}
