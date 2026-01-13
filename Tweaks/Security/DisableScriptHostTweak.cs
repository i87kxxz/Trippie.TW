using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Security;

/// <summary>
/// Disables Windows Script Host to block malicious scripts.
/// </summary>
public class DisableScriptHostTweak : TweakBase
{
    public override string Id => "disable-script-host";
    public override string Name => "Disable Windows Script Host";
    public override string Description => "Block malicious .vbs and .js scripts from running";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    private const string WshPath = @"SOFTWARE\Microsoft\Windows Script Host\Settings";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, WshPath, "Enabled", 1);
        return Convert.ToInt32(value) == 0;
    }

    public override TweakResult Apply()
    {
        SecurityTweakLogger.LogAction("Disabling Windows Script Host...");

        bool success = true;

        // Disable for all users (HKLM)
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, WshPath, "Enabled", 0))
            SecurityTweakLogger.Log("Disabling Windows Script Host (HKLM)", SecStatus.Protected);
        else
        {
            SecurityTweakLogger.Log("Disabling Windows Script Host (HKLM)", SecStatus.Failed);
            success = false;
        }

        // Also disable for current user
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, WshPath, "Enabled", 0))
            SecurityTweakLogger.Log("Disabling Windows Script Host (HKCU)", SecStatus.Protected);

        return success 
            ? Success("Windows Script Host disabled - .vbs/.js scripts blocked") 
            : Failure("Could not disable Windows Script Host");
    }

    public override TweakResult Revert()
    {
        SecurityTweakLogger.LogAction("Re-enabling Windows Script Host...");

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, WshPath, "Enabled");
        RegistryHelper.DeleteValue(RegistryHive.CurrentUser, WshPath, "Enabled");

        SecurityTweakLogger.Log("Re-enabling Windows Script Host", SecStatus.Reverted);
        return Success("Windows Script Host re-enabled");
    }
}
