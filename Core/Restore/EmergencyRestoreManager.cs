using Trippie.TW.Core.Interfaces;
using Trippie.TW.Core.Registry;
using Trippie.TW.Helpers;
using System.Diagnostics;

namespace Trippie.TW.Core.Restore;

/// <summary>
/// Manages emergency restore and undo operations for all tweaks.
/// </summary>
public class EmergencyRestoreManager
{
    private readonly CategoryRegistry _registry;
    private readonly RegistryBackupManager _backupManager;

    public EmergencyRestoreManager(CategoryRegistry registry)
    {
        _registry = registry;
        _backupManager = new RegistryBackupManager();
    }

    public RegistryBackupManager BackupManager => _backupManager;

    /// <summary>
    /// Creates a full system backup before applying tweaks.
    /// </summary>
    public bool CreateFullBackup()
    {
        SecurityLogger.LogAction("Creating full system backup...");
        Console.WriteLine();

        bool success = true;

        // Create System Restore Point
        if (!RestorePointManager.CreateRestorePoint("Trippie.TW Pre-Tweak Backup"))
        {
            SecurityLogger.Log("System Restore Point creation", SecurityStatus.Warning, 
                "Continuing without restore point");
        }

        // Export critical registry keys
        var criticalKeys = new[]
        {
            (Microsoft.Win32.RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows"),
            (Microsoft.Win32.RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power"),
            (Microsoft.Win32.RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services"),
            (Microsoft.Win32.RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion"),
        };

        foreach (var (hive, path) in criticalKeys)
        {
            _backupManager.ExportKey(hive, path);
        }

        SecurityLogger.Log("Full system backup", SecurityStatus.Done);
        return success;
    }

    /// <summary>
    /// Reverts all applied tweaks to Windows defaults.
    /// </summary>
    public RestoreResult RevertAllTweaks()
    {
        SecurityLogger.LogAction("Starting full system restore...");
        Console.WriteLine();

        int totalTweaks = 0;
        int revertedTweaks = 0;
        int failedTweaks = 0;
        var failedNames = new List<string>();

        foreach (var category in _registry.Categories)
        {
            SecurityLogger.LogAction($"Processing category: {category.Name}...");

            foreach (var tweak in category.Tweaks)
            {
                totalTweaks++;

                try
                {
                    if (tweak.IsApplied())
                    {
                        SecurityLogger.Log($"Reverting Tweak: {tweak.Name}", SecurityStatus.Restoring);
                        
                        var result = tweak.Revert();
                        
                        if (result.Success)
                        {
                            SecurityLogger.Log($"Reverting Tweak: {tweak.Name}", SecurityStatus.Reverted);
                            revertedTweaks++;
                        }
                        else
                        {
                            SecurityLogger.Log($"Reverting Tweak: {tweak.Name}", SecurityStatus.Failed, result.Message);
                            failedTweaks++;
                            failedNames.Add(tweak.Name);
                        }
                    }
                    else
                    {
                        SecurityLogger.Log($"Tweak: {tweak.Name}", SecurityStatus.Skipped, "Not applied");
                    }
                }
                catch (Exception ex)
                {
                    SecurityLogger.Log($"Reverting Tweak: {tweak.Name}", SecurityStatus.Failed, ex.Message);
                    failedTweaks++;
                    failedNames.Add(tweak.Name);
                }
            }
        }

        // Additional system restorations
        RestoreSystemDefaults();

        // Restore backed-up registry values
        SecurityLogger.LogAction("Restoring backed-up registry values...");
        int registryRestored = _backupManager.RestoreAll();
        SecurityLogger.Log($"Registry values restored: {registryRestored}", SecurityStatus.Done);

        return new RestoreResult
        {
            TotalTweaks = totalTweaks,
            RevertedTweaks = revertedTweaks,
            FailedTweaks = failedTweaks,
            FailedTweakNames = failedNames,
            RegistryValuesRestored = registryRestored
        };
    }

    /// <summary>
    /// Restores specific system defaults that may have been changed.
    /// </summary>
    private void RestoreSystemDefaults()
    {
        SecurityLogger.LogAction("Restoring system defaults...");

        // Re-enable hibernation
        try
        {
            SecurityLogger.Log("Restoring Hibernation", SecurityStatus.InProgress);
            var psi = new ProcessStartInfo
            {
                FileName = "powercfg.exe",
                Arguments = "-h on",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            process?.WaitForExit();
            SecurityLogger.Log("Restoring Hibernation", SecurityStatus.Reverted);
        }
        catch (Exception ex)
        {
            SecurityLogger.Log("Restoring Hibernation", SecurityStatus.Failed, ex.Message);
        }

        // Reset power plan to Balanced
        try
        {
            SecurityLogger.Log("Restoring Power Plan to Balanced", SecurityStatus.InProgress);
            var psi = new ProcessStartInfo
            {
                FileName = "powercfg.exe",
                Arguments = "/setactive 381b4222-f694-41f0-9685-ff5bb260df2e",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            process?.WaitForExit();
            SecurityLogger.Log("Restoring Power Plan to Balanced", SecurityStatus.Reverted);
        }
        catch (Exception ex)
        {
            SecurityLogger.Log("Restoring Power Plan", SecurityStatus.Failed, ex.Message);
        }

        // Re-enable critical services (Privacy + Services module)
        var servicesToRestore = new[]
        {
            // Privacy services
            ("DiagTrack", "auto"),
            ("dmwappushservice", "auto"),
            ("WSearch", "delayed-auto"),
            ("lfsvc", "manual"),
            // Services module
            ("Spooler", "auto"),
            ("bthserv", "manual"),
            ("BTAGService", "manual"),
            ("RemoteRegistry", "manual"),
            ("SysMain", "auto"),
            ("wisvc", "manual"),
            ("TabletInputService", "manual"),
            ("Fax", "manual"),
            ("WerSvc", "manual")
        };

        foreach (var (service, startType) in servicesToRestore)
        {
            try
            {
                if (ServiceHelper.ServiceExists(service))
                {
                    SecurityLogger.Log($"Restoring Service: {service}", SecurityStatus.InProgress);
                    ServiceHelper.SetStartupType(service, startType);
                    ServiceHelper.StartService(service);
                    SecurityLogger.Log($"Restoring Service: {service}", SecurityStatus.Reverted, $"StartType: {startType}");
                }
            }
            catch (Exception ex)
            {
                SecurityLogger.Log($"Restoring Service: {service}", SecurityStatus.Failed, ex.Message);
            }
        }
    }

    /// <summary>
    /// Gets a summary of currently applied tweaks.
    /// </summary>
    public TweakSummary GetAppliedTweaksSummary()
    {
        int total = 0;
        int applied = 0;
        var appliedTweaks = new List<(string Category, string TweakName)>();

        foreach (var category in _registry.Categories)
        {
            foreach (var tweak in category.Tweaks)
            {
                total++;
                if (tweak.IsApplied())
                {
                    applied++;
                    appliedTweaks.Add((category.Name, tweak.Name));
                }
            }
        }

        return new TweakSummary
        {
            TotalTweaks = total,
            AppliedTweaks = applied,
            AppliedTweaksList = appliedTweaks
        };
    }
}

public class RestoreResult
{
    public int TotalTweaks { get; set; }
    public int RevertedTweaks { get; set; }
    public int FailedTweaks { get; set; }
    public List<string> FailedTweakNames { get; set; } = new();
    public int RegistryValuesRestored { get; set; }
    public bool FullSuccess => FailedTweaks == 0;
}

public class TweakSummary
{
    public int TotalTweaks { get; set; }
    public int AppliedTweaks { get; set; }
    public List<(string Category, string TweakName)> AppliedTweaksList { get; set; } = new();
}
