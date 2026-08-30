using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PoseidonGUI;

public sealed class SystemInfo
{
    public string OperatingSystem { get; init; } = "Unknown";
    public string OsFamily { get; init; } = "Unknown";
    public string PackageManager { get; init; } = "Unknown";
    public string ArchitectureName { get; init; } = "Unknown";
    public bool Supported { get; init; }

    public string PoseidonStatus
    {
        get
        {
            if (ArchitectureName == "ARM64")
                return "bro what";

            return Supported ? "ONLINE" : "LIMITED";
        }
    }

    public string ArchitectureMessage
    {
        get
        {
            if (ArchitectureName == "ARM64")
                return "son how'd you even get ts running on your system";

            return Supported
                ? "Arch-based environment detected."
                : "This platform is not officially supported.";
        }
    }

    public static SystemInfo Detect()
    {
        string operatingSystem = "Unknown";
        string osFamily = "Unknown";
        string packageManager = "Unknown";

        try
        {
            if (File.Exists("/etc/os-release"))
            {
                string[] lines = File.ReadAllLines("/etc/os-release");

                foreach (string line in lines)
                {
                    if (line.StartsWith("PRETTY_NAME=", StringComparison.Ordinal))
                    {
                        operatingSystem = line["PRETTY_NAME=".Length..]
                            .Trim('"');

                        break;
                    }
                }
            }
        }
        catch
        {
            // Keep defaults if /etc/os-release cannot be read.
        }

        if (operatingSystem.Contains("Arch", StringComparison.OrdinalIgnoreCase) ||
            operatingSystem.Contains("BlackArch", StringComparison.OrdinalIgnoreCase) ||
            operatingSystem.Contains("CachyOS", StringComparison.OrdinalIgnoreCase) ||
            operatingSystem.Contains("Manjaro", StringComparison.OrdinalIgnoreCase) ||
            operatingSystem.Contains("EndeavourOS", StringComparison.OrdinalIgnoreCase))
        {
            osFamily = "Arch Linux";
            packageManager = "pacman";
        }
        else if (operatingSystem.Contains("Ubuntu", StringComparison.OrdinalIgnoreCase) ||
                 operatingSystem.Contains("Debian", StringComparison.OrdinalIgnoreCase) ||
                 operatingSystem.Contains("Mint", StringComparison.OrdinalIgnoreCase) ||
                 operatingSystem.Contains("Pop!_OS", StringComparison.OrdinalIgnoreCase))
        {
            osFamily = "Debian";
            packageManager = "apt";
        }
        else if (operatingSystem.Contains("Fedora", StringComparison.OrdinalIgnoreCase))
        {
            osFamily = "Red Hat";
            packageManager = "dnf";
        }

        string architectureName = RuntimeInformation.OSArchitecture switch
        {
            System.Runtime.InteropServices.Architecture.X64 => "x86_64",
            System.Runtime.InteropServices.Architecture.Arm64 => "ARM64",
            System.Runtime.InteropServices.Architecture.Arm => "ARM",
            System.Runtime.InteropServices.Architecture.X86 => "x86",
            _ => RuntimeInformation.OSArchitecture.ToString()
        };

        bool supported = osFamily == "Arch Linux";

        return new SystemInfo
        {
            OperatingSystem = operatingSystem,
            OsFamily = osFamily,
            PackageManager = packageManager,
            ArchitectureName = architectureName,
            Supported = supported
        };
    }
}
