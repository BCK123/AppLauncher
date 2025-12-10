using AppLauncher.Models;
using AppLauncher.Services;
using AppLauncher.Utils;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;


namespace AppLauncher.ViewModels
{
    public class MainViewModel : DependencyObject
    {
        private readonly ShortcutStore _store = new ShortcutStore();
        public ObservableCollection<ShortcutItem> Shortcuts { get; } = new ObservableCollection<ShortcutItem>();

        public ICommand ItemDoubleClickCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand RenameCommand { get; }

        public MainViewModel()
        {
            // 加载
            var items = _store.Load();
            foreach (var it in items) Shortcuts.Add(it);

            ItemDoubleClickCommand = new RelayCommand(p => ExecuteItem(p as ShortcutItem));
            RemoveCommand = new RelayCommand(p => RemoveItem(p as ShortcutItem));
            RenameCommand = new RelayCommand(p => RenameItem(p as ShortcutItem));
        }

        public void AddShortcutFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            string target = path;
            string? args = null;
            if (Path.GetExtension(path).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                var res = ShortcutResolver.ResolveLink(path);
                target = res.targetPath;
                args = res.arguments;
            }

            // 避免重复（以 target 或 raw path 判重）
            if (Shortcuts.Any(s => string.Equals(s.RawSourcePath, path, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(s.TargetPath, target, StringComparison.OrdinalIgnoreCase)))
                return;

            var item = new ShortcutItem
            {
                DisplayName = Path.GetFileNameWithoutExtension(target) ?? Path.GetFileName(path),
                TargetPath = target,
                Arguments = args,
                RawSourcePath = path,
                IconPath = target
            };

            Shortcuts.Add(item);
            Save();
        }

        public void Save()
        {
            _store.Save(Shortcuts);
        }

        private void ExecuteItem(ShortcutItem? item)
        {
            if (item == null) return;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = item.TargetPath,
                    Arguments = item.Arguments ?? "",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法启动：{ex.Message}");
            }
        }

        private void RemoveItem(ShortcutItem? item)
        {
            if (item == null) return;
            if (MessageBox.Show($"移除 {item.DisplayName} ?", "确认", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            Shortcuts.Remove(item);
            Save();
        }

        private void RenameItem(ShortcutItem? item)
        {
            if (item == null) return;
            // 简单示例：弹窗输入框（可以用自定义 Dialog）
            var input = Microsoft.VisualBasic.Interaction.InputBox("重命名", "修改显示名", item.DisplayName);
            if (!string.IsNullOrWhiteSpace(input))
            {
                item.DisplayName = input;
                Save();
            }
        }
    }
}
