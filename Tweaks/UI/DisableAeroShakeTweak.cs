using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.UI;

/// <summary>
/// Disables Aero Shake (minimize windows by shaking).
/// </summary>
public class DisableAeroShakeTweak : TweakBase
{
    public override string Id => "disable-aero-shake";
    public override string Name => "Disable Aero Shake";
    public override string Description => "Disable the shake-to-minimize feature";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string AdvancedPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.CurrentUser, AdvancedPath, "DisallowShaking", 0);
        return Convert.ToInt32(value) == 1;
    }

    public override TweakResult Apply()
    {
        UILogger.LogAction("Disabling Aero Shake...");

        bool success = RegistryHelper.SetValue(
            RegistryHive.CurrentUser, 
            AdvancedPath, 
            "DisallowShaking", 
            1);

        // Verify
        var verify = RegistryHelper.GetValue(RegistryHive.CurrentUser, AdvancedPath, "DisallowShaking", 0);
        bool verified = Convert.ToInt32(verify) == 1;

        UILogger.Log("Setting DisallowShaking -> 1", verified ? UIStatus.Success : UIStatus.Failed);

        return verified 
            ? Success("Aero Shake disabled") 
            : Failure("Could not disable Aero Shake");
    }

    public override TweakResult Revert()
    {
        UILogger.LogAction("Re-enabling Aero Shake...");

        RegistryHelper.DeleteValue(RegistryHive.CurrentUser, AdvancedPath, "DisallowShaking");
        UILogger.Log("Removed DisallowShaking", UIStatus.Done);

        return Success("Aero Shake re-enabled");
    }
}
