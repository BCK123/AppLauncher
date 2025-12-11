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
        private readonly string _file;
        public ShortcutStore()
        {
            _file = SettingsService.Instance.GetShortcutsFilePath();
        }

        public IList<ShortcutItem> Load()
        {
            if (!File.Exists(_file)) return new List<ShortcutItem>();
            var json = File.ReadAllText(_file);
            var items = JsonSerializer.Deserialize<List<ShortcutItem>>(json);
            return items ?? new List<ShortcutItem>();
        }

        public void Save(IEnumerable<ShortcutItem> items)
        {
            var tmp = _file + ".tmp";
            var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(tmp, json);
            File.Move(tmp, _file, true);
        }
    }

}
