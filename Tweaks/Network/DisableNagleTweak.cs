using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Network;

/// <summary>
/// Disables Nagle's Algorithm for reduced network latency.
/// </summary>
public class DisableNagleTweak : TweakBase
{
    public override string Id => "disable-nagle";
    public override string Name => "Disable Nagle's Algorithm";
    public override string Description => "Reduce network latency by disabling TCP packet batching";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    private const string InterfacesPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";

    public override bool IsApplied()
    {
        var guids = NetworkHelper.GetActiveInterfaceGuids();
        if (guids.Count == 0) return false;

        foreach (var guid in guids)
        {
            var path = $@"{InterfacesPath}\{guid}";
            var tcpNoDelay = RegistryHelper.GetValue(RegistryHive.LocalMachine, path, "TcpNoDelay", 0);
            if (Convert.ToInt32(tcpNoDelay) == 1)
                return true;
        }
        return false;
    }

    public override TweakResult Apply()
    {
        NetworkLogger.LogAction("Disabling Nagle's Algorithm...");

        var adapter = NetworkHelper.GetActiveAdapter();
        var guids = NetworkHelper.GetActiveInterfaceGuids();

        if (guids.Count == 0)
        {
            NetworkLogger.Log("Disabling Nagle's Algorithm", NetStatus.Failed, "No active interfaces found");
            return Failure("No active network interfaces found");
        }

        bool anySuccess = false;

        foreach (var guid in guids)
        {
            var path = $@"{InterfacesPath}\{guid}";
            
            try
            {
                NetworkLogger.Log($"Applying to interface {guid}", NetStatus.InProgress);

                // Set TcpNoDelay = 1 (disable Nagle)
                bool noDelay = RegistryHelper.SetValue(RegistryHive.LocalMachine, path, "TcpNoDelay", 1);
                
                // Set TcpAckFrequency = 1 (send ACK immediately)
                bool ackFreq = RegistryHelper.SetValue(RegistryHive.LocalMachine, path, "TcpAckFrequency", 1);

                if (noDelay && ackFreq)
                {
                    NetworkLogger.Log($"Interface {guid.Substring(0, 8)}...", NetStatus.Success, 
                        "TcpNoDelay=1, TcpAckFrequency=1");
                    anySuccess = true;
                }
                else
                {
                    NetworkLogger.Log($"Interface {guid.Substring(0, 8)}...", NetStatus.Failed);
                }
            }
            catch (Exception ex)
            {
                NetworkLogger.Log($"Interface {guid.Substring(0, 8)}...", NetStatus.Failed, ex.Message);
            }
        }

        // Verify connectivity
        var pingResult = NetworkHelper.TestConnectivity();
        
        if (!pingResult.Success)
        {
            NetworkLogger.Log("Connectivity lost - reverting changes", NetStatus.Warning);
            Revert();
            return Failure("Connectivity lost after applying tweak - changes reverted");
        }

        NetworkLogger.Log("Disabling Nagle's Algorithm", anySuccess ? NetStatus.Done : NetStatus.Failed);
        
        return anySuccess 
            ? Success($"Nagle's Algorithm disabled on {guids.Count} interface(s)") 
            : Failure("Could not disable Nagle's Algorithm");
    }

    public override TweakResult Revert()
    {
        NetworkLogger.LogAction("Re-enabling Nagle's Algorithm...");

        var guids = NetworkHelper.GetActiveInterfaceGuids();

        foreach (var guid in guids)
        {
            var path = $@"{InterfacesPath}\{guid}";
            
            try
            {
                RegistryHelper.DeleteValue(RegistryHive.LocalMachine, path, "TcpNoDelay");
                RegistryHelper.DeleteValue(RegistryHive.LocalMachine, path, "TcpAckFrequency");
                NetworkLogger.Log($"Interface {guid.Substring(0, 8)}...", NetStatus.Reverted);
            }
            catch { }
        }

        NetworkLogger.Log("Re-enabling Nagle's Algorithm", NetStatus.Done);
        return Success("Nagle's Algorithm re-enabled (default behavior)");
    }
}
