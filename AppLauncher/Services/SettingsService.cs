using System;
using System.IO;
using System.Text.Json;
using AppLauncher.Models;

namespace AppLauncher.Services
{
    public class SettingsService
    {
        private static readonly Lazy<SettingsService> _lazy = new(() => new SettingsService());
        public static SettingsService Instance => _lazy.Value;

        private readonly string _cfgDir;
        private readonly string _cfgFile;
        private JsonSerializerOptions _jsonOptions;

        public AppSettings Settings { get; private set; }

        private SettingsService()
        {
            _cfgDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppLauncher");
            Directory.CreateDirectory(_cfgDir);
            _cfgFile = Path.Combine(_cfgDir, "appsettings.json");

            _jsonOptions = new JsonSerializerOptions { WriteIndented = true };

            Settings = Load() ?? new AppSettings();
        }

        private AppSettings? Load()
        {
            try
            {
                if (!File.Exists(_cfgFile)) return null;
                var json = File.ReadAllText(_cfgFile);
                var s = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
                return s;
            }
            catch
            {
                return null;
            }
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(Settings, _jsonOptions);
                File.WriteAllText(_cfgFile, json);
            }
            catch (Exception ex)
            {
                // 你可以在这里记录日志或提示用户
                throw new Exception("保存设置失败: " + ex.Message, ex);
            }
        }

        // 方便外部快速取最终 shortcuts.json 的完整路径
        public string GetShortcutsFilePath()
        {
            // Ensure directory exists
            var dir = string.IsNullOrWhiteSpace(Settings.ShortcutsDirectory)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppLauncher")
                : Path.Combine(Settings.ShortcutsDirectory!, "AppLauncher");

            Directory.CreateDirectory(dir);
            return Path.Combine(dir, Settings.ShortcutsFileName);
        }

        // 可选：设置新的目录并保存
        public void SetShortcutsDirectory(string? directory)
        {
            Settings.ShortcutsDirectory = string.IsNullOrWhiteSpace(directory) ? null : directory;
            Save();
        }
    }
}
