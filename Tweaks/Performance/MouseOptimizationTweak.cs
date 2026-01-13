using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Disables mouse acceleration and optimizes mouse settings for gaming.
/// Provides raw 1:1 mouse input for better aim precision.
/// </summary>
public class MouseOptimizationTweak : TweakBase
{
    public override string Id => "mouse-optimization";
    public override string Name => "Disable Mouse Acceleration";
    public override string Description => "Remove mouse acceleration for raw 1:1 input (better for gaming)";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string MousePath = @"Control Panel\Mouse";

    public override bool IsApplied()
    {
        var mouseSpeed = RegistryHelper.GetValue(RegistryHive.CurrentUser, MousePath, "MouseSpeed", "1");
        return mouseSpeed?.ToString() == "0";
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Disabling Mouse Acceleration...");
        bool allSuccess = true;

        // MouseSpeed: 0 = No acceleration curve applied
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, MousePath, "MouseSpeed", "0", RegistryValueKind.String))
        {
            PerformanceLogger.Log("Mouse_Optimization", "Registry", PerfStatus.VerifiedSuccess, "MouseSpeed -> 0");
        }
        else
        {
            allSuccess = false;
        }

        // MouseThreshold1 and MouseThreshold2: 0 = Disable acceleration thresholds
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, MousePath, "MouseThreshold1", "0", RegistryValueKind.String))
        {
            PerformanceLogger.Log("Mouse_Optimization", "Registry", PerfStatus.VerifiedSuccess, "MouseThreshold1 -> 0");
        }

        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, MousePath, "MouseThreshold2", "0", RegistryValueKind.String))
        {
            PerformanceLogger.Log("Mouse_Optimization", "Registry", PerfStatus.VerifiedSuccess, "MouseThreshold2 -> 0");
        }

        // MouseSensitivity: 10 = Default (1:1 with no acceleration)
        RegistryHelper.SetValue(RegistryHive.CurrentUser, MousePath, "MouseSensitivity", "10", RegistryValueKind.String);

        // Apply changes immediately via SystemParametersInfo
        var result = PowerShellHelper.Execute(
            "[System.Runtime.InteropServices.Marshal]::GetLastWin32Error(); " +
            "Add-Type -TypeDefinition 'using System; using System.Runtime.InteropServices; " +
            "public class Mouse { [DllImport(\"user32.dll\")] public static extern bool SystemParametersInfo(int uAction, int uParam, int[] lpvParam, int fuWinIni); }'; " +
            "[Mouse]::SystemParametersInfo(4, 0, @(0,0,0), 2)");

        if (allSuccess)
        {
            PerformanceLogger.Log("Mouse_Optimization", "Registry", PerfStatus.VerifiedSuccess);
            return Success("Mouse acceleration disabled (may need re-login)");
        }

        return Failure("Some mouse settings could not be applied");
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Re-enabling Mouse Acceleration...");

        // Restore Windows defaults
        RegistryHelper.SetValue(RegistryHive.CurrentUser, MousePath, "MouseSpeed", "1", RegistryValueKind.String);
        RegistryHelper.SetValue(RegistryHive.CurrentUser, MousePath, "MouseThreshold1", "6", RegistryValueKind.String);
        RegistryHelper.SetValue(RegistryHive.CurrentUser, MousePath, "MouseThreshold2", "10", RegistryValueKind.String);
        RegistryHelper.SetValue(RegistryHive.CurrentUser, MousePath, "MouseSensitivity", "10", RegistryValueKind.String);

        PerformanceLogger.Log("Mouse_Optimization", "Registry", PerfStatus.Reverted);
        return Success("Mouse acceleration re-enabled (default)");
    }
}
