using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Enables Hardware-Accelerated GPU Scheduling (HAGS) for improved GPU performance.
/// </summary>
public class EnableHAGSTweak : TweakBase
{
    public override string Id => "enable-hags";
    public override string Name => "Enable HAGS";
    public override string Description => "Enable Hardware-Accelerated GPU Scheduling for better GPU performance";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    private const string GraphicsDriversPath = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, GraphicsDriversPath, "HwSchMode", 1);
        return Convert.ToInt32(value) == 2; // 2 = Enabled
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Enabling Hardware-Accelerated GPU Scheduling...");

        // Check if GPU supports HAGS
        var supported = RegistryHelper.GetValue(RegistryHive.LocalMachine, GraphicsDriversPath, "HwSchSupported", 0);
        if (Convert.ToInt32(supported) == 0)
        {
            PerformanceLogger.Log("Enable_HAGS", "Registry", PerfStatus.Failed, "GPU does not support HAGS");
            return Failure("Your GPU does not support Hardware-Accelerated GPU Scheduling");
        }

        // Enable HAGS (2 = On, 1 = Off)
        bool verified = PerformanceLogger.SetAndVerify(
            RegistryHive.LocalMachine, 
            GraphicsDriversPath, 
            "HwSchMode", 
            2);

        if (verified)
        {
            PerformanceLogger.Log("Enable_HAGS", "Registry", PerfStatus.VerifiedSuccess, "HwSchMode -> 2");
            PerformanceLogger.Log("Enable_HAGS", "Registry", PerfStatus.RebootRequired);
            return Success("HAGS enabled (reboot required to take effect)");
        }
        else
        {
            PerformanceLogger.Log("Enable_HAGS", "Registry", PerfStatus.VerifiedFailed);
            return Failure("Could not enable HAGS");
        }
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Disabling Hardware-Accelerated GPU Scheduling...");

        bool verified = PerformanceLogger.SetAndVerify(
            RegistryHive.LocalMachine, 
            GraphicsDriversPath, 
            "HwSchMode", 
            1); // 1 = Off

        PerformanceLogger.Log("Enable_HAGS", "Registry", 
            verified ? PerfStatus.Reverted : PerfStatus.Failed, "HwSchMode -> 1");

        if (verified)
        {
            PerformanceLogger.Log("Enable_HAGS", "Registry", PerfStatus.RebootRequired);
        }

        return verified 
            ? Success("HAGS disabled (reboot required)") 
            : Failure("Could not disable HAGS");
    }
}
