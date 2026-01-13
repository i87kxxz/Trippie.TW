using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Network;

/// <summary>
/// Disables NetBIOS over TCP/IP to reduce network overhead.
/// </summary>
public class DisableNetBiosTweak : TweakBase
{
    public override string Id => "disable-netbios";
    public override string Name => "Disable NetBIOS";
    public override string Description => "Disable NetBIOS over TCP/IP to reduce network overhead";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    private const string InterfacesPath = @"SYSTEM\CurrentControlSet\Services\NetBT\Parameters\Interfaces";

    public override bool IsApplied()
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var interfacesKey = baseKey.OpenSubKey(InterfacesPath);
            
            if (interfacesKey == null) return false;

            foreach (var subKeyName in interfacesKey.GetSubKeyNames())
            {
                using var subKey = interfacesKey.OpenSubKey(subKeyName);
                var value = subKey?.GetValue("NetbiosOptions");
                if (value != null && Convert.ToInt32(value) == 2)
                    return true;
            }
        }
        catch { }
        return false;
    }

    public override TweakResult Apply()
    {
        NetworkLogger.LogAction("Disabling NetBIOS over TCP/IP...");

        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var interfacesKey = baseKey.OpenSubKey(InterfacesPath);
            
            if (interfacesKey == null)
            {
                NetworkLogger.Log("Disabling NetBIOS", NetStatus.Failed, "NetBT interfaces not found");
                return Failure("NetBT interfaces not found");
            }

            int modified = 0;
            foreach (var subKeyName in interfacesKey.GetSubKeyNames())
            {
                try
                {
                    var path = $@"{InterfacesPath}\{subKeyName}";
                    
                    // NetbiosOptions: 0 = Default, 1 = Enable, 2 = Disable
                    if (RegistryHelper.SetValue(RegistryHive.LocalMachine, path, "NetbiosOptions", 2))
                    {
                        NetworkLogger.Log($"Interface {subKeyName.Substring(0, Math.Min(15, subKeyName.Length))}...", 
                            NetStatus.Success, "NetbiosOptions -> 2");
                        modified++;
                    }
                }
                catch { }
            }

            // Verify connectivity
            var pingResult = NetworkHelper.TestConnectivity();
            
            if (!pingResult.Success)
            {
                NetworkLogger.Log("Connectivity issue - reverting", NetStatus.Warning);
                Revert();
                return Failure("Connectivity lost - changes reverted");
            }

            NetworkLogger.Log("Disabling NetBIOS", modified > 0 ? NetStatus.Done : NetStatus.Failed, 
                $"{modified} interface(s) modified");

            return modified > 0 
                ? Success($"NetBIOS disabled on {modified} interface(s)") 
                : Failure("Could not disable NetBIOS");
        }
        catch (Exception ex)
        {
            NetworkLogger.Log("Disabling NetBIOS", NetStatus.Failed, ex.Message);
            return Failure($"Error: {ex.Message}");
        }
    }

    public override TweakResult Revert()
    {
        NetworkLogger.LogAction("Re-enabling NetBIOS over TCP/IP...");

        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var interfacesKey = baseKey.OpenSubKey(InterfacesPath);
            
            if (interfacesKey == null) return Success("Nothing to revert");

            foreach (var subKeyName in interfacesKey.GetSubKeyNames())
            {
                var path = $@"{InterfacesPath}\{subKeyName}";
                // Set to 0 (Default - use DHCP setting)
                RegistryHelper.SetValue(RegistryHive.LocalMachine, path, "NetbiosOptions", 0);
            }

            NetworkLogger.Log("Re-enabling NetBIOS", NetStatus.Reverted);
            return Success("NetBIOS settings reverted to default");
        }
        catch (Exception ex)
        {
            return Failure($"Error: {ex.Message}");
        }
    }
}
