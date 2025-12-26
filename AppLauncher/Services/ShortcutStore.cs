using AppLauncher.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppLauncher.Services
{
    public class ShortcutStore
    {
        // 替换原有单例代码，保证全局只有一个实例
        private static readonly ShortcutStore _instance = new ShortcutStore(); // 饿汉式，启动就创建
        public static ShortcutStore Instance => _instance; // 只读，无法被重新赋值
        private readonly string _file;
        public ObservableCollection<ShortcutItem> Shortcuts { get; } = new ObservableCollection<ShortcutItem>();
        public ShortcutStore()
        {
            _file = SettingsService.Instance.GetShortcutsFilePath();
          
           Load();
           
        }
      

       
        public IList<ShortcutItem> Load()
        {
          
            if (!File.Exists(_file)) return new List<ShortcutItem>();
            var json = File.ReadAllText(_file);
            var items = JsonSerializer.Deserialize<List<ShortcutItem>>(json);
            foreach (var item in items)
            {
                Shortcuts.Add(item);
            }
            return items ?? new List<ShortcutItem>();
        }

        public void Save(IEnumerable<ShortcutItem> items)
        {
            var tmp = _file + ".tmp";
            var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(tmp, json);
            File.Move(tmp, _file, true);
            Load();
        }
        // 查询 所有ShortcutItem 通过Category 字段
        public IEnumerable<ShortcutItem> Query(string category)
        {
          // MessageBox.Show($"Shortcuts 元素数量：{Shortcuts?.Count() ?? -1}");
            return Shortcuts.Where(item => item.Category == category);
        }

        // 更新
        // 修正后的 Update 方法（逻辑对+有调试+有防御）
        public void Update(string oldCategory, string newCategory)
        {
            // 防御1：空值校验（避免传 null/空字符串）
            if (string.IsNullOrWhiteSpace(oldCategory))
            {
                MessageBox.Show("旧分类不能为空！");
                return;
            }
            if (string.IsNullOrWhiteSpace(newCategory))
            {
                MessageBox.Show("新分类不能为空！");
                return;
            }
            // 防御2：新旧分类相同，无需更新
            //if (oldCategory == newCategory)
            //{
            //    MessageBox.Show("新旧分类不能相同！");
            //    return;
            //}

            // 关键：转 ToList() 固化查询结果，避免延迟执行问题
            var itemsToUpdate = Query(oldCategory).ToList();

            // 调试：打印要更新的项数（能直接看出是否查到数据）
            System.Diagnostics.Debug.WriteLine($"找到要更新的项数：{itemsToUpdate.Count}");

            // 防御3：没查到数据直接提示
            if (itemsToUpdate.Count == 0)
            {
                MessageBox.Show($"未找到分类为「{oldCategory}」的项，无法更新！");
                return;
            }

            // 真正执行更新（修改分类）
            foreach (var item in itemsToUpdate)
            {
                item.Category = newCategory;
                // 调试：打印修改前后的分类（确认改了）
               // System.Diagnostics.Debug.WriteLine($"项 {item.Name}：旧分类={oldCategory} → 新分类={newCategory}");
            }

            // 核心：直接保存当前类的 Shortcuts（类内方法无需再调 Instance）
            try
            {
                Save(Shortcuts);
                MessageBox.Show($"成功更新 {itemsToUpdate.Count} 个项！");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败：{ex.Message}");
            }
        }
    }

}
