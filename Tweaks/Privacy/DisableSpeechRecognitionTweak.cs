using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Privacy;

/// <summary>
/// Disables Online Speech Recognition (Cortana voice data collection).
/// </summary>
public class DisableSpeechRecognitionTweak : TweakBase
{
    public override string Id => "disable-speech-recognition";
    public override string Name => "Disable Online Speech Recognition";
    public override string Description => "Stop Windows from sending voice data to Microsoft for speech recognition";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string SpeechRegPath = @"SOFTWARE\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy";
    private const string SpeechPolicyPath = @"SOFTWARE\Policies\Microsoft\InputPersonalization";

    public override bool IsApplied()
    {
        var hasAccepted = RegistryHelper.GetValue(RegistryHive.CurrentUser, SpeechRegPath, "HasAccepted", 1);
        return Convert.ToInt32(hasAccepted) == 0;
    }

    public override TweakResult Apply()
    {
        DebugLogger.LogAction("Disabling Online Speech Recognition...");
        bool allSuccess = true;

        // Disable via user setting
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, SpeechRegPath, "HasAccepted", 0))
            DebugLogger.LogRegistry($"HKCU\\{SpeechRegPath}", "HasAccepted", 0, true);
        else
        {
            DebugLogger.LogRegistry($"HKCU\\{SpeechRegPath}", "HasAccepted", 0, false);
            allSuccess = false;
        }

        // Disable via policy - RestrictImplicitTextCollection
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, SpeechPolicyPath, "RestrictImplicitTextCollection", 1))
            DebugLogger.LogRegistry(SpeechPolicyPath, "RestrictImplicitTextCollection", 1, true);
        else
        {
            DebugLogger.LogRegistry(SpeechPolicyPath, "RestrictImplicitTextCollection", 1, false);
            allSuccess = false;
        }

        // Disable via policy - RestrictImplicitInkCollection
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, SpeechPolicyPath, "RestrictImplicitInkCollection", 1))
            DebugLogger.LogRegistry(SpeechPolicyPath, "RestrictImplicitInkCollection", 1, true);
        else
        {
            DebugLogger.LogRegistry(SpeechPolicyPath, "RestrictImplicitInkCollection", 1, false);
            allSuccess = false;
        }

        // Disable AllowInputPersonalization
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, SpeechPolicyPath, "AllowInputPersonalization", 0))
            DebugLogger.LogRegistry(SpeechPolicyPath, "AllowInputPersonalization", 0, true);
        else
            DebugLogger.LogRegistry(SpeechPolicyPath, "AllowInputPersonalization", 0, false);

        DebugLogger.Log("Disabling Online Speech Recognition", allSuccess ? LogStatus.Done : LogStatus.Failed);
        return allSuccess ? Success("Online Speech Recognition disabled") : Failure("Some speech settings could not be applied");
    }

    public override TweakResult Revert()
    {
        DebugLogger.LogAction("Re-enabling Online Speech Recognition...");

        RegistryHelper.SetValue(RegistryHive.CurrentUser, SpeechRegPath, "HasAccepted", 1);
        DebugLogger.LogRegistry($"HKCU\\{SpeechRegPath}", "HasAccepted", 1, true);

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, SpeechPolicyPath, "RestrictImplicitTextCollection");
        DebugLogger.Log("Removed RestrictImplicitTextCollection policy", LogStatus.Success);

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, SpeechPolicyPath, "RestrictImplicitInkCollection");
        DebugLogger.Log("Removed RestrictImplicitInkCollection policy", LogStatus.Success);

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, SpeechPolicyPath, "AllowInputPersonalization");
        DebugLogger.Log("Removed AllowInputPersonalization policy", LogStatus.Success);

        DebugLogger.Log("Re-enabling Online Speech Recognition", LogStatus.Done);
        return Success("Online Speech Recognition re-enabled");
    }
}
