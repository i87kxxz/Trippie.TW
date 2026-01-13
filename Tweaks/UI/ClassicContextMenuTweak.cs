using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.UI;

/// <summary>
/// Restores the classic Windows 10 context menu in Windows 11.
/// </summary>
public class ClassicContextMenuTweak : TweakBase
{
    public override string Id => "classic-context-menu";
    public override string Name => "Classic Context Menu (Win 11)";
    public override string Description => "Restore the full Windows 10 style right-click context menu";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string ContextMenuCLSID = @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32";

    public override bool IsApplied()
    {
        // Check if the key exists with empty default value
        return RegistryHelper.KeyExists(RegistryHive.CurrentUser, ContextMenuCLSID);
    }

    public override TweakResult Apply()
    {
        UILogger.LogAction("Applying Classic Context Menu...");

        // Check Windows version
        if (!SystemHelper.IsWindows11)
        {
            UILogger.Log("Classic Context Menu", UIStatus.Win11Only, $"Current OS: {SystemHelper.GetOSName()}");
            return Failure("This tweak is only applicable to Windows 11");
        }

        // Create the CLSID key with empty InprocServer32
        // This overrides the new context menu handler
        bool success = RegistryHelper.SetValue(
            RegistryHive.CurrentUser, 
            ContextMenuCLSID, 
            "", // Default value
            "", 
            RegistryValueKind.String);

        if (success)
        {
            UILogger.Log("Creating InprocServer32 key", UIStatus.Success);
            UILogger.Log("Classic Context Menu", UIStatus.RestartRequired, "Explorer restart needed");

            // Ask to restart explorer
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("  Restart Explorer now to apply changes? (Y/N): ");
            Console.ResetColor();
            var key = Console.ReadKey(true);
            Console.WriteLine(key.KeyChar);

            if (key.Key == ConsoleKey.Y)
            {
                SystemHelper.RestartExplorer();
            }
            else
            {
                UILogger.Log("Explorer restart", UIStatus.Skipped, "Manual restart required");
            }

            return Success("Classic context menu enabled");
        }
        else
        {
            UILogger.Log("Creating InprocServer32 key", UIStatus.Failed);
            return Failure("Could not enable classic context menu");
        }
    }

    public override TweakResult Revert()
    {
        UILogger.LogAction("Reverting to Windows 11 Context Menu...");

        if (!SystemHelper.IsWindows11)
        {
            UILogger.Log("Classic Context Menu Revert", UIStatus.Win11Only);
            return Failure("This tweak is only applicable to Windows 11");
        }

        // Delete the entire CLSID key
        bool success = RegistryHelper.DeleteKey(
            RegistryHive.CurrentUser, 
            @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}");

        if (success)
        {
            UILogger.Log("Removing InprocServer32 key", UIStatus.Done);
            UILogger.Log("Windows 11 Context Menu", UIStatus.RestartRequired);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("  Restart Explorer now to apply changes? (Y/N): ");
            Console.ResetColor();
            var key = Console.ReadKey(true);
            Console.WriteLine(key.KeyChar);

            if (key.Key == ConsoleKey.Y)
            {
                SystemHelper.RestartExplorer();
            }

            return Success("Windows 11 context menu restored");
        }
        else
        {
            UILogger.Log("Removing InprocServer32 key", UIStatus.Failed);
            return Failure("Could not restore Windows 11 context menu");
        }
    }
}
