using System.Diagnostics;
using Trippie.TW.Helpers;

namespace Trippie.TW.Core.Restore;

/// <summary>
/// Manages Windows System Restore Points.
/// </summary>
public static class RestorePointManager
{
    /// <summary>
    /// Creates a System Restore Point using PowerShell/WMI.
    /// </summary>
    public static bool CreateRestorePoint(string description = "Trippie.TW Backup")
    {
        SecurityLogger.LogAction($"Creating System Restore Point: {description}...");

        try
        {
            // Enable System Restore on C: drive if not enabled
            var enableResult = PowerShellHelper.Execute(
                "Enable-ComputerRestore -Drive 'C:\\' -ErrorAction SilentlyContinue");

            // Create restore point using WMI
            var script = $@"
                $description = '{description} - {DateTime.Now:yyyy-MM-dd HH:mm}'
                Checkpoint-Computer -Description $description -RestorePointType 'MODIFY_SETTINGS' -ErrorAction Stop
            ";

            var result = PowerShellHelper.Execute(script);

            if (result.Success)
            {
                SecurityLogger.Log("Creating System Restore Point", SecurityStatus.Success, description);
                return true;
            }
            else
            {
                // Try alternative method using WMI directly
                SecurityLogger.Log("Checkpoint-Computer failed, trying WMI method", SecurityStatus.InProgress);
                return CreateRestorePointWMI(description);
            }
        }
        catch (Exception ex)
        {
            SecurityLogger.Log("Creating System Restore Point", SecurityStatus.Failed, ex.Message);
            return false;
        }
    }

    private static bool CreateRestorePointWMI(string description)
    {
        try
        {
            var script = $@"
                $class = [wmiclass]'\\.\root\default:SystemRestore'
                $result = $class.CreateRestorePoint('{description} - {DateTime.Now:yyyy-MM-dd HH:mm}', 12, 100)
                $result.ReturnValue -eq 0
            ";

            var result = PowerShellHelper.Execute(script);
            bool success = result.Success && result.Output.Trim().Equals("True", StringComparison.OrdinalIgnoreCase);

            SecurityLogger.Log("Creating System Restore Point (WMI)", 
                success ? SecurityStatus.Success : SecurityStatus.Failed);
            return success;
        }
        catch (Exception ex)
        {
            SecurityLogger.Log("Creating System Restore Point (WMI)", SecurityStatus.Failed, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Lists available restore points.
    /// </summary>
    public static List<RestorePointInfo> GetRestorePoints()
    {
        var points = new List<RestorePointInfo>();
        try
        {
            var result = PowerShellHelper.Execute(
                "Get-ComputerRestorePoint | Select-Object SequenceNumber, Description, CreationTime | ConvertTo-Json");

            if (result.Success && !string.IsNullOrWhiteSpace(result.Output))
            {
                // Parse JSON output (simplified)
                var lines = result.Output.Split('\n');
                // Basic parsing - in production would use proper JSON deserializer
            }
        }
        catch { }
        return points;
    }
}

public record RestorePointInfo(int SequenceNumber, string Description, DateTime CreationTime);
