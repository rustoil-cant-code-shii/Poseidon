using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;

namespace PoseidonGUI;

public partial class HashcatTerminalWindow : Window
{
    private TerminalSession? _terminal;

    public HashcatTerminalWindow()
    {
        InitializeComponent();

        RunButton.Click += RunButton_Click;
        TerminalInput.KeyDown += TerminalInput_KeyDown;
        Closed += HashcatTerminalWindow_Closed;

        StartTerminal();
    }

    private void StartTerminal()
    {
        try
        {
            _terminal?.Dispose();

            _terminal = new TerminalSession();

            _terminal.OutputReceived += Terminal_OutputReceived;

            TerminalInput.Focus();
        }
        catch (Exception ex)
        {
            AppendOutput(
                $"\n[POSEIDON] Failed to start terminal:\n{ex.Message}\n");
        }
    }

    private async void RunButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        await SendCommand();
    }

    private async void TerminalInput_KeyDown(
        object? sender,
        KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        await SendCommand();

        e.Handled = true;
    }

    private async System.Threading.Tasks.Task SendCommand()
    {
        if (_terminal is null || !_terminal.IsRunning)
            return;

        string command = TerminalInput.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(command))
            return;

        TerminalInput.Text = string.Empty;

        // ------------------------------------------------------------
        // HASHCAT COMMAND FILTER
        // ------------------------------------------------------------

        string trimmedCommand = command.Trim();

        if (!trimmedCommand.StartsWith(
                "hashcat ",
                StringComparison.OrdinalIgnoreCase) &&
            !trimmedCommand.Equals(
                "hashcat",
                StringComparison.OrdinalIgnoreCase))
        {
            AppendOutput(
                "\n[POSEIDON] Error: Only 'hashcat' commands are allowed in this window.\n" +
                "Example: hashcat [options]\n\n");

            TerminalInput.Focus();
            return;
        }

        // ------------------------------------------------------------

        await _terminal.SendCommandAsync(command);

        TerminalInput.Focus();
    }

    private void Terminal_OutputReceived(string text)
    {
        Dispatcher.UIThread.Post(() =>
        {
            AppendOutput(text);
            TerminalScroll.ScrollToEnd();
        });
    }

    private void AppendOutput(string text)
    {
        if (TerminalOutput is null)
            return;

        TerminalOutput.Text += text;
        TerminalScroll.ScrollToEnd();
    }

    private void HashcatTerminalWindow_Closed(
        object? sender,
        EventArgs e)
    {
        _terminal?.Dispose();
        _terminal = null;
    }
}
