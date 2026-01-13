using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.UI;

/// <summary>
/// Sets menu show delay to 0 for instant menu appearance.
/// </summary>
public class MenuShowDelayTweak : TweakBase
{
    public override string Id => "menu-show-delay";
    public override string Name => "Menu Show Delay (0ms)";
    public override string Description => "Make menus appear instantly without delay";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string DesktopPath = @"Control Panel\Desktop";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.CurrentUser, DesktopPath, "MenuShowDelay", "400");
        return value?.ToString() == "0";
    }

    public override TweakResult Apply()
    {
        UILogger.LogAction("Setting Menu Show Delay to 0ms...");

        bool success = RegistryHelper.SetValue(
            RegistryHive.CurrentUser, 
            DesktopPath, 
            "MenuShowDelay", 
            "0", 
            RegistryValueKind.String);

        // Verify
        var verify = RegistryHelper.GetValue(RegistryHive.CurrentUser, DesktopPath, "MenuShowDelay", null);
        bool verified = verify?.ToString() == "0";

        UILogger.Log("Setting MenuShowDelay -> 0", verified ? UIStatus.Success : UIStatus.Failed);

        return verified 
            ? Success("Menu delay set to 0ms - menus will appear instantly") 
            : Failure("Could not set menu delay");
    }

    public override TweakResult Revert()
    {
        UILogger.LogAction("Reverting Menu Show Delay to default (400ms)...");

        RegistryHelper.SetValue(
            RegistryHive.CurrentUser, 
            DesktopPath, 
            "MenuShowDelay", 
            "400", 
            RegistryValueKind.String);

        UILogger.Log("Setting MenuShowDelay -> 400", UIStatus.Done);
        return Success("Menu delay reverted to default (400ms)");
    }
}
