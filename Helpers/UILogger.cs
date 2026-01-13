namespace Trippie.TW.Helpers;

/// <summary>
/// UI-specific logging for visual tweaks.
/// </summary>
public static class UILogger
{
    public static void Log(string message, UIStatus status, string? details = null)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var statusText = status switch
        {
            UIStatus.Success => "SUCCESS",
            UIStatus.Failed => "FAILED",
            UIStatus.Done => "DONE",
            UIStatus.InProgress => "IN_PROGRESS",
            UIStatus.Skipped => "SKIPPED",
            UIStatus.RestartRequired => "RESTART_REQUIRED",
            UIStatus.Win11Only => "WIN11_ONLY",
            _ => "INFO"
        };

        var color = status switch
        {
            UIStatus.Success or UIStatus.Done => ConsoleColor.Green,
            UIStatus.Failed => ConsoleColor.Red,
            UIStatus.Skipped or UIStatus.Win11Only => ConsoleColor.Yellow,
            UIStatus.RestartRequired => ConsoleColor.Cyan,
            UIStatus.InProgress => ConsoleColor.Gray,
            _ => ConsoleColor.White
        };

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write($"  [UI_LOG][{timestamp}] ");
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
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write($"  [UI_LOG][{timestamp}] ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}

public enum UIStatus
{
    Success,
    Failed,
    Done,
    InProgress,
    Skipped,
    RestartRequired,
    Win11Only
}
