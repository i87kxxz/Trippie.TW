using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Privacy;

/// <summary>
/// Disables Wi-Fi Sense (auto-connect to open hotspots and shared networks).
/// </summary>
public class DisableWiFiSenseTweak : TweakBase
{
    public override string Id => "disable-wifi-sense";
    public override string Name => "Disable Wi-Fi Sense";
    public override string Description => "Stop Windows from auto-connecting to suggested open hotspots";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string WiFiSensePath = @"SOFTWARE\Microsoft\WcmSvc\wifinetworkmanager\config";
    private const string WiFiPolicyPath = @"SOFTWARE\Microsoft\PolicyManager\default\WiFi";
    private const string HotspotPath = @"SOFTWARE\Microsoft\PolicyManager\default\WiFi\AllowAutoConnectToWiFiSenseHotspots";

    public override bool IsApplied()
    {
        var autoConnect = RegistryHelper.GetValue(RegistryHive.LocalMachine, WiFiSensePath, "AutoConnectAllowedOEM", 1);
        return Convert.ToInt32(autoConnect) == 0;
    }

    public override TweakResult Apply()
    {
        DebugLogger.LogAction("Disabling Wi-Fi Sense...");
        bool allSuccess = true;

        // Disable auto-connect to open hotspots
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, WiFiSensePath, "AutoConnectAllowedOEM", 0))
            DebugLogger.LogRegistry(WiFiSensePath, "AutoConnectAllowedOEM", 0, true);
        else
        {
            DebugLogger.LogRegistry(WiFiSensePath, "AutoConnectAllowedOEM", 0, false);
            allSuccess = false;
        }

        // Disable hotspot reporting
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, HotspotPath, "value", 0))
            DebugLogger.LogRegistry(HotspotPath, "value", 0, true);
        else
            DebugLogger.LogRegistry(HotspotPath, "value", 0, false);

        // Disable WiFi Sense sharing
        const string sharePath = @"SOFTWARE\Microsoft\PolicyManager\default\WiFi\AllowWiFiHotSpotReporting";
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, sharePath, "value", 0))
            DebugLogger.LogRegistry(sharePath, "value", 0, true);
        else
            DebugLogger.LogRegistry(sharePath, "value", 0, false);

        // Additional: Disable connect to suggested open hotspots
        const string wcmPath = @"SOFTWARE\Microsoft\WcmSvc\wifinetworkmanager\features";
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, wcmPath, "WiFiSenseCredShared", 0))
            DebugLogger.LogRegistry(wcmPath, "WiFiSenseCredShared", 0, true);
        else
            DebugLogger.LogRegistry(wcmPath, "WiFiSenseCredShared", 0, false);

        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, wcmPath, "WiFiSenseOpen", 0))
            DebugLogger.LogRegistry(wcmPath, "WiFiSenseOpen", 0, true);
        else
            DebugLogger.LogRegistry(wcmPath, "WiFiSenseOpen", 0, false);

        DebugLogger.Log("Disabling Wi-Fi Sense", allSuccess ? LogStatus.Done : LogStatus.Failed);
        return allSuccess ? Success("Wi-Fi Sense disabled") : Failure("Some Wi-Fi Sense settings could not be applied");
    }

    public override TweakResult Revert()
    {
        DebugLogger.LogAction("Re-enabling Wi-Fi Sense...");

        RegistryHelper.SetValue(RegistryHive.LocalMachine, WiFiSensePath, "AutoConnectAllowedOEM", 1);
        DebugLogger.LogRegistry(WiFiSensePath, "AutoConnectAllowedOEM", 1, true);

        RegistryHelper.SetValue(RegistryHive.LocalMachine, HotspotPath, "value", 1);
        DebugLogger.LogRegistry(HotspotPath, "value", 1, true);

        const string sharePath = @"SOFTWARE\Microsoft\PolicyManager\default\WiFi\AllowWiFiHotSpotReporting";
        RegistryHelper.SetValue(RegistryHive.LocalMachine, sharePath, "value", 1);
        DebugLogger.LogRegistry(sharePath, "value", 1, true);

        const string wcmPath = @"SOFTWARE\Microsoft\WcmSvc\wifinetworkmanager\features";
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, wcmPath, "WiFiSenseCredShared");
        DebugLogger.Log("Removed WiFiSenseCredShared", LogStatus.Success);

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, wcmPath, "WiFiSenseOpen");
        DebugLogger.Log("Removed WiFiSenseOpen", LogStatus.Success);

        DebugLogger.Log("Re-enabling Wi-Fi Sense", LogStatus.Done);
        return Success("Wi-Fi Sense re-enabled");
    }
}
