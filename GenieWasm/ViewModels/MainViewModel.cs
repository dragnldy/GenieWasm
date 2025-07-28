using Avalonia.Controls;
using GenieCoreLib;

namespace GenieWasm.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ConfigSettings ConfigSettings { get; set; }
    private string _GameWindowText = "This is some plain text, followed by \r\n    <Bold>bold text</Bold>, \r\n    <Italic>italic text</Italic>, \r\n    and even a <Span Foreground=\"Green\">custom green span</Span>.\r\n    You can also use a <Run FontSize=\"24\">Run with a different font size</Run>.";
    public string GameWindowText
    {
        get => "Testing";
        set => _GameWindowText = value;
    }
    public MainViewModel(IConfigSettings configsettings)
    {
        ConfigSettings = (configsettings as ConfigSettings);
        // Initialize the GameWindowText with some default text
        GameWindowText = "Welcome to the Genie Game!";
    }
}
