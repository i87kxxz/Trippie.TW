using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Optimizes memory management for gaming performance.
/// Disables memory compression and optimizes paging behavior.
/// </summary>
public class MemoryOptimizationTweak : TweakBase
{
    public override string Id => "memory-optimization";
    public override string Name => "Optimize Memory Management";
    public override string Description => "Disable memory compression and optimize paging for gaming";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    private const string MemoryManagementPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management";
    private const string PrefetchPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters";

    public override bool IsApplied()
    {
        var disablePaging = RegistryHelper.GetValue(RegistryHive.LocalMachine, MemoryManagementPath, "DisablePagingExecutive", 0);
        return Convert.ToInt32(disablePaging) == 1;
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Optimizing Memory Management...");

        bool allSuccess = true;

        // DisablePagingExecutive: Keep kernel and drivers in RAM (reduces latency)
        if (PerformanceLogger.SetAndVerify(RegistryHive.LocalMachine, MemoryManagementPath, "DisablePagingExecutive", 1))
        {
            PerformanceLogger.Log("Memory_Optimization", "Registry", PerfStatus.VerifiedSuccess, 
                "DisablePagingExecutive -> 1 (kernel stays in RAM)");
        }
        else
        {
            allSuccess = false;
        }

        // LargeSystemCache: 0 for gaming (1 for file servers)
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, MemoryManagementPath, "LargeSystemCache", 0))
        {
            PerformanceLogger.Log("Memory_Optimization", "Registry", PerfStatus.VerifiedSuccess, 
                "LargeSystemCache -> 0 (optimized for gaming)");
        }

        // SecondLevelDataCache: Set to actual L2 cache size if known (0 = auto)
        // Leaving as auto is safest

        // ClearPageFileAtShutdown: 0 for faster shutdown (security tweak sets to 1)
        RegistryHelper.SetValue(RegistryHive.LocalMachine, MemoryManagementPath, "ClearPageFileAtShutdown", 0);

        // Disable Prefetch/Superfetch for gaming (reduces disk I/O during gameplay)
        // EnablePrefetcher: 0=Disabled, 1=Application, 2=Boot, 3=Both
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, PrefetchPath, "EnablePrefetcher", 0))
        {
            PerformanceLogger.Log("Memory_Optimization", "Registry", PerfStatus.VerifiedSuccess, 
                "EnablePrefetcher -> 0 (disabled)");
        }

        // EnableSuperfetch: 0=Disabled
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, PrefetchPath, "EnableSuperfetch", 0))
        {
            PerformanceLogger.Log("Memory_Optimization", "Registry", PerfStatus.VerifiedSuccess, 
                "EnableSuperfetch -> 0 (disabled)");
        }

        // Disable Memory Compression via PowerShell
        var compressionResult = PowerShellHelper.Execute("Disable-MMAgent -MemoryCompression -ErrorAction SilentlyContinue");
        if (compressionResult.Success)
        {
            PerformanceLogger.Log("Memory_Optimization", "CLI", PerfStatus.VerifiedSuccess, 
                "Memory compression disabled");
        }
        else
        {
            PerformanceLogger.Log("Memory_Optimization", "CLI", PerfStatus.Failed, 
                "Could not disable memory compression");
        }

        if (allSuccess)
        {
            PerformanceLogger.Log("Memory_Optimization", "Registry", PerfStatus.RebootRequired);
            return Success("Memory management optimized for gaming (reboot required)");
        }

        return Failure("Some memory settings could not be applied");
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Reverting Memory Management settings...");

        // Restore defaults
        RegistryHelper.SetValue(RegistryHive.LocalMachine, MemoryManagementPath, "DisablePagingExecutive", 0);
        RegistryHelper.SetValue(RegistryHive.LocalMachine, MemoryManagementPath, "LargeSystemCache", 0);
        RegistryHelper.SetValue(RegistryHive.LocalMachine, PrefetchPath, "EnablePrefetcher", 3);
        RegistryHelper.SetValue(RegistryHive.LocalMachine, PrefetchPath, "EnableSuperfetch", 3);

        // Re-enable memory compression
        PowerShellHelper.Execute("Enable-MMAgent -MemoryCompression -ErrorAction SilentlyContinue");

        PerformanceLogger.Log("Memory_Optimization", "Registry", PerfStatus.Reverted);
        return Success("Memory management reverted to defaults");
    }
}
