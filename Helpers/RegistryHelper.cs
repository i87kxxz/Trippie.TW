using Microsoft.Win32;

namespace Trippie.TW.Helpers;

/// <summary>
/// Helper class for Windows Registry operations.
/// </summary>
public static class RegistryHelper
{
    public static bool KeyExists(RegistryHive hive, string path)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(path);
        return key != null;
    }

    public static object? GetValue(RegistryHive hive, string path, string name, object? defaultValue = null)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(path);
        return key?.GetValue(name, defaultValue) ?? defaultValue;
    }

    public static bool SetValue(RegistryHive hive, string path, string name, object value, RegistryValueKind kind = RegistryValueKind.DWord)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = baseKey.CreateSubKey(path, true);
            key?.SetValue(name, value, kind);
            return true;
        }
        catch { return false; }
    }

    public static bool DeleteValue(RegistryHive hive, string path, string name)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = baseKey.OpenSubKey(path, true);
            key?.DeleteValue(name, false);
            return true;
        }
        catch { return false; }
    }

    public static bool DeleteKey(RegistryHive hive, string path)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            baseKey.DeleteSubKeyTree(path, false);
            return true;
        }
        catch { return false; }
    }
}
