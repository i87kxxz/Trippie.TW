using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Security;

/// <summary>
/// Clears the pagefile on shutdown to wipe sensitive data.
/// </summary>
public class ClearPagefileTweak : TweakBase
{
    public override string Id => "clear-pagefile";
    public override string Name => "Clear Pagefile on Shutdown";
    public override string Description => "Wipe sensitive data from pagefile when shutting down";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string MemMgmtPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, MemMgmtPath, "ClearPageFileAtShutdown", 0);
        return Convert.ToInt32(value) == 1;
    }

    public override TweakResult Apply()
    {
        SecurityTweakLogger.LogAction("Enabling Clear Pagefile on Shutdown...");

        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, MemMgmtPath, "ClearPageFileAtShutdown", 1))
        {
            SecurityTweakLogger.Log("Setting ClearPageFileAtShutdown -> 1", SecStatus.Protected);
            SecurityTweakLogger.Log("Clear Pagefile on Shutdown", SecStatus.Done, 
                "Note: May slightly increase shutdown time");
            return Success("Pagefile will be cleared on shutdown");
        }
        else
        {
            SecurityTweakLogger.Log("Setting ClearPageFileAtShutdown", SecStatus.Failed);
            return Failure("Could not enable pagefile clearing");
        }
    }

    public override TweakResult Revert()
    {
        SecurityTweakLogger.LogAction("Disabling Clear Pagefile on Shutdown...");

        RegistryHelper.SetValue(RegistryHive.LocalMachine, MemMgmtPath, "ClearPageFileAtShutdown", 0);
        SecurityTweakLogger.Log("Disabling Clear Pagefile on Shutdown", SecStatus.Reverted);
        return Success("Pagefile clearing disabled");
    }
}
