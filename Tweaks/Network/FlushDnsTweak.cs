using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Network;

/// <summary>
/// Flushes the DNS resolver cache.
/// </summary>
public class FlushDnsTweak : TweakBase
{
    public override string Id => "flush-dns";
    public override string Name => "Flush DNS Cache";
    public override string Description => "Clear the DNS resolver cache to fix connectivity issues";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    public override bool IsApplied()
    {
        // This is a one-time action, always show as "OFF" (available to run)
        return false;
    }

    public override TweakResult Apply()
    {
        NetworkLogger.LogAction("Flushing DNS Cache...");

        try
        {
            bool success = NetworkHelper.FlushDns();

            // Also reset Winsock and IP stack for thorough cleanup
            NetworkLogger.LogAction("Resetting DNS client service...");
            var resetResult = PowerShellHelper.Execute("Clear-DnsClientCache -ErrorAction SilentlyContinue");
            
            // Verify connectivity
            var pingResult = NetworkHelper.TestConnectivity();

            if (success)
            {
                NetworkLogger.Log("Flush DNS Cache", NetStatus.Done);
                return Success("DNS cache flushed successfully");
            }
            else
            {
                return Failure("Could not flush DNS cache");
            }
        }
        catch (Exception ex)
        {
            NetworkLogger.Log("Flush DNS Cache", NetStatus.Failed, ex.Message);
            return Failure($"Error: {ex.Message}");
        }
    }

    public override TweakResult Revert()
    {
        // Nothing to revert - this is a one-time action
        NetworkLogger.Log("Flush DNS Cache", NetStatus.Skipped, "Nothing to revert");
        return Success("DNS cache flush is a one-time action");
    }
}
