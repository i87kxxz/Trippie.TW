using Microsoft.Win32;

namespace Trippie.TW.Helpers;

/// <summary>
/// Performance-specific logging with verification support.
/// </summary>
public static class PerformanceLogger
{
    public static void Log(string action, string method, PerfStatus status, string? details = null)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var statusText = status switch
        {
            PerfStatus.VerifiedSuccess => "VERIFIED_SUCCESS",
            PerfStatus.VerifiedFailed => "VERIFIED_FAILED",
            PerfStatus.RebootRequired => "REBOOT_REQUIRED",
            PerfStatus.AlreadyApplied => "ALREADY_APPLIED",
            PerfStatus.Reverted => "REVERTED",
            PerfStatus.Failed => "FAILED",
            PerfStatus.InProgress => "IN_PROGRESS",
            _ => "UNKNOWN"
        };

        var color = status switch
        {
            PerfStatus.VerifiedSuccess => ConsoleColor.Green,
            PerfStatus.VerifiedFailed or PerfStatus.Failed => ConsoleColor.Red,
            PerfStatus.RebootRequired => ConsoleColor.Yellow,
            PerfStatus.AlreadyApplied => ConsoleColor.DarkYellow,
            PerfStatus.Reverted => ConsoleColor.Cyan,
            PerfStatus.InProgress => ConsoleColor.Gray,
            _ => ConsoleColor.White
        };

        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.Write($"  [PERF_LOG][{timestamp}] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"Action: {action} | Method: {method} | Status: ");
        Console.ForegroundColor = color;
        Console.Write(statusText);
        if (details != null)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" ({details})");
        }
        Console.WriteLine();
        Console.ResetColor();
    }

    public static void LogAction(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.Write($"  [PERF_LOG][{timestamp}] ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    /// <summary>
    /// Sets a registry value and verifies it was written correctly.
    /// </summary>
    public static bool SetAndVerify(RegistryHive hive, string path, string name, object value, RegistryValueKind kind = RegistryValueKind.DWord)
    {
        bool setResult = RegistryHelper.SetValue(hive, path, name, value, kind);
        if (!setResult) return false;

        // Verify by re-reading
        var readBack = RegistryHelper.GetValue(hive, path, name, null);
        if (readBack == null) return false;

        return kind switch
        {
            RegistryValueKind.DWord => Convert.ToInt32(readBack) == Convert.ToInt32(value),
            RegistryValueKind.QWord => Convert.ToInt64(readBack) == Convert.ToInt64(value),
            RegistryValueKind.String => readBack.ToString() == value.ToString(),
            _ => true
        };
    }
}

public enum PerfStatus
{
    VerifiedSuccess,
    VerifiedFailed,
    RebootRequired,
    AlreadyApplied,
    Reverted,
    Failed,
    InProgress
}
