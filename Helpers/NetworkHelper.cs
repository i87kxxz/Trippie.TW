using Microsoft.Win32;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Trippie.TW.Helpers;

/// <summary>
/// Network utilities for adapter detection and connectivity testing.
/// </summary>
public static class NetworkHelper
{
    private const string InterfacesPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";

    /// <summary>
    /// Gets the GUID of the active network adapter.
    /// </summary>
    public static NetworkAdapterInfo? GetActiveAdapter()
    {
        try
        {
            NetworkLogger.LogAction("Identifying active Network Adapter...");

            var adapters = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                .Where(n => !n.Description.Contains("Virtual", StringComparison.OrdinalIgnoreCase))
                .Where(n => !n.Description.Contains("Hyper-V", StringComparison.OrdinalIgnoreCase))
                .Where(n => !n.Description.Contains("VPN", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Prefer Ethernet over Wi-Fi
            var ethernet = adapters.FirstOrDefault(n => 
                n.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                n.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet);

            var wifi = adapters.FirstOrDefault(n => 
                n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211);

            var selected = ethernet ?? wifi ?? adapters.FirstOrDefault();

            if (selected == null)
            {
                NetworkLogger.Log("Identifying active Network Adapter", NetStatus.GuidNotFound, "No active adapter found");
                return null;
            }

            // Get the adapter GUID from registry
            var guid = GetAdapterGuid(selected);

            var info = new NetworkAdapterInfo
            {
                Name = selected.Name,
                Description = selected.Description,
                Guid = guid ?? selected.Id,
                Type = selected.NetworkInterfaceType.ToString(),
                MacAddress = selected.GetPhysicalAddress().ToString()
            };

            NetworkLogger.Log("Identifying active Network Adapter", NetStatus.GuidFound, 
                $"{info.Description} [{info.Guid}]");

            return info;
        }
        catch (Exception ex)
        {
            NetworkLogger.Log("Identifying active Network Adapter", NetStatus.Failed, ex.Message);
            return null;
        }
    }

    private static string? GetAdapterGuid(NetworkInterface adapter)
    {
        try
        {
            // The adapter ID is usually the GUID
            var id = adapter.Id;
            if (id.StartsWith("{") && id.EndsWith("}"))
                return id.Trim('{', '}');
            return id;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets all interface GUIDs from registry that have an IP address.
    /// </summary>
    public static List<string> GetActiveInterfaceGuids()
    {
        var guids = new List<string>();
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var interfacesKey = baseKey.OpenSubKey(InterfacesPath);
            
            if (interfacesKey == null) return guids;

            foreach (var subKeyName in interfacesKey.GetSubKeyNames())
            {
                using var subKey = interfacesKey.OpenSubKey(subKeyName);
                if (subKey == null) continue;

                // Check if this interface has a valid IP (DhcpIPAddress or IPAddress)
                var dhcpIp = subKey.GetValue("DhcpIPAddress") as string;
                var staticIp = subKey.GetValue("IPAddress") as string[];

                bool hasIp = !string.IsNullOrEmpty(dhcpIp) && dhcpIp != "0.0.0.0";
                if (!hasIp && staticIp != null && staticIp.Length > 0)
                    hasIp = staticIp.Any(ip => !string.IsNullOrEmpty(ip) && ip != "0.0.0.0");

                if (hasIp)
                {
                    guids.Add(subKeyName);
                }
            }
        }
        catch { }
        return guids;
    }

    /// <summary>
    /// Performs a connectivity test by pinging a reliable host.
    /// </summary>
    public static PingResult TestConnectivity(string host = "8.8.8.8", int timeout = 3000)
    {
        try
        {
            NetworkLogger.Log("Performing Connectivity Test", NetStatus.InProgress, host);

            using var ping = new Ping();
            var reply = ping.Send(host, timeout);

            if (reply.Status == IPStatus.Success)
            {
                NetworkLogger.Log("Performing Connectivity Test", NetStatus.ConnectivityOK, 
                    $"Latency: {reply.RoundtripTime}ms");
                return new PingResult(true, reply.RoundtripTime);
            }
            else
            {
                NetworkLogger.Log("Performing Connectivity Test", NetStatus.ConnectivityFailed, 
                    reply.Status.ToString());
                return new PingResult(false, -1);
            }
        }
        catch (Exception ex)
        {
            NetworkLogger.Log("Performing Connectivity Test", NetStatus.ConnectivityFailed, ex.Message);
            return new PingResult(false, -1);
        }
    }

    /// <summary>
    /// Flushes the DNS cache.
    /// </summary>
    public static bool FlushDns()
    {
        try
        {
            NetworkLogger.LogAction("Flushing DNS Cache...");

            var psi = new ProcessStartInfo
            {
                FileName = "ipconfig.exe",
                Arguments = "/flushdns",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            using var process = Process.Start(psi);
            var output = process?.StandardOutput.ReadToEnd() ?? "";
            process?.WaitForExit();

            bool success = process?.ExitCode == 0;
            NetworkLogger.Log("Flushing DNS Cache", success ? NetStatus.Success : NetStatus.Failed);
            return success;
        }
        catch (Exception ex)
        {
            NetworkLogger.Log("Flushing DNS Cache", NetStatus.Failed, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Sets DNS servers for the active adapter using netsh.
    /// </summary>
    public static bool SetDnsServers(string adapterName, string primary, string? secondary = null)
    {
        try
        {
            // Set primary DNS
            var psi = new ProcessStartInfo
            {
                FileName = "netsh.exe",
                Arguments = $"interface ip set dns name=\"{adapterName}\" static {primary}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(psi))
            {
                process?.WaitForExit();
                if (process?.ExitCode != 0) return false;
            }

            // Set secondary DNS if provided
            if (!string.IsNullOrEmpty(secondary))
            {
                psi.Arguments = $"interface ip add dns name=\"{adapterName}\" {secondary} index=2";
                using var process = Process.Start(psi);
                process?.WaitForExit();
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Resets DNS to automatic (DHCP).
    /// </summary>
    public static bool ResetDnsToAutomatic(string adapterName)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netsh.exe",
                Arguments = $"interface ip set dns name=\"{adapterName}\" dhcp",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            process?.WaitForExit();
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}

public class NetworkAdapterInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Guid { get; set; } = "";
    public string Type { get; set; } = "";
    public string MacAddress { get; set; } = "";
}

public record PingResult(bool Success, long Latency);
