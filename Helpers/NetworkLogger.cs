namespace Trippie.TW.Helpers;

/// <summary>
/// Network-specific logging with connectivity status.
/// </summary>
public static class NetworkLogger
{
    public static void Log(string message, NetStatus status, string? details = null)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var statusText = status switch
        {
            NetStatus.Success => "SUCCESS",
            NetStatus.Failed => "FAILED",
            NetStatus.Done => "DONE",
            NetStatus.InProgress => "IN_PROGRESS",
            NetStatus.GuidFound => "GUID_FOUND",
            NetStatus.GuidNotFound => "GUID_NOT_FOUND",
            NetStatus.ConnectivityOK => "OK",
            NetStatus.ConnectivityFailed => "CONNECTIVITY_LOST",
            NetStatus.Reverted => "REVERTED",
            NetStatus.Warning => "WARNING",
            NetStatus.Skipped => "SKIPPED",
            _ => "INFO"
        };

        var color = status switch
        {
            NetStatus.Success or NetStatus.Done or NetStatus.GuidFound or NetStatus.ConnectivityOK => ConsoleColor.Green,
            NetStatus.Failed or NetStatus.GuidNotFound or NetStatus.ConnectivityFailed => ConsoleColor.Red,
            NetStatus.Warning => ConsoleColor.Yellow,
            NetStatus.Reverted => ConsoleColor.Cyan,
            NetStatus.InProgress or NetStatus.Skipped => ConsoleColor.Gray,
            _ => ConsoleColor.White
        };

        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.Write($"  [NET_LOG][{timestamp}] ");
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
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.Write($"  [NET_LOG][{timestamp}] ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}

public enum NetStatus
{
    Success,
    Failed,
    Done,
    InProgress,
    GuidFound,
    GuidNotFound,
    ConnectivityOK,
    ConnectivityFailed,
    Reverted,
    Warning,
    Skipped
}
