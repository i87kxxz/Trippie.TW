using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Network;

/// <summary>
/// Disables network throttling during multimedia playback.
/// </summary>
public class NetworkThrottlingTweak : TweakBase
{
    public override string Id => "network-throttling";
    public override string Name => "Disable Network Throttling";
    public override string Description => "Prevent Windows from throttling network during media playback";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string SystemProfilePath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, SystemProfilePath, "NetworkThrottlingIndex", 10);
        // 0xFFFFFFFF = disabled
        return Convert.ToUInt32(value) == 0xFFFFFFFF;
    }

    public override TweakResult Apply()
    {
        NetworkLogger.LogAction("Disabling Network Throttling...");

        try
        {
            // Set NetworkThrottlingIndex to 0xFFFFFFFF (disabled)
            bool success = RegistryHelper.SetValue(
                RegistryHive.LocalMachine, 
                SystemProfilePath, 
                "NetworkThrottlingIndex", 
                unchecked((int)0xFFFFFFFF),
                RegistryValueKind.DWord);

            // Verify
            var verify = RegistryHelper.GetValue(RegistryHive.LocalMachine, SystemProfilePath, "NetworkThrottlingIndex", 0);
            bool verified = Convert.ToUInt32(verify) == 0xFFFFFFFF;

            NetworkLogger.Log("Setting NetworkThrottlingIndex -> 0xFFFFFFFF", 
                verified ? NetStatus.Success : NetStatus.Failed);

            // Also optimize SystemResponsiveness
            if (RegistryHelper.SetValue(RegistryHive.LocalMachine, SystemProfilePath, "SystemResponsiveness", 0))
                NetworkLogger.Log("Setting SystemResponsiveness -> 0", NetStatus.Success);

            return verified 
                ? Success("Network throttling disabled") 
                : Failure("Could not disable network throttling");
        }
        catch (Exception ex)
        {
            NetworkLogger.Log("Disabling Network Throttling", NetStatus.Failed, ex.Message);
            return Failure($"Error: {ex.Message}");
        }
    }

    public override TweakResult Revert()
    {
        NetworkLogger.LogAction("Re-enabling Network Throttling...");

        // Default value is 10
        RegistryHelper.SetValue(RegistryHive.LocalMachine, SystemProfilePath, "NetworkThrottlingIndex", 10);
        NetworkLogger.Log("Setting NetworkThrottlingIndex -> 10 (default)", NetStatus.Reverted);

        RegistryHelper.SetValue(RegistryHive.LocalMachine, SystemProfilePath, "SystemResponsiveness", 20);
        NetworkLogger.Log("Setting SystemResponsiveness -> 20 (default)", NetStatus.Reverted);

        return Success("Network throttling re-enabled (default)");
    }
}
