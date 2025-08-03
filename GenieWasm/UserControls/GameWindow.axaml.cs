using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace GenieWasm.UserControls;

public partial class GameWindow : UserControl
{
    #region Styled Properties (for passing parameters from the parent control)
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
    #endregion

    #region Command Properties
    // Add ICommand properties
    public ICommand CopyCommand { get; }
    public ICommand SelectAllCommand { get; }
    public ICommand ScrollToEndCommand { get; }
    #endregion Command Properties


    #region Properties for controlling size and position of the game window
    SelectableTextBlock? mainGameTextBlock = null;
    public SelectableTextBlock? MainGameTextBlock
    {
        get => mainGameTextBlock ?? this.FindControl<SelectableTextBlock>("GameTextBlock");
        set => mainGameTextBlock = value;
    }
    private double borderHeight = 200;
    public double BorderHeight {
        get => borderHeight; 
        set { if (borderHeight != value) { borderHeight = value; NotifyPropertyChanged(); } } 
    }

    private double maxBorderHeight = 600;
    public double MaxBorderHeight
    {
        get => maxBorderHeight;
        set { if (maxBorderHeight != value) { maxBorderHeight = value; NotifyPropertyChanged(); } }
    }

    public double MinBorderHeight { get; private set; } = 100;
    #endregion Properties for controlling size and position of the game window

    #region Constructor
    public GameWindow()
    {
        // Initialize commands
        CopyCommand = new RelayCommand(_ => CopyText());
        SelectAllCommand = new RelayCommand(_ => SelectAllText());
        ScrollToEndCommand = new RelayCommand(_ => ScrollGameToEnd());

        InitializeComponent();
        DataContext = this;
        Loaded += LoadedEventHandler;
    }
    bool isDisplayed = true;
    private void LoadedEventHandler(object? sender, RoutedEventArgs e)
    {
        Loaded -= LoadedEventHandler;
        mainGameTextBlock = this.FindControl<SelectableTextBlock>("GameTextBlock");
    }
    #endregion Constructor

    #region Methods to manage size of the game window
    private void Border_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        Border border = sender as Border;
        StackPanel panel = border.FindControl<StackPanel>("GameStacker");
        panel.Height = border.Bounds.Height - 20;
    }
    #endregion Methods to manage size of the game window


    #region Commands for TextBlock
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
    private void CopyText()
    {
        // Copy text to the clipboard
        MainGameTextBlock.Copy();
    }

    private void SelectAllText()
    {
        // Implement your select all logic here
        MainGameTextBlock?.SelectAll();
    }
    private void ScrollGameToEnd()
    {
        this.GameScroller.ScrollToEnd();
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
    #endregion Commands for TextBlock

    #region Property Changed Notification
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
                // Otherwise, just append the new content
                // MainGameTextBlock?.AppendText(BodyContent);
                if (IsAtBottom)
                {
                    // If the scroll viewer is already at the bottom, scroll to the end
                    // Otherise leave position as is- user maybe reading something
                    this.GameScroller.ScrollToEnd();
                }
            });
        }
    }
    #endregion Property Changed Notification

    #region Border Sizing and Context Menu
    Border? originalSender = null;
    bool bBorderSizing = false;
    double visualPositionX = 0;
    double visualPositionY = 0;
    bool bContextMenu = false;

    private void Border_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.KeyModifiers != Avalonia.Input.KeyModifiers.Control || e.Properties.IsRightButtonPressed)
        {
            // Checking for context menu
            bContextMenu = e.Properties.IsRightButtonPressed;
            bBorderSizing = false;
            return;
        }
        if (sender is Border border)
        {
            if (border.Name.Equals("GameBorder", StringComparison.OrdinalIgnoreCase))
            {
                bContextMenu = false;
                bBorderSizing = true;
                originalSender = border;
                // get the position relative to the main window
                var position = e.GetPosition(this as Control);
                visualPositionX = position.X;
                visualPositionY = position.Y;

                e.Handled = true;
            }
        }
    }
    private void Border_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        // If asking for a context menu
        if (bContextMenu)
        {
            SelectableTextBlock textBlock = this.MainGameTextBlock;
            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(new MenuItem { Header = "Copy", Command = CopyCommand });
            contextMenu.Items.Add(new MenuItem { Header = "Select All", Command = SelectAllCommand });
            contextMenu.Items.Add(new MenuItem { Header = "Scroll To End", Command = ScrollToEndCommand });
            contextMenu.Open(textBlock);
            e.Handled = true;
            return;
        }
        if (!bBorderSizing) { e.Handled = false; return; }
        if (sender is Border border)
        {
            if (border.Name.Equals("GameBorder", StringComparison.OrdinalIgnoreCase))
            {
                if (originalSender == border)
                {
                    // get the position relative to the main window
                    var position = e.GetPosition(this as Control);
                    double newPositionX = position.X;
                    double newPositionY = position.Y;
                    double diffX = newPositionX - visualPositionX;
                    double diffY = newPositionY - visualPositionY;
                    if (diffY > 0)
                        BorderHeight = Math.Min(originalSender.Height + diffY, MaxBorderHeight);
                    else
                        BorderHeight = Math.Max(originalSender.Height + diffY,MinBorderHeight);

                    StackPanel panel = originalSender.FindControl<StackPanel>("GameStacker");
                    ScrollViewer scroller = originalSender.FindControl<ScrollViewer>("GameScroller");
                    panel.Height = BorderHeight - 20;
                    scroller.Height = BorderHeight - 12;
                    originalSender.Height = BorderHeight;
                    originalSender.InvalidateMeasure();
                    panel.InvalidateMeasure(); // InvalidateArrange();//
                    scroller.InvalidateMeasure();
                }
                bBorderSizing = false;
            }
        }
    }

    private void Border_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (!bBorderSizing) return;
        if (sender is Border border)
        {
            if (border.Name.Equals("GameBorder", StringComparison.OrdinalIgnoreCase))
            {
                e.Handled = true;
            }
        }
    }

    private void Border_PointerCaptureLost(object? sender, Avalonia.Input.PointerCaptureLostEventArgs e)
    {
        if (bBorderSizing && sender is Border border)
        {
            if (border.Name.Equals("GameBorder", StringComparison.OrdinalIgnoreCase))
            {
                bBorderSizing = false;
                e.Handled = true;
            }
        }
    }
    #endregion

    #region Scrollviewer Position Monitor
    public bool IsAtBottom => GameScroller.Offset.Y >= (GameScroller.Extent.Height - GameScroller.Viewport.Height);

    //sv.GetObservable(ScrollViewer.OffsetProperty)
    //.Subscribe(offset =>
    //{
    //    // Calculate the maximum possible vertical offset
    //    var maxVerticalOffset = sv.Extent.Height - sv.Viewport.Height;

    //    // Check if the current vertical offset is at or very close to the maximum
    //    if (Math.Abs(maxVerticalOffset - offset.Y) <= Double.Epsilon)
    //    {
    //        Console.WriteLine("ScrollViewer is at the bottom!");
    //        // Perform actions when at the bottom, e.g., load more data
    //    }
    //})
    //.DisposeWith(disposables); // Remember to dispose of the subscription when the control is no longer needed
    #endregion Scrollviewer Position Monitor
}