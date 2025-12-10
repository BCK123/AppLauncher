using AppLauncher.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppLauncher.Services
{
    public class ShortcutStore
    {
        private readonly string _dir;
        private readonly string _file;
        // 类内定义静态只读的序列化配置（只创建一次，提升性能）
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

  

        public ShortcutStore()
        {
            _dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppLauncher");
            Directory.CreateDirectory(_dir);
            _file = Path.Combine(_dir, "shortcuts.json");
        }

        // 异步Load
        public async Task<IList<ShortcutItem>> AsycLoad()
        {
            if (!File.Exists(_file)) return new List<ShortcutItem>();
            try
            {
                // 异步读取文件
                var json = await File.ReadAllTextAsync(_file);
                var list = JsonSerializer.Deserialize<List<ShortcutItem>>(json);
                return list ?? new List<ShortcutItem>();
            }
            catch (IOException ex)
            {
                // 可记录日志（比如Serilog/NLog）
                Console.WriteLine($"读取文件失败：{ex.Message}");
                return new List<ShortcutItem>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON解析失败：{ex.Message}");
                return new List<ShortcutItem>();
            }
        }

        public IList<ShortcutItem> Load()
        {
            if (!File.Exists(_file)) return new List<ShortcutItem>();
            try
            {
                var json = File.ReadAllText(_file);
                var list = JsonSerializer.Deserialize<List<ShortcutItem>>(json);
                return list ?? new List<ShortcutItem>();
            }
            catch
            {
                return new List<ShortcutItem>();
            }
        }

        // 异步Save
        public async Task SaveAsync(IEnumerable<ShortcutItem> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            var json = JsonSerializer.Serialize(items, _jsonOptions);
            await File.WriteAllTextAsync(_file, json);
        }

        public void Save(IEnumerable<ShortcutItem> items)
        { 
            // 防止传入null，抛明确的参数异常（类比Java的Objects.requireNonNull
            if (items == null) throw new ArgumentNullException(nameof(items));
            var json = JsonSerializer.Serialize(items, _jsonOptions); // 复用配置
            File.WriteAllText(_file, json);
        }
    }
    }
