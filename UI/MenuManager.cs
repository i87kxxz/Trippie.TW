using Trippie.TW.Core.Interfaces;
using Trippie.TW.Core.Registry;
using Trippie.TW.Core.Restore;

namespace Trippie.TW.UI;

/// <summary>
/// Manages menu navigation and user interaction.
/// </summary>
public class MenuManager
{
    private readonly CategoryRegistry _registry;
    private readonly EmergencyRestoreManager _restoreManager;

    public MenuManager(CategoryRegistry registry)
    {
        _registry = registry;
        _restoreManager = new EmergencyRestoreManager(registry);
    }

    public void Run()
    {
        while (true)
        {
            ShowMainMenu();
            var input = ConsoleUI.Prompt("Select option (0 to exit)");
            
            if (input == "0" || string.IsNullOrEmpty(input))
            {
                if (ConsoleUI.Confirm("Are you sure you want to exit?"))
                    break;
                continue;
            }

            // Handle Emergency Restore option (7)
            if (input == "7")
            {
                ShowEmergencyRestoreMenu();
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
        ConsoleUI.WriteLine("  SYSTEM", ConsoleColor.White);
        ConsoleUI.PrintLine();
        ConsoleUI.PrintMenuItem(7, "Emergency Restore & Undo - Revert all changes to defaults", ConsoleColor.Red);
        ConsoleUI.PrintLine();
        ConsoleUI.PrintMenuItem(0, "Exit", ConsoleColor.DarkGray);
    }

    private void ShowEmergencyRestoreMenu()
    {
        while (true)
        {
            ConsoleUI.Clear();
            ConsoleUI.PrintHeader();
            ConsoleUI.WriteLine();
            ConsoleUI.Write("  ", ConsoleColor.White);
            ConsoleUI.Write("EMERGENCY RESTORE & UNDO", ConsoleColor.Red);
            ConsoleUI.WriteLine(" - Safety Net", ConsoleColor.Gray);
            ConsoleUI.PrintLine(ConsoleColor.Red);

            // Show current status
            var summary = _restoreManager.GetAppliedTweaksSummary();
            ConsoleUI.WriteLine();
            ConsoleUI.Write("  Currently Applied Tweaks: ", ConsoleColor.Gray);
            ConsoleUI.WriteLine($"{summary.AppliedTweaks} / {summary.TotalTweaks}", 
                summary.AppliedTweaks > 0 ? ConsoleColor.Yellow : ConsoleColor.Green);
            ConsoleUI.WriteLine();

            if (summary.AppliedTweaks > 0)
            {
                ConsoleUI.WriteLine("  Applied tweaks:", ConsoleColor.DarkGray);
                foreach (var (category, tweakName) in summary.AppliedTweaksList.Take(10))
                {
                    ConsoleUI.Write("    • ", ConsoleColor.DarkGray);
                    ConsoleUI.Write($"[{category}] ", ConsoleColor.Cyan);
                    ConsoleUI.WriteLine(tweakName, ConsoleColor.White);
                }
                if (summary.AppliedTweaksList.Count > 10)
                {
                    ConsoleUI.WriteLine($"    ... and {summary.AppliedTweaksList.Count - 10} more", ConsoleColor.DarkGray);
                }
                ConsoleUI.WriteLine();
            }

            ConsoleUI.PrintLine(ConsoleColor.Red);
            ConsoleUI.PrintMenuItem(1, "Create System Restore Point", ConsoleColor.Green);
            ConsoleUI.PrintMenuItem(2, "Create Full Backup (Restore Point + Registry Export)", ConsoleColor.Green);
            ConsoleUI.PrintMenuItem(3, "View Applied Tweaks", ConsoleColor.Cyan);
            ConsoleUI.PrintMenuItem(4, "REVERT ALL TWEAKS TO DEFAULTS", ConsoleColor.Red);
            ConsoleUI.PrintLine(ConsoleColor.Red);
            ConsoleUI.PrintMenuItem(0, "Back to Main Menu", ConsoleColor.Yellow);

            var input = ConsoleUI.Prompt("Select option");

            switch (input)
            {
                case "1":
                    CreateRestorePoint();
                    break;
                case "2":
                    CreateFullBackup();
                    break;
                case "3":
                    ShowAppliedTweaks(summary);
                    break;
                case "4":
                    RevertAllTweaks();
                    break;
                case "0":
                case "":
                case null:
                    return;
            }
        }
    }

    private void CreateRestorePoint()
    {
        ConsoleUI.Clear();
        ConsoleUI.PrintHeader();
        ConsoleUI.WriteLine();
        ConsoleUI.WriteLine("  CREATE SYSTEM RESTORE POINT", ConsoleColor.Green);
        ConsoleUI.PrintLine(ConsoleColor.Green);
        ConsoleUI.WriteLine();

        if (!ConsoleUI.Confirm("Create a System Restore Point now?"))
        {
            ConsoleUI.PrintInfo("Operation cancelled.");
            ConsoleUI.WaitForKey();
            return;
        }

        ConsoleUI.WriteLine();
        bool success = RestorePointManager.CreateRestorePoint("Trippie.TW Manual Backup");
        ConsoleUI.WriteLine();

        if (success)
            ConsoleUI.PrintSuccess("System Restore Point created successfully!");
        else
            ConsoleUI.PrintWarning("Could not create restore point. You may need to enable System Protection.");

        ConsoleUI.WaitForKey();
    }

    private void CreateFullBackup()
    {
        ConsoleUI.Clear();
        ConsoleUI.PrintHeader();
        ConsoleUI.WriteLine();
        ConsoleUI.WriteLine("  CREATE FULL BACKUP", ConsoleColor.Green);
        ConsoleUI.PrintLine(ConsoleColor.Green);
        ConsoleUI.WriteLine();
        ConsoleUI.PrintInfo("This will create:");
        ConsoleUI.WriteLine("    • System Restore Point", ConsoleColor.Gray);
        ConsoleUI.WriteLine("    • Registry key exports (.reg files)", ConsoleColor.Gray);
        ConsoleUI.WriteLine();

        if (!ConsoleUI.Confirm("Create full backup now?"))
        {
            ConsoleUI.PrintInfo("Operation cancelled.");
            ConsoleUI.WaitForKey();
            return;
        }

        ConsoleUI.WriteLine();
        _restoreManager.CreateFullBackup();
        ConsoleUI.WriteLine();
        ConsoleUI.PrintSuccess("Full backup completed!");
        ConsoleUI.PrintInfo($"Backup files saved to: {AppDomain.CurrentDomain.BaseDirectory}Backups\\");
        ConsoleUI.WaitForKey();
    }

    private void ShowAppliedTweaks(TweakSummary summary)
    {
        ConsoleUI.Clear();
        ConsoleUI.PrintHeader();
        ConsoleUI.WriteLine();
        ConsoleUI.WriteLine("  APPLIED TWEAKS", ConsoleColor.Cyan);
        ConsoleUI.PrintLine(ConsoleColor.Cyan);
        ConsoleUI.WriteLine();

        if (summary.AppliedTweaks == 0)
        {
            ConsoleUI.PrintInfo("No tweaks are currently applied.");
        }
        else
        {
            string? currentCategory = null;
            foreach (var (category, tweakName) in summary.AppliedTweaksList)
            {
                if (category != currentCategory)
                {
                    if (currentCategory != null) ConsoleUI.WriteLine();
                    ConsoleUI.WriteLine($"  [{category}]", ConsoleColor.Cyan);
                    currentCategory = category;
                }
                ConsoleUI.Write("    ✓ ", ConsoleColor.Green);
                ConsoleUI.WriteLine(tweakName, ConsoleColor.White);
            }
        }

        ConsoleUI.WriteLine();
        ConsoleUI.PrintLine(ConsoleColor.Cyan);
        ConsoleUI.Write($"  Total: ", ConsoleColor.Gray);
        ConsoleUI.WriteLine($"{summary.AppliedTweaks} applied / {summary.TotalTweaks} available", ConsoleColor.White);
        ConsoleUI.WaitForKey();
    }

    private void RevertAllTweaks()
    {
        ConsoleUI.Clear();
        ConsoleUI.PrintHeader();
        ConsoleUI.WriteLine();
        ConsoleUI.WriteLine("  ⚠ REVERT ALL TWEAKS ⚠", ConsoleColor.Red);
        ConsoleUI.PrintLine(ConsoleColor.Red);
        ConsoleUI.WriteLine();

        // Warning message
        ConsoleUI.PrintWarning("This will revert all changes made by Trippie.TW to Windows defaults.");
        ConsoleUI.WriteLine();
        ConsoleUI.WriteLine("  The following actions will be performed:", ConsoleColor.Gray);
        ConsoleUI.WriteLine("    • Re-enable all disabled services", ConsoleColor.Gray);
        ConsoleUI.WriteLine("    • Restore registry values to defaults", ConsoleColor.Gray);
        ConsoleUI.WriteLine("    • Re-enable hibernation", ConsoleColor.Gray);
        ConsoleUI.WriteLine("    • Reset power plan to Balanced", ConsoleColor.Gray);
        ConsoleUI.WriteLine("    • Restore all backed-up settings", ConsoleColor.Gray);
        ConsoleUI.WriteLine();

        ConsoleUI.Write("  ", ConsoleColor.White);
        ConsoleUI.Write("Do you wish to proceed? (Y/N): ", ConsoleColor.Yellow);
        var key = Console.ReadKey(true);
        Console.WriteLine(key.KeyChar);

        if (key.Key != ConsoleKey.Y)
        {
            ConsoleUI.PrintInfo("Operation cancelled.");
            ConsoleUI.WaitForKey();
            return;
        }

        // Double confirmation for safety
        ConsoleUI.WriteLine();
        ConsoleUI.Write("  ", ConsoleColor.White);
        ConsoleUI.Write("Type 'RESTORE' to confirm: ", ConsoleColor.Red);
        var confirmation = Console.ReadLine()?.Trim();

        if (confirmation != "RESTORE")
        {
            ConsoleUI.PrintInfo("Operation cancelled - confirmation not matched.");
            ConsoleUI.WaitForKey();
            return;
        }

        ConsoleUI.WriteLine();
        ConsoleUI.PrintLine(ConsoleColor.Red);
        ConsoleUI.WriteLine();

        // Perform restoration
        var result = _restoreManager.RevertAllTweaks();

        ConsoleUI.WriteLine();
        ConsoleUI.PrintLine(ConsoleColor.Red);
        ConsoleUI.WriteLine();

        // Show results
        ConsoleUI.WriteLine("  RESTORE COMPLETE", ConsoleColor.White);
        ConsoleUI.WriteLine();
        ConsoleUI.Write("  Tweaks Reverted: ", ConsoleColor.Gray);
        ConsoleUI.WriteLine($"{result.RevertedTweaks}", ConsoleColor.Green);
        ConsoleUI.Write("  Registry Values Restored: ", ConsoleColor.Gray);
        ConsoleUI.WriteLine($"{result.RegistryValuesRestored}", ConsoleColor.Green);

        if (result.FailedTweaks > 0)
        {
            ConsoleUI.Write("  Failed to Revert: ", ConsoleColor.Gray);
            ConsoleUI.WriteLine($"{result.FailedTweaks}", ConsoleColor.Red);
            ConsoleUI.WriteLine();
            ConsoleUI.WriteLine("  Failed tweaks:", ConsoleColor.DarkGray);
            foreach (var name in result.FailedTweakNames)
            {
                ConsoleUI.WriteLine($"    • {name}", ConsoleColor.Red);
            }
        }

        ConsoleUI.WriteLine();
        if (result.FullSuccess)
            ConsoleUI.PrintSuccess("All tweaks have been reverted successfully!");
        else
            ConsoleUI.PrintWarning("Some tweaks could not be reverted. A system restart may help.");

        ConsoleUI.PrintInfo("A system restart is recommended to apply all changes.");
        ConsoleUI.WaitForKey();
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
        ConsoleUI.WriteLine();

        var result = isApplied ? tweak.Revert() : tweak.Apply();

        ConsoleUI.WriteLine();
        if (result.Success)
            ConsoleUI.PrintSuccess(result.Message);
        else
            ConsoleUI.PrintError(result.Message);

        ConsoleUI.WaitForKey();
    }
}
