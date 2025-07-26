using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace GenieWasm.UserControls;

public partial class GameWindow : UserControl
{

    public static readonly StyledProperty<string> GameWindowNameProperty =
        AvaloniaProperty.Register<GameWindow, string>(nameof(GameWindowName));
    public static readonly StyledProperty<string> BodyContentProperty =
        AvaloniaProperty.Register<GameWindow, string>(nameof(BodyContent));
    public string GameWindowName
    {
        get => GetValue(GameWindowNameProperty);
        set => SetValue(GameWindowNameProperty, value);
    }

    public string BodyContent
    {
        get => GetValue(BodyContentProperty);
        set => SetValue(BodyContentProperty, value);
    }

    SelectableTextBlock? _MainGameTextBlock = null;
    public SelectableTextBlock MainGameTextBlock
    {
        get => _MainGameTextBlock ?? this.FindControl<SelectableTextBlock>("mainTextBlock");
        set => _MainGameTextBlock = value;
    }
    public GameWindow()
    {
        InitializeComponent();
        DataContext = this;
    }
    public void CopyCommand()
    {
        // Implement copy command logic here
    }
    public void SelectAllCommand()
    {
        // Implement select all command logic here
    }
    internal void ClearTextBlock()
    {
        if (MainGameTextBlock is null)
        {
            throw new InvalidOperationException("Main Textblock not found in game window");
        }
        ClearGameContent(MainGameTextBlock);
    }
    public void ClearGameContent(SelectableTextBlock mainGameTB)
    {
        if (mainGameTB is null)
        {
            throw new ArgumentNullException(nameof(mainGameTB), "Main Textblock cannot be null");
        }
        mainGameTB.Text = string.Empty;
    }
}