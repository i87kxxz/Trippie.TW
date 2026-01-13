using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.UI;

/// <summary>
/// Enables AutoEndTasks to prevent hung apps from delaying shutdown.
/// </summary>
public class AutoEndTasksTweak : TweakBase
{
    public override string Id => "auto-end-tasks";
    public override string Name => "Auto End Tasks";
    public override string Description => "Automatically end hung applications during shutdown";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string DesktopPath = @"Control Panel\Desktop";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.CurrentUser, DesktopPath, "AutoEndTasks", "0");
        return value?.ToString() == "1";
    }

    public override TweakResult Apply()
    {
        UILogger.LogAction("Enabling Auto End Tasks...");
        bool allSuccess = true;

        // Enable AutoEndTasks
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, DesktopPath, "AutoEndTasks", "1", RegistryValueKind.String))
        {
            var verify = RegistryHelper.GetValue(RegistryHive.CurrentUser, DesktopPath, "AutoEndTasks", null);
            bool verified = verify?.ToString() == "1";
            UILogger.Log("Setting AutoEndTasks -> 1", verified ? UIStatus.Success : UIStatus.Failed);
            if (!verified) allSuccess = false;
        }
        else
        {
            UILogger.Log("Setting AutoEndTasks", UIStatus.Failed);
            allSuccess = false;
        }

        // Reduce WaitToKillAppTimeout (default 20000ms -> 2000ms)
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, DesktopPath, "WaitToKillAppTimeout", "2000", RegistryValueKind.String))
            UILogger.Log("Setting WaitToKillAppTimeout -> 2000ms", UIStatus.Success);

        // Reduce HungAppTimeout (default 5000ms -> 1000ms)
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, DesktopPath, "HungAppTimeout", "1000", RegistryValueKind.String))
            UILogger.Log("Setting HungAppTimeout -> 1000ms", UIStatus.Success);

        return allSuccess 
            ? Success("Auto End Tasks enabled - faster shutdown") 
            : Failure("Could not enable Auto End Tasks");
    }

    public override TweakResult Revert()
    {
        UILogger.LogAction("Disabling Auto End Tasks...");

        RegistryHelper.SetValue(RegistryHive.CurrentUser, DesktopPath, "AutoEndTasks", "0", RegistryValueKind.String);
        UILogger.Log("Setting AutoEndTasks -> 0", UIStatus.Done);

        RegistryHelper.SetValue(RegistryHive.CurrentUser, DesktopPath, "WaitToKillAppTimeout", "20000", RegistryValueKind.String);
        UILogger.Log("Setting WaitToKillAppTimeout -> 20000ms", UIStatus.Done);

        RegistryHelper.SetValue(RegistryHive.CurrentUser, DesktopPath, "HungAppTimeout", "5000", RegistryValueKind.String);
        UILogger.Log("Setting HungAppTimeout -> 5000ms", UIStatus.Done);

        return Success("Auto End Tasks disabled - default behavior restored");
    }
}
