using System;
using System.Text.Json.Serialization;

namespace AppLauncher.Models
{

    //全局变量
    public class AppSettings
    {
        // 默认json数据存储路径
        [JsonPropertyName("JsonStoreDirectory")]
        public string? JsonStoreDirectory { get; set; } // 用户选择的目录（若为 null 则使用默认 AppData）
        //默认路径 %APPDATA%\AppLauncher\
        [JsonPropertyName("shortcutsFileName")]
        public string ShortcutsFileName { get; set; } = "shortcuts.json";
        [JsonPropertyName("categoryFileName")]
        public string CategoryFileName { get; set; } = "category.json";

    

        // 辅助属性：完整文件路径（非序列化）
        [JsonIgnore]
        public string ShortcutsFilePath
        {
            get
            {
                var baseDir = string.IsNullOrWhiteSpace(JsonStoreDirectory)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                    : JsonStoreDirectory!;
                var appDir = System.IO.Path.Combine(baseDir, "AppLauncher");
                return System.IO.Path.Combine(appDir, ShortcutsFileName);
            }
        }
    }
}
