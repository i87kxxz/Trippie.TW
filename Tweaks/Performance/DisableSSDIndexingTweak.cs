using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;
using System.Diagnostics;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Disables search indexing specifically for SSD drives to preserve longevity.
/// </summary>
public class DisableSSDIndexingTweak : TweakBase
{
    public override string Id => "disable-ssd-indexing";
    public override string Name => "Disable SSD Indexing";
    public override string Description => "Disable search indexing on SSD drives to preserve drive longevity";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    private const string IndexingPolicyPath = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, IndexingPolicyPath, "AllowIndexingEncryptedStoresOrItems", 1);
        var preventIndex = RegistryHelper.GetValue(RegistryHive.LocalMachine, IndexingPolicyPath, "PreventIndexingLowDiskSpaceMB", 0);
        return Convert.ToInt32(value) == 0 || Convert.ToInt32(preventIndex) > 0;
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Disabling SSD Indexing...");

        // Detect SSDs
        var ssdDrives = DetectSSDDrives();
        if (ssdDrives.Count > 0)
        {
            PerformanceLogger.Log("Detect_SSDs", "WMI", PerfStatus.VerifiedSuccess, 
                $"Found SSDs: {string.Join(", ", ssdDrives)}");
        }
        else
        {
            PerformanceLogger.Log("Detect_SSDs", "WMI", PerfStatus.InProgress, "No SSDs detected or detection failed");
        }

        bool allVerified = true;

        // Disable indexing via policy
        if (PerformanceLogger.SetAndVerify(RegistryHive.LocalMachine, IndexingPolicyPath, "AllowIndexingEncryptedStoresOrItems", 0))
            PerformanceLogger.Log("SSD_Indexing_Encrypted", "Registry", PerfStatus.VerifiedSuccess);
        else
        {
            PerformanceLogger.Log("SSD_Indexing_Encrypted", "Registry", PerfStatus.VerifiedFailed);
            allVerified = false;
        }

        // Set high threshold to prevent indexing
        if (PerformanceLogger.SetAndVerify(RegistryHive.LocalMachine, IndexingPolicyPath, "PreventIndexingLowDiskSpaceMB", 999999))
            PerformanceLogger.Log("SSD_Indexing_LowDisk", "Registry", PerfStatus.VerifiedSuccess, "Threshold set to 999999 MB");
        else
            PerformanceLogger.Log("SSD_Indexing_LowDisk", "Registry", PerfStatus.VerifiedFailed);

        // Disable indexing on removable drives
        if (PerformanceLogger.SetAndVerify(RegistryHive.LocalMachine, IndexingPolicyPath, "DisableRemovableDriveIndexing", 1))
            PerformanceLogger.Log("SSD_Indexing_Removable", "Registry", PerfStatus.VerifiedSuccess);
        else
            PerformanceLogger.Log("SSD_Indexing_Removable", "Registry", PerfStatus.VerifiedFailed);

        // Disable content indexing for each SSD via fsutil
        foreach (var drive in ssdDrives)
        {
            DisableDriveIndexing(drive);
        }

        return allVerified 
            ? Success("SSD indexing optimizations applied") 
            : Failure("Some SSD indexing settings could not be applied");
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Reverting SSD Indexing settings...");

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, IndexingPolicyPath, "AllowIndexingEncryptedStoresOrItems");
        PerformanceLogger.Log("SSD_Indexing_Encrypted", "Registry", PerfStatus.Reverted);

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, IndexingPolicyPath, "PreventIndexingLowDiskSpaceMB");
        PerformanceLogger.Log("SSD_Indexing_LowDisk", "Registry", PerfStatus.Reverted);

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, IndexingPolicyPath, "DisableRemovableDriveIndexing");
        PerformanceLogger.Log("SSD_Indexing_Removable", "Registry", PerfStatus.Reverted);

        return Success("SSD indexing settings reverted");
    }

    private List<string> DetectSSDDrives()
    {
        var ssds = new List<string>();
        try
        {
            // Use PowerShell to detect SSDs
            var result = PowerShellHelper.Execute(
                "Get-PhysicalDisk | Where-Object MediaType -eq 'SSD' | " +
                "ForEach-Object { Get-Partition -DiskNumber $_.DeviceId -ErrorAction SilentlyContinue | " +
                "Where-Object DriveLetter | Select-Object -ExpandProperty DriveLetter }");

            if (result.Success && !string.IsNullOrWhiteSpace(result.Output))
            {
                var drives = result.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var drive in drives)
                {
                    var letter = drive.Trim();
                    if (!string.IsNullOrEmpty(letter) && char.IsLetter(letter[0]))
                    {
                        ssds.Add($"{letter}:");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            PerformanceLogger.Log("Detect_SSDs", "WMI", PerfStatus.Failed, ex.Message);
        }
        return ssds;
    }

    private void DisableDriveIndexing(string driveLetter)
    {
        try
        {
            // Use fsutil to disable indexing behavior
            var psi = new ProcessStartInfo
            {
                FileName = "fsutil.exe",
                Arguments = $"behavior set disablelastaccess 1",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };
            using var process = Process.Start(psi);
            process?.WaitForExit();

            PerformanceLogger.Log($"SSD_Indexing_{driveLetter}", "CLI", 
                process?.ExitCode == 0 ? PerfStatus.VerifiedSuccess : PerfStatus.VerifiedFailed,
                "Disabled last access timestamp");
        }
        catch (Exception ex)
        {
            PerformanceLogger.Log($"SSD_Indexing_{driveLetter}", "CLI", PerfStatus.Failed, ex.Message);
        }
    }
}
