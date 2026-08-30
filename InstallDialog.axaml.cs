using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace PoseidonGUI;

public partial class InstallDialog : Window
{
    public bool Confirmed { get; private set; }

    private DispatcherTimer? _pulseTimer;
    private int _pulseCount;
    private bool _expanded;

    public InstallDialog()
    {
        InitializeComponent();

        CloseButton.Click += Close_Click;

        StartWarningPulse();
    }

    public InstallDialog(ToolManager.ToolInfo tool)
        : this()
    {
        ToolText.Text = $"Tool: {tool.Name}";
        PackageText.Text = $"Arch package: {tool.Package}";
        CommandText.Text = $"sudo pacman -S {tool.Package}";
    }

    private void StartWarningPulse()
    {
        _pulseCount = 0;
        _expanded = false;

        _pulseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(350)
        };

        _pulseTimer.Tick += WarningPulse_Tick;
        _pulseTimer.Start();
    }

    private void WarningPulse_Tick(object? sender, EventArgs e)
    {
        if (_pulseTimer is null)
            return;

        _expanded = !_expanded;

        if (_expanded)
        {
            WarningText.FontSize = 30;
            WarningBorder.BorderThickness = new Thickness(3);
        }
        else
        {
            WarningText.FontSize = 24;
            WarningBorder.BorderThickness = new Thickness(2);

            _pulseCount++;

            if (_pulseCount >= 2)
            {
                _pulseTimer.Stop();
                _pulseTimer.Tick -= WarningPulse_Tick;
                _pulseTimer = null;
            }
        }
    }

    private void Close_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Confirmed = false;
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _pulseTimer?.Stop();
        _pulseTimer = null;

        base.OnClosed(e);
    }
}
