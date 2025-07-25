using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GenieWasm.ViewModels;

namespace GenieWasm.UserControls;

public partial class MainMenu : UserControl
{
    public MainMenu()
    {
        InitializeComponent();
        DataContext = new MainMenuViewModel();
    }
}