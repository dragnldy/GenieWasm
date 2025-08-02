using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GenieWasm.UserControls;

public partial class GameWindow : UserControl
{

    public static readonly StyledProperty<string> GameWindowNameProperty =
        AvaloniaProperty.Register<GameWindow, string>(nameof(GameWindowName));
    public static readonly StyledProperty<string> BodyContentProperty =
        AvaloniaProperty.Register<GameWindow, string>(nameof(BodyContent));
    public static readonly StyledProperty<int> WindowLocationProperty =
        AvaloniaProperty.Register<GameWindow, int>(nameof(WindowLocation));
    public string GameWindowName
    {
        get => GetValue(GameWindowNameProperty);
        set { SetValue(GameWindowNameProperty, value); NotifyPropertyChanged(); }
    }

    public string BodyContent
    {
        get => GetValue(BodyContentProperty);
        set { SetValue(BodyContentProperty, value); NotifyPropertyChanged(); }
    }
    public int WindowLocation
    {
        get => GetValue(WindowLocationProperty);
        set { SetValue(WindowLocationProperty, value); NotifyPropertyChanged(); }
    }

    SelectableTextBlock? mainGameTextBlock = null;
    public SelectableTextBlock? MainGameTextBlock
    {
        get => mainGameTextBlock ?? this.FindControl<SelectableTextBlock>("GameTextBlock");
        set => mainGameTextBlock = value;
    }
    public GameWindow()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += LoadedEventHandler;
    }
    bool isDisplayed = true;
    private void LoadedEventHandler(object? sender, RoutedEventArgs e)
    {
        Loaded -= LoadedEventHandler;
        mainGameTextBlock = this.FindControl<SelectableTextBlock>("GameTextBlock");
        if (mainGameTextBlock is null)
        {
            throw new InvalidOperationException("Main Textblock not found in game window");
        }
        Border border = this.FindControl<Border>("GameBorder");

        StackPanel owner = mainGameTextBlock.Parent.Parent as StackPanel;
        if (owner is null)
        {
            throw new InvalidOperationException("Main Textblock owner is not stack panel");
        }
        SetTextWindowDimensions(owner, mainGameTextBlock);
    }
    private void Border_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        Border border = sender as Border;
        StackPanel panel = border.FindControl<StackPanel>("GameStacker");
        panel.Height = border.Bounds.Height - 20;
    }
    private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (this.GameWindowName.Equals("Game"))
        {

        }
        // Handle size changes if needed
        // This can be used to adjust the layout dynamically
        if (sender is StackPanel stackPanel)
        {
            SetTextWindowDimensions(sender as StackPanel, mainGameTextBlock?? this.FindControl<SelectableTextBlock>("GameSelectableTextBlock"));
        }
    }
    private void SetTextWindowDimensions(StackPanel parent, SelectableTextBlock textBlock)
    {

        //// Example: Adjust the width of the text block based on the stack panel's width
        //textBlock.Width = parent.Bounds.Width - 60; // Leave some padding
        //textBlock.Height = parent.Bounds.Height - 20;
    }
    public void Close()
    {
        // Get rid of the window by removing it from Parent collection
        StackPanel parent = this.Parent as StackPanel;
        if (parent is null)
        {
            throw new Exception("not the right owner");
        }
        parent.Children.Remove(this);
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

    public event PropertyChangedEventHandler PropertyChanged;

    // This method is called by the Set accessor of each property.
    // The CallerMemberName attribute that is applied to the optional propertyName
    // parameter causes the property name of the caller to be substituted as an argument.
    public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        if (propertyName.Equals(nameof(BodyContent)) && BodyContent.Length > 0)
        {
            Dispatcher.UIThread.Post(() =>
            {
                this.GameScroller.ScrollToEnd();
            });
        }
    }

}