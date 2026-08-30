using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Linq;

namespace PoseidonGUI;

public partial class AircrackTerminalWindow : Window
{
    private TerminalSession? _terminal;

    // Full list of official active tools in the standard Aircrack-ng suite
    private static readonly string[] AllowedTools = new[]
    {
        "aircrack-ng", "airmon-ng", "airodump-ng", "aireplay-ng", 
        "airbase-ng", "airdecap-ng", "airdecloak-ng", "airdrop-ng", 
        "airgraph-ng", "airolib-ng", "airserv-ng", "airtun-ng", 
        "besside-ng", "easside-ng", "packetforge-ng", "tkiptun-ng", 
        "wesside-ng", "dcrack"
    };

    public AircrackTerminalWindow()
    {
        InitializeComponent();

        RunButton.Click += RunButton_Click;
        TerminalInput.KeyDown += TerminalInput_KeyDown;
        Closed += AircrackTerminalWindow_Closed;

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
            AppendOutput($"\n[POSEIDON] Failed to start wireless audit sandbox:\n{ex.Message}\n");
        }
    }

    private async void RunButton_Click(object? sender, RoutedEventArgs e)
    {
        await SendCommand();
    }

    private async void TerminalInput_KeyDown(object? sender, KeyEventArgs e)
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

        // --- AIRCRACK-NG MULTI-COMMAND VALIDATION ---
        string trimmedCommand = command.Trim();
        
        // Extract the base executable name being typed (e.g., "airmon-ng")
        string firstWord = trimmedCommand.Split(' ')[0].ToLower();

        bool isAllowed = AllowedTools.Any(tool => firstWord == tool);

        if (!isAllowed)
        {
            AppendOutput($"\n[POSEIDON] Error: Invalid command. Only Aircrack-ng suite tools are allowed here.\n" +
                         $"Allowed commands: airmon-ng, airodump-ng, aireplay-ng, aircrack-ng, etc.\n\n");
            TerminalInput.Focus();
            return;
        }
        // ---------------------------------------------

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

    private void AircrackTerminalWindow_Closed(object? sender, EventArgs e)
    {
        _terminal?.Dispose();
        _terminal = null;
    }
}

