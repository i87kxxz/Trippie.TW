using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Optimizes Win32PrioritySeparation for better gaming/foreground responsiveness.
/// </summary>
public class SystemResponsivenessTweak : TweakBase
{
    public override string Id => "system-responsiveness";
    public override string Name => "Increase System Responsiveness";
    public override string Description => "Optimize CPU scheduling for better foreground application performance";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    private const string PriorityControlPath = @"SYSTEM\CurrentControlSet\Control\PriorityControl";
    private const int OptimalValue = 38; // 0x26 - Short quantum, variable, high foreground boost

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, PriorityControlPath, "Win32PrioritySeparation", 2);
        return Convert.ToInt32(value) == OptimalValue;
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Optimizing System Responsiveness...");

        // Get current value for logging
        var currentValue = RegistryHelper.GetValue(RegistryHive.LocalMachine, PriorityControlPath, "Win32PrioritySeparation", 2);
        PerformanceLogger.Log("System_Responsiveness", "Registry", PerfStatus.InProgress, 
            $"Current value: {currentValue} (0x{Convert.ToInt32(currentValue):X2})");

        // Set optimal value: 38 (0x26)
        // Bits: 00 10 01 10
        // - Short, variable quantum
        // - High foreground boost
        bool verified = PerformanceLogger.SetAndVerify(
            RegistryHive.LocalMachine, 
            PriorityControlPath, 
            "Win32PrioritySeparation", 
            OptimalValue);

        if (verified)
        {
            PerformanceLogger.Log("System_Responsiveness", "Registry", PerfStatus.VerifiedSuccess, 
                $"Win32PrioritySeparation -> {OptimalValue} (0x{OptimalValue:X2})");
            return Success("System responsiveness optimized for gaming");
        }
        else
        {
            PerformanceLogger.Log("System_Responsiveness", "Registry", PerfStatus.VerifiedFailed);
            return Failure("Could not optimize system responsiveness");
        }
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Reverting System Responsiveness to default...");

        // Default Windows value is 2 (0x02)
        const int defaultValue = 2;
        
        bool verified = PerformanceLogger.SetAndVerify(
            RegistryHive.LocalMachine, 
            PriorityControlPath, 
            "Win32PrioritySeparation", 
            defaultValue);

        PerformanceLogger.Log("System_Responsiveness", "Registry", 
            verified ? PerfStatus.Reverted : PerfStatus.Failed,
            $"Win32PrioritySeparation -> {defaultValue}");

        return verified 
            ? Success("System responsiveness reverted to default") 
            : Failure("Could not revert system responsiveness");
    }
}
