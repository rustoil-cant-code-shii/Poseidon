using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;

namespace PoseidonGUI;

public partial class SqlmapTerminalWindow : Window
{
    private TerminalSession? _terminal;

    public SqlmapTerminalWindow()
    {
        InitializeComponent();

        RunButton.Click += RunButton_Click;
        TerminalInput.KeyDown += TerminalInput_KeyDown;
        Closed += SqlmapTerminalWindow_Closed;

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

        // --- SQLMAP FILTER LOGIC ---
        string trimmedCommand = command.Trim();
        if (!trimmedCommand.StartsWith("sqlmap ", StringComparison.OrdinalIgnoreCase) && 
            !trimmedCommand.Equals("sqlmap", StringComparison.OrdinalIgnoreCase))
        {
            AppendOutput($"\n[POSEIDON] Error: Only 'sqlmap' commands are allowed in this window.\nExample: sqlmap -u \"http://testsite.com\" --dbs\n\n");
            TerminalInput.Focus();
            return;
        }
        // ---------------------------

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

    private void SqlmapTerminalWindow_Closed(
        object? sender,
        EventArgs e)
    {
        _terminal?.Dispose();
        _terminal = null;
    }
}

