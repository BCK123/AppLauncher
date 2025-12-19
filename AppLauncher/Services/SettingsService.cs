using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Documents;
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
        string oldPath = "";

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
                // 获取json.JsonStoreDirectory 赋值给oldPath
          
                 oldPath = s?.JsonStoreDirectory + "\\AppLauncher"; // 加?避免空引用（若反序列化失败s为null）

                return s;
            }
            catch
            {
                return null;
            }
        }

        public String Save()
        {
            try
            {
                // 修改路径后 文件进行覆盖

                var newBase = Path.Combine(Settings.JsonStoreDirectory!, "AppLauncher");
                Directory.CreateDirectory(newBase);

                var oldShortcut = Path.Combine(oldPath, Settings.ShortcutsFileName);
                var newShortcut = Path.Combine(newBase, Settings.ShortcutsFileName);

                if (newBase.Equals(oldPath))
                {
                   
                    return "请选择一个不同的路径！";
                }
                var oldCategory = Path.Combine(oldPath, Settings.CategoryFileName);
                var newCategory = Path.Combine(newBase, Settings.CategoryFileName);

                if (File.Exists(oldCategory))
                {

                  
                   // 
                    return "当前路径存在配置文件！";
                }
                if (File.Exists(oldShortcut))
                {
               

                    return "当前路径存在配置文件";
                }
           

                var json = JsonSerializer.Serialize(Settings, _jsonOptions);
                File.WriteAllText(_cfgFile, json);
                Load();

                return "yes";
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
            var dir = string.IsNullOrWhiteSpace(Settings.JsonStoreDirectory)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppLauncher")
                : Path.Combine(Settings.JsonStoreDirectory!, "AppLauncher");

            Directory.CreateDirectory(dir);
            return Path.Combine(dir, Settings.ShortcutsFileName);
        }


        public string GetCategoryFilePath()
        {
            // Ensure directory exists
            var dir = string.IsNullOrWhiteSpace(Settings.JsonStoreDirectory)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppLauncher")
                : Path.Combine(Settings.JsonStoreDirectory!, "AppLauncher");

            Directory.CreateDirectory(dir);
            return Path.Combine(dir, Settings.CategoryFileName);
        }
        // 可选：设置新的目录并保存
        public String SetJsonStoreDirectory(string? directory)
        {
            Settings.JsonStoreDirectory = string.IsNullOrWhiteSpace(directory) ? null : directory;

            String message = Save();
            return message;
        }

    




    }
}
