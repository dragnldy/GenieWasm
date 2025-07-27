using Avalonia.Controls;
using GenieCoreLib;

namespace GenieWasm.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private string _GameWindowText = "This is some plain text, followed by \r\n    <Bold>bold text</Bold>, \r\n    <Italic>italic text</Italic>, \r\n    and even a <Span Foreground=\"Green\">custom green span</Span>.\r\n    You can also use a <Run FontSize=\"24\">Run with a different font size</Run>.";
    public string GameWindowText
    {
        get => "Testing";
        set => _GameWindowText = value;
    }
}
