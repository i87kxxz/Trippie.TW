using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Privacy;

/// <summary>
/// Disables Windows Telemetry by stopping DiagTrack and dmwappushservice services.
/// </summary>
public class DisableTelemetryTweak : TweakBase
{
    public override string Id => "disable-telemetry";
    public override string Name => "Disable Telemetry";
    public override string Description => "Block Windows from sending usage data to Microsoft (DiagTrack, dmwappushservice)";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private readonly string[] _services = { "DiagTrack", "dmwappushservice" };
    private const string TelemetryRegPath = @"SOFTWARE\Policies\Microsoft\Windows\DataCollection";

    public override bool IsApplied()
    {
        // Check if telemetry is disabled via registry
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, TelemetryRegPath, "AllowTelemetry", 1);
        return Convert.ToInt32(value) == 0;
    }

    public override TweakResult Apply()
    {
        DebugLogger.LogAction("Disabling Windows Telemetry...");
        bool allSuccess = true;

        // Disable telemetry via registry
        DebugLogger.LogAction("Setting telemetry registry keys...");
        
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, TelemetryRegPath, "AllowTelemetry", 0))
            DebugLogger.LogRegistry(TelemetryRegPath, "AllowTelemetry", 0, true);
        else
        {
            DebugLogger.LogRegistry(TelemetryRegPath, "AllowTelemetry", 0, false);
            allSuccess = false;
        }

        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, TelemetryRegPath, "MaxTelemetryAllowed", 0))
            DebugLogger.LogRegistry(TelemetryRegPath, "MaxTelemetryAllowed", 0, true);
        else
            DebugLogger.LogRegistry(TelemetryRegPath, "MaxTelemetryAllowed", 0, false);

        // Stop and disable services
        foreach (var service in _services)
        {
            DebugLogger.LogAction($"Disabling service: {service}...");
            
            if (!ServiceHelper.ServiceExists(service))
            {
                DebugLogger.Log($"Service '{service}' not found", LogStatus.Warning);
                continue;
            }

            bool stopped = ServiceHelper.StopService(service);
            DebugLogger.LogService(service, "Stop", stopped);

            bool disabled = ServiceHelper.SetStartupType(service, "disabled");
            DebugLogger.LogService(service, "Set startup to Disabled", disabled);

            if (!stopped || !disabled) allSuccess = false;
        }

        DebugLogger.Log("Disabling Telemetry", allSuccess ? LogStatus.Done : LogStatus.Failed);
        return allSuccess ? Success("Telemetry disabled successfully") : Failure("Some telemetry settings could not be applied");
    }

    public override TweakResult Revert()
    {
        DebugLogger.LogAction("Re-enabling Windows Telemetry...");
        bool allSuccess = true;

        // Re-enable telemetry via registry (set to Basic = 1)
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, TelemetryRegPath, "AllowTelemetry", 1))
            DebugLogger.LogRegistry(TelemetryRegPath, "AllowTelemetry", 1, true);
        else
        {
            DebugLogger.LogRegistry(TelemetryRegPath, "AllowTelemetry", 1, false);
            allSuccess = false;
        }

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, TelemetryRegPath, "MaxTelemetryAllowed");

        // Re-enable services
        foreach (var service in _services)
        {
            if (!ServiceHelper.ServiceExists(service)) continue;

            bool enabled = ServiceHelper.SetStartupType(service, "auto");
            DebugLogger.LogService(service, "Set startup to Auto", enabled);

            bool started = ServiceHelper.StartService(service);
            DebugLogger.LogService(service, "Start", started);

            if (!enabled) allSuccess = false;
        }

        DebugLogger.Log("Re-enabling Telemetry", allSuccess ? LogStatus.Done : LogStatus.Failed);
        return allSuccess ? Success("Telemetry re-enabled successfully") : Failure("Some telemetry settings could not be reverted");
    }
}
