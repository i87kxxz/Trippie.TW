using System.Diagnostics;

namespace Trippie.TW.Helpers;

/// <summary>
/// Helper class for executing PowerShell commands.
/// </summary>
public static class PowerShellHelper
{
    public static (bool Success, string Output) Execute(string command)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi);
            if (process == null)
                return (false, "Failed to start PowerShell process");

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
                return (false, error);

            return (true, output);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public static bool ExecuteScript(string scriptPath)
    {
        var result = Execute($"& '{scriptPath}'");
        return result.Success;
    }
}
