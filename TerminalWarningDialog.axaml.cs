using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;

namespace PoseidonGUI;

public partial class TerminalWarningDialog : Window
{
    public bool Confirmed { get; private set; }

    private int _seconds = 10;

    private readonly DispatcherTimer _timer;

    public TerminalWarningDialog()
    {
        InitializeComponent();

        CancelButton.Click += Cancel_Click;
        ContinueButton.Click += Continue_Click;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(
        object? sender,
        EventArgs e)
    {
        _seconds--;

        CountdownText.Text =
            $"AUTO-CLOSE IN: {_seconds}";

        if (_seconds <= 0)
        {
            _timer.Stop();

            Confirmed = false;

            Close();
        }
    }

    private void Cancel_Click(
        object? sender,
        RoutedEventArgs e)
    {
        _timer.Stop();

        Confirmed = false;

        Close();
    }

    private void Continue_Click(
        object? sender,
        RoutedEventArgs e)
    {
        _timer.Stop();

        Confirmed = true;

        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _timer.Stop();

        base.OnClosed(e);
    }
}
