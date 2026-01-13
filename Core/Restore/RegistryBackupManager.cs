using Microsoft.Win32;
using System.Diagnostics;
using Trippie.TW.Helpers;

namespace Trippie.TW.Core.Restore;

/// <summary>
/// Manages Registry key backups for safe restoration.
/// </summary>
public class RegistryBackupManager
{
    private readonly string _backupFolder;
    private readonly Dictionary<string, RegistryBackupEntry> _backups = new();

    public RegistryBackupManager()
    {
        _backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups", "Registry");
        Directory.CreateDirectory(_backupFolder);
    }

    /// <summary>
    /// Backs up a registry key before modification.
    /// </summary>
    public bool BackupKey(RegistryHive hive, string path, string valueName)
    {
        var fullPath = $"{GetHivePrefix(hive)}\\{path}";
        var backupId = $"{fullPath}\\{valueName}".Replace("\\", "_").Replace(":", "");

        try
        {
            SecurityLogger.Log($"Backing up Registry Key: {fullPath}\\{valueName}", SecurityStatus.InProgress);

            // Get current value
            object? currentValue = null;
            RegistryValueKind valueKind = RegistryValueKind.Unknown;

            using (var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64))
            using (var key = baseKey.OpenSubKey(path))
            {
                if (key != null)
                {
                    currentValue = key.GetValue(valueName);
                    if (currentValue != null)
                    {
                        valueKind = key.GetValueKind(valueName);
                    }
                }
            }

            var entry = new RegistryBackupEntry
            {
                Hive = hive,
                Path = path,
                ValueName = valueName,
                OriginalValue = currentValue,
                ValueKind = valueKind,
                BackupTime = DateTime.Now,
                KeyExisted = currentValue != null
            };

            _backups[backupId] = entry;
            SaveBackupToFile(backupId, entry);

            SecurityLogger.Log($"Backing up Registry Key: {fullPath}\\{valueName}", SecurityStatus.Done,
                currentValue?.ToString() ?? "NULL");
            return true;
        }
        catch (Exception ex)
        {
            SecurityLogger.Log($"Backing up Registry Key: {fullPath}", SecurityStatus.Failed, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Exports a full registry key to a .reg file.
    /// </summary>
    public bool ExportKey(RegistryHive hive, string path)
    {
        var fullPath = $"{GetHivePrefix(hive)}\\{path}";
        var fileName = $"{path.Replace("\\", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.reg";
        var filePath = Path.Combine(_backupFolder, fileName);

        try
        {
            SecurityLogger.Log($"Exporting Registry Key: {fullPath}", SecurityStatus.InProgress);

            var psi = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = $"export \"{fullPath}\" \"{filePath}\" /y",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi);
            process?.WaitForExit();

            bool success = process?.ExitCode == 0 && File.Exists(filePath);
            SecurityLogger.Log($"Exporting Registry Key: {fullPath}", 
                success ? SecurityStatus.Done : SecurityStatus.Failed,
                success ? filePath : null);

            return success;
        }
        catch (Exception ex)
        {
            SecurityLogger.Log($"Exporting Registry Key: {fullPath}", SecurityStatus.Failed, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Restores a backed-up registry value.
    /// </summary>
    public bool RestoreValue(string backupId)
    {
        if (!_backups.TryGetValue(backupId, out var entry))
        {
            // Try to load from file
            entry = LoadBackupFromFile(backupId);
            if (entry == null)
            {
                SecurityLogger.Log($"Restore backup: {backupId}", SecurityStatus.Failed, "Backup not found");
                return false;
            }
        }

        try
        {
            var fullPath = $"{GetHivePrefix(entry.Hive)}\\{entry.Path}\\{entry.ValueName}";
            SecurityLogger.Log($"Restoring: {fullPath}", SecurityStatus.Restoring);

            using var baseKey = RegistryKey.OpenBaseKey(entry.Hive, RegistryView.Registry64);

            if (!entry.KeyExisted || entry.OriginalValue == null)
            {
                // Value didn't exist before - delete it
                using var key = baseKey.OpenSubKey(entry.Path, true);
                key?.DeleteValue(entry.ValueName, false);
                SecurityLogger.Log($"Restoring: {fullPath}", SecurityStatus.Reverted, "Deleted (was not present)");
            }
            else
            {
                // Restore original value
                using var key = baseKey.CreateSubKey(entry.Path, true);
                key?.SetValue(entry.ValueName, entry.OriginalValue, entry.ValueKind);
                SecurityLogger.Log($"Restoring: {fullPath}", SecurityStatus.Reverted, 
                    entry.OriginalValue.ToString());
            }

            return true;
        }
        catch (Exception ex)
        {
            SecurityLogger.Log($"Restore backup: {backupId}", SecurityStatus.Failed, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Restores all backed-up values.
    /// </summary>
    public int RestoreAll()
    {
        LoadAllBackups();
        int restored = 0;

        foreach (var backupId in _backups.Keys.ToList())
        {
            if (RestoreValue(backupId))
                restored++;
        }

        return restored;
    }

    private void SaveBackupToFile(string backupId, RegistryBackupEntry entry)
    {
        try
        {
            var filePath = Path.Combine(_backupFolder, $"{backupId}.bak");
            var lines = new[]
            {
                $"Hive={entry.Hive}",
                $"Path={entry.Path}",
                $"ValueName={entry.ValueName}",
                $"OriginalValue={entry.OriginalValue ?? "NULL"}",
                $"ValueKind={entry.ValueKind}",
                $"BackupTime={entry.BackupTime:O}",
                $"KeyExisted={entry.KeyExisted}"
            };
            File.WriteAllLines(filePath, lines);
        }
        catch { }
    }

    private RegistryBackupEntry? LoadBackupFromFile(string backupId)
    {
        try
        {
            var filePath = Path.Combine(_backupFolder, $"{backupId}.bak");
            if (!File.Exists(filePath)) return null;

            var lines = File.ReadAllLines(filePath);
            var dict = lines.Select(l => l.Split('=', 2))
                           .Where(p => p.Length == 2)
                           .ToDictionary(p => p[0], p => p[1]);

            return new RegistryBackupEntry
            {
                Hive = Enum.Parse<RegistryHive>(dict["Hive"]),
                Path = dict["Path"],
                ValueName = dict["ValueName"],
                OriginalValue = dict["OriginalValue"] == "NULL" ? null : dict["OriginalValue"],
                ValueKind = Enum.Parse<RegistryValueKind>(dict["ValueKind"]),
                BackupTime = DateTime.Parse(dict["BackupTime"]),
                KeyExisted = bool.Parse(dict["KeyExisted"])
            };
        }
        catch { return null; }
    }

    private void LoadAllBackups()
    {
        try
        {
            var files = Directory.GetFiles(_backupFolder, "*.bak");
            foreach (var file in files)
            {
                var backupId = Path.GetFileNameWithoutExtension(file);
                if (!_backups.ContainsKey(backupId))
                {
                    var entry = LoadBackupFromFile(backupId);
                    if (entry != null)
                        _backups[backupId] = entry;
                }
            }
        }
        catch { }
    }

    public int BackupCount => _backups.Count;

    public void ClearBackups()
    {
        _backups.Clear();
        try
        {
            foreach (var file in Directory.GetFiles(_backupFolder, "*.bak"))
                File.Delete(file);
        }
        catch { }
    }

    private static string GetHivePrefix(RegistryHive hive) => hive switch
    {
        RegistryHive.LocalMachine => "HKLM",
        RegistryHive.CurrentUser => "HKCU",
        RegistryHive.ClassesRoot => "HKCR",
        RegistryHive.Users => "HKU",
        RegistryHive.CurrentConfig => "HKCC",
        _ => hive.ToString()
    };
}

public class RegistryBackupEntry
{
    public RegistryHive Hive { get; set; }
    public string Path { get; set; } = "";
    public string ValueName { get; set; } = "";
    public object? OriginalValue { get; set; }
    public RegistryValueKind ValueKind { get; set; }
    public DateTime BackupTime { get; set; }
    public bool KeyExisted { get; set; }
}
