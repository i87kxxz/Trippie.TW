using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Security;

/// <summary>
/// Toggles Windows Defender real-time monitoring.
/// </summary>
public class DefenderToggleTweak : TweakBase
{
    public override string Id => "defender-toggle";
    public override string Name => "Disable Windows Defender (Temporary)";
    public override string Description => "Temporarily disable real-time protection (use with caution)";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Advanced;

    private const string DefenderPolicyPath = @"SOFTWARE\Policies\Microsoft\Windows Defender";
    private const string RealTimeProtectionPath = @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, DefenderPolicyPath, "DisableAntiSpyware", 0);
        return Convert.ToInt32(value) == 1;
    }

    public override TweakResult Apply()
    {
        SecurityTweakLogger.LogAction("Disabling Windows Defender...");

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  ⚠ WARNING: Disabling Windows Defender leaves your system vulnerable!");
        Console.WriteLine("  ⚠ Only disable temporarily for specific tasks, then re-enable.");
        Console.WriteLine("  ⚠ Windows may automatically re-enable Defender after some time.");
        Console.ResetColor();
        Console.WriteLine();
        Console.Write("  Type 'UNDERSTOOD' to continue: ");
        var confirm = Console.ReadLine()?.Trim();

        if (confirm != "UNDERSTOOD")
        {
            SecurityTweakLogger.Log("Windows Defender", SecStatus.Skipped, "User cancelled");
            return Failure("Operation cancelled by user");
        }

        Console.WriteLine();

        bool success = true;

        // Disable via Group Policy (may not work on Home editions)
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, DefenderPolicyPath, "DisableAntiSpyware", 1))
            SecurityTweakLogger.Log("Setting DisableAntiSpyware -> 1", SecStatus.Warning);
        else
        {
            SecurityTweakLogger.Log("Setting DisableAntiSpyware", SecStatus.Failed, 
                "May require Tamper Protection to be disabled first");
            success = false;
        }

        // Disable Real-Time Protection
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, RealTimeProtectionPath, "DisableRealtimeMonitoring", 1))
            SecurityTweakLogger.Log("Setting DisableRealtimeMonitoring -> 1", SecStatus.Warning);
        else
            SecurityTweakLogger.Log("Setting DisableRealtimeMonitoring", SecStatus.Failed);

        // Disable behavior monitoring
        RegistryHelper.SetValue(RegistryHive.LocalMachine, RealTimeProtectionPath, "DisableBehaviorMonitoring", 1);
        
        // Disable on-access protection
        RegistryHelper.SetValue(RegistryHive.LocalMachine, RealTimeProtectionPath, "DisableOnAccessProtection", 1);
        
        // Disable scan on download
        RegistryHelper.SetValue(RegistryHive.LocalMachine, RealTimeProtectionPath, "DisableIOAVProtection", 1);

        if (success)
        {
            SecurityTweakLogger.Log("Windows Defender", SecStatus.Warning, 
                "Disabled - REMEMBER TO RE-ENABLE!");
            SecurityTweakLogger.Log("Windows Defender", SecStatus.RequiresReboot);
            return Success("Windows Defender disabled (REBOOT MAY BE REQUIRED)");
        }
        else
        {
            SecurityTweakLogger.Log("Windows Defender", SecStatus.Failed, 
                "Tamper Protection may be blocking changes");
            return Failure("Could not disable Defender. Disable Tamper Protection in Windows Security first.");
        }
    }

    public override TweakResult Revert()
    {
        SecurityTweakLogger.LogAction("Re-enabling Windows Defender...");

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, DefenderPolicyPath, "DisableAntiSpyware");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, RealTimeProtectionPath, "DisableRealtimeMonitoring");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, RealTimeProtectionPath, "DisableBehaviorMonitoring");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, RealTimeProtectionPath, "DisableOnAccessProtection");
        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, RealTimeProtectionPath, "DisableIOAVProtection");

        SecurityTweakLogger.Log("Re-enabling Windows Defender", SecStatus.Protected);
        SecurityTweakLogger.Log("Windows Defender", SecStatus.RequiresReboot);
        return Success("Windows Defender re-enabled (REBOOT MAY BE REQUIRED)");
    }
}
