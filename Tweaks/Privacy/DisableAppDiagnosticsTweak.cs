using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Privacy;

/// <summary>
/// Restricts app access to diagnostic information.
/// </summary>
public class DisableAppDiagnosticsTweak : TweakBase
{
    public override string Id => "disable-app-diagnostics";
    public override string Name => "Disable App Diagnostics";
    public override string Description => "Restrict apps from accessing diagnostic information about other apps";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string AppPrivacyPath = @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy";
    private const string DiagnosticsPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appDiagnostics";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, AppPrivacyPath, "LetAppsGetDiagnosticInfo", 0);
        return Convert.ToInt32(value) == 2; // 2 = Force Deny
    }

    public override TweakResult Apply()
    {
        DebugLogger.LogAction("Disabling App Diagnostics access...");
        bool allSuccess = true;

        // Force deny app diagnostics via policy (2 = Force Deny)
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, AppPrivacyPath, "LetAppsGetDiagnosticInfo", 2))
            DebugLogger.LogRegistry(AppPrivacyPath, "LetAppsGetDiagnosticInfo", 2, true);
        else
        {
            DebugLogger.LogRegistry(AppPrivacyPath, "LetAppsGetDiagnosticInfo", 2, false);
            allSuccess = false;
        }

        // Set user consent to Deny
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, DiagnosticsPath, "Value", "Deny", RegistryValueKind.String))
            DebugLogger.LogRegistry(DiagnosticsPath, "Value", "Deny", true);
        else
        {
            DebugLogger.LogRegistry(DiagnosticsPath, "Value", "Deny", false);
            allSuccess = false;
        }

        // Also restrict for current user
        if (RegistryHelper.SetValue(RegistryHive.CurrentUser, DiagnosticsPath, "Value", "Deny", RegistryValueKind.String))
            DebugLogger.LogRegistry($"HKCU\\{DiagnosticsPath}", "Value", "Deny", true);
        else
            DebugLogger.LogRegistry($"HKCU\\{DiagnosticsPath}", "Value", "Deny", false);

        DebugLogger.Log("Disabling App Diagnostics", allSuccess ? LogStatus.Done : LogStatus.Failed);
        return allSuccess ? Success("App Diagnostics access disabled") : Failure("Some app diagnostics settings could not be applied");
    }

    public override TweakResult Revert()
    {
        DebugLogger.LogAction("Re-enabling App Diagnostics access...");

        // Remove policy (0 = User Control)
        RegistryHelper.SetValue(RegistryHive.LocalMachine, AppPrivacyPath, "LetAppsGetDiagnosticInfo", 0);
        DebugLogger.LogRegistry(AppPrivacyPath, "LetAppsGetDiagnosticInfo", 0, true);

        // Set consent to Allow
        RegistryHelper.SetValue(RegistryHive.LocalMachine, DiagnosticsPath, "Value", "Allow", RegistryValueKind.String);
        DebugLogger.LogRegistry(DiagnosticsPath, "Value", "Allow", true);

        RegistryHelper.SetValue(RegistryHive.CurrentUser, DiagnosticsPath, "Value", "Allow", RegistryValueKind.String);
        DebugLogger.LogRegistry($"HKCU\\{DiagnosticsPath}", "Value", "Allow", true);

        DebugLogger.Log("Re-enabling App Diagnostics", LogStatus.Done);
        return Success("App Diagnostics access re-enabled");
    }
}
