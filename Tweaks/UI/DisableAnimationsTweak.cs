using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.UI;

/// <summary>
/// Disables Windows animations for snappier UI response.
/// </summary>
public class DisableAnimationsTweak : TweakBase
{
    public override string Id => "disable-animations";
    public override string Name => "Disable Animations";
    public override string Description => "Disable window animations for faster UI response";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string VisualFXPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects";
    private const string DesktopPath = @"Control Panel\Desktop";
    private const string WindowMetricsPath = @"Control Panel\Desktop\WindowMetrics";
    private const string DWMPath = @"SOFTWARE\Microsoft\Windows\DWM";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.CurrentUser, VisualFXPath, "VisualFXSetting", 0);
        return Convert.ToInt32(value) == 2; // 2 = Best Performance
    }

    public override TweakResult Apply()
    {
        UILogger.LogAction("Disabling Windows Animations...");
        bool allSuccess = true;

        // Set Visual Effects to Best Performance (2)
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, VisualFXPath, "VisualFXSetting", 2))
            UILogger.Log("Setting VisualFXSetting -> 2 (Best Performance)", UIStatus.Success);
        else
        {
            UILogger.Log("Setting VisualFXSetting", UIStatus.Failed);
            allSuccess = false;
        }

        // Disable specific animations via UserPreferencesMask
        // This byte array disables most animations
        byte[] noAnimMask = { 0x90, 0x12, 0x03, 0x80, 0x10, 0x00, 0x00, 0x00 };
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, DesktopPath, "UserPreferencesMask", noAnimMask, RegistryValueKind.Binary))
            UILogger.Log("Setting UserPreferencesMask (animations disabled)", UIStatus.Success);
        else
            UILogger.Log("Setting UserPreferencesMask", UIStatus.Failed);

        // Disable window animation
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, WindowMetricsPath, "MinAnimate", "0", RegistryValueKind.String))
            UILogger.Log("Setting MinAnimate -> 0", UIStatus.Success);

        // Disable DWM animations
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, DWMPath, "EnableAeroPeek", 0))
            UILogger.Log("Setting EnableAeroPeek -> 0", UIStatus.Success);

        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, DWMPath, "AlwaysHibernateThumbnails", 0))
            UILogger.Log("Setting AlwaysHibernateThumbnails -> 0", UIStatus.Success);

        // Disable drag full windows
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, DesktopPath, "DragFullWindows", "0", RegistryValueKind.String))
            UILogger.Log("Setting DragFullWindows -> 0", UIStatus.Success);

        UILogger.Log("Disabling Animations", allSuccess ? UIStatus.Done : UIStatus.Failed);
        return allSuccess 
            ? Success("Animations disabled - UI will be snappier") 
            : Failure("Some animation settings could not be applied");
    }

    public override TweakResult Revert()
    {
        UILogger.LogAction("Re-enabling Windows Animations...");

        // Set to Let Windows Choose (0)
        RegistryHelper.SetValue(RegistryHive.CurrentUser, VisualFXPath, "VisualFXSetting", 0);
        UILogger.Log("Setting VisualFXSetting -> 0 (Let Windows Choose)", UIStatus.Done);

        // Default UserPreferencesMask with animations
        byte[] defaultMask = { 0x9E, 0x3E, 0x07, 0x80, 0x12, 0x00, 0x00, 0x00 };
        RegistryHelper.SetValue(RegistryHive.CurrentUser, DesktopPath, "UserPreferencesMask", defaultMask, RegistryValueKind.Binary);
        UILogger.Log("Restored UserPreferencesMask", UIStatus.Done);

        RegistryHelper.SetValue(RegistryHive.CurrentUser, WindowMetricsPath, "MinAnimate", "1", RegistryValueKind.String);
        RegistryHelper.SetValue(RegistryHive.CurrentUser, DWMPath, "EnableAeroPeek", 1);
        RegistryHelper.SetValue(RegistryHive.CurrentUser, DesktopPath, "DragFullWindows", "1", RegistryValueKind.String);
        UILogger.Log("Restored animation settings", UIStatus.Done);

        return Success("Animations re-enabled");
    }
}
