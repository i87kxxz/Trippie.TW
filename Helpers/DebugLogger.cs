namespace Trippie.TW.Helpers;

/// <summary>
/// Debug logging helper with timestamps and status indicators.
/// </summary>
public static class DebugLogger
{
    public static void Log(string message, LogStatus status = LogStatus.Info)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var statusText = status switch
        {
            LogStatus.Success => "SUCCESS",
            LogStatus.Failed => "FAILED",
            LogStatus.AlreadyApplied => "ALREADY_APPLIED",
            LogStatus.AlreadyDisabled => "ALREADY_DISABLED",
            LogStatus.Done => "DONE",
            LogStatus.Info => "INFO",
            LogStatus.Warning => "WARNING",
            _ => "INFO"
        };

        var color = status switch
        {
            LogStatus.Success => ConsoleColor.Green,
            LogStatus.Failed => ConsoleColor.Red,
            LogStatus.AlreadyApplied or LogStatus.AlreadyDisabled => ConsoleColor.Yellow,
            LogStatus.Done => ConsoleColor.Green,
            LogStatus.Warning => ConsoleColor.Yellow,
            _ => ConsoleColor.Gray
        };

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"  [DEBUG][{timestamp}] ");
        Console.ResetColor();
        Console.Write(message);
        Console.ForegroundColor = color;
        Console.WriteLine($" [{statusText}]");
        Console.ResetColor();
    }

    public static void LogAction(string action)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"  [DEBUG][{timestamp}] ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(action);
        Console.ResetColor();
    }

    public static void LogRegistry(string path, string name, object value, bool success)
    {
        Log($"Registry: {path}\\{name} -> {value}", success ? LogStatus.Success : LogStatus.Failed);
    }

    public static void LogService(string serviceName, string action, bool success)
    {
        Log($"Service '{serviceName}': {action}", success ? LogStatus.Success : LogStatus.Failed);
    }
}

public enum LogStatus
{
    Info,
    Success,
    Failed,
    AlreadyApplied,
    AlreadyDisabled,
    Done,
    Warning
}
