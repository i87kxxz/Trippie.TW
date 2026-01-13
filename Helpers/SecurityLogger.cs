namespace Trippie.TW.Helpers;

/// <summary>
/// Security and restore operation logging.
/// </summary>
public static class SecurityLogger
{
    public static void Log(string message, SecurityStatus status, string? details = null)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var prefix = status == SecurityStatus.Restoring ? "RESTORE" : "SECURITY";
        var statusText = status switch
        {
            SecurityStatus.Success => "SUCCESS",
            SecurityStatus.Failed => "FAILED",
            SecurityStatus.Done => "DONE",
            SecurityStatus.InProgress => "IN_PROGRESS",
            SecurityStatus.Reverted => "REVERTED",
            SecurityStatus.Skipped => "SKIPPED",
            SecurityStatus.Warning => "WARNING",
            SecurityStatus.Restoring => "REVERTING",
            _ => "INFO"
        };

        var color = status switch
        {
            SecurityStatus.Success or SecurityStatus.Done or SecurityStatus.Reverted => ConsoleColor.Green,
            SecurityStatus.Failed => ConsoleColor.Red,
            SecurityStatus.Warning or SecurityStatus.Skipped => ConsoleColor.Yellow,
            SecurityStatus.InProgress or SecurityStatus.Restoring => ConsoleColor.Cyan,
            _ => ConsoleColor.Gray
        };

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write($"  [{prefix}][{timestamp}] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(message);
        Console.Write(" ");
        Console.ForegroundColor = color;
        Console.Write($"[{statusText}]");
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
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write($"  [SECURITY][{timestamp}] ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}

public enum SecurityStatus
{
    Success,
    Failed,
    Done,
    InProgress,
    Reverted,
    Skipped,
    Warning,
    Restoring
}
