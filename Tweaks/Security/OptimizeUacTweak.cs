using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Security;

/// <summary>
/// Optimizes UAC to reduce dimming lag while maintaining security.
/// </summary>
public class OptimizeUacTweak : TweakBase
{
    public override string Id => "optimize-uac";
    public override string Name => "Enable UAC (Optimized)";
    public override string Description => "Keep UAC security while reducing the dimming effect lag";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string UacPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";

    public override bool IsApplied()
    {
        var consent = RegistryHelper.GetValue(RegistryHive.LocalMachine, UacPath, "ConsentPromptBehaviorAdmin", 5);
        var dimming = RegistryHelper.GetValue(RegistryHive.LocalMachine, UacPath, "PromptOnSecureDesktop", 1);
        return Convert.ToInt32(consent) == 5 && Convert.ToInt32(dimming) == 0;
    }

    public override TweakResult Apply()
    {
        SecurityTweakLogger.LogAction("Optimizing UAC settings...");

        bool success = true;

        // ConsentPromptBehaviorAdmin = 5 (Prompt for consent for non-Windows binaries)
        // This is the default secure setting
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, UacPath, "ConsentPromptBehaviorAdmin", 5))
            SecurityTweakLogger.Log("Setting ConsentPromptBehaviorAdmin -> 5", SecStatus.Protected);
        else
        {
            SecurityTweakLogger.Log("Setting ConsentPromptBehaviorAdmin", SecStatus.Failed);
            success = false;
        }

        // Disable secure desktop dimming (reduces lag)
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, UacPath, "PromptOnSecureDesktop", 0))
            SecurityTweakLogger.Log("Setting PromptOnSecureDesktop -> 0 (no dimming)", SecStatus.Done);
        else
            SecurityTweakLogger.Log("Setting PromptOnSecureDesktop", SecStatus.Failed);

        // Ensure UAC is enabled
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, UacPath, "EnableLUA", 1))
            SecurityTweakLogger.Log("Setting EnableLUA -> 1 (UAC enabled)", SecStatus.Protected);

        return success 
            ? Success("UAC optimized - security maintained, dimming disabled") 
            : Failure("Could not optimize UAC settings");
    }

    public override TweakResult Revert()
    {
        SecurityTweakLogger.LogAction("Reverting UAC to defaults...");

        RegistryHelper.SetValue(RegistryHive.LocalMachine, UacPath, "ConsentPromptBehaviorAdmin", 5);
        RegistryHelper.SetValue(RegistryHive.LocalMachine, UacPath, "PromptOnSecureDesktop", 1);
        RegistryHelper.SetValue(RegistryHive.LocalMachine, UacPath, "EnableLUA", 1);

        SecurityTweakLogger.Log("Reverting UAC to defaults", SecStatus.Reverted);
        return Success("UAC settings reverted to Windows defaults");
    }
}
