using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;
using System.Diagnostics;

namespace Trippie.TW.Tweaks.Network;

/// <summary>
/// Disables Large Send Offload (LSO) to prevent packet loss in games.
/// </summary>
public class DisableLsoTweak : TweakBase
{
    public override string Id => "disable-lso";
    public override string Name => "Disable Large Send Offload (LSO)";
    public override string Description => "Disable LSO to prevent packet loss and improve game stability";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    public override bool IsApplied()
    {
        // Check via PowerShell if LSO is disabled
        var result = PowerShellHelper.Execute(
            "Get-NetAdapterLso | Where-Object { $_.V1IPv4Enabled -eq $false -and $_.IPv4Enabled -eq $false } | Measure-Object | Select-Object -ExpandProperty Count");
        
        if (result.Success && int.TryParse(result.Output.Trim(), out int count))
            return count > 0;
        
        return false;
    }

    public override TweakResult Apply()
    {
        NetworkLogger.LogAction("Disabling Large Send Offload (LSO)...");

        var adapter = NetworkHelper.GetActiveAdapter();
        if (adapter == null)
        {
            NetworkLogger.Log("Disabling LSO", NetStatus.Failed, "No active adapter found");
            return Failure("No active network adapter found");
        }

        try
        {
            // Disable LSO v1 and v2 for IPv4 and IPv6 using PowerShell
            NetworkLogger.Log($"Disabling LSO on {adapter.Name}", NetStatus.InProgress);

            // Disable LSO v2 IPv4
            var result1 = PowerShellHelper.Execute(
                $"Disable-NetAdapterLso -Name '{adapter.Name}' -IPv4 -ErrorAction SilentlyContinue");
            
            // Disable LSO v2 IPv6
            var result2 = PowerShellHelper.Execute(
                $"Disable-NetAdapterLso -Name '{adapter.Name}' -IPv6 -ErrorAction SilentlyContinue");

            // Also try via netsh for older systems
            ExecuteNetsh($"int tcp set global chimney=disabled");
            ExecuteNetsh($"int ip set global taskoffload=disabled");

            // Disable via registry for specific adapters
            DisableLsoViaRegistry(adapter);

            // Verify
            var verifyResult = PowerShellHelper.Execute(
                $"Get-NetAdapterLso -Name '{adapter.Name}' | Select-Object -ExpandProperty IPv4Enabled");
            
            bool verified = verifyResult.Success && 
                           verifyResult.Output.Trim().Equals("False", StringComparison.OrdinalIgnoreCase);

            if (verified)
            {
                NetworkLogger.Log("Disabling LSO IPv4", NetStatus.Success);
            }
            else
            {
                NetworkLogger.Log("Disabling LSO IPv4", NetStatus.Warning, "May require adapter restart");
            }

            // Verify connectivity
            var pingResult = NetworkHelper.TestConnectivity();
            
            if (!pingResult.Success)
            {
                NetworkLogger.Log("Connectivity issue - reverting", NetStatus.Warning);
                Revert();
                return Failure("Connectivity lost - changes reverted");
            }

            NetworkLogger.Log("Disabling Large Send Offload", NetStatus.Done);
            return Success("LSO disabled - may improve game stability");
        }
        catch (Exception ex)
        {
            NetworkLogger.Log("Disabling LSO", NetStatus.Failed, ex.Message);
            return Failure($"Error: {ex.Message}");
        }
    }

    public override TweakResult Revert()
    {
        NetworkLogger.LogAction("Re-enabling Large Send Offload (LSO)...");

        var adapter = NetworkHelper.GetActiveAdapter();
        if (adapter == null)
        {
            return Failure("No active network adapter found");
        }

        try
        {
            // Re-enable LSO
            PowerShellHelper.Execute($"Enable-NetAdapterLso -Name '{adapter.Name}' -IPv4 -ErrorAction SilentlyContinue");
            PowerShellHelper.Execute($"Enable-NetAdapterLso -Name '{adapter.Name}' -IPv6 -ErrorAction SilentlyContinue");

            // Re-enable via netsh
            ExecuteNetsh("int tcp set global chimney=default");
            ExecuteNetsh("int ip set global taskoffload=default");

            NetworkLogger.Log("Re-enabling Large Send Offload", NetStatus.Reverted);
            return Success("LSO re-enabled (default)");
        }
        catch (Exception ex)
        {
            return Failure($"Error: {ex.Message}");
        }
    }

    private void DisableLsoViaRegistry(NetworkAdapterInfo adapter)
    {
        try
        {
            // Find adapter in registry and disable LSO properties
            var adapterConfigPath = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}";
            
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var classKey = baseKey.OpenSubKey(adapterConfigPath);
            
            if (classKey == null) return;

            foreach (var subKeyName in classKey.GetSubKeyNames())
            {
                if (!int.TryParse(subKeyName, out _)) continue;

                using var adapterKey = classKey.OpenSubKey(subKeyName, true);
                if (adapterKey == null) continue;

                var driverDesc = adapterKey.GetValue("DriverDesc") as string;
                if (driverDesc != null && driverDesc.Contains(adapter.Description, StringComparison.OrdinalIgnoreCase))
                {
                    // Disable LSO settings
                    adapterKey.SetValue("*LsoV2IPv4", 0, RegistryValueKind.DWord);
                    adapterKey.SetValue("*LsoV2IPv6", 0, RegistryValueKind.DWord);
                    adapterKey.SetValue("*LsoV1IPv4", 0, RegistryValueKind.DWord);
                    NetworkLogger.Log("Disabling LSO via Registry", NetStatus.Success);
                    break;
                }
            }
        }
        catch { }
    }

    private void ExecuteNetsh(string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netsh.exe",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            process?.WaitForExit(5000);
        }
        catch { }
    }
}
