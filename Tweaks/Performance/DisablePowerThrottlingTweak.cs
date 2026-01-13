using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Disables Power Throttling to prevent CPU performance reduction.
/// </summary>
public class DisablePowerThrottlingTweak : TweakBase
{
    public override string Id => "disable-power-throttling";
    public override string Name => "Disable Power Throttling";
    public override string Description => "Prevent Windows from throttling CPU performance for power saving";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    private const string PowerThrottlePath = @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, PowerThrottlePath, "PowerThrottlingOff", 0);
        return Convert.ToInt32(value) == 1;
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Disabling Power Throttling...");

        bool verified = PerformanceLogger.SetAndVerify(
            RegistryHive.LocalMachine, 
            PowerThrottlePath, 
            "PowerThrottlingOff", 
            1);

        PerformanceLogger.Log("Disable_Power_Throttling", "Registry", 
            verified ? PerfStatus.VerifiedSuccess : PerfStatus.VerifiedFailed,
            $"{PowerThrottlePath}\\PowerThrottlingOff -> 1");

        if (verified)
        {
            PerformanceLogger.Log("Disable_Power_Throttling", "Registry", PerfStatus.RebootRequired);
        }

        return verified 
            ? Success("Power Throttling disabled (reboot recommended)") 
            : Failure("Could not disable Power Throttling");
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Re-enabling Power Throttling...");

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, PowerThrottlePath, "PowerThrottlingOff");
        
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, PowerThrottlePath, "PowerThrottlingOff", null);
        bool verified = value == null;

        PerformanceLogger.Log("Disable_Power_Throttling", "Registry", 
            verified ? PerfStatus.Reverted : PerfStatus.Failed);

        return verified 
            ? Success("Power Throttling re-enabled") 
            : Failure("Could not re-enable Power Throttling");
    }
}
