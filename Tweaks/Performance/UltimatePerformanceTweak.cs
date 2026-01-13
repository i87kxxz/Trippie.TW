using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;
using System.Diagnostics;

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

    private const string UltimateGuid = "e9a42b02-d5df-448d-aa00-03f14749eb61";

    public override bool IsApplied()
    {
        var result = ExecutePowerCfg("/getactivescheme");
        return result.Contains(UltimateGuid, StringComparison.OrdinalIgnoreCase);
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Enabling Ultimate Performance Power Plan...");

        // Check if plan already exists
        var listResult = ExecutePowerCfg("/list");
        bool planExists = listResult.Contains(UltimateGuid, StringComparison.OrdinalIgnoreCase);

        if (!planExists)
        {
            PerformanceLogger.Log("Ultimate_Performance", "CLI", PerfStatus.InProgress, "Creating power plan");
            
            // Import/duplicate the Ultimate Performance plan
            var duplicateResult = ExecutePowerCfg($"powercfg -duplicatescheme {UltimateGuid}");
            
            // Re-check if it exists now
            listResult = ExecutePowerCfg("/list");
            planExists = listResult.Contains(UltimateGuid, StringComparison.OrdinalIgnoreCase) ||
                         listResult.Contains("Ultimate Performance", StringComparison.OrdinalIgnoreCase);

            if (!planExists)
            {
                // Try alternative: create from High Performance
                PerformanceLogger.Log("Ultimate_Performance", "CLI", PerfStatus.InProgress, "Trying alternative creation method");
                ExecutePowerCfg($"-duplicatescheme 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c {UltimateGuid}");
                ExecutePowerCfg($"-changename {UltimateGuid} \"Ultimate Performance\" \"Maximum performance power plan\"");
                
                // Configure for max performance
                ConfigureUltimateSettings();
            }
        }
        else
        {
            PerformanceLogger.Log("Ultimate_Performance", "CLI", PerfStatus.AlreadyApplied, "Plan already exists");
        }

        // Set as active
        PerformanceLogger.LogAction("Setting Ultimate Performance as active plan...");
        ExecutePowerCfg($"/setactive {UltimateGuid}");

        // Verify
        var verifyResult = ExecutePowerCfg("/getactivescheme");
        bool verified = verifyResult.Contains(UltimateGuid, StringComparison.OrdinalIgnoreCase) ||
                       verifyResult.Contains("Ultimate Performance", StringComparison.OrdinalIgnoreCase);

        PerformanceLogger.Log("Ultimate_Performance", "CLI", 
            verified ? PerfStatus.VerifiedSuccess : PerfStatus.VerifiedFailed);

        return verified 
            ? Success("Ultimate Performance plan activated") 
            : Failure("Could not activate Ultimate Performance plan");
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Reverting to Balanced power plan...");
        
        const string balancedGuid = "381b4222-f694-41f0-9685-ff5bb260df2e";
        ExecutePowerCfg($"/setactive {balancedGuid}");

        var verifyResult = ExecutePowerCfg("/getactivescheme");
        bool verified = verifyResult.Contains(balancedGuid, StringComparison.OrdinalIgnoreCase);

        PerformanceLogger.Log("Ultimate_Performance", "CLI", 
            verified ? PerfStatus.Reverted : PerfStatus.Failed, "Switched to Balanced");

        return verified 
            ? Success("Reverted to Balanced power plan") 
            : Failure("Could not revert power plan");
    }

    private void ConfigureUltimateSettings()
    {
        // Disable processor throttling
        ExecutePowerCfg($"-setacvalueindex {UltimateGuid} 54533251-82be-4824-96c1-47b60b740d00 893dee8e-2bef-41e0-89c6-b55d0929964c 0");
        // Min processor state 100%
        ExecutePowerCfg($"-setacvalueindex {UltimateGuid} 54533251-82be-4824-96c1-47b60b740d00 893dee8e-2bef-41e0-89c6-b55d0929964c 100");
        // Max processor state 100%
        ExecutePowerCfg($"-setacvalueindex {UltimateGuid} 54533251-82be-4824-96c1-47b60b740d00 bc5038f7-23e0-4960-96da-33abaf5935ec 100");
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
            using var process = Process.Start(psi);
            string output = process?.StandardOutput.ReadToEnd() ?? "";
            process?.WaitForExit();
            return output;
        }
        catch { return ""; }
    }
}
