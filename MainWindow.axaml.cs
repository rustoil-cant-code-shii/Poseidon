using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace PoseidonGUI;

public partial class MainWindow : Window
{
    private TerminalSession? _terminal;
    private bool _terminalWarningShown;

    private static readonly Regex AnsiRegex =
        new(@"\x1B\[(?<codes>[0-9;]*)m",
            RegexOptions.Compiled);

    private IBrush _terminalColor =
        new SolidColorBrush(Color.Parse("#00FFFF"));

    private bool _bold;

    private readonly StringBuilder _plainTerminalText = new();

    private string _ansiPending = string.Empty;

    public MainWindow()
    {
        InitializeComponent();

        SystemInfo system = SystemInfo.Detect();

        AppendTerminalOutput(
            "POSEIDON SYSTEM DIAGNOSTICS\n" +
            "===========================\n\n" +
            $"OPERATING SYSTEM : {system.OperatingSystem}\n" +
            $"OS FAMILY        : {system.OsFamily}\n" +
            $"PACKAGE MANAGER  : {system.PackageManager}\n" +
            $"ARCHITECTURE     : {system.ArchitectureName}\n\n" +
            $"PLATFORM STATUS  : {(system.Supported ? "SUPPORTED" : "UNSUPPORTED")}\n" +
            $"POSEIDON STATUS  : {system.PoseidonStatus}\n\n" +
            $"{system.ArchitectureMessage}\n\n" +
            "POSEIDON TERMINAL OFFLINE\n" +
            "Press OPEN TERMINAL to start a shell.\n");

        // ============================================================
        // SECURITY MODULES
        // ============================================================

        ZenmapButton.Click += Tool_Click;
        WiresharkButton.Click += Tool_Click;

        // Hashcat gets its own dedicated terminal.
        HashcatButton.Click += HashcatButton_Click;

        // Aircrack-NG gets its own dedicated terminal.
        AircrackButton.Click += AircrackButton_Click;

        // Hydra gets its own dedicated terminal.
        HydraButton.Click += HydraButton_Click;

        // SQLMap gets its own dedicated terminal.
        SqlmapButton.Click += SqlmapButton_Click;

        // Main terminal.
        TerminalButton.Click += TerminalButton_Click;
        TerminalInput.KeyDown += TerminalInput_KeyDown;

        // Window lifecycle.
        Closed += MainWindow_Closed;
    }

    // ================================================================
    // TAB SYSTEM
    // ================================================================

    private void DashboardTab_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ShowTab("dashboard");
    }

    private void OffensiveTab_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ShowTab("offensive");
    }

    private void DefensiveTab_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ShowTab("defensive");
    }

    private void ShowTab(string tab)
    {
        DashboardPanel.IsVisible =
            tab == "dashboard";

        OffensivePanel.IsVisible =
            tab == "offensive";

        DefensivePanel.IsVisible =
            tab == "defensive";

        DashboardTab.Classes.Remove("selected");
        OffensiveTab.Classes.Remove("selected");
        DefensiveTab.Classes.Remove("selected");

        switch (tab)
        {
            case "dashboard":
                DashboardTab.Classes.Add("selected");
                break;

            case "offensive":
                OffensiveTab.Classes.Add("selected");
                break;

            case "defensive":
                DefensiveTab.Classes.Add("selected");
                break;
        }
    }

    // ================================================================
    // HASHCAT
    // ================================================================

    private async void HashcatButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var warning =
            new WarningDialog("HASHCAT");

        await warning.ShowDialog(this);

        if (!warning.Confirmed)
        {
            AppendTerminalOutput(
                "\n\n[POSEIDON] HASHCAT terminal launch cancelled.\n");

            return;
        }

        AppendTerminalOutput(
            "\n\n[POSEIDON] Opening HASHCAT terminal...\n");

        var hashcatTerminal =
            new HashcatTerminalWindow();

        await hashcatTerminal.ShowDialog(this);
    }

    // ================================================================
    // AIRCRACK-NG
    // ================================================================

    private async void AircrackButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var warning =
            new WarningDialog("AIRCRACK-NG");

        await warning.ShowDialog(this);

        if (!warning.Confirmed)
        {
            AppendTerminalOutput(
                "\n\n[POSEIDON] AIRCRACK-NG terminal launch cancelled.\n");

            return;
        }

        AppendTerminalOutput(
            "\n\n[POSEIDON] Opening AIRCRACK-NG terminal...\n");

        var aircrackTerminal =
            new AircrackTerminalWindow();

        await aircrackTerminal.ShowDialog(this);
    }

    // ================================================================
    // HYDRA
    // ================================================================

    private async void HydraButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var warning =
            new WarningDialog("HYDRA");

        await warning.ShowDialog(this);

        if (!warning.Confirmed)
        {
            AppendTerminalOutput(
                "\n\n[POSEIDON] HYDRA terminal launch cancelled.\n");

            return;
        }

        AppendTerminalOutput(
            "\n\n[POSEIDON] Opening HYDRA terminal...\n");

        var hydraTerminal =
            new HydraTerminalWindow();

        await hydraTerminal.ShowDialog(this);
    }

    // ================================================================
    // SQLMAP
    // ================================================================

    private async void SqlmapButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var warning =
            new WarningDialog("SQLMAP");

        await warning.ShowDialog(this);

        if (!warning.Confirmed)
        {
            AppendTerminalOutput(
                "\n\n[POSEIDON] SQLMAP terminal launch cancelled.\n");

            return;
        }

        AppendTerminalOutput(
            "\n\n[POSEIDON] Opening SQLMAP terminal...\n");

        var sqlmapTerminal =
            new SqlmapTerminalWindow();

        await sqlmapTerminal.ShowDialog(this);
    }

    // ================================================================
    // MAIN TERMINAL
    // ================================================================

    private async void TerminalButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (_terminal is not null &&
            _terminal.IsRunning)
        {
            TerminalInput.Focus();
            return;
        }

        if (!_terminalWarningShown)
        {
            var warning =
                new TerminalWarningDialog();

            await warning.ShowDialog(this);

            if (!warning.Confirmed)
                return;

            _terminalWarningShown = true;
        }

        StartTerminal();
    }

    private void StartTerminal()
    {
        try
        {
            _terminal?.Dispose();

            _terminal =
                new TerminalSession();

            _terminal.OutputReceived +=
                Terminal_OutputReceived;

            TerminalInput.IsEnabled = true;

            TerminalStatus.Text =
                "STATUS: ● TERMINAL ONLINE";

            TerminalOutput.Inlines?.Clear();

            TerminalSelection.Text =
                string.Empty;

            _plainTerminalText.Clear();

            _ansiPending =
                string.Empty;

            _terminalColor =
                new SolidColorBrush(
                    Color.Parse("#00FFFF"));

            _bold = false;

            AppendTerminalOutput(
                "POSEIDON TERMINAL\n" +
                "=================\n\n");

            TerminalInput.Focus();
        }
        catch (Exception ex)
        {
            AppendTerminalOutput(
                $"\n[POSEIDON] Failed to start terminal:\n" +
                $"{ex.Message}\n");

            TerminalStatus.Text =
                "STATUS: ● TERMINAL ERROR";
        }
    }

    private void Terminal_OutputReceived(
        string text)
    {
        Dispatcher.UIThread.Post(() =>
        {
            AppendTerminalOutput(text);

            TerminalScroll.ScrollToEnd();
        });
    }

    private async void TerminalInput_KeyDown(
        object? sender,
        KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        if (_terminal is null ||
            !_terminal.IsRunning)
            return;

        string command =
            TerminalInput.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(command))
            return;

        TerminalInput.Text =
            string.Empty;

        await _terminal.SendCommandAsync(command);

        e.Handled = true;
    }

    // ================================================================
    // SECURITY TOOLS
    // ================================================================

    private async void Tool_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        string toolName =
            button.Content?.ToString() ?? "UNKNOWN";

        ToolManager.ToolInfo? tool =
            ToolManager.GetTool(
                toolName.ToUpper().Trim());

        if (tool is null)
        {
            AppendTerminalOutput(
                $"\n\n[POSEIDON] Unknown module: " +
                $"{toolName}\n");

            return;
        }

        var warning =
            new WarningDialog(tool.Name);

        await warning.ShowDialog(this);

        if (!warning.Confirmed)
        {
            AppendTerminalOutput(
                $"\n\n[POSEIDON] {tool.Name} " +
                "launch cancelled.\n");

            return;
        }

        AppendTerminalOutput(
            $"\n\n[POSEIDON] Checking " +
            $"{tool.Name}...\n");

        bool installed =
            ToolManager.IsInstalled(tool);

        if (installed)
        {
            AppendTerminalOutput(
                $"[+] {tool.Name} detected.\n" +
                $"[+] Command: {tool.Command}\n" +
                $"[+] Package: {tool.Package}\n" +
                $"[+] Launching process...\n");

            bool started =
                ToolManager.Launch(tool);

            if (!started)
            {
                AppendTerminalOutput(
                    $"[-] Failed to start terminal engine wrapper " +
                    $"for {tool.Name}.\n");
            }

            return;
        }

        AppendTerminalOutput(
            $"[-] {tool.Name} was not found.\n" +
            $"[-] Arch package: {tool.Package}\n");

        var installDialog =
            new InstallDialog(tool);

        await installDialog.ShowDialog(this);

        if (!installDialog.Confirmed)
        {
            AppendTerminalOutput(
                "\n[POSEIDON] Installation cancelled.\n");

            return;
        }

        AppendTerminalOutput(
            $"\n[POSEIDON] Installation requested: " +
            $"{tool.Package}\n" +
            "[POSEIDON] Package installation module ready.\n");
    }

    // ================================================================
    // ANSI TERMINAL RENDERING
    // ================================================================

    private void AppendTerminalOutput(string text)
    {
        if (TerminalOutput.Inlines is null)
            return;

        string plainText =
            AnsiRegex.Replace(
                text,
                string.Empty);

        _plainTerminalText.Append(
            plainText);

        TerminalSelection.Text =
            _plainTerminalText.ToString();

        string input =
            _ansiPending + text;

        _ansiPending =
            string.Empty;

        int position = 0;

        while (position < input.Length)
        {
            int escapeStart =
                input.IndexOf(
                    '\x1B',
                    position);

            if (escapeStart == -1)
            {
                AddRun(
                    input[position..]);

                break;
            }

            if (escapeStart > position)
            {
                AddRun(
                    input[position..escapeStart]);
            }

            if (escapeStart + 1 >= input.Length)
            {
                _ansiPending =
                    input[escapeStart..];

                break;
            }

            if (input[escapeStart + 1] != '[')
            {
                AddRun("\x1B");

                position =
                    escapeStart + 1;

                continue;
            }

            int commandEnd =
                FindCsiCommandEnd(
                    input,
                    escapeStart + 2);

            if (commandEnd == -1)
            {
                _ansiPending =
                    input[escapeStart..];

                break;
            }

            char command =
                input[commandEnd];

            string sequence =
                input[
                    escapeStart..
                    (commandEnd + 1)];

            if (command == 'm')
            {
                Match match =
                    AnsiRegex.Match(
                        sequence);

                if (match.Success)
                {
                    ApplyAnsiCodes(
                        match.Groups["codes"].Value);
                }
            }

            position =
                commandEnd + 1;
        }
    }

    private static int FindCsiCommandEnd(
        string text,
        int start)
    {
        for (int i = start;
             i < text.Length;
             i++)
        {
            char c =
                text[i];

            if (c >= '@' && c <= '~')
                return i;
        }

        return -1;
    }

    private void AddRun(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        TerminalOutput.Inlines?.Add(
            new Run
            {
                Text = text,
                Foreground =
                    _terminalColor,
                FontWeight =
                    _bold
                        ? FontWeight.Bold
                        : FontWeight.Normal
            });
    }

    private void ApplyAnsiCodes(
        string codeString)
    {
        if (string.IsNullOrEmpty(codeString))
            codeString = "0";

        string[] codes =
            codeString.Split(';');

        foreach (string code in codes)
        {
            if (!int.TryParse(
                    code,
                    out int codeValue))
            {
                continue;
            }

            switch (codeValue)
            {
                case 0:
                    _terminalColor =
                        new SolidColorBrush(
                            Color.Parse("#00FFFF"));

                    _bold = false;
                    break;

                case 1:
                    _bold = true;
                    break;

                case 22:
                    _bold = false;
                    break;

                case 30:
                    SetTerminalColor("#000000");
                    break;

                case 31:
                    SetTerminalColor("#FF5555");
                    break;

                case 32:
                    SetTerminalColor("#55FF55");
                    break;

                case 33:
                    SetTerminalColor("#FFFF55");
                    break;

                case 34:
                    SetTerminalColor("#5555FF");
                    break;

                case 35:
                    SetTerminalColor("#FF55FF");
                    break;

                case 36:
                    SetTerminalColor("#55FFFF");
                    break;

                case 37:
                    SetTerminalColor("#FFFFFF");
                    break;

                case 39:
                    SetTerminalColor("#00FFFF");
                    break;

                case 90:
                    SetTerminalColor("#777777");
                    break;

                case 91:
                    SetTerminalColor("#FF7777");
                    break;

                case 92:
                    SetTerminalColor("#77FF77");
                    break;

                case 93:
                    SetTerminalColor("#FFFF77");
                    break;

                case 94:
                    SetTerminalColor("#7777FF");
                    break;

                case 95:
                    SetTerminalColor("#FF77FF");
                    break;

                case 96:
                    SetTerminalColor("#77FFFF");
                    break;

                case 97:
                    SetTerminalColor("#FFFFFF");
                    break;

                case 48:
                case 49:
                    break;
            }
        }
    }

    private void SetTerminalColor(
        string hexColor)
    {
        _terminalColor =
            new SolidColorBrush(
                Color.Parse(hexColor));
    }

    // ================================================================
    // WINDOW LIFECYCLE
    // ================================================================

    private void MainWindow_Closed(
        object? sender,
        EventArgs e)
    {
        _terminal?.Dispose();

        _terminal = null;
    }
}
