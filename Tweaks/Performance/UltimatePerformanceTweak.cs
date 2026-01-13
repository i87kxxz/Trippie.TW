using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Enables the Ultimate Performance power plan for maximum system performance.
/// </summary>
public class UltimatePerformanceTweak : TweakBase
{
    public override string Id => "ultimate-performance";
    public override string Name => "Ultimate Performance Plan";
    public override string Description => "Enable hidden Ultimate Performance power plan for maximum performance";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    private const string UltimateSourceGuid = "e9a42b02-d5df-448d-aa00-03f14749eb61";
    private const string HighPerfGuid = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";
    
    private string? _createdPlanGuid;

    public override bool IsApplied()
    {
        var result = ExecutePowerCfg("/getactivescheme");
        return result.Contains("Ultimate Performance", StringComparison.OrdinalIgnoreCase);
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Enabling Ultimate Performance Power Plan...");

        var listResult = ExecutePowerCfg("/list");
        
        if (listResult.Contains("Ultimate Performance", StringComparison.OrdinalIgnoreCase))
        {
            PerformanceLogger.Log("Ultimate_Performance", "CLI", PerfStatus.AlreadyApplied, "Plan exists");
            var guid = FindPlanGuid(listResult, "Ultimate Performance");
            if (guid != null) return ActivatePlan(guid);
        }

        // Method 1: Duplicate hidden Ultimate Performance
        PerformanceLogger.Log("Ultimate_Performance", "CLI", PerfStatus.InProgress, "Duplicating hidden scheme");
        var dupResult = ExecutePowerCfg($"/duplicatescheme {UltimateSourceGuid}");
        var newGuid = ExtractGuid(dupResult);
        
        if (newGuid != null)
        {
            _createdPlanGuid = newGuid;
            PerformanceLogger.Log("Ultimate_Performance", "CLI", PerfStatus.VerifiedSuccess, $"Created: {newGuid}");
            return ActivatePlan(newGuid);
        }

        // Method 2: Create from High Performance
        PerformanceLogger.Log("Ultimate_Performance", "CLI", PerfStatus.InProgress, "Creating from High Performance");
        dupResult = ExecutePowerCfg($"/duplicatescheme {HighPerfGuid}");
        newGuid = ExtractGuid(dupResult);
        
        if (newGuid != null)
        {
            _createdPlanGuid = newGuid;
            ExecutePowerCfg($"/changename {newGuid} \"Ultimate Performance\" \"Maximum performance\"");
            ConfigureUltimateSettings(newGuid);
            PerformanceLogger.Log("Ultimate_Performance", "CLI", PerfStatus.VerifiedSuccess, "Created custom plan");
            return ActivatePlan(newGuid);
        }

        PerformanceLogger.Log("Ultimate_Performance", "CLI", PerfStatus.VerifiedFailed, "All methods failed");
        return Failure("Could not create Ultimate Performance plan");
    }

    private TweakResult ActivatePlan(string guid)
    {
        PerformanceLogger.LogAction($"Activating plan: {guid}...");
        ExecutePowerCfg($"/setactive {guid}");
        Thread.Sleep(300);
        
        var verify = ExecutePowerCfg("/getactivescheme");
        bool ok = verify.Contains(guid, StringComparison.OrdinalIgnoreCase) ||
                  verify.Contains("Ultimate Performance", StringComparison.OrdinalIgnoreCase);

        PerformanceLogger.Log("Ultimate_Performance", "CLI", 
            ok ? PerfStatus.VerifiedSuccess : PerfStatus.VerifiedFailed,
            ok ? "Activated" : "Failed");

        return ok ? Success("Ultimate Performance activated") : Failure("Could not activate plan");
    }

    private void ConfigureUltimateSettings(string guid)
    {
        var cpu = "54533251-82be-4824-96c1-47b60b740d00";
        ExecutePowerCfg($"/setacvalueindex {guid} {cpu} 893dee8e-2bef-41e0-89c6-b55d0929964c 100");
        ExecutePowerCfg($"/setacvalueindex {guid} {cpu} bc5038f7-23e0-4960-96da-33abaf5935ec 100");
        ExecutePowerCfg($"/setacvalueindex {guid} 0012ee47-9041-4b5d-9b77-535fba8b1442 6738e2c4-e8a5-4a42-b16a-e040e769756e 0");
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Reverting to Balanced...");
        ExecutePowerCfg("/setactive 381b4222-f694-41f0-9685-ff5bb260df2e");
        Thread.Sleep(300);
        
        var verify = ExecutePowerCfg("/getactivescheme");
        bool ok = verify.Contains("Balanced", StringComparison.OrdinalIgnoreCase);
        
        PerformanceLogger.Log("Ultimate_Performance", "CLI", ok ? PerfStatus.Reverted : PerfStatus.Failed);
        return ok ? Success("Reverted to Balanced") : Failure("Could not revert");
    }

    private string? FindPlanGuid(string list, string name)
    {
        foreach (var line in list.Split('\n'))
            if (line.Contains(name, StringComparison.OrdinalIgnoreCase))
                return ExtractGuid(line);
        return null;
    }

    private string? ExtractGuid(string text)
    {
        var m = Regex.Match(text, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
        return m.Success ? m.Value : null;
    }

    private string ExecutePowerCfg(string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powercfg.exe",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var p = Process.Start(psi);
            var output = p?.StandardOutput.ReadToEnd() ?? "";
            var error = p?.StandardError.ReadToEnd() ?? "";
            p?.WaitForExit();
            return output + error;
        }
        catch { return ""; }
    }
}
