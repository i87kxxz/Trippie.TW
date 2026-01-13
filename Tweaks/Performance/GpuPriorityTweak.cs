using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Optimizes GPU and game process priority for better gaming performance.
/// Configures Windows Multimedia Class Scheduler Service (MMCSS) for games.
/// </summary>
public class GpuPriorityTweak : TweakBase
{
    public override string Id => "gpu-priority";
    public override string Name => "Optimize GPU & Game Priority";
    public override string Description => "Maximize GPU scheduling priority and game process priority";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    private const string GamesTaskPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games";
    private const string SystemProfilePath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";

    public override bool IsApplied()
    {
        var gpuPriority = RegistryHelper.GetValue(RegistryHive.LocalMachine, GamesTaskPath, "GPU Priority", 0);
        return Convert.ToInt32(gpuPriority) == 8;
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Optimizing GPU & Game Priority...");

        bool allSuccess = true;

        // Configure Games task profile
        // GPU Priority: 0-31, 8 is maximum for MMCSS
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, GamesTaskPath, "GPU Priority", 8))
        {
            PerformanceLogger.Log("GPU_Priority", "Registry", PerfStatus.VerifiedSuccess, "GPU Priority -> 8 (max)");
        }
        else
        {
            allSuccess = false;
        }

        // Priority: 1=Low, 2=Normal, 3=High, 6=Critical
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, GamesTaskPath, "Priority", 6))
        {
            PerformanceLogger.Log("GPU_Priority", "Registry", PerfStatus.VerifiedSuccess, "Priority -> 6 (Critical)");
        }

        // Scheduling Category: High, Medium, Low
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, GamesTaskPath, "Scheduling Category", "High", RegistryValueKind.String))
        {
            PerformanceLogger.Log("GPU_Priority", "Registry", PerfStatus.VerifiedSuccess, "Scheduling Category -> High");
        }

        // SFIO Priority: High, Normal, Low, Idle
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, GamesTaskPath, "SFIO Priority", "High", RegistryValueKind.String))
        {
            PerformanceLogger.Log("GPU_Priority", "Registry", PerfStatus.VerifiedSuccess, "SFIO Priority -> High");
        }

        // Background Only: false (games run in foreground)
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, GamesTaskPath, "Background Only", 0, RegistryValueKind.String))
        {
            PerformanceLogger.Log("GPU_Priority", "Registry", PerfStatus.VerifiedSuccess, "Background Only -> False");
        }

        // Affinity: 0 = use all cores
        RegistryHelper.SetValue(RegistryHive.LocalMachine, GamesTaskPath, "Affinity", 0);

        // Clock Rate: 10000 (100ns units = 1ms)
        RegistryHelper.SetValue(RegistryHive.LocalMachine, GamesTaskPath, "Clock Rate", 10000);

        // System Profile optimizations
        // SystemResponsiveness: 0 = prioritize foreground (games)
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, SystemProfilePath, "SystemResponsiveness", 0))
        {
            PerformanceLogger.Log("GPU_Priority", "Registry", PerfStatus.VerifiedSuccess, "SystemResponsiveness -> 0");
        }

        // LazyMode: 0 = disable lazy mode for immediate scheduling
        RegistryHelper.SetValue(RegistryHive.LocalMachine, SystemProfilePath, "LazyModeTimeout", 0);

        if (allSuccess)
        {
            PerformanceLogger.Log("GPU_Priority", "Registry", PerfStatus.VerifiedSuccess);
            return Success("GPU and game priority optimized");
        }

        return Failure("Some priority settings could not be applied");
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Reverting GPU & Game Priority settings...");

        // Restore defaults
        RegistryHelper.SetValue(RegistryHive.LocalMachine, GamesTaskPath, "GPU Priority", 2);
        RegistryHelper.SetValue(RegistryHive.LocalMachine, GamesTaskPath, "Priority", 2);
        RegistryHelper.SetValue(RegistryHive.LocalMachine, GamesTaskPath, "Scheduling Category", "Medium", RegistryValueKind.String);
        RegistryHelper.SetValue(RegistryHive.LocalMachine, GamesTaskPath, "SFIO Priority", "Normal", RegistryValueKind.String);
        RegistryHelper.SetValue(RegistryHive.LocalMachine, SystemProfilePath, "SystemResponsiveness", 20);

        PerformanceLogger.Log("GPU_Priority", "Registry", PerfStatus.Reverted);
        return Success("GPU and game priority reverted to defaults");
    }
}
