using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Security;

/// <summary>
/// Toggles Spectre/Meltdown mitigations for performance on older CPUs.
/// </summary>
public class SpectreMeltdownTweak : TweakBase
{
    public override string Id => "spectre-meltdown";
    public override string Name => "Disable Spectre/Meltdown Patches";
    public override string Description => "Disable CPU vulnerability patches for performance (older CPUs)";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Experimental;

    private const string MemMgmtPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management";

    public override bool IsApplied()
    {
        var override1 = RegistryHelper.GetValue(RegistryHive.LocalMachine, MemMgmtPath, "FeatureSettingsOverride", -1);
        var override2 = RegistryHelper.GetValue(RegistryHive.LocalMachine, MemMgmtPath, "FeatureSettingsOverrideMask", -1);
        return Convert.ToInt32(override1) == 3 && Convert.ToInt32(override2) == 3;
    }

    public override TweakResult Apply()
    {
        SecurityTweakLogger.LogAction("Disabling Spectre/Meltdown mitigations...");

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  ⚠ WARNING: This tweak disables important CPU security patches!");
        Console.WriteLine("  ⚠ Only use this on isolated gaming PCs or older CPUs where");
        Console.WriteLine("  ⚠ performance is more important than security.");
        Console.ResetColor();
        Console.WriteLine();
        Console.Write("  Type 'DISABLE' to confirm: ");
        var confirm = Console.ReadLine()?.Trim();

        if (confirm != "DISABLE")
        {
            SecurityTweakLogger.Log("Spectre/Meltdown Patches", SecStatus.Skipped, "User cancelled");
            return Failure("Operation cancelled by user");
        }

        Console.WriteLine();

        bool success = true;

        // FeatureSettingsOverride = 3 disables mitigations
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, MemMgmtPath, "FeatureSettingsOverride", 3))
            SecurityTweakLogger.Log("Setting FeatureSettingsOverride -> 3", SecStatus.Warning, "Mitigations disabled");
        else
        {
            SecurityTweakLogger.Log("Setting FeatureSettingsOverride", SecStatus.Failed);
            success = false;
        }

        // FeatureSettingsOverrideMask = 3
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, MemMgmtPath, "FeatureSettingsOverrideMask", 3))
            SecurityTweakLogger.Log("Setting FeatureSettingsOverrideMask -> 3", SecStatus.Warning);
        else
        {
            SecurityTweakLogger.Log("Setting FeatureSettingsOverrideMask", SecStatus.Failed);
            success = false;
        }

        if (success)
        {
            SecurityTweakLogger.Log("Spectre/Meltdown mitigations", SecStatus.RequiresReboot);
            return Success("Spectre/Meltdown patches disabled (REBOOT REQUIRED)");
        }
        else
        {
            return Failure("Could not disable mitigations");
        }
    }

    public override TweakResult Revert()
    {
        SecurityTweakLogger.LogAction("Re-enabling Spectre/Meltdown mitigations...");

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, MemMgmtPath, "FeatureSettingsOverride");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, MemMgmtPath, "FeatureSettingsOverrideMask");

        SecurityTweakLogger.Log("Re-enabling Spectre/Meltdown mitigations", SecStatus.Protected);
        SecurityTweakLogger.Log("Spectre/Meltdown mitigations", SecStatus.RequiresReboot);
        return Success("Spectre/Meltdown patches re-enabled (REBOOT REQUIRED)");
    }
}
