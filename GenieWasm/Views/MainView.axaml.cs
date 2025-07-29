using Avalonia.Controls;
using GenieCoreLib;
using GenieWasm.UserControls;
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
        SetCustomStyles();
        DataContext = this;
        GameWindow gameWindow = this.FindControl<GameWindow>("mainGame");
        if (gameWindow is null)
        {
            throw new InvalidOperationException("GameWindow control not found.");
        }
        gameWindow.ClearTextBlock();
    }
    private void SetCustomStyles()
    {
        //// Set custom styles for the GameWindow text block
        //GameWindow gameWindow = this.FindControl<GameWindow>("mainGame");
        //if (gameWindow is not null)
        //{
        //    gameWindow.SetTextBlockStyle("GameWindowTextBlock", "Courier New", 16, "WhiteSmoke");
        //}
    }   

    private static bool SoundIsOn = false;
    public bool PlaySoundCommand()
    {
        if (!SoundIsOn)
        {
            SoundIsOn = true;
            EventCallBacks.OnPlaySoundRequested("https://github.com/rafaelreis-hotmart/Audio-Sample-files/raw/refs/heads/master/sample.mp3");
        }
        else
        {
            SoundIsOn = false;
            EventCallBacks.OnPlaySoundRequested(string.Empty);
        }
        return SoundIsOn;
    }

}