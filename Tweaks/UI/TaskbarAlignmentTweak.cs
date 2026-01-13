using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.UI;

/// <summary>
/// Toggles Windows 11 taskbar alignment between Left and Center.
/// </summary>
public class TaskbarAlignmentTweak : TweakBase
{
    public override string Id => "taskbar-alignment";
    public override string Name => "Taskbar Alignment Left (Win 11)";
    public override string Description => "Move Windows 11 taskbar icons to the left (like Windows 10)";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string AdvancedPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced";

    public override bool IsApplied()
    {
        // 0 = Left, 1 = Center (default)
        var value = RegistryHelper.GetValue(RegistryHive.CurrentUser, AdvancedPath, "TaskbarAl", 1);
        return Convert.ToInt32(value) == 0;
    }

    public override TweakResult Apply()
    {
        UILogger.LogAction("Setting Taskbar Alignment to Left...");

        // Check Windows version
        if (!SystemHelper.IsWindows11)
        {
            UILogger.Log("Taskbar Alignment", UIStatus.Win11Only, $"Current OS: {SystemHelper.GetOSName()}");
            return Failure("This tweak is only applicable to Windows 11");
        }

        // Set TaskbarAl to 0 (Left)
        bool success = RegistryHelper.SetValue(
            RegistryHive.CurrentUser, 
            AdvancedPath, 
            "TaskbarAl", 
            0);

        // Verify
        var verify = RegistryHelper.GetValue(RegistryHive.CurrentUser, AdvancedPath, "TaskbarAl", 1);
        bool verified = Convert.ToInt32(verify) == 0;

        UILogger.Log("Setting TaskbarAl -> 0 (Left)", verified ? UIStatus.Success : UIStatus.Failed);

        if (verified)
        {
            // Taskbar updates automatically, but notify user
            UILogger.Log("Taskbar Alignment", UIStatus.Done, "Change applied immediately");
            return Success("Taskbar alignment set to Left");
        }
        else
        {
            return Failure("Could not change taskbar alignment");
        }
    }

    public override TweakResult Revert()
    {
        UILogger.LogAction("Setting Taskbar Alignment to Center...");

        if (!SystemHelper.IsWindows11)
        {
            UILogger.Log("Taskbar Alignment Revert", UIStatus.Win11Only);
            return Failure("This tweak is only applicable to Windows 11");
        }

        // Set TaskbarAl to 1 (Center)
        RegistryHelper.SetValue(
            RegistryHive.CurrentUser, 
            AdvancedPath, 
            "TaskbarAl", 
            1);

        UILogger.Log("Setting TaskbarAl -> 1 (Center)", UIStatus.Done);
        return Success("Taskbar alignment set to Center (default)");
    }
}
