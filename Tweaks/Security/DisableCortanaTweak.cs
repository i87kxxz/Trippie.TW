using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Security;

/// <summary>
/// Fully disables Cortana via Group Policy registry keys.
/// </summary>
public class DisableCortanaTweak : TweakBase
{
    public override string Id => "disable-cortana";
    public override string Name => "Disable Cortana";
    public override string Description => "Fully disable Cortana assistant and background processes";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string SearchPolicyPath = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search";
    private const string CortanaConsentPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, SearchPolicyPath, "AllowCortana", 1);
        return Convert.ToInt32(value) == 0;
    }

    public override TweakResult Apply()
    {
        SecurityTweakLogger.LogAction("Disabling Cortana...");

        bool success = true;

        // Disable via Group Policy
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, SearchPolicyPath, "AllowCortana", 0))
            SecurityTweakLogger.Log("Setting AllowCortana -> 0", SecStatus.Done);
        else
        {
            SecurityTweakLogger.Log("Setting AllowCortana", SecStatus.Failed);
            success = false;
        }

        // Disable Cortana above lock screen
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, SearchPolicyPath, "AllowCortanaAboveLock", 0))
            SecurityTweakLogger.Log("Setting AllowCortanaAboveLock -> 0", SecStatus.Done);

        // Disable search highlights
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, SearchPolicyPath, "EnableDynamicContentInWSB", 0))
            SecurityTweakLogger.Log("Setting EnableDynamicContentInWSB -> 0", SecStatus.Done);

        // Disable web search in Start menu
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, SearchPolicyPath, "DisableWebSearch", 1))
            SecurityTweakLogger.Log("Setting DisableWebSearch -> 1", SecStatus.Done);

        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, SearchPolicyPath, "ConnectedSearchUseWeb", 0))
            SecurityTweakLogger.Log("Setting ConnectedSearchUseWeb -> 0", SecStatus.Done);

        // Disable for current user
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, CortanaConsentPath, "CortanaConsent", 0))
            SecurityTweakLogger.Log("Setting CortanaConsent -> 0", SecStatus.Done);

        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, CortanaConsentPath, "BingSearchEnabled", 0))
            SecurityTweakLogger.Log("Setting BingSearchEnabled -> 0", SecStatus.Done);

        if (success)
        {
            SecurityTweakLogger.Log("Cortana has been fully decommissioned", SecStatus.Success);
            return Success("Cortana disabled completely");
        }
        else
        {
            return Failure("Could not fully disable Cortana");
        }
    }

    public override TweakResult Revert()
    {
        SecurityTweakLogger.LogAction("Re-enabling Cortana...");

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, SearchPolicyPath, "AllowCortana");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, SearchPolicyPath, "AllowCortanaAboveLock");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, SearchPolicyPath, "EnableDynamicContentInWSB");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, SearchPolicyPath, "DisableWebSearch");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, SearchPolicyPath, "ConnectedSearchUseWeb");
        RegistryHelper.DeleteValue(RegistryHive.CurrentUser, CortanaConsentPath, "CortanaConsent");
        RegistryHelper.DeleteValue(RegistryHive.CurrentUser, CortanaConsentPath, "BingSearchEnabled");

        SecurityTweakLogger.Log("Re-enabling Cortana", SecStatus.Reverted);
        return Success("Cortana re-enabled");
    }
}
