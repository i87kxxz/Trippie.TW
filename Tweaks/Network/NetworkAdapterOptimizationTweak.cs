using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Network;

/// <summary>
/// Optimizes network adapter settings including RSS, ECN, and interrupt moderation.
/// </summary>
public class NetworkAdapterOptimizationTweak : TweakBase
{
    public override string Id => "network-adapter-optimization";
    public override string Name => "Optimize Network Adapter";
    public override string Description => "Enable RSS, ECN, and optimize adapter settings for gaming";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    public override bool IsApplied()
    {
        var result = PowerShellHelper.Execute("(Get-NetTCPSetting -SettingName Internet).EcnCapability");
        return result.Success && result.Output.Trim().Equals("Enabled", StringComparison.OrdinalIgnoreCase);
    }

    public override TweakResult Apply()
    {
        NetworkLogger.LogAction("Optimizing Network Adapter Settings...");
        bool allSuccess = true;

        // Enable Receive Side Scaling (RSS) - distributes network processing across CPU cores
        var rssResult = PowerShellHelper.Execute("netsh int tcp set global rss=enabled");
        if (rssResult.Success)
        {
            NetworkLogger.Log("Setting RSS -> Enabled", NetStatus.Success);
        }
        else
        {
            NetworkLogger.Log("Setting RSS", NetStatus.Failed, rssResult.Output);
            allSuccess = false;
        }

        // Enable ECN (Explicit Congestion Notification) - reduces packet loss
        var ecnResult = PowerShellHelper.Execute("netsh int tcp set global ecncapability=enabled");
        if (ecnResult.Success)
        {
            NetworkLogger.Log("Setting ECN -> Enabled", NetStatus.Success);
        }

        // Enable Direct Cache Access (DCA) if supported
        PowerShellHelper.Execute("netsh int tcp set global dca=enabled");

        // Set congestion provider to CTCP (Compound TCP) for better throughput
        var ctcpResult = PowerShellHelper.Execute("netsh int tcp set global congestionprovider=ctcp");
        if (ctcpResult.Success)
        {
            NetworkLogger.Log("Setting Congestion Provider -> CTCP", NetStatus.Success);
        }

        // Disable TCP chimney offload (can cause issues with some games)
        PowerShellHelper.Execute("netsh int tcp set global chimney=disabled");

        // Enable timestamps for better RTT estimation
        PowerShellHelper.Execute("netsh int tcp set global timestamps=enabled");

        // Set autotuning to normal (allows Windows to optimize window size)
        PowerShellHelper.Execute("netsh int tcp set global autotuninglevel=normal");

        // Disable interrupt moderation for lower latency (advanced)
        // This is adapter-specific, so we try common methods
        var adapters = PowerShellHelper.Execute(
            "Get-NetAdapter | Where-Object {$_.Status -eq 'Up'} | Select-Object -ExpandProperty Name");
        
        if (adapters.Success)
        {
            foreach (var adapter in adapters.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var adapterName = adapter.Trim();
                if (!string.IsNullOrEmpty(adapterName))
                {
                    // Try to disable interrupt moderation
                    PowerShellHelper.Execute(
                        $"Set-NetAdapterAdvancedProperty -Name '{adapterName}' -RegistryKeyword '*InterruptModeration' -RegistryValue 0 -ErrorAction SilentlyContinue");
                }
            }
            NetworkLogger.Log("Interrupt Moderation optimization attempted", NetStatus.Success);
        }

        // Verify connectivity
        var pingResult = NetworkHelper.TestConnectivity();
        if (!pingResult.Success)
        {
            NetworkLogger.Log("Connectivity issue detected - reverting", NetStatus.Warning);
            Revert();
            return Failure("Connectivity lost after applying tweak - changes reverted");
        }

        NetworkLogger.Log("Network Adapter Optimization", allSuccess ? NetStatus.Done : NetStatus.Failed);
        return allSuccess 
            ? Success("Network adapter optimized for gaming") 
            : Failure("Some adapter settings could not be applied");
    }

    public override TweakResult Revert()
    {
        NetworkLogger.LogAction("Reverting Network Adapter optimizations...");

        PowerShellHelper.Execute("netsh int tcp set global rss=enabled");
        PowerShellHelper.Execute("netsh int tcp set global ecncapability=default");
        PowerShellHelper.Execute("netsh int tcp set global congestionprovider=default");
        PowerShellHelper.Execute("netsh int tcp set global chimney=default");
        PowerShellHelper.Execute("netsh int tcp set global timestamps=default");
        PowerShellHelper.Execute("netsh int tcp set global autotuninglevel=normal");

        NetworkLogger.Log("Reverting Network Adapter optimizations", NetStatus.Reverted);
        return Success("Network adapter settings reverted to defaults");
    }
}
