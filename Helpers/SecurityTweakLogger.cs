namespace Trippie.TW.Helpers;

/// <summary>
/// Security-specific logging for hardening tweaks.
/// </summary>
public static class SecurityTweakLogger
{
    public static void Log(string message, SecStatus status, string? details = null)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var statusText = status switch
        {
            SecStatus.Success => "SUCCESS",
            SecStatus.Protected => "PROTECTED",
            SecStatus.Done => "DONE",
            SecStatus.Failed => "FAILED",
            SecStatus.Warning => "WARNING",
            SecStatus.Skipped => "SKIPPED",
            SecStatus.InProgress => "IN_PROGRESS",
            SecStatus.Reverted => "REVERTED",
            SecStatus.RequiresReboot => "REBOOT_REQUIRED",
            SecStatus.AdminRequired => "ADMIN_REQUIRED",
            _ => "INFO"
        };

        var color = status switch
        {
            SecStatus.Success or SecStatus.Protected or SecStatus.Done => ConsoleColor.Green,
            SecStatus.Failed or SecStatus.AdminRequired => ConsoleColor.Red,
            SecStatus.Warning or SecStatus.Skipped => ConsoleColor.Yellow,
            SecStatus.RequiresReboot => ConsoleColor.Cyan,
            SecStatus.InProgress => ConsoleColor.Gray,
            SecStatus.Reverted => ConsoleColor.DarkCyan,
            _ => ConsoleColor.White
        };

        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write($"  [SEC_LOG][{timestamp}] ");
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
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write($"  [SEC_LOG][{timestamp}] ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}

public enum SecStatus
{
    Success,
    Protected,
    Done,
    Failed,
    Warning,
    Skipped,
    InProgress,
    Reverted,
    RequiresReboot,
    AdminRequired
}
