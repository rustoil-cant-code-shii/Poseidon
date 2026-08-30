using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PoseidonGUI;

public partial class WarningDialog : Window
{
    public bool Confirmed { get; private set; }

    // Required by Avalonia's runtime loader.
    public WarningDialog()
    {
        InitializeComponent();

        CancelButton.Click += Cancel_Click;
        ContinueButton.Click += Continue_Click;
    }

    // Used by Poseidon when displaying a warning for a specific tool.
    public WarningDialog(string toolName)
        : this()
    {
        ToolNameText.Text = toolName;
    }

    private void Cancel_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Confirmed = false;
        Close();
    }

    private void Continue_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Confirmed = true;
        Close();
    }
}
