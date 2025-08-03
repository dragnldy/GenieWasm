using Avalonia.Controls;
using GenieWasm.ViewModels;
using System.ComponentModel;
using System.Threading.Tasks.Dataflow;

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
        Close(ConnectViewModel.DialogResult.Equals("Connected",System.StringComparison.OrdinalIgnoreCase));
    }
    public void OnSaveButtonClicked()
    {
        // Perform some validation or data processing
        bool result = true; // Or get from user input
        Close(result); // Pass the return value
    }

    public void OnCancelButtonClicked()
    {
        Close(false); // Return false on cancel
    }
}