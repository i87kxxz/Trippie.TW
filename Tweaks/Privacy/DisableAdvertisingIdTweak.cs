using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Privacy;

/// <summary>
/// Disables the Windows Advertising ID used for personalized ads.
/// </summary>
public class DisableAdvertisingIdTweak : TweakBase
{
    public override string Id => "disable-advertising-id";
    public override string Name => "Disable Advertising ID";
    public override string Description => "Prevent apps from using your advertising ID for personalized ads";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string AdvInfoRegPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo";
    private const string AdvPolicyPath = @"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, AdvInfoRegPath, "Enabled", 1);
        var policyValue = RegistryHelper.GetValue(RegistryHive.LocalMachine, AdvPolicyPath, "DisabledByGroupPolicy", 0);
        return Convert.ToInt32(value) == 0 || Convert.ToInt32(policyValue) == 1;
    }

    public override TweakResult Apply()
    {
        DebugLogger.LogAction("Disabling Advertising ID...");
        bool allSuccess = true;

        // Disable via main registry key
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, AdvInfoRegPath, "Enabled", 0))
            DebugLogger.LogRegistry(AdvInfoRegPath, "Enabled", 0, true);
        else
        {
            DebugLogger.LogRegistry(AdvInfoRegPath, "Enabled", 0, false);
            allSuccess = false;
        }

        // Disable via policy
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, AdvPolicyPath, "DisabledByGroupPolicy", 1))
            DebugLogger.LogRegistry(AdvPolicyPath, "DisabledByGroupPolicy", 1, true);
        else
        {
            DebugLogger.LogRegistry(AdvPolicyPath, "DisabledByGroupPolicy", 1, false);
            allSuccess = false;
        }

        // Also disable for current user
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, AdvInfoRegPath, "Enabled", 0))
            DebugLogger.LogRegistry($"HKCU\\{AdvInfoRegPath}", "Enabled", 0, true);
        else
            DebugLogger.LogRegistry($"HKCU\\{AdvInfoRegPath}", "Enabled", 0, false);

        DebugLogger.Log("Disabling Advertising ID", allSuccess ? LogStatus.Done : LogStatus.Failed);
        return allSuccess ? Success("Advertising ID disabled") : Failure("Some advertising settings could not be applied");
    }

    public override TweakResult Revert()
    {
        DebugLogger.LogAction("Re-enabling Advertising ID...");

        RegistryHelper.SetValue(RegistryHive.LocalMachine, AdvInfoRegPath, "Enabled", 1);
        DebugLogger.LogRegistry(AdvInfoRegPath, "Enabled", 1, true);

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, AdvPolicyPath, "DisabledByGroupPolicy");
        DebugLogger.Log("Removed DisabledByGroupPolicy policy", LogStatus.Success);

        RegistryHelper.SetValue(RegistryHive.CurrentUser, AdvInfoRegPath, "Enabled", 1);
        DebugLogger.LogRegistry($"HKCU\\{AdvInfoRegPath}", "Enabled", 1, true);

        DebugLogger.Log("Re-enabling Advertising ID", LogStatus.Done);
        return Success("Advertising ID re-enabled");
    }
}
