using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;
using System.Diagnostics;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Disables Hibernation to free up disk space (hiberfil.sys).
/// </summary>
public class DisableHibernationTweak : TweakBase
{
    public override string Id => "disable-hibernation";
    public override string Name => "Disable Hibernation";
    public override string Description => "Disable hibernation and delete hiberfil.sys to free disk space";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string HiberfilPath = @"C:\hiberfil.sys";

    public override bool IsApplied()
    {
        // Check if hiberfil.sys exists
        return !File.Exists(HiberfilPath);
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Disabling Hibernation...");

        // Check current hiberfil.sys size for logging
        if (File.Exists(HiberfilPath))
        {
            try
            {
                var fileInfo = new FileInfo(HiberfilPath);
                var sizeGB = fileInfo.Length / (1024.0 * 1024.0 * 1024.0);
                PerformanceLogger.Log("Disable_Hibernation", "CLI", PerfStatus.InProgress, 
                    $"hiberfil.sys size: {sizeGB:F2} GB");
            }
            catch { }
        }

        // Execute powercfg -h off
        var result = ExecuteCommand("powercfg.exe", "-h off");
        
        // Verify by checking if hiberfil.sys is gone
        // Note: File might take a moment to be deleted
        System.Threading.Thread.Sleep(500);
        bool verified = !File.Exists(HiberfilPath);

        PerformanceLogger.Log("Disable_Hibernation", "CLI", 
            verified ? PerfStatus.VerifiedSuccess : PerfStatus.VerifiedFailed,
            verified ? "hiberfil.sys deleted" : "hiberfil.sys still exists");

        return verified 
            ? Success("Hibernation disabled, disk space freed") 
            : Failure("Could not disable hibernation");
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Re-enabling Hibernation...");

        var result = ExecuteCommand("powercfg.exe", "-h on");
        
        // Verify
        System.Threading.Thread.Sleep(500);
        bool verified = File.Exists(HiberfilPath);

        PerformanceLogger.Log("Enable_Hibernation", "CLI", 
            verified ? PerfStatus.Reverted : PerfStatus.Failed,
            verified ? "hiberfil.sys created" : "hiberfil.sys not created");

        return verified 
            ? Success("Hibernation re-enabled") 
            : Failure("Could not re-enable hibernation");
    }

    private (bool Success, string Output) ExecuteCommand(string fileName, string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var process = Process.Start(psi);
            string output = process?.StandardOutput.ReadToEnd() ?? "";
            string error = process?.StandardError.ReadToEnd() ?? "";
            process?.WaitForExit();
            return (process?.ExitCode == 0, output + error);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
