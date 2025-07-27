using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GenieWasm.UserControls;
using GenieCoreLib;
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