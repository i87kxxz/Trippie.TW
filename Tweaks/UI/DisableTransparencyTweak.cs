using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.UI;

/// <summary>
/// Disables Windows transparency effects for better performance.
/// </summary>
public class DisableTransparencyTweak : TweakBase
{
    public override string Id => "disable-transparency";
    public override string Name => "Disable Transparency";
    public override string Description => "Disable transparency effects for improved performance";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string PersonalizePath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.CurrentUser, PersonalizePath, "EnableTransparency", 1);
        return Convert.ToInt32(value) == 0;
    }

    public override TweakResult Apply()
    {
        UILogger.LogAction("Disabling Transparency Effects...");

        bool success = RegistryHelper.SetValue(
            RegistryHive.CurrentUser, 
            PersonalizePath, 
            "EnableTransparency", 
            0);

        // Verify
        var verify = RegistryHelper.GetValue(RegistryHive.CurrentUser, PersonalizePath, "EnableTransparency", 1);
        bool verified = Convert.ToInt32(verify) == 0;

        UILogger.Log("Setting EnableTransparency -> 0", verified ? UIStatus.Success : UIStatus.Failed);

        return verified 
            ? Success("Transparency effects disabled") 
            : Failure("Could not disable transparency");
    }

    public override TweakResult Revert()
    {
        UILogger.LogAction("Re-enabling Transparency Effects...");

        RegistryHelper.SetValue(
            RegistryHive.CurrentUser, 
            PersonalizePath, 
            "EnableTransparency", 
            1);

        UILogger.Log("Setting EnableTransparency -> 1", UIStatus.Done);
        return Success("Transparency effects re-enabled");
    }
}
