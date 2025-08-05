using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GenieWasm.ViewModels;
using System.ComponentModel;

namespace GenieWasm;

public partial class ConnectView : Window
{
    ConnectViewModel ConnectViewModel => DataContext as ConnectViewModel;
    public ConnectView()
    {
        InitializeComponent();
        DataContext = new ConnectViewModel();
        // Register a handler to listen for the message sent by the view model.
        ConnectViewModel.PropertyChanged += OnClose_PropertyChanged; // Subscribe to the event
    }

    public void OnClose_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ConnectViewModel.DialogResult)) return;

        // Get the interaction from the view model
        // Close the dialog with the result

        ConnectViewModel.ConnectionRequest.IsValid = ConnectViewModel.DialogResult == "Connected";
        Close(ConnectViewModel.ConnectionRequest);
    }
    private void AutoCompleteBox_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is AutoCompleteBox autoCompleteBox)
        {
            ConnectViewModel.CharacterCompleteBox_LostFocus(sender, e);
        }
    }
}
