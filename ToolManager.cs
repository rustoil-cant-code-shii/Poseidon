using System;
using System.Diagnostics;

namespace PoseidonGUI;

public static class ToolManager
{
    public sealed record ToolInfo(
        string Name,
        string Command,
        string Package,
        bool RequiresTerminal = true
    );

    public static readonly ToolInfo Zenmap =
        new("ZENMAP", "zenmap", "zenmap", RequiresTerminal: false);

    public static readonly ToolInfo Metasploit =
        new("METASPLOIT", "msfconsole", "metasploit", RequiresTerminal: true);

    public static readonly ToolInfo Wireshark =
        new("WIRESHARK", "wireshark", "wireshark-qt", RequiresTerminal: false);

    public static readonly ToolInfo Hashcat =
        new("HASHCAT", "hashcat", "hashcat", RequiresTerminal: true);

    public static readonly ToolInfo Hydra =
        new("HYDRA", "hydra", "hydra", RequiresTerminal: true);

    public static readonly ToolInfo Sqlmap =
        new("SQLMAP", "sqlmap", "sqlmap", RequiresTerminal: true);

    public static readonly ToolInfo Aircrack =
        new("AIRCRACK-NG", "aircrack-ng", "aircrack-ng", RequiresTerminal: true);

    public static bool Launch(ToolInfo tool)
    {
        if (!IsInstalled(tool))
        {
            return false;
        }

        try
        {
            using Process process = new();

            if (tool.RequiresTerminal)
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments =
                        $"-c \"{tool.Command}; echo -e '\\nProcess finished. Press Enter to close...'; read\"",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };
            }
            else
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = tool.Command,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            }

            return process.Start();
        }
        catch
        {
            return false;
        }
    }

    public static bool IsInstalled(ToolInfo tool)
    {
        try
        {
            using Process process = new();

            process.StartInfo = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-lc \"command -v {tool.Command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            process.WaitForExit();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsPacmanAvailable()
    {
        try
        {
            using Process process = new();

            process.StartInfo = new ProcessStartInfo
            {
                FileName = "pacman",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            process.WaitForExit();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static ToolInfo? GetTool(string name)
    {
        return name switch
        {
            "ZENMAP" => Zenmap,
            "METASPLOIT" => Metasploit,
            "WIRESHARK" => Wireshark,
            "HASHCAT" => Hashcat,
            "HYDRA" => Hydra,
            "SQLMAP" => Sqlmap,
            "AIRCRACK-NG" => Aircrack,
            _ => null
        };
    }
}
