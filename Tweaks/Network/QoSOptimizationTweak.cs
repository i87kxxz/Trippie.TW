using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Network;

/// <summary>
/// Optimizes QoS (Quality of Service) settings to prioritize gaming traffic.
/// </summary>
public class QoSOptimizationTweak : TweakBase
{
    public override string Id => "qos-optimization";
    public override string Name => "Optimize QoS for Gaming";
    public override string Description => "Disable bandwidth reservation and optimize packet scheduling";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string PschedPath = @"SOFTWARE\Policies\Microsoft\Windows\Psched";
    private const string QosPath = @"SOFTWARE\Policies\Microsoft\Windows\QoS";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, PschedPath, "NonBestEffortLimit", 20);
        return Convert.ToInt32(value) == 0;
    }

    public override TweakResult Apply()
    {
        NetworkLogger.LogAction("Optimizing QoS for Gaming...");
        bool allSuccess = true;

        // NonBestEffortLimit: 0 = No bandwidth reservation for QoS
        // Windows reserves 20% by default, this gives it all to applications
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, PschedPath, "NonBestEffortLimit", 0))
        {
            NetworkLogger.Log("Setting NonBestEffortLimit -> 0 (no reservation)", NetStatus.Success);
        }
        else
        {
            allSuccess = false;
        }

        // TimerResolution: Lower value = more responsive packet scheduling
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, PschedPath, "TimerResolution", 1))
        {
            NetworkLogger.Log("Setting TimerResolution -> 1", NetStatus.Success);
        }

        // MaxOutstandingSends: Increase for better throughput
        RegistryHelper.SetValue(RegistryHive.LocalMachine, PschedPath, "MaxOutstandingSends", 65000);

        // Disable QoS packet scheduler throttling
        // This applies to all adapters

        // Also optimize via Group Policy equivalent
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, 
            @"SOFTWARE\Policies\Microsoft\Windows\Psched", "NonBestEffortLimit", 0))
        {
            NetworkLogger.Log("Setting Policy NonBestEffortLimit -> 0", NetStatus.Success);
        }

        // Verify connectivity
        var pingResult = NetworkHelper.TestConnectivity();
        if (!pingResult.Success)
        {
            NetworkLogger.Log("Connectivity issue detected - reverting", NetStatus.Warning);
            Revert();
            return Failure("Connectivity lost after applying tweak - changes reverted");
        }

        NetworkLogger.Log("QoS Optimization", allSuccess ? NetStatus.Done : NetStatus.Failed);
        return allSuccess 
            ? Success("QoS optimized for gaming (no bandwidth reservation)") 
            : Failure("Some QoS settings could not be applied");
    }

    public override TweakResult Revert()
    {
        NetworkLogger.LogAction("Reverting QoS optimizations...");

        // Restore default 20% reservation
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, PschedPath, "NonBestEffortLimit");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, PschedPath, "TimerResolution");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, PschedPath, "MaxOutstandingSends");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, 
            @"SOFTWARE\Policies\Microsoft\Windows\Psched", "NonBestEffortLimit");

        NetworkLogger.Log("Reverting QoS optimizations", NetStatus.Reverted);
        return Success("QoS settings reverted to defaults");
    }
}
