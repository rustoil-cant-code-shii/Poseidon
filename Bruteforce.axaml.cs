using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace PoseidonGUI;

public partial class BruteforceDialog : Window
{
    public BruteforceDialog()
    {
        InitializeComponent();

        BackendStatus.Text = "READY";

        StartButton.Click += StartButton_Click;
        StopButton.Click += StopButton_Click;
    }

    private void StartButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        BackendStatus.Text = "READY";

        // Process-management/backend integration will be added
        // separately. This dialog currently provides the GUI layer.
    }

    private void StopButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        BackendStatus.Text = "STOPPED";
    }
}
