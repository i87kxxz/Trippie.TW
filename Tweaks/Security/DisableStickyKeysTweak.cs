using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Security;

/// <summary>
/// Disables Sticky Keys hotkey to prevent gaming interruptions.
/// </summary>
public class DisableStickyKeysTweak : TweakBase
{
    public override string Id => "disable-sticky-keys";
    public override string Name => "Disable Sticky Keys";
    public override string Description => "Disable Sticky Keys hotkey to prevent gaming interruptions";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string StickyKeysPath = @"Control Panel\Accessibility\StickyKeys";
    private const string ToggleKeysPath = @"Control Panel\Accessibility\ToggleKeys";
    private const string FilterKeysPath = @"Control Panel\Accessibility\Keyboard Response";

    public override bool IsApplied()
    {
        var flags = RegistryHelper.GetValue(RegistryHive.CurrentUser, StickyKeysPath, "Flags", "511");
        return flags?.ToString() == "506";
    }

    public override TweakResult Apply()
    {
        SecurityTweakLogger.LogAction("Disabling Sticky Keys hotkey...");

        bool success = true;

        // Disable Sticky Keys (506 = disabled hotkey, 511 = enabled)
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, StickyKeysPath, "Flags", "506", RegistryValueKind.String))
            SecurityTweakLogger.Log("Disabling Sticky Keys hotkey", SecStatus.Done);
        else
        {
            SecurityTweakLogger.Log("Disabling Sticky Keys", SecStatus.Failed);
            success = false;
        }

        // Disable Toggle Keys hotkey
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, ToggleKeysPath, "Flags", "58", RegistryValueKind.String))
            SecurityTweakLogger.Log("Disabling Toggle Keys hotkey", SecStatus.Done);

        // Disable Filter Keys hotkey
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, FilterKeysPath, "Flags", "122", RegistryValueKind.String))
            SecurityTweakLogger.Log("Disabling Filter Keys hotkey", SecStatus.Done);

        return success 
            ? Success("Sticky Keys disabled - no more gaming interruptions") 
            : Failure("Could not disable Sticky Keys");
    }

    public override TweakResult Revert()
    {
        SecurityTweakLogger.LogAction("Re-enabling Sticky Keys hotkey...");

        RegistryHelper.SetValue(RegistryHive.CurrentUser, StickyKeysPath, "Flags", "511", RegistryValueKind.String);
        RegistryHelper.SetValue(RegistryHive.CurrentUser, ToggleKeysPath, "Flags", "63", RegistryValueKind.String);
        RegistryHelper.SetValue(RegistryHive.CurrentUser, FilterKeysPath, "Flags", "126", RegistryValueKind.String);

        SecurityTweakLogger.Log("Re-enabling Sticky Keys hotkey", SecStatus.Reverted);
        return Success("Sticky Keys hotkey re-enabled");
    }
}
