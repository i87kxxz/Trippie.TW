using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Security;

/// <summary>
/// Disables AutoRun to prevent malware spreading via USB drives.
/// </summary>
public class DisableAutoRunTweak : TweakBase
{
    public override string Id => "disable-autorun";
    public override string Name => "Disable AutoRun";
    public override string Description => "Prevent malware from spreading via USB drives and CDs";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string ExplorerPoliciesPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, ExplorerPoliciesPath, "NoDriveTypeAutoRun", 0);
        return Convert.ToInt32(value) == 0xFF;
    }

    public override TweakResult Apply()
    {
        SecurityTweakLogger.LogAction("Disabling AutoRun on all drives...");

        bool success = true;

        // Disable for all drive types (0xFF = all drives)
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, ExplorerPoliciesPath, "NoDriveTypeAutoRun", 0xFF))
            SecurityTweakLogger.Log("Setting NoDriveTypeAutoRun -> 0xFF (HKLM)", SecStatus.Protected);
        else
        {
            SecurityTweakLogger.Log("Setting NoDriveTypeAutoRun (HKLM)", SecStatus.Failed);
            success = false;
        }

        // Also set for current user
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, ExplorerPoliciesPath, "NoDriveTypeAutoRun", 0xFF))
            SecurityTweakLogger.Log("Setting NoDriveTypeAutoRun -> 0xFF (HKCU)", SecStatus.Protected);

        // Disable AutoPlay
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, ExplorerPoliciesPath, "NoAutorun", 1))
            SecurityTweakLogger.Log("Setting NoAutorun -> 1", SecStatus.Protected);

        return success 
            ? Success("AutoRun disabled - USB drives are now safer") 
            : Failure("Could not fully disable AutoRun");
    }

    public override TweakResult Revert()
    {
        SecurityTweakLogger.LogAction("Re-enabling AutoRun...");

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, ExplorerPoliciesPath, "NoDriveTypeAutoRun");
        RegistryHelper.DeleteValue(RegistryHive.CurrentUser, ExplorerPoliciesPath, "NoDriveTypeAutoRun");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, ExplorerPoliciesPath, "NoAutorun");

        SecurityTweakLogger.Log("Re-enabling AutoRun", SecStatus.Reverted);
        return Success("AutoRun re-enabled");
    }
}
