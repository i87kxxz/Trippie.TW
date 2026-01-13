using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Optimizes NTFS memory usage for better file system performance.
/// </summary>
public class OptimizeNTFSTweak : TweakBase
{
    public override string Id => "optimize-ntfs";
    public override string Name => "Optimize NTFS Memory Usage";
    public override string Description => "Increase NTFS memory allocation for better file system performance";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    private const string FileSystemPath = @"SYSTEM\CurrentControlSet\Control\FileSystem";

    public override bool IsApplied()
    {
        var memUsage = RegistryHelper.GetValue(RegistryHive.LocalMachine, FileSystemPath, "NtfsMemoryUsage", 1);
        return Convert.ToInt32(memUsage) == 2;
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Optimizing NTFS Memory Usage...");
        bool allVerified = true;

        // Increase NTFS memory usage (2 = increased paged pool usage)
        if (PerformanceLogger.SetAndVerify(RegistryHive.LocalMachine, FileSystemPath, "NtfsMemoryUsage", 2))
            PerformanceLogger.Log("NTFS_MemoryUsage", "Registry", PerfStatus.VerifiedSuccess, "NtfsMemoryUsage -> 2");
        else
        {
            PerformanceLogger.Log("NTFS_MemoryUsage", "Registry", PerfStatus.VerifiedFailed);
            allVerified = false;
        }

        // Disable last access timestamp updates (reduces disk writes)
        if (PerformanceLogger.SetAndVerify(RegistryHive.LocalMachine, FileSystemPath, "NtfsDisableLastAccessUpdate", 1))
            PerformanceLogger.Log("NTFS_DisableLastAccess", "Registry", PerfStatus.VerifiedSuccess);
        else
            PerformanceLogger.Log("NTFS_DisableLastAccess", "Registry", PerfStatus.VerifiedFailed);

        // Disable 8.3 name creation (legacy DOS compatibility)
        if (PerformanceLogger.SetAndVerify(RegistryHive.LocalMachine, FileSystemPath, "NtfsDisable8dot3NameCreation", 1))
            PerformanceLogger.Log("NTFS_Disable8dot3", "Registry", PerfStatus.VerifiedSuccess);
        else
            PerformanceLogger.Log("NTFS_Disable8dot3", "Registry", PerfStatus.VerifiedFailed);

        // Enable large system cache
        const string memMgmtPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management";
        if (PerformanceLogger.SetAndVerify(RegistryHive.LocalMachine, memMgmtPath, "LargeSystemCache", 1))
            PerformanceLogger.Log("NTFS_LargeSystemCache", "Registry", PerfStatus.VerifiedSuccess);
        else
            PerformanceLogger.Log("NTFS_LargeSystemCache", "Registry", PerfStatus.VerifiedFailed);

        if (allVerified)
        {
            PerformanceLogger.Log("Optimize_NTFS", "Registry", PerfStatus.RebootRequired);
        }

        return allVerified 
            ? Success("NTFS optimized (reboot recommended)") 
            : Failure("Some NTFS settings could not be applied");
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Reverting NTFS optimizations...");

        // Revert to defaults
        PerformanceLogger.SetAndVerify(RegistryHive.LocalMachine, FileSystemPath, "NtfsMemoryUsage", 1);
        PerformanceLogger.Log("NTFS_MemoryUsage", "Registry", PerfStatus.Reverted, "NtfsMemoryUsage -> 1");

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, FileSystemPath, "NtfsDisableLastAccessUpdate");
        PerformanceLogger.Log("NTFS_DisableLastAccess", "Registry", PerfStatus.Reverted);

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, FileSystemPath, "NtfsDisable8dot3NameCreation");
        PerformanceLogger.Log("NTFS_Disable8dot3", "Registry", PerfStatus.Reverted);

        const string memMgmtPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management";
        RegistryHelper.SetValue(RegistryHive.LocalMachine, memMgmtPath, "LargeSystemCache", 0);
        PerformanceLogger.Log("NTFS_LargeSystemCache", "Registry", PerfStatus.Reverted);

        PerformanceLogger.Log("Optimize_NTFS", "Registry", PerfStatus.RebootRequired);
        return Success("NTFS settings reverted to defaults");
    }
}
