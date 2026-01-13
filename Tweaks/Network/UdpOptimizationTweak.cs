using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Network;

/// <summary>
/// Optimizes UDP settings for gaming. Most online games use UDP for real-time data.
/// </summary>
public class UdpOptimizationTweak : TweakBase
{
    public override string Id => "udp-optimization";
    public override string Name => "Optimize UDP for Gaming";
    public override string Description => "Optimize UDP packet handling for lower latency in online games";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    private const string AfdPath = @"SYSTEM\CurrentControlSet\Services\AFD\Parameters";
    private const string TcpipPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, AfdPath, "FastSendDatagramThreshold", 0);
        return Convert.ToInt32(value) == 1024;
    }

    public override TweakResult Apply()
    {
        NetworkLogger.LogAction("Optimizing UDP for Gaming...");
        bool allSuccess = true;

        // FastSendDatagramThreshold: Threshold for fast path UDP sends
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, AfdPath, "FastSendDatagramThreshold", 1024))
        {
            NetworkLogger.Log("Setting FastSendDatagramThreshold -> 1024", NetStatus.Success);
        }
        else
        {
            allSuccess = false;
        }

        // DefaultReceiveWindow: Increase receive buffer
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, AfdPath, "DefaultReceiveWindow", 65535))
        {
            NetworkLogger.Log("Setting DefaultReceiveWindow -> 65535", NetStatus.Success);
        }

        // DefaultSendWindow: Increase send buffer
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, AfdPath, "DefaultSendWindow", 65535))
        {
            NetworkLogger.Log("Setting DefaultSendWindow -> 65535", NetStatus.Success);
        }

        // FastCopyReceiveThreshold: Optimize receive path
        RegistryHelper.SetValue(RegistryHive.LocalMachine, AfdPath, "FastCopyReceiveThreshold", 1024);

        // DisableTaskOffload: 0 = Enable hardware offloading
        RegistryHelper.SetValue(RegistryHive.LocalMachine, TcpipPath, "DisableTaskOffload", 0);

        // MaxUserPort: Increase available ports for connections
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, TcpipPath, "MaxUserPort", 65534))
        {
            NetworkLogger.Log("Setting MaxUserPort -> 65534", NetStatus.Success);
        }

        // TcpTimedWaitDelay: Reduce TIME_WAIT state duration (30 seconds)
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, TcpipPath, "TcpTimedWaitDelay", 30))
        {
            NetworkLogger.Log("Setting TcpTimedWaitDelay -> 30", NetStatus.Success);
        }

        // Verify connectivity
        var pingResult = NetworkHelper.TestConnectivity();
        if (!pingResult.Success)
        {
            NetworkLogger.Log("Connectivity issue detected - reverting", NetStatus.Warning);
            Revert();
            return Failure("Connectivity lost after applying tweak - changes reverted");
        }

        NetworkLogger.Log("Optimizing UDP for Gaming", allSuccess ? NetStatus.Done : NetStatus.Failed);
        return allSuccess 
            ? Success("UDP optimized for gaming") 
            : Failure("Some UDP settings could not be applied");
    }

    public override TweakResult Revert()
    {
        NetworkLogger.LogAction("Reverting UDP optimizations...");

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, AfdPath, "FastSendDatagramThreshold");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, AfdPath, "DefaultReceiveWindow");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, AfdPath, "DefaultSendWindow");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, AfdPath, "FastCopyReceiveThreshold");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, TcpipPath, "MaxUserPort");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, TcpipPath, "TcpTimedWaitDelay");

        NetworkLogger.Log("Reverting UDP optimizations", NetStatus.Reverted);
        return Success("UDP settings reverted to defaults");
    }
}
