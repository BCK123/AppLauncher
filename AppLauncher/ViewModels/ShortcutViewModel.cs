using AppLauncher.Models;
using AppLauncher.Services;
using AppLauncher.Core.Log;
using AppLauncher.Utils;
using AppLauncher.Views;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace AppLauncher.ViewModels
{
    public class ShortcutViewModel : BindableBase
    {
        private readonly ShortcutStore _store;
        public ObservableCollection<ShortcutItem> Shortcuts { get; } = new ObservableCollection<ShortcutItem>();

        public ICommand ItemDoubleClickCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand RenameCommand { get; }

        public ICommand SelectCategoryCommand { get; }

        public ICommand RenameCategoryCommand { get; }

        public ICommand DeleteCategoryCommand { get; }

  


        private string _currentCategory = "全部";

        private readonly CategoryService _categoryService;

        private readonly SettingsWindow _settingsWindow;
       
        private readonly ILoggerService _logger;
        public string CurrentCategory
        {
            get => _currentCategory;
            set
            {
                _currentCategory = value;

                ShortcutsView.Refresh();
            }
        }

        public ICollectionView ShortcutsView { get; }
        public ObservableCollection<CategoryItem> Categories { get; }


        public void LoadModel()
        {
            Shortcuts.Clear();
            // 加载
            var items = _store.Load();
            foreach (var it in items) Shortcuts.Add(it);
        }



        public ShortcutViewModel(CategoryService categoryService, ShortcutStore shortcutStore, SettingsWindow settingsWindow, ILoggerService loggerService)
        {
            _logger = loggerService;
           
           
            _categoryService = categoryService;
            _store = shortcutStore;
            _settingsWindow = settingsWindow;
            LoadModel();

            // ⭐ 新增分类
            ShortcutsView = CollectionViewSource.GetDefaultView(Shortcuts);
            ShortcutsView.Filter = FilterShortcut;


            ItemDoubleClickCommand = new RelayCommand(p => ExecuteItem(p as ShortcutItem));
            // 分类 对应的事件
            RemoveCommand = new RelayCommand(p => RemoveItem(p as ShortcutItem));
            RenameCommand = new RelayCommand(p => RenameItem(p as ShortcutItem));

            SelectCategoryCommand = new RelayCommand(p =>
            {
                if (p is CategoryItem c)
                    CurrentCategory = c.Name;
            });

            RenameCategoryCommand = new RelayCommand(p =>
            {
                if (p is not CategoryItem category) return;

                var input = Microsoft.VisualBasic.Interaction.InputBox(
                    "重命名分类",
                    "修改分类名",
                    category.Name);

                if (string.IsNullOrWhiteSpace(input) || input == category.Name)
                    return;

                _categoryService.Rename(category, input);

                // 修正快捷方式分类名
                foreach (var s in Shortcuts)
                {
                    if (s.Category == category.Name)
                        s.Category = input;
                }

                Save();
            });


            DeleteCategoryCommand = new RelayCommand(p =>
            {
                if (p is not CategoryItem category) return;
                DeleteCategory(category);
            });



         
            // 数据对应的事件 
            Categories = _categoryService.Categories;

            // 确保有“全部”
            if (!Categories.Any(c => c.Name == "全部"))
                Categories.Insert(0, new CategoryItem { Name = "全部" });
      


        }

      

        private bool FilterShortcut(object obj)
        {
            if (obj is not ShortcutItem item)
                return false;

            if (CurrentCategory == "全部")
                return true;

            return item.Category == CurrentCategory;
        }


        public void AddShortcutFromPath(string path)
        {
            if (CurrentCategory == "全部")
            {
                // 弹窗报警 不能添加到全部项里
                System.Windows.MessageBox.Show("请选择一个分类！不能添加到全部选项！");
                return;
            }
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


            string end = Path.GetFileNameWithoutExtension(Path.GetFileName(path));


            var item = new ShortcutItem
            {
                DisplayName = end,
                TargetPath = target,
                Arguments = args,
                RawSourcePath = path,
                IconPath = target,
                Category = CurrentCategory
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
                System.Windows.MessageBox.Show($"无法启动：{ex.Message}");
            }
        }

        private void RemoveItem(ShortcutItem? item)
        {
            if (item == null) return;
            if (System.Windows.MessageBox.Show($"移除 {item.DisplayName} ?", "确认", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            Shortcuts.Remove(item);
            Save();
        }


        // 4. 修正重命名方法（参数是 ShortcutItem，恢复正确逻辑）
        private void RenameItem(ShortcutItem? item)
        {
            if (item == null) return;

            // 弹出输入框，默认显示当前名称
            var input = Microsoft.VisualBasic.Interaction.InputBox("重命名", "修改显示名", item.DisplayName);
            if (!string.IsNullOrWhiteSpace(input) && input != item.DisplayName)
            {
                // 修改 ShortcutItem 的 DisplayName（关键！）
                item.DisplayName = input;
                Save();
                // 如果 ShortcutItem 实现了 INotifyPropertyChanged，这里不需要 Refresh
                // ShortcutsView.Refresh();
            }
        }

        public void DeleteCategory(CategoryItem category)
        {
            if (category == null) return;

            _logger.Info($"删除分类：{category.Name}");

            // 1️⃣ 删分类（持久化）
            _categoryService.Delete(category);

            // 2️⃣ 修复快捷方式引用
            foreach (var s in Shortcuts)
            {
                if (s.Category == category.Name)
                {
                    s.Category = "全部";
                }
            }

            // 3️⃣ 当前分类回退
            if (CurrentCategory == category.Name)
            {
                CurrentCategory = "全部";
            }

            Save(); // 保存快捷方式
        }


     

    }
}
