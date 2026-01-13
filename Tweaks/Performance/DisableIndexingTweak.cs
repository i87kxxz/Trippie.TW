using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;
using System.ServiceProcess;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Disables Windows Search Indexing service to reduce disk and CPU usage.
/// </summary>
public class DisableIndexingTweak : TweakBase
{
    public override string Id => "disable-indexing";
    public override string Name => "Disable Indexing";
    public override string Description => "Stop Windows Search indexing service to reduce disk/CPU usage";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    private const string ServiceName = "WSearch";

    public override bool IsApplied()
    {
        var status = ServiceHelper.GetStatus(ServiceName);
        return status == ServiceControllerStatus.Stopped;
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Disabling Windows Search Indexing...");

        if (!ServiceHelper.ServiceExists(ServiceName))
        {
            PerformanceLogger.Log("Disable_Indexing", "Service", PerfStatus.Failed, "WSearch service not found");
            return Failure("Windows Search service not found");
        }

        // Stop the service
        PerformanceLogger.Log("Disable_Indexing", "Service", PerfStatus.InProgress, "Stopping WSearch service");
        bool stopped = ServiceHelper.StopService(ServiceName);
        
        // Verify stopped
        var status = ServiceHelper.GetStatus(ServiceName);
        bool verifyStopped = status == ServiceControllerStatus.Stopped;
        
        PerformanceLogger.Log("Disable_Indexing_Stop", "Service", 
            verifyStopped ? PerfStatus.VerifiedSuccess : PerfStatus.VerifiedFailed,
            $"Service status: {status}");

        // Disable startup
        bool disabled = ServiceHelper.SetStartupType(ServiceName, "disabled");
        PerformanceLogger.Log("Disable_Indexing_Startup", "Service", 
            disabled ? PerfStatus.VerifiedSuccess : PerfStatus.VerifiedFailed,
            "Set startup to Disabled");

        return verifyStopped && disabled 
            ? Success("Windows Search Indexing disabled") 
            : Failure("Could not fully disable indexing service");
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Re-enabling Windows Search Indexing...");

        if (!ServiceHelper.ServiceExists(ServiceName))
        {
            PerformanceLogger.Log("Enable_Indexing", "Service", PerfStatus.Failed, "WSearch service not found");
            return Failure("Windows Search service not found");
        }

        // Set to automatic
        bool enabled = ServiceHelper.SetStartupType(ServiceName, "delayed-auto");
        PerformanceLogger.Log("Enable_Indexing_Startup", "Service", 
            enabled ? PerfStatus.Reverted : PerfStatus.Failed,
            "Set startup to Delayed-Auto");

        // Start the service
        bool started = ServiceHelper.StartService(ServiceName);
        var status = ServiceHelper.GetStatus(ServiceName);
        
        PerformanceLogger.Log("Enable_Indexing_Start", "Service", 
            status == ServiceControllerStatus.Running ? PerfStatus.Reverted : PerfStatus.Failed,
            $"Service status: {status}");

        return enabled 
            ? Success("Windows Search Indexing re-enabled") 
            : Failure("Could not re-enable indexing service");
    }
}
