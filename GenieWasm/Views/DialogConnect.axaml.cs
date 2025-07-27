using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GenieWasm.ViewModels;

namespace GenieWasm;

public partial class DialogConnect : Window
{
    public DialogConnect()
    {
        InitializeComponent();
        DataContext = new ConnectViewModel();
    }
}