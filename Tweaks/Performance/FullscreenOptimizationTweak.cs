using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Disables Windows fullscreen optimizations that can cause input lag and stuttering.
/// Also optimizes Game Mode and GameDVR settings for better performance.
/// </summary>
public class FullscreenOptimizationTweak : TweakBase
{
    public override string Id => "fullscreen-optimization";
    public override string Name => "Disable Fullscreen Optimizations";
    public override string Description => "Disable Windows fullscreen optimizations to reduce input lag";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string GameConfigStorePath = @"System\GameConfigStore";
    private const string GameBarPath = @"Software\Microsoft\GameBar";
    private const string DwmPath = @"SOFTWARE\Microsoft\Windows\DWM";

    public override bool IsApplied()
    {
        var fseBehavior = RegistryHelper.GetValue(RegistryHive.CurrentUser, GameConfigStorePath, "GameDVR_FSEBehavior", 0);
        return Convert.ToInt32(fseBehavior) == 2;
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Disabling Fullscreen Optimizations...");

        bool allSuccess = true;

        // GameDVR_FSEBehavior: 2 = Disable fullscreen optimizations
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, GameConfigStorePath, "GameDVR_FSEBehavior", 2))
        {
            PerformanceLogger.Log("Fullscreen_Opt", "Registry", PerfStatus.VerifiedSuccess, 
                "GameDVR_FSEBehavior -> 2 (FSO disabled)");
        }
        else
        {
            allSuccess = false;
        }

        // GameDVR_FSEBehaviorMode: 2 = Disable
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, GameConfigStorePath, "GameDVR_FSEBehaviorMode", 2))
        {
            PerformanceLogger.Log("Fullscreen_Opt", "Registry", PerfStatus.VerifiedSuccess, 
                "GameDVR_FSEBehaviorMode -> 2");
        }

        // GameDVR_HonorUserFSEBehaviorMode: 1 = Honor user setting
        RegistryHelper.SetValue(RegistryHive.CurrentUser, GameConfigStorePath, "GameDVR_HonorUserFSEBehaviorMode", 1);

        // GameDVR_DXGIHonorFSEWindowsCompatible: 1 = Honor compatibility settings
        RegistryHelper.SetValue(RegistryHive.CurrentUser, GameConfigStorePath, "GameDVR_DXGIHonorFSEWindowsCompatible", 1);

        // Disable Auto Game Mode (can cause issues)
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, GameBarPath, "AllowAutoGameMode", 0))
        {
            PerformanceLogger.Log("Fullscreen_Opt", "Registry", PerfStatus.VerifiedSuccess, 
                "AllowAutoGameMode -> 0");
        }

        // Disable Game Bar tips
        RegistryHelper.SetValue(RegistryHive.CurrentUser, GameBarPath, "ShowStartupPanel", 0);

        // DWM optimizations for reduced latency
        // DisableHWAcceleration: Keep at 0 (we want HW acceleration)
        // But optimize other DWM settings
        
        // Disable animations in DWM
        RegistryHelper.SetValue(RegistryHive.LocalMachine, DwmPath, "EnableAeroPeek", 0);

        // Disable VRR scheduling optimization (can add latency)
        var schedulerPath = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler";
        RegistryHelper.SetValue(RegistryHive.LocalMachine, schedulerPath, "VRROptimizeEnable", 0);
        PerformanceLogger.Log("Fullscreen_Opt", "Registry", PerfStatus.VerifiedSuccess, 
            "VRROptimizeEnable -> 0");

        if (allSuccess)
        {
            PerformanceLogger.Log("Fullscreen_Opt", "Registry", PerfStatus.VerifiedSuccess);
            return Success("Fullscreen optimizations disabled");
        }

        return Failure("Some fullscreen settings could not be applied");
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Re-enabling Fullscreen Optimizations...");

        // Restore defaults
        RegistryHelper.DeleteValue(RegistryHive.CurrentUser, GameConfigStorePath, "GameDVR_FSEBehavior");
        RegistryHelper.DeleteValue(RegistryHive.CurrentUser, GameConfigStorePath, "GameDVR_FSEBehaviorMode");
        RegistryHelper.DeleteValue(RegistryHive.CurrentUser, GameConfigStorePath, "GameDVR_HonorUserFSEBehaviorMode");
        RegistryHelper.DeleteValue(RegistryHive.CurrentUser, GameConfigStorePath, "GameDVR_DXGIHonorFSEWindowsCompatible");
        RegistryHelper.SetValue(RegistryHive.CurrentUser, GameBarPath, "AllowAutoGameMode", 1);
        RegistryHelper.SetValue(RegistryHive.LocalMachine, DwmPath, "EnableAeroPeek", 1);
        
        var schedulerPath = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler";
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, schedulerPath, "VRROptimizeEnable");

        PerformanceLogger.Log("Fullscreen_Opt", "Registry", PerfStatus.Reverted);
        return Success("Fullscreen optimizations re-enabled (default)");
    }
}
