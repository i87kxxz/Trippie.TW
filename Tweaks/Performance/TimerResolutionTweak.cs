using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Optimizes Windows timer resolution for reduced input lag in games.
/// Default Windows timer is 15.6ms, this reduces it to improve responsiveness.
/// </summary>
public class TimerResolutionTweak : TweakBase
{
    public override string Id => "timer-resolution";
    public override string Name => "Optimize Timer Resolution";
    public override string Description => "Reduce system timer latency for better input responsiveness in games";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    private const string KernelPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\kernel";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, KernelPath, "GlobalTimerResolutionRequests", 0);
        return Convert.ToInt32(value) == 1;
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Optimizing Timer Resolution...");

        bool allSuccess = true;

        // Enable global timer resolution requests
        if (PerformanceLogger.SetAndVerify(RegistryHive.LocalMachine, KernelPath, "GlobalTimerResolutionRequests", 1))
        {
            PerformanceLogger.Log("Timer_Resolution", "Registry", PerfStatus.VerifiedSuccess, 
                "GlobalTimerResolutionRequests -> 1");
        }
        else
        {
            PerformanceLogger.Log("Timer_Resolution", "Registry", PerfStatus.VerifiedFailed);
            allSuccess = false;
        }

        // Disable dynamic tick (forces consistent timer)
        var bcdeditResult = PowerShellHelper.Execute("bcdedit /set disabledynamictick yes");
        if (bcdeditResult.Success)
        {
            PerformanceLogger.Log("Timer_Resolution", "CLI", PerfStatus.VerifiedSuccess, "Disabled dynamic tick");
        }
        else
        {
            PerformanceLogger.Log("Timer_Resolution", "CLI", PerfStatus.Failed, "Could not disable dynamic tick");
        }

        // Set platform clock source to TSC for lower latency
        var tscResult = PowerShellHelper.Execute("bcdedit /set useplatformclock false");
        if (tscResult.Success)
        {
            PerformanceLogger.Log("Timer_Resolution", "CLI", PerfStatus.VerifiedSuccess, "Using TSC clock source");
        }

        if (allSuccess)
        {
            PerformanceLogger.Log("Timer_Resolution", "Registry", PerfStatus.RebootRequired);
            return Success("Timer resolution optimized (reboot required)");
        }
        
        return Failure("Some timer settings could not be applied");
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Reverting Timer Resolution settings...");

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, KernelPath, "GlobalTimerResolutionRequests");
        PerformanceLogger.Log("Timer_Resolution", "Registry", PerfStatus.Reverted, "GlobalTimerResolutionRequests removed");

        PowerShellHelper.Execute("bcdedit /deletevalue disabledynamictick");
        PowerShellHelper.Execute("bcdedit /deletevalue useplatformclock");
        PerformanceLogger.Log("Timer_Resolution", "CLI", PerfStatus.Reverted, "BCD values reset");

        return Success("Timer resolution settings reverted to defaults");
    }
}
