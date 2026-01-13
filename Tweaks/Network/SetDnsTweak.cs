using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Network;

/// <summary>
/// Sets DNS servers to Google or Cloudflare for faster resolution.
/// </summary>
public class SetDnsTweak : TweakBase
{
    public override string Id => "set-dns";
    public override string Name => "Set Fast DNS (Google/Cloudflare)";
    public override string Description => "Use Google (8.8.8.8) or Cloudflare (1.1.1.1) DNS for faster resolution";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    private string? _adapterName;

    public override bool IsApplied()
    {
        // Check if custom DNS is set by testing if we can resolve using the adapter
        var adapter = NetworkHelper.GetActiveAdapter();
        if (adapter == null) return false;

        // This is a simplified check - in reality we'd query the adapter's DNS settings
        return false; // Always show as available since user might want to change provider
    }

    public override TweakResult Apply()
    {
        NetworkLogger.LogAction("Setting Fast DNS Servers...");

        var adapter = NetworkHelper.GetActiveAdapter();
        if (adapter == null)
        {
            NetworkLogger.Log("Setting DNS", NetStatus.Failed, "No active adapter found");
            return Failure("No active network adapter found");
        }

        _adapterName = adapter.Name;

        // Let user choose DNS provider
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Select DNS Provider:");
        Console.ResetColor();
        Console.WriteLine("    [1] Cloudflare (1.1.1.1 / 1.0.0.1) - Fastest, Privacy-focused");
        Console.WriteLine("    [2] Google (8.8.8.8 / 8.8.4.4) - Reliable, Global");
        Console.WriteLine("    [3] Quad9 (9.9.9.9 / 149.112.112.112) - Security-focused");
        Console.WriteLine();
        Console.Write("  Choice (1-3): ");
        
        var choice = Console.ReadKey(true);
        Console.WriteLine(choice.KeyChar);

        string primary, secondary;
        string providerName;

        switch (choice.KeyChar)
        {
            case '1':
                primary = "1.1.1.1";
                secondary = "1.0.0.1";
                providerName = "Cloudflare";
                break;
            case '2':
                primary = "8.8.8.8";
                secondary = "8.8.4.4";
                providerName = "Google";
                break;
            case '3':
                primary = "9.9.9.9";
                secondary = "149.112.112.112";
                providerName = "Quad9";
                break;
            default:
                NetworkLogger.Log("Setting DNS", NetStatus.Skipped, "Invalid choice");
                return Failure("Invalid DNS provider selection");
        }

        Console.WriteLine();
        NetworkLogger.Log($"Setting DNS to {providerName}", NetStatus.InProgress, $"{primary} / {secondary}");

        try
        {
            bool success = NetworkHelper.SetDnsServers(_adapterName, primary, secondary);

            if (success)
            {
                NetworkLogger.Log($"Primary DNS -> {primary}", NetStatus.Success);
                NetworkLogger.Log($"Secondary DNS -> {secondary}", NetStatus.Success);

                // Flush DNS to apply changes
                NetworkHelper.FlushDns();

                // Verify connectivity
                var pingResult = NetworkHelper.TestConnectivity(primary);
                
                if (!pingResult.Success)
                {
                    NetworkLogger.Log("DNS connectivity test failed - reverting", NetStatus.Warning);
                    Revert();
                    return Failure("Could not reach DNS server - changes reverted");
                }

                NetworkLogger.Log($"Setting DNS to {providerName}", NetStatus.Done);
                return Success($"DNS set to {providerName} ({primary})");
            }
            else
            {
                NetworkLogger.Log("Setting DNS", NetStatus.Failed);
                return Failure("Could not set DNS servers");
            }
        }
        catch (Exception ex)
        {
            NetworkLogger.Log("Setting DNS", NetStatus.Failed, ex.Message);
            return Failure($"Error: {ex.Message}");
        }
    }

    public override TweakResult Revert()
    {
        NetworkLogger.LogAction("Reverting DNS to automatic (DHCP)...");

        var adapter = NetworkHelper.GetActiveAdapter();
        var adapterName = _adapterName ?? adapter?.Name;

        if (string.IsNullOrEmpty(adapterName))
        {
            return Failure("Could not identify network adapter");
        }

        try
        {
            bool success = NetworkHelper.ResetDnsToAutomatic(adapterName);

            if (success)
            {
                NetworkHelper.FlushDns();
                NetworkLogger.Log("Reverting DNS to DHCP", NetStatus.Reverted);
                return Success("DNS reverted to automatic (DHCP)");
            }
            else
            {
                return Failure("Could not reset DNS settings");
            }
        }
        catch (Exception ex)
        {
            return Failure($"Error: {ex.Message}");
        }
    }
}
