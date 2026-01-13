using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Privacy;

/// <summary>
/// Disables Windows Location Tracking service and sensor settings.
/// </summary>
public class DisableLocationTrackingTweak : TweakBase
{
    public override string Id => "disable-location";
    public override string Name => "Disable Location Tracking";
    public override string Description => "Stop Windows and apps from tracking your location";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string LocationService = "lfsvc";
    private const string SensorRegPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location";
    private const string LocationPolicyPath = @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors";

    public override bool IsApplied()
    {
        var status = RegistryHelper.GetValue(RegistryHive.LocalMachine, LocationPolicyPath, "DisableLocation", 0);
        return Convert.ToInt32(status) == 1;
    }

    public override TweakResult Apply()
    {
        DebugLogger.LogAction("Disabling Location Tracking...");
        bool allSuccess = true;

        // Disable location via policy
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, LocationPolicyPath, "DisableLocation", 1))
            DebugLogger.LogRegistry(LocationPolicyPath, "DisableLocation", 1, true);
        else
        {
            DebugLogger.LogRegistry(LocationPolicyPath, "DisableLocation", 1, false);
            allSuccess = false;
        }

        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, LocationPolicyPath, "DisableLocationScripting", 1))
            DebugLogger.LogRegistry(LocationPolicyPath, "DisableLocationScripting", 1, true);
        else
            DebugLogger.LogRegistry(LocationPolicyPath, "DisableLocationScripting", 1, false);

        // Disable sensor permissions
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, SensorRegPath, "Value", "Deny", RegistryValueKind.String))
            DebugLogger.LogRegistry(SensorRegPath, "Value", "Deny", true);
        else
            DebugLogger.LogRegistry(SensorRegPath, "Value", "Deny", false);

        // Stop and disable location service
        DebugLogger.LogAction($"Disabling service: {LocationService}...");
        
        if (ServiceHelper.ServiceExists(LocationService))
        {
            bool stopped = ServiceHelper.StopService(LocationService);
            DebugLogger.LogService(LocationService, "Stop", stopped);

            bool disabled = ServiceHelper.SetStartupType(LocationService, "disabled");
            DebugLogger.LogService(LocationService, "Set startup to Disabled", disabled);

            if (!disabled) allSuccess = false;
        }
        else
        {
            DebugLogger.Log($"Service '{LocationService}' not found", LogStatus.Warning);
        }

        DebugLogger.Log("Disabling Location Tracking", allSuccess ? LogStatus.Done : LogStatus.Failed);
        return allSuccess ? Success("Location tracking disabled") : Failure("Some location settings could not be applied");
    }

    public override TweakResult Revert()
    {
        DebugLogger.LogAction("Re-enabling Location Tracking...");

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, LocationPolicyPath, "DisableLocation");
        DebugLogger.Log("Removed DisableLocation policy", LogStatus.Success);

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, LocationPolicyPath, "DisableLocationScripting");
        DebugLogger.Log("Removed DisableLocationScripting policy", LogStatus.Success);

        RegistryHelper.SetValue(RegistryHive.LocalMachine, SensorRegPath, "Value", "Allow", RegistryValueKind.String);
        DebugLogger.LogRegistry(SensorRegPath, "Value", "Allow", true);

        if (ServiceHelper.ServiceExists(LocationService))
        {
            ServiceHelper.SetStartupType(LocationService, "manual");
            DebugLogger.LogService(LocationService, "Set startup to Manual", true);
        }

        DebugLogger.Log("Re-enabling Location Tracking", LogStatus.Done);
        return Success("Location tracking re-enabled");
    }
}
