using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.UI;

/// <summary>
/// Disables the Windows startup sound.
/// </summary>
public class DisableStartupSoundTweak : TweakBase
{
    public override string Id => "disable-startup-sound";
    public override string Name => "Disable Startup Sound";
    public override string Description => "Disable the Windows startup/logon sound";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string BootAnimPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\BootAnimation";
    private const string SoundPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";

    public override bool IsApplied()
    {
        var value = RegistryHelper.GetValue(RegistryHive.LocalMachine, BootAnimPath, "DisableStartupSound", 0);
        return Convert.ToInt32(value) == 1;
    }

    public override TweakResult Apply()
    {
        UILogger.LogAction("Disabling Startup Sound...");
        bool allSuccess = true;

        // Method 1: BootAnimation key
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, BootAnimPath, "DisableStartupSound", 1))
        {
            var verify = RegistryHelper.GetValue(RegistryHive.LocalMachine, BootAnimPath, "DisableStartupSound", 0);
            bool verified = Convert.ToInt32(verify) == 1;
            UILogger.Log("Setting DisableStartupSound -> 1", verified ? UIStatus.Success : UIStatus.Failed);
            if (!verified) allSuccess = false;
        }
        else
        {
            UILogger.Log("Setting DisableStartupSound", UIStatus.Failed);
            allSuccess = false;
        }

        // Method 2: Disable via Sound policy
        if (RegistryHelper.SetValue(RegistryHive.LocalMachine, SoundPath, "DisableStartupSound", 1))
            UILogger.Log("Setting Policy DisableStartupSound -> 1", UIStatus.Success);

        return allSuccess 
            ? Success("Startup sound disabled") 
            : Failure("Could not fully disable startup sound");
    }

    public override TweakResult Revert()
    {
        UILogger.LogAction("Re-enabling Startup Sound...");

        RegistryHelper.SetValue(RegistryHive.LocalMachine, BootAnimPath, "DisableStartupSound", 0);
        UILogger.Log("Setting DisableStartupSound -> 0", UIStatus.Done);

        RegistryHelper.DeleteValue(RegistryHive.LocalMachine, SoundPath, "DisableStartupSound");
        UILogger.Log("Removed Policy DisableStartupSound", UIStatus.Done);

        return Success("Startup sound re-enabled");
    }
}
