using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Disables CPU core parking to prevent micro-stutters in games.
/// Windows parks CPU cores to save power, causing latency spikes.
/// </summary>
public class DisableCoreParkingTweak : TweakBase
{
    public override string Id => "disable-core-parking";
    public override string Name => "Disable CPU Core Parking";
    public override string Description => "Prevent Windows from parking CPU cores to eliminate micro-stutters";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    // Power settings GUIDs
    private const string ProcessorPowerPath = @"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00";
    private const string CoreParkingMinCores = "0cc5b647-c1df-4637-891a-dec35c318583"; // Processor performance core parking min cores
    private const string CoreParkingMaxCores = "ea062031-0e34-4ff1-9b6d-eb1059334028"; // Processor performance core parking max cores
    private const string CoreParkingDecreaseTime = "dfd10d17-d5eb-45dd-877a-9a34ddd15c82"; // Core parking decrease time

    public override bool IsApplied()
    {
        var minPath = $@"{ProcessorPowerPath}\{CoreParkingMinCores}";
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, minPath, "ValueMax", 0);
        return Convert.ToInt32(value) == 100;
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Disabling CPU Core Parking...");

        bool allSuccess = true;

        // Set minimum cores to 100% (no parking)
        var minPath = $@"{ProcessorPowerPath}\{CoreParkingMinCores}";
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, minPath, "ValueMax", 100) &&
            RegistryHelper.SetValue(RegistryHive.LocalMachine, minPath, "ValueMin", 100))
        {
            PerformanceLogger.Log("Core_Parking", "Registry", PerfStatus.VerifiedSuccess, "Min cores -> 100%");
        }
        else
        {
            allSuccess = false;
        }

        // Set maximum cores to 100%
        var maxPath = $@"{ProcessorPowerPath}\{CoreParkingMaxCores}";
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, maxPath, "ValueMax", 100) &&
            RegistryHelper.SetValue(RegistryHive.LocalMachine, maxPath, "ValueMin", 100))
        {
            PerformanceLogger.Log("Core_Parking", "Registry", PerfStatus.VerifiedSuccess, "Max cores -> 100%");
        }

        // Set decrease time to 0 (instant unpark)
        var decreasePath = $@"{ProcessorPowerPath}\{CoreParkingDecreaseTime}";
        RegistryHelper.SetValue(RegistryHive.LocalMachine, decreasePath, "ValueMax", 0);
        RegistryHelper.SetValue(RegistryHive.LocalMachine, decreasePath, "ValueMin", 0);

        // Apply via powercfg for active power plan
        var activeScheme = GetActivePowerScheme();
        if (!string.IsNullOrEmpty(activeScheme))
        {
            // Disable core parking for AC power
            PowerShellHelper.Execute($"powercfg /setacvalueindex {activeScheme} 54533251-82be-4824-96c1-47b60b740d00 0cc5b647-c1df-4637-891a-dec35c318583 100");
            PowerShellHelper.Execute($"powercfg /setactive {activeScheme}");
            PerformanceLogger.Log("Core_Parking", "CLI", PerfStatus.VerifiedSuccess, "Applied to active power plan");
        }

        if (allSuccess)
        {
            PerformanceLogger.Log("Core_Parking", "Registry", PerfStatus.VerifiedSuccess);
            return Success("CPU core parking disabled");
        }

        return Failure("Some core parking settings could not be applied");
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Re-enabling CPU Core Parking...");

        // Restore defaults (50% min cores)
        var minPath = $@"{ProcessorPowerPath}\{CoreParkingMinCores}";
        RegistryHelper.SetValue(RegistryHive.LocalMachine, minPath, "ValueMax", 100);
        RegistryHelper.SetValue(RegistryHive.LocalMachine, minPath, "ValueMin", 0);

        var maxPath = $@"{ProcessorPowerPath}\{CoreParkingMaxCores}";
        RegistryHelper.SetValue(RegistryHive.LocalMachine, maxPath, "ValueMax", 100);
        RegistryHelper.SetValue(RegistryHive.LocalMachine, maxPath, "ValueMin", 0);

        // Apply default via powercfg
        var activeScheme = GetActivePowerScheme();
        if (!string.IsNullOrEmpty(activeScheme))
        {
            PowerShellHelper.Execute($"powercfg /setacvalueindex {activeScheme} 54533251-82be-4824-96c1-47b60b740d00 0cc5b647-c1df-4637-891a-dec35c318583 50");
            PowerShellHelper.Execute($"powercfg /setactive {activeScheme}");
        }

        PerformanceLogger.Log("Core_Parking", "Registry", PerfStatus.Reverted);
        return Success("CPU core parking re-enabled (default)");
    }

    private string? GetActivePowerScheme()
    {
        var result = PowerShellHelper.Execute("powercfg /getactivescheme");
        if (result.Success)
        {
            var match = System.Text.RegularExpressions.Regex.Match(result.Output, 
                @"([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})");
            return match.Success ? match.Value : null;
        }
        return null;
    }
}
