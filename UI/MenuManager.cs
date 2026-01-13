using Trippie.TW.Core.Interfaces;
using Trippie.TW.Core.Registry;

namespace Trippie.TW.UI;

/// <summary>
/// Manages menu navigation and user interaction.
/// </summary>
public class MenuManager
{
    private readonly CategoryRegistry _registry;

    public MenuManager(CategoryRegistry registry)
    {
        _registry = registry;
    }

    public void Run()
    {
        while (true)
        {
            ShowMainMenu();
            var input = ConsoleUI.Prompt("Select category (0 to exit)");
            
            if (input == "0" || string.IsNullOrEmpty(input))
            {
                if (ConsoleUI.Confirm("Are you sure you want to exit?"))
                    break;
                continue;
            }

            if (int.TryParse(input, out int index) && index > 0 && index <= _registry.Categories.Count)
            {
                ShowCategoryMenu(_registry.Categories[index - 1]);
            }
        }
    }

    private void ShowMainMenu()
    {
        ConsoleUI.Clear();
        ConsoleUI.PrintHeader();
        ConsoleUI.WriteLine();
        ConsoleUI.WriteLine("  CATEGORIES", ConsoleColor.White);
        ConsoleUI.PrintLine();

        for (int i = 0; i < _registry.Categories.Count; i++)
        {
            var cat = _registry.Categories[i];
            ConsoleUI.PrintMenuItem(i + 1, $"{cat.Name} - {cat.Description}", cat.AccentColor);
        }

        ConsoleUI.PrintLine();
        ConsoleUI.PrintMenuItem(0, "Exit", ConsoleColor.Red);
    }

    private void ShowCategoryMenu(ITweakCategory category)
    {
        while (true)
        {
            ConsoleUI.Clear();
            ConsoleUI.PrintHeader();
            ConsoleUI.WriteLine();
            ConsoleUI.Write("  ", ConsoleColor.White);
            ConsoleUI.Write(category.Name.ToUpper(), category.AccentColor);
            ConsoleUI.WriteLine($" - {category.Description}", ConsoleColor.Gray);
            ConsoleUI.PrintLine(category.AccentColor);

            if (category.Tweaks.Count == 0)
            {
                ConsoleUI.PrintInfo("No tweaks available in this category yet.");
            }
            else
            {
                for (int i = 0; i < category.Tweaks.Count; i++)
                {
                    var tweak = category.Tweaks[i];
                    var status = tweak.IsApplied();
                    
                    ConsoleUI.Write("  [", ConsoleColor.DarkGray);
                    ConsoleUI.Write($"{i + 1}", category.AccentColor);
                    ConsoleUI.Write("] ", ConsoleColor.DarkGray);
                    ConsoleUI.Write(tweak.Name, ConsoleColor.White);
                    ConsoleUI.Write(" - ", ConsoleColor.DarkGray);
                    ConsoleUI.PrintRiskBadge(tweak.RiskLevel);
                    ConsoleUI.Write(" ", ConsoleColor.White);
                    ConsoleUI.Write(status ? "[ON]" : "[OFF]", status ? ConsoleColor.Green : ConsoleColor.DarkGray);
                    Console.WriteLine();
                }
            }

            ConsoleUI.PrintLine(category.AccentColor);
            ConsoleUI.PrintMenuItem(0, "Back to Main Menu", ConsoleColor.Yellow);

            var input = ConsoleUI.Prompt("Select tweak to toggle");
            
            if (input == "0" || string.IsNullOrEmpty(input))
                break;

            if (int.TryParse(input, out int index) && index > 0 && index <= category.Tweaks.Count)
            {
                ExecuteTweak(category.Tweaks[index - 1]);
            }
        }
    }

    private void ExecuteTweak(ITweak tweak)
    {
        ConsoleUI.Clear();
        ConsoleUI.PrintHeader();
        ConsoleUI.WriteLine();
        ConsoleUI.WriteLine($"  {tweak.Name}", ConsoleColor.White);
        ConsoleUI.WriteLine($"  {tweak.Description}", ConsoleColor.Gray);
        ConsoleUI.Write("  Risk Level: ");
        ConsoleUI.PrintRiskBadge(tweak.RiskLevel);
        Console.WriteLine();
        ConsoleUI.PrintLine();

        bool isApplied = tweak.IsApplied();
        string action = isApplied ? "revert" : "apply";

        if (tweak.RiskLevel >= TweakRiskLevel.Advanced)
        {
            ConsoleUI.PrintWarning($"This is an {tweak.RiskLevel} tweak. Proceed with caution.");
        }

        if (!ConsoleUI.Confirm($"Do you want to {action} this tweak?"))
        {
            ConsoleUI.PrintInfo("Operation cancelled.");
            ConsoleUI.WaitForKey();
            return;
        }

        ConsoleUI.WriteLine();
        ConsoleUI.PrintInfo($"{(isApplied ? "Reverting" : "Applying")} tweak...");

        var result = isApplied ? tweak.Revert() : tweak.Apply();

        if (result.Success)
            ConsoleUI.PrintSuccess(result.Message);
        else
            ConsoleUI.PrintError(result.Message);

        ConsoleUI.WaitForKey();
    }
}
