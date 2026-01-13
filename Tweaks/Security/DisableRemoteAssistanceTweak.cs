using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Security;

/// <summary>
/// Disables Remote Assistance for improved security.
/// </summary>
public class DisableRemoteAssistanceTweak : TweakBase
{
    public override string Id => "disable-remote-assistance";
    public override string Name => "Disable Remote Assistance";
    public override string Description => "Prevent remote assistance connections for better security";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string RemoteAssistPath = @"SYSTEM\CurrentControlSet\Control\Remote Assistance";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, RemoteAssistPath, "fAllowToGetHelp", 1);
        return Convert.ToInt32(value) == 0;
    }

    public override TweakResult Apply()
    {
        SecurityTweakLogger.LogAction("Disabling Remote Assistance...");

        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, RemoteAssistPath, "fAllowToGetHelp", 0))
        {
            SecurityTweakLogger.Log("Setting fAllowToGetHelp -> 0", SecStatus.Protected);
            
            // Also disable unsolicited remote assistance
            RegistryHelper.SetValue(RegistryHive.LocalMachine, RemoteAssistPath, "fAllowFullControl", 0);
            SecurityTweakLogger.Log("Setting fAllowFullControl -> 0", SecStatus.Protected);

            return Success("Remote Assistance disabled");
        }
        else
        {
            SecurityTweakLogger.Log("Disabling Remote Assistance", SecStatus.Failed);
            return Failure("Could not disable Remote Assistance");
        }
    }

    public override TweakResult Revert()
    {
        SecurityTweakLogger.LogAction("Re-enabling Remote Assistance...");

        RegistryHelper.SetValue(RegistryHive.LocalMachine, RemoteAssistPath, "fAllowToGetHelp", 1);
        RegistryHelper.SetValue(RegistryHive.LocalMachine, RemoteAssistPath, "fAllowFullControl", 0);

        SecurityTweakLogger.Log("Re-enabling Remote Assistance", SecStatus.Reverted);
        return Success("Remote Assistance re-enabled");
    }
}
