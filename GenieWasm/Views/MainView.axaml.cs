using Avalonia.Controls;
using GenieWasm.UserControls;
using GenieWasm.ViewModels;
using System;

namespace GenieWasm.Views;

public partial class MainView : UserControl
{
    private string _GameWindowText = "This is some plain text, followed by \r\n    <Bold>bold text</Bold>, \r\n    <Italic>italic text</Italic>, \r\n    and even a <Span Foreground=\"Green\">custom green span</Span>.\r\n    You can also use a <Run FontSize=\"24\">Run with a different font size</Run>.";
    public string GameWindowText
    {
        get => _GameWindowText;
        set => _GameWindowText = value;
    }

    public MainView()
    {
        InitializeComponent();
        DataContext = this;
        GameWindow gameWindow = this.FindControl<GameWindow>("mainGame");
        if (gameWindow is null)
        {
            throw new InvalidOperationException("GameWindow control not found.");
        }
        gameWindow.ClearTextBlock();
    }
}
