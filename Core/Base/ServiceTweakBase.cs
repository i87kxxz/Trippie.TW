using System.ServiceProcess;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Core.Base;

/// <summary>
/// Base class for service-based tweaks with common disable/enable logic.
/// </summary>
public abstract class ServiceTweakBase : TweakBase
{
    /// <summary>
    /// The Windows service name(s) to manage.
    /// </summary>
    protected abstract string[] ServiceNames { get; }

    /// <summary>
    /// The default startup type when reverting (usually "auto" or "manual").
    /// </summary>
    protected virtual string DefaultStartType => "manual";

    /// <summary>
    /// Timeout in seconds for stopping a service.
    /// </summary>
    protected virtual int StopTimeoutSeconds => 30;

    public override bool IsApplied()
    {
        foreach (var serviceName in ServiceNames)
        {
            if (!ServiceHelper.ServiceExists(serviceName))
                continue;

            var status = ServiceHelper.GetStatus(serviceName);
            if (status != ServiceControllerStatus.Stopped)
                return false;
        }
        return true;
    }

    public override TweakResult Apply()
    {
        ServiceLogger.LogAction($"Disabling {Name}...");

        int disabled = 0;
        int notFound = 0;
        int failed = 0;

        foreach (var serviceName in ServiceNames)
        {
            try
            {
                if (!ServiceHelper.ServiceExists(serviceName))
                {
                    ServiceLogger.LogError(serviceName, "not found on this system");
                    notFound++;
                    continue;
                }

                // Check if already disabled
                var currentStatus = ServiceHelper.GetStatus(serviceName);
                if (currentStatus == ServiceControllerStatus.Stopped)
                {
                    // Check startup type
                    var startType = GetServiceStartType(serviceName);
                    if (startType == "Disabled")
                    {
                        ServiceLogger.Log(serviceName, SvcStatus.AlreadyDisabled);
                        disabled++;
                        continue;
                    }
                }

                ServiceLogger.Log(serviceName, SvcStatus.InProgress);

                // Stop the service with timeout
                if (currentStatus != ServiceControllerStatus.Stopped)
                {
                    bool stopped = StopServiceWithTimeout(serviceName, StopTimeoutSeconds);
                    if (!stopped)
                    {
                        ServiceLogger.Log(serviceName, SvcStatus.Failed, "Could not stop service");
                        failed++;
                        continue;
                    }
                }

                // Disable the service
                bool disabledOk = ServiceHelper.SetStartupType(serviceName, "disabled");
                if (disabledOk)
                {
                    ServiceLogger.Log(serviceName, SvcStatus.DisabledAndStopped);
                    disabled++;
                }
                else
                {
                    ServiceLogger.Log(serviceName, SvcStatus.Failed, "Could not disable startup");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                ServiceLogger.Log(serviceName, SvcStatus.Failed, ex.Message);
                failed++;
            }
        }

        if (disabled > 0)
            return Success($"{disabled} service(s) disabled");
        else if (notFound == ServiceNames.Length)
            return Failure("No applicable services found on this system");
        else
            return Failure($"Failed to disable {failed} service(s)");
    }

    public override TweakResult Revert()
    {
        ServiceLogger.LogAction($"Re-enabling {Name}...");

        int enabled = 0;
        int failed = 0;

        foreach (var serviceName in ServiceNames)
        {
            try
            {
                if (!ServiceHelper.ServiceExists(serviceName))
                    continue;

                // Set startup type back to default
                bool enabledOk = ServiceHelper.SetStartupType(serviceName, DefaultStartType);
                if (enabledOk)
                {
                    ServiceLogger.Log(serviceName, SvcStatus.Reverted, $"StartType: {DefaultStartType}");
                    enabled++;
                }
                else
                {
                    ServiceLogger.Log(serviceName, SvcStatus.Failed);
                    failed++;
                }
            }
            catch (Exception ex)
            {
                ServiceLogger.Log(serviceName, SvcStatus.Failed, ex.Message);
                failed++;
            }
        }

        return enabled > 0 
            ? Success($"{enabled} service(s) re-enabled") 
            : Failure("Could not re-enable services");
    }

    private bool StopServiceWithTimeout(string serviceName, int timeoutSeconds)
    {
        try
        {
            using var sc = new ServiceController(serviceName);
            
            if (sc.Status == ServiceControllerStatus.Stopped)
                return true;

            if (sc.CanStop)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(timeoutSeconds));
                return sc.Status == ServiceControllerStatus.Stopped;
            }
            
            // Try force stop via sc.exe
            return ServiceHelper.StopService(serviceName);
        }
        catch (System.ServiceProcess.TimeoutException)
        {
            ServiceLogger.Log(serviceName, SvcStatus.Failed, $"Timeout after {timeoutSeconds}s");
            return false;
        }
        catch
        {
            return false;
        }
    }

    private string GetServiceStartType(string serviceName)
    {
        try
        {
            var result = PowerShellHelper.Execute(
                $"(Get-Service -Name '{serviceName}').StartType");
            return result.Success ? result.Output.Trim() : "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}
