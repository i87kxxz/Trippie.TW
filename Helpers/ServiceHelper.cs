using System.Diagnostics;
using System.ServiceProcess;

namespace Trippie.TW.Helpers;

/// <summary>
/// Helper class for Windows Service operations.
/// </summary>
public static class ServiceHelper
{
    public static bool ServiceExists(string serviceName)
    {
        return ServiceController.GetServices().Any(s => 
            s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
    }

    public static ServiceControllerStatus? GetStatus(string serviceName)
    {
        try
        {
            using var sc = new ServiceController(serviceName);
            return sc.Status;
        }
        catch { return null; }
    }

    public static bool SetStartupType(string serviceName, string startupType)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"config \"{serviceName}\" start= {startupType}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };
            using var process = Process.Start(psi);
            process?.WaitForExit();
            return process?.ExitCode == 0;
        }
        catch { return false; }
    }

    public static bool StopService(string serviceName)
    {
        try
        {
            using var sc = new ServiceController(serviceName);
            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
            }
            return true;
        }
        catch { return false; }
    }

    public static bool StartService(string serviceName)
    {
        try
        {
            using var sc = new ServiceController(serviceName);
            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
            }
            return true;
        }
        catch { return false; }
    }
}
