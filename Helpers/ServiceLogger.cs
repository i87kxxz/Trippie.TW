namespace Trippie.TW.Helpers;

/// <summary>
/// Service-specific logging for service management operations.
/// </summary>
public static class ServiceLogger
{
    public static void Log(string serviceName, SvcStatus status, string? details = null)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var statusText = status switch
        {
            SvcStatus.DisabledAndStopped => "DISABLED & STOPPED",
            SvcStatus.AlreadyDisabled => "ALREADY_DISABLED",
            SvcStatus.Enabled => "ENABLED",
            SvcStatus.Started => "STARTED",
            SvcStatus.Stopped => "STOPPED",
            SvcStatus.NotFound => "NOT_FOUND",
            SvcStatus.Failed => "FAILED",
            SvcStatus.Skipping => "SKIPPING",
            SvcStatus.InProgress => "IN_PROGRESS",
            SvcStatus.Reverted => "REVERTED",
            _ => "INFO"
        };

        var color = status switch
        {
            SvcStatus.DisabledAndStopped or SvcStatus.Enabled or SvcStatus.Started or SvcStatus.Reverted => ConsoleColor.Green,
            SvcStatus.AlreadyDisabled => ConsoleColor.DarkYellow,
            SvcStatus.NotFound or SvcStatus.Skipping => ConsoleColor.Yellow,
            SvcStatus.Failed => ConsoleColor.Red,
            SvcStatus.InProgress or SvcStatus.Stopped => ConsoleColor.Cyan,
            _ => ConsoleColor.White
        };

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write($"  [SERVICE_LOG][{timestamp}] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"Modifying Service: {serviceName} | Status: ");
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

    public static void LogError(string serviceName, string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write($"  [SERVICE_LOG][{timestamp}] ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("ERROR: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"Service '{serviceName}' {message}. ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[SKIPPING]");
        Console.ResetColor();
    }

    public static void LogAction(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write($"  [SERVICE_LOG][{timestamp}] ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}

public enum SvcStatus
{
    DisabledAndStopped,
    AlreadyDisabled,
    Enabled,
    Started,
    Stopped,
    NotFound,
    Failed,
    Skipping,
    InProgress,
    Reverted
}
