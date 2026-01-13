using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Privacy;

/// <summary>
/// Disables Windows Feedback requests by setting Period and NumberOfSIUFInPeriod to 0.
/// </summary>
public class DisableFeedbackTweak : TweakBase
{
    public override string Id => "disable-feedback";
    public override string Name => "Disable Feedback Requests";
    public override string Description => "Stop Windows from asking for feedback";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string SiufRegPath = @"SOFTWARE\Policies\Microsoft\Windows\DataCollection";
    private const string SiufUserPath = @"SOFTWARE\Microsoft\Siuf\Rules";

    public override bool IsApplied()
    {
        var period = RegistryHelper.GetValue(RegistryHive.CurrentUser, SiufUserPath, "NumberOfSIUFInPeriod", -1);
        return Convert.ToInt32(period) == 0;
    }

    public override TweakResult Apply()
    {
        DebugLogger.LogAction("Disabling Feedback Requests...");
        bool allSuccess = true;

        // Disable via policy
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, SiufRegPath, "DoNotShowFeedbackNotifications", 1))
            DebugLogger.LogRegistry(SiufRegPath, "DoNotShowFeedbackNotifications", 1, true);
        else
        {
            DebugLogger.LogRegistry(SiufRegPath, "DoNotShowFeedbackNotifications", 1, false);
            allSuccess = false;
        }

        // Disable via user settings
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, SiufUserPath, "NumberOfSIUFInPeriod", 0))
            DebugLogger.LogRegistry(SiufUserPath, "NumberOfSIUFInPeriod", 0, true);
        else
        {
            DebugLogger.LogRegistry(SiufUserPath, "NumberOfSIUFInPeriod", 0, false);
            allSuccess = false;
        }

        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, SiufUserPath, "PeriodInNanoSeconds", 0, RegistryValueKind.QWord))
            DebugLogger.LogRegistry(SiufUserPath, "PeriodInNanoSeconds", 0, true);
        else
            DebugLogger.LogRegistry(SiufUserPath, "PeriodInNanoSeconds", 0, false);

        DebugLogger.Log("Disabling Feedback Requests", allSuccess ? LogStatus.Done : LogStatus.Failed);
        return allSuccess ? Success("Feedback requests disabled") : Failure("Some feedback settings could not be applied");
    }

    public override TweakResult Revert()
    {
        DebugLogger.LogAction("Re-enabling Feedback Requests...");

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, SiufRegPath, "DoNotShowFeedbackNotifications");
        DebugLogger.Log("Removed DoNotShowFeedbackNotifications policy", LogStatus.Success);

        RegistryHelper.DeleteValue(RegistryHive.CurrentUser, SiufUserPath, "NumberOfSIUFInPeriod");
        DebugLogger.Log("Removed NumberOfSIUFInPeriod", LogStatus.Success);

        RegistryHelper.DeleteValue(RegistryHive.CurrentUser, SiufUserPath, "PeriodInNanoSeconds");
        DebugLogger.Log("Removed PeriodInNanoSeconds", LogStatus.Success);

        DebugLogger.Log("Re-enabling Feedback Requests", LogStatus.Done);
        return Success("Feedback requests re-enabled");
    }
}
