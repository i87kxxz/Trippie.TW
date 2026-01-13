using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Privacy;

/// <summary>
/// Disables Activity History (Timeline) and cloud sync of activities.
/// </summary>
public class DisableActivityHistoryTweak : TweakBase
{
    public override string Id => "disable-activity-history";
    public override string Name => "Disable Activity History";
    public override string Description => "Stop Windows from collecting and uploading your activity history";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string ActivityRegPath = @"SOFTWARE\Policies\Microsoft\Windows\System";

    public override bool IsApplied()
    {
        var publish = RegistryHelper.GetValue(RegistryHive.LocalMachine, ActivityRegPath, "PublishUserActivities", 1);
        var upload = RegistryHelper.GetValue(RegistryHive.LocalMachine, ActivityRegPath, "UploadUserActivities", 1);
        return Convert.ToInt32(publish) == 0 && Convert.ToInt32(upload) == 0;
    }

    public override TweakResult Apply()
    {
        DebugLogger.LogAction("Disabling Activity History...");
        bool allSuccess = true;

        // Disable publishing user activities
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, ActivityRegPath, "PublishUserActivities", 0))
            DebugLogger.LogRegistry(ActivityRegPath, "PublishUserActivities", 0, true);
        else
        {
            DebugLogger.LogRegistry(ActivityRegPath, "PublishUserActivities", 0, false);
            allSuccess = false;
        }

        // Disable uploading user activities
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, ActivityRegPath, "UploadUserActivities", 0))
            DebugLogger.LogRegistry(ActivityRegPath, "UploadUserActivities", 0, true);
        else
        {
            DebugLogger.LogRegistry(ActivityRegPath, "UploadUserActivities", 0, false);
            allSuccess = false;
        }

        // Disable activity feed
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, ActivityRegPath, "EnableActivityFeed", 0))
            DebugLogger.LogRegistry(ActivityRegPath, "EnableActivityFeed", 0, true);
        else
            DebugLogger.LogRegistry(ActivityRegPath, "EnableActivityFeed", 0, false);

        DebugLogger.Log("Disabling Activity History", allSuccess ? LogStatus.Done : LogStatus.Failed);
        return allSuccess ? Success("Activity History disabled") : Failure("Some activity settings could not be applied");
    }

    public override TweakResult Revert()
    {
        DebugLogger.LogAction("Re-enabling Activity History...");

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, ActivityRegPath, "PublishUserActivities");
        DebugLogger.Log("Removed PublishUserActivities", LogStatus.Success);

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, ActivityRegPath, "UploadUserActivities");
        DebugLogger.Log("Removed UploadUserActivities", LogStatus.Success);

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, ActivityRegPath, "EnableActivityFeed");
        DebugLogger.Log("Removed EnableActivityFeed", LogStatus.Success);

        DebugLogger.Log("Re-enabling Activity History", LogStatus.Done);
        return Success("Activity History re-enabled");
    }
}
