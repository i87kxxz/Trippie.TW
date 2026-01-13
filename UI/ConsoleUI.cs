namespace Trippie.TW.UI;

/// <summary>
/// Handles all console rendering and styling.
/// </summary>
public static class ConsoleUI
{
    public const int Width = 80;
    
    public static void Initialize()
    {
        Console.Title = "Trippie.TW - Windows System Tweaker";
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        try { Console.SetWindowSize(Math.Min(Width + 5, Console.LargestWindowWidth), 35); } catch { }
    }

    public static void Clear() => Console.Clear();

    public static void PrintHeader()
    {
        var accent = ConsoleColor.Magenta;
        PrintLine(accent);
        PrintCentered("╔╦╗╦═╗╦╔═╗╔═╗╦╔═╗  ╔╦╗╦ ╦", accent);
        PrintCentered(" ║ ╠╦╝║╠═╝╠═╝║║╣    ║ ║║║", accent);
        PrintCentered(" ╩ ╩╚═╩╩  ╩  ╩╚═╝o  ╩ ╚╩╝", accent);
        PrintCentered("Made By Trippie", ConsoleColor.White);
        PrintCentered("v1.0.0", ConsoleColor.DarkGray);
        PrintLine(accent);
    }

    public static void PrintLine(ConsoleColor color = ConsoleColor.DarkGray)
    {
        Write(new string('─', Width), color);
        Console.WriteLine();
    }

    public static void PrintCentered(string text, ConsoleColor color = ConsoleColor.White)
    {
        int padding = (Width - text.Length) / 2;
        Console.Write(new string(' ', Math.Max(0, padding)));
        WriteLine(text, color);
    }

    public static void Write(string text, ConsoleColor color = ConsoleColor.White)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    public static void WriteLine(string text = "", ConsoleColor color = ConsoleColor.White)
    {
        Write(text, color);
        Console.WriteLine();
    }

    public static void PrintMenuItem(int index, string text, ConsoleColor accent = ConsoleColor.Cyan, bool selected = false)
    {
        Write("  [", ConsoleColor.DarkGray);
        Write($"{index}", accent);
        Write("] ", ConsoleColor.DarkGray);
        WriteLine(text, selected ? ConsoleColor.White : ConsoleColor.Gray);
    }

    public static void PrintStatus(string label, bool enabled, string? suffix = null)
    {
        Write($"  {label}: ", ConsoleColor.Gray);
        Write(enabled ? "ON " : "OFF", enabled ? ConsoleColor.Green : ConsoleColor.Red);
        if (suffix != null)
            Write($" {suffix}", ConsoleColor.DarkGray);
        Console.WriteLine();
    }

    public static void PrintSuccess(string message) => WriteLine($"  ✓ {message}", ConsoleColor.Green);
    public static void PrintError(string message) => WriteLine($"  ✗ {message}", ConsoleColor.Red);
    public static void PrintWarning(string message) => WriteLine($"  ⚠ {message}", ConsoleColor.Yellow);
    public static void PrintInfo(string message) => WriteLine($"  ℹ {message}", ConsoleColor.Cyan);

    public static void PrintRiskBadge(Core.Interfaces.TweakRiskLevel level)
    {
        var (text, color) = level switch
        {
            Core.Interfaces.TweakRiskLevel.Safe => ("SAFE", ConsoleColor.Green),
            Core.Interfaces.TweakRiskLevel.Moderate => ("MODERATE", ConsoleColor.Yellow),
            Core.Interfaces.TweakRiskLevel.Advanced => ("ADVANCED", ConsoleColor.DarkYellow),
            Core.Interfaces.TweakRiskLevel.Experimental => ("EXPERIMENTAL", ConsoleColor.Red),
            _ => ("UNKNOWN", ConsoleColor.Gray)
        };
        Write("[", ConsoleColor.DarkGray);
        Write(text, color);
        Write("]", ConsoleColor.DarkGray);
    }

    public static string? Prompt(string message, ConsoleColor color = ConsoleColor.Yellow)
    {
        Console.WriteLine();
        Write($"  {message}: ", color);
        return Console.ReadLine()?.Trim();
    }

    public static bool Confirm(string message)
    {
        Write($"  {message} (y/n): ", ConsoleColor.Yellow);
        var key = Console.ReadKey(true);
        Console.WriteLine(key.KeyChar);
        return key.Key == ConsoleKey.Y;
    }

    public static void WaitForKey(string message = "Press any key to continue...")
    {
        Console.WriteLine();
        Write($"  {message}", ConsoleColor.DarkGray);
        Console.ReadKey(true);
    }
}
