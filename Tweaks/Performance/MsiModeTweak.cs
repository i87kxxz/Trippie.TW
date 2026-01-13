using Microsoft.Win32;
using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Performance;

/// <summary>
/// Enables Message Signaled Interrupts (MSI) mode for GPU and network adapters.
/// MSI reduces interrupt latency and DPC latency for better gaming performance.
/// </summary>
public class MsiModeTweak : TweakBase
{
    public override string Id => "msi-mode";
    public override string Name => "Enable MSI Mode";
    public override string Description => "Enable Message Signaled Interrupts for GPU/NIC to reduce latency";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Advanced;

    private const string PciEnumPath = @"SYSTEM\CurrentControlSet\Enum\PCI";
    private readonly List<string> _modifiedDevices = new();

    public override bool IsApplied()
    {
        // Check if any GPU has MSI enabled
        var gpuDevices = FindDevices("VGA", "3D");
        foreach (var device in gpuDevices)
        {
            var msiPath = $@"{device}\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties";
            var msiSupported = RegistryHelper.GetValue(RegistryHive.LocalMachine, msiPath, "MSISupported", 0);
            if (Convert.ToInt32(msiSupported) == 1)
                return true;
        }
        return false;
    }

    public override TweakResult Apply()
    {
        PerformanceLogger.LogAction("Enabling MSI Mode for GPU and Network Adapters...");

        int enabled = 0;
        int failed = 0;

        // Find and enable MSI for GPUs
        var gpuDevices = FindDevices("VGA", "3D", "Display");
        foreach (var device in gpuDevices)
        {
            if (EnableMsiForDevice(device, "GPU"))
                enabled++;
            else
                failed++;
        }

        // Find and enable MSI for Network adapters
        var netDevices = FindDevices("Ethernet", "Network", "Wireless");
        foreach (var device in netDevices)
        {
            if (EnableMsiForDevice(device, "NIC"))
                enabled++;
            else
                failed++;
        }

        if (enabled > 0)
        {
            PerformanceLogger.Log("MSI_Mode", "Registry", PerfStatus.VerifiedSuccess, 
                $"Enabled for {enabled} device(s)");
            PerformanceLogger.Log("MSI_Mode", "Registry", PerfStatus.RebootRequired);
            return Success($"MSI mode enabled for {enabled} device(s) (reboot required)");
        }

        return Failure("Could not enable MSI mode for any devices");
    }

    private bool EnableMsiForDevice(string devicePath, string deviceType)
    {
        try
        {
            var msiPath = $@"{devicePath}\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties";
            
            // Create the key path if it doesn't exist
            var interruptPath = $@"{devicePath}\Device Parameters\Interrupt Management";
            var msiPropsPath = $@"{interruptPath}\MessageSignaledInterruptProperties";

            // Enable MSI
            if (RegistryHelper.SetValue(RegistryHive.LocalMachine, msiPropsPath, "MSISupported", 1))
            {
                _modifiedDevices.Add(devicePath);
                var shortPath = devicePath.Length > 50 ? "..." + devicePath.Substring(devicePath.Length - 40) : devicePath;
                PerformanceLogger.Log($"MSI_Mode ({deviceType})", "Registry", PerfStatus.VerifiedSuccess, shortPath);
                return true;
            }
        }
        catch (Exception ex)
        {
            PerformanceLogger.Log($"MSI_Mode ({deviceType})", "Registry", PerfStatus.Failed, ex.Message);
        }
        return false;
    }

    private List<string> FindDevices(params string[] keywords)
    {
        var devices = new List<string>();
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var pciKey = baseKey.OpenSubKey(PciEnumPath);
            
            if (pciKey == null) return devices;

            foreach (var vendorDevice in pciKey.GetSubKeyNames())
            {
                using var vendorKey = pciKey.OpenSubKey(vendorDevice);
                if (vendorKey == null) continue;

                foreach (var instance in vendorKey.GetSubKeyNames())
                {
                    var fullPath = $@"{PciEnumPath}\{vendorDevice}\{instance}";
                    using var instanceKey = vendorKey.OpenSubKey(instance);
                    
                    if (instanceKey == null) continue;

                    var deviceDesc = instanceKey.GetValue("DeviceDesc")?.ToString() ?? "";
                    var friendlyName = instanceKey.GetValue("FriendlyName")?.ToString() ?? "";
                    var classGuid = instanceKey.GetValue("ClassGUID")?.ToString() ?? "";

                    // Check if device matches any keyword
                    foreach (var keyword in keywords)
                    {
                        if (deviceDesc.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            friendlyName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            devices.Add(fullPath);
                            break;
                        }
                    }
                }
            }
        }
        catch { }
        return devices;
    }

    public override TweakResult Revert()
    {
        PerformanceLogger.LogAction("Disabling MSI Mode...");

        // Disable MSI for all previously modified devices
        var allDevices = FindDevices("VGA", "3D", "Display", "Ethernet", "Network", "Wireless");
        int reverted = 0;

        foreach (var device in allDevices)
        {
            var msiPath = $@"{device}\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties";
            if (RegistryHelper.DeleteValue(RegistryHive.LocalMachine, msiPath, "MSISupported"))
                reverted++;
        }

        PerformanceLogger.Log("MSI_Mode", "Registry", PerfStatus.Reverted, $"{reverted} device(s)");
        return Success($"MSI mode disabled for {reverted} device(s) (reboot required)");
    }
}
