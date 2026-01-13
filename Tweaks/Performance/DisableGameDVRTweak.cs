using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Disables Game DVR and Game Bar to reduce gaming overhead.
/// </summary>
public class DisableGameDVRTweak : TweakBase
{
    public override string Id => "disable-game-dvr";
    public override string Name => "Disable Game DVR/Bar";
    public override string Description => "Disable Xbox Game Bar and DVR recording for better gaming performance";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string GameDVRPolicyPath = @"SOFTWARE\Policies\Microsoft\Windows\GameDVR";
    private const string GameConfigPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR";
    private const string GameBarPath = @"SOFTWARE\Microsoft\GameBar";
    private const string XboxGameBarPath = @"System\GameConfigStore";

    public override bool IsApplied()
    {
        var policyValue = RegistryHelper.GetValue(RegistryHive.LocalMachine, GameDVRPolicyPath, "AllowGameDVR", 1);
        return Convert.ToInt32(policyValue) == 0;
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Disabling Game DVR and Game Bar...");
        bool allVerified = true;

        // Disable via Group Policy
        if (PerformanceLogger.SetAndVerify(RegistryHive.LocalMachine, GameDVRPolicyPath, "AllowGameDVR", 0))
            PerformanceLogger.Log("Disable_GameDVR_Policy", "Registry", PerfStatus.VerifiedSuccess);
        else
        {
            PerformanceLogger.Log("Disable_GameDVR_Policy", "Registry", PerfStatus.VerifiedFailed);
            allVerified = false;
        }

        // Disable AppCaptureEnabled (HKLM)
        if (PerformanceLogger.SetAndVerify(RegistryHive.LocalMachine, GameConfigPath, "AppCaptureEnabled", 0))
            PerformanceLogger.Log("Disable_AppCapture_HKLM", "Registry", PerfStatus.VerifiedSuccess);
        else
            PerformanceLogger.Log("Disable_AppCapture_HKLM", "Registry", PerfStatus.VerifiedFailed);

        // Disable AppCaptureEnabled (HKCU)
        if (PerformanceLogger.SetAndVerify(RegistryHive.CurrentUser, GameConfigPath, "AppCaptureEnabled", 0))
            PerformanceLogger.Log("Disable_AppCapture_HKCU", "Registry", PerfStatus.VerifiedSuccess);
        else
            PerformanceLogger.Log("Disable_AppCapture_HKCU", "Registry", PerfStatus.VerifiedFailed);

        // Disable GameDVR_Enabled (HKCU)
        if (PerformanceLogger.SetAndVerify(RegistryHive.CurrentUser, GameConfigPath, "GameDVR_Enabled", 0))
            PerformanceLogger.Log("Disable_GameDVR_Enabled", "Registry", PerfStatus.VerifiedSuccess);
        else
            PerformanceLogger.Log("Disable_GameDVR_Enabled", "Registry", PerfStatus.VerifiedFailed);

        // Disable Game Bar
        if (PerformanceLogger.SetAndVerify(RegistryHive.CurrentUser, GameBarPath, "AllowAutoGameMode", 0))
            PerformanceLogger.Log("Disable_AutoGameMode", "Registry", PerfStatus.VerifiedSuccess);
        else
            PerformanceLogger.Log("Disable_AutoGameMode", "Registry", PerfStatus.VerifiedFailed);

        if (PerformanceLogger.SetAndVerify(RegistryHive.CurrentUser, GameBarPath, "UseNexusForGameBarEnabled", 0))
            PerformanceLogger.Log("Disable_GameBar_Nexus", "Registry", PerfStatus.VerifiedSuccess);
        else
            PerformanceLogger.Log("Disable_GameBar_Nexus", "Registry", PerfStatus.VerifiedFailed);

        // Disable GameConfigStore
        if (PerformanceLogger.SetAndVerify(RegistryHive.CurrentUser, XboxGameBarPath, "GameDVR_Enabled", 0))
            PerformanceLogger.Log("Disable_GameConfigStore", "Registry", PerfStatus.VerifiedSuccess);
        else
            PerformanceLogger.Log("Disable_GameConfigStore", "Registry", PerfStatus.VerifiedFailed);

        return allVerified 
            ? Success("Game DVR and Game Bar disabled") 
            : Failure("Some Game DVR settings could not be applied");
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Re-enabling Game DVR and Game Bar...");

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, GameDVRPolicyPath, "AllowGameDVR");
        PerformanceLogger.Log("Enable_GameDVR_Policy", "Registry", PerfStatus.Reverted);

        RegistryHelper.SetValue(RegistryHive.LocalMachine, GameConfigPath, "AppCaptureEnabled", 1);
        RegistryHelper.SetValue(RegistryHive.CurrentUser, GameConfigPath, "AppCaptureEnabled", 1);
        RegistryHelper.SetValue(RegistryHive.CurrentUser, GameConfigPath, "GameDVR_Enabled", 1);
        PerformanceLogger.Log("Enable_AppCapture", "Registry", PerfStatus.Reverted);

        RegistryHelper.DeleteValue(RegistryHive.CurrentUser, GameBarPath, "AllowAutoGameMode");
        RegistryHelper.DeleteValue(RegistryHive.CurrentUser, GameBarPath, "UseNexusForGameBarEnabled");
        PerformanceLogger.Log("Enable_GameBar", "Registry", PerfStatus.Reverted);

        RegistryHelper.SetValue(RegistryHive.CurrentUser, XboxGameBarPath, "GameDVR_Enabled", 1);
        PerformanceLogger.Log("Enable_GameConfigStore", "Registry", PerfStatus.Reverted);

        return Success("Game DVR and Game Bar re-enabled");
    }
}
