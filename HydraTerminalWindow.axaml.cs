using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;

namespace PoseidonGUI;

public partial class HydraTerminalWindow : Window
{
    private TerminalSession? _terminal;

    public HydraTerminalWindow()
    {
        InitializeComponent();

        RunButton.Click += RunButton_Click;
        TerminalInput.KeyDown += TerminalInput_KeyDown;
        Closed += HydraTerminalWindow_Closed;

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

        // --- FILTER LOGIC ADDED HERE ---
        // Trim whitespace and check if the input strictly begins with "hydra"
        string trimmedCommand = command.Trim();
        if (!trimmedCommand.StartsWith("hydra ", StringComparison.OrdinalIgnoreCase) && 
            !trimmedCommand.Equals("hydra", StringComparison.OrdinalIgnoreCase))
        {
            AppendOutput($"\n[POSEIDON] Error: Only 'hydra' commands are allowed in this window.\nExample: hydra -l admin -P pass.txt 192.168.1.50 ssh\n\n");
            TerminalInput.Focus();
            return;
        }
        // -------------------------------

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

    private void HydraTerminalWindow_Closed(
        object? sender,
        EventArgs e)
    {
        _terminal?.Dispose();
        _terminal = null;
    }
}
