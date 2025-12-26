using AppLauncher.Models;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.WebRequestMethods;

namespace AppLauncher.Services
{
    public class CategoryService
    {
        private readonly string _filePath;
        private static CategoryService? _instance;
        public static CategoryService Instance
            => _instance ??= new CategoryService();

        public ObservableCollection<CategoryItem> Categories { get; }

        public CategoryService()
        {
            _filePath = SettingsService.Instance.GetCategoryFilePath();


            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            Categories = Load();
        }

        #region Public API

        /// <summary>
        /// 新增分类
        /// </summary>
        public CategoryItem Add(string name)
         {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("分类名不能为空");

            if (Categories.Any(c => c.Name == name))
                throw new InvalidOperationException("分类已存在");

            var item = new CategoryItem
            {
                Name = name
            };

            Categories.Add(item);
            Save();

            return item;
        }

        public void Delete(CategoryItem category)
        {
            if (category == null) return;

            if (category.Name == "全部")
                throw new InvalidOperationException("不能删除默认分类");
            // 弹窗二次确认
            var confirmResult = HandyControl.Controls.MessageBox.Show($"确定要删除分类「{category.Name}」吗？",
                "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);

            Categories.Remove(category);
            Save();
        }

        public void Rename(CategoryItem category, string newName)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            newName = newName.Trim();

            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("分类名不能为空");

            if (category.Name == "全部")
                throw new InvalidOperationException("不能重命名默认分类");

            if (Categories.Any(c =>
                    !ReferenceEquals(c, category) &&
                    string.Equals(c.Name, newName, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("分类名已存在");

            string oldName = category.Name;

            // ① 更新分类自身
            category.Name = newName;

   

            // ③ 持久化
            Save();
        }




        private ObservableCollection<CategoryItem> Load()
        {
            if (!System.IO.File.Exists(_filePath))
            {
                // 初始默认分类
                return new ObservableCollection<CategoryItem>
                {
                    new CategoryItem { Name = "全部" }
                };
            }

            try
            {
                var json = System.IO.File.ReadAllText(_filePath);
                var list = JsonSerializer.Deserialize<List<CategoryItem>>(json);

                return new ObservableCollection<CategoryItem>(
                    list ?? new List<CategoryItem>()
                );
            }
            catch
            {
                // JSON 损坏时兜底
                return new ObservableCollection<CategoryItem>
                {
                    new CategoryItem { Name = "全部" }
                };
            }
        }
        public void Save()
        {
            var json = JsonSerializer.Serialize(
         Categories,
         new JsonSerializerOptions { WriteIndented = true }
     );

            var tmp = _filePath + ".tmp";

            // ① 先写临时文件
            System.IO.File.WriteAllText(tmp, json);

            // ② 再原子替换正式文件
            System.IO.File.Move(tmp,_filePath, true);
        }

    }
    #endregion
}

