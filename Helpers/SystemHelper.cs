using System.Diagnostics;
using Microsoft.Win32;

namespace Trippie.TW.Helpers;

/// <summary>
/// System utilities for OS detection and process management.
/// </summary>
public static class SystemHelper
{
    private static readonly Lazy<WindowsVersion> _version = new(DetectWindowsVersion);

    public static WindowsVersion WindowsVersion => _version.Value;
    public static bool IsWindows11 => WindowsVersion == WindowsVersion.Windows11;
    public static bool IsWindows10 => WindowsVersion == WindowsVersion.Windows10;

    private static WindowsVersion DetectWindowsVersion()
    {
        try
        {
            // Check build number - Windows 11 starts at build 22000
            var buildNumber = Environment.OSVersion.Version.Build;
            
            if (buildNumber >= 22000)
                return WindowsVersion.Windows11;
            else if (buildNumber >= 10240)
                return WindowsVersion.Windows10;
            else
                return WindowsVersion.Older;
        }
        catch
        {
            return WindowsVersion.Unknown;
        }
    }

    /// <summary>
    /// Restarts Windows Explorer safely.
    /// </summary>
    public static bool RestartExplorer()
    {
        try
        {
            UILogger.Log("Restarting Explorer.exe to apply changes", UIStatus.InProgress);

            // Kill explorer
            var killPsi = new ProcessStartInfo
            {
                FileName = "taskkill.exe",
                Arguments = "/f /im explorer.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };
            
            using (var killProcess = Process.Start(killPsi))
            {
                killProcess?.WaitForExit(5000);
            }

            // Wait a moment
            Thread.Sleep(500);

            // Start explorer
            var startPsi = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                UseShellExecute = true
            };
            
            Process.Start(startPsi);

            // Wait for explorer to start
            Thread.Sleep(1000);

            // Verify explorer is running
            var explorers = Process.GetProcessesByName("explorer");
            bool success = explorers.Length > 0;

            UILogger.Log("Restarting Explorer.exe", success ? UIStatus.Done : UIStatus.Failed);
            return success;
        }
        catch (Exception ex)
        {
            UILogger.Log("Restarting Explorer.exe", UIStatus.Failed, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Gets the Windows build number.
    /// </summary>
    public static int GetBuildNumber() => Environment.OSVersion.Version.Build;

    /// <summary>
    /// Gets a friendly OS name string.
    /// </summary>
    public static string GetOSName()
    {
        var build = GetBuildNumber();
        return build >= 22000 ? $"Windows 11 (Build {build})" : $"Windows 10 (Build {build})";
    }
}

public enum WindowsVersion
{
    Unknown,
    Older,
    Windows10,
    Windows11
}
