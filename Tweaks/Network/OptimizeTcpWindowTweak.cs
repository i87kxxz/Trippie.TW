using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Network;

/// <summary>
/// Optimizes TCP window size and enables TCP extensions for better throughput.
/// </summary>
public class OptimizeTcpWindowTweak : TweakBase
{
    public override string Id => "optimize-tcp-window";
    public override string Name => "Optimize TCP Window Size";
    public override string Description => "Enable TCP extensions and optimize window size for better throughput";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    private const string TcpipParamsPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
    private const string Tcpip6ParamsPath = @"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters";

    public override bool IsApplied()
    {
        var tcp1323 = RegistryHelper.GetValue(RegistryHive.LocalMachine, TcpipParamsPath, "Tcp1323Opts", 0);
        return Convert.ToInt32(tcp1323) == 3;
    }

    public override TweakResult Apply()
    {
        NetworkLogger.LogAction("Optimizing TCP Window Size...");

        try
        {
            bool allSuccess = true;

            // Enable TCP 1323 Options (Window Scaling + Timestamps)
            // 0 = disabled, 1 = window scaling only, 2 = timestamps only, 3 = both
            if (RegistryHelper.SetValue(RegistryHive.LocalMachine, TcpipParamsPath, "Tcp1323Opts", 3))
                NetworkLogger.Log("Setting Tcp1323Opts -> 3 (Window Scaling + Timestamps)", NetStatus.Success);
            else
            {
                NetworkLogger.Log("Setting Tcp1323Opts", NetStatus.Failed);
                allSuccess = false;
            }

            // Set GlobalMaxTcpWindowSize (64KB default, increase to 64MB for high-speed connections)
            // 65535 = 64KB (default), 16777216 = 16MB (good for gaming)
            if (RegistryHelper.SetValue(RegistryHive.LocalMachine, TcpipParamsPath, "GlobalMaxTcpWindowSize", 16777216))
                NetworkLogger.Log("Setting GlobalMaxTcpWindowSize -> 16MB", NetStatus.Success);
            else
                NetworkLogger.Log("Setting GlobalMaxTcpWindowSize", NetStatus.Failed);

            // Set TcpWindowSize
            if (RegistryHelper.SetValue(RegistryHive.LocalMachine, TcpipParamsPath, "TcpWindowSize", 65535))
                NetworkLogger.Log("Setting TcpWindowSize -> 65535", NetStatus.Success);

            // Enable SackOpts (Selective Acknowledgment)
            if (RegistryHelper.SetValue(RegistryHive.LocalMachine, TcpipParamsPath, "SackOpts", 1))
                NetworkLogger.Log("Setting SackOpts -> 1 (Enabled)", NetStatus.Success);

            // Set DefaultTTL to 64 (standard)
            if (RegistryHelper.SetValue(RegistryHive.LocalMachine, TcpipParamsPath, "DefaultTTL", 64))
                NetworkLogger.Log("Setting DefaultTTL -> 64", NetStatus.Success);

            // Apply same to IPv6
            RegistryHelper.SetValue(RegistryHive.LocalMachine, Tcpip6ParamsPath, "Tcp1323Opts", 3);

            // Verify connectivity
            var pingResult = NetworkHelper.TestConnectivity();
            
            if (!pingResult.Success)
            {
                NetworkLogger.Log("Connectivity issue detected - reverting", NetStatus.Warning);
                Revert();
                return Failure("Connectivity lost after applying tweak - changes reverted");
            }

            NetworkLogger.Log("Optimizing TCP Window Size", allSuccess ? NetStatus.Done : NetStatus.Failed);
            return allSuccess 
                ? Success("TCP window size optimized for better throughput") 
                : Failure("Some TCP settings could not be applied");
        }
        catch (Exception ex)
        {
            NetworkLogger.Log("Optimizing TCP Window Size", NetStatus.Failed, ex.Message);
            Revert();
            return Failure($"Error: {ex.Message}");
        }
    }

    public override TweakResult Revert()
    {
        NetworkLogger.LogAction("Reverting TCP Window optimizations...");

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, TcpipParamsPath, "Tcp1323Opts");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, TcpipParamsPath, "GlobalMaxTcpWindowSize");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, TcpipParamsPath, "TcpWindowSize");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, TcpipParamsPath, "SackOpts");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, TcpipParamsPath, "DefaultTTL");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, Tcpip6ParamsPath, "Tcp1323Opts");

        NetworkLogger.Log("Reverting TCP Window optimizations", NetStatus.Reverted);
        return Success("TCP settings reverted to defaults");
    }
}
