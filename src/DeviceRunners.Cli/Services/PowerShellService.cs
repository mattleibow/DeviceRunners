using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DeviceRunners.Cli.Services;

public class PowerShellService
{
    public string ExecuteCommand(string command, bool requiresElevation = false)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("PowerShell commands are only supported on Windows.");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-Command \"{command}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (requiresElevation)
        {
            startInfo.Verb = "runas";
        }

        var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start PowerShell process");
        }

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
        {
            throw new InvalidOperationException($"PowerShell command failed: {error}");
        }

        return output;
    }

    public bool TryExecuteCommand(string command, out string output, bool requiresElevation = false)
    {
        try
        {
            output = ExecuteCommand(command, requiresElevation);
            return true;
        }
        catch
        {
            output = string.Empty;
            return false;
        }
    }

    public int ExecuteCommandWithExitCode(string command, out string output, out string error, bool requiresElevation = false)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            output = string.Empty;
            error = "PowerShell commands are only supported on Windows.";
            return -1;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-Command \"{command}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (requiresElevation)
        {
            startInfo.Verb = "runas";
        }

        var process = Process.Start(startInfo);
        if (process == null)
        {
            output = string.Empty;
            error = "Failed to start PowerShell process";
            return -1;
        }

        output = process.StandardOutput.ReadToEnd();
        error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return process.ExitCode;
    }
}