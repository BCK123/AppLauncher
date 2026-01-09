using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AppLauncher.Models
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices; // 需添加这个引用（CallerMemberName 用）

    public class ShortcutItem : INotifyPropertyChanged
    {
        // 1. 已有字段（保留）
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // 2. DisplayName（已有正确的通知逻辑，保留）
        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    OnPropertyChanged();
                }
            }
        }

        // 3. 其他普通属性（保留，若后续需要修改这些属性并刷新UI，也按此格式改）
        public string TargetPath { get; set; } = "";
        public string? Arguments { get; set; }
        public string? IconPath { get; set; }
        public string? RawSourcePath { get; set; }

        // ===== 核心修改：给 Category 加通知逻辑 =====
        private string _category = "全部"; // 私有字段，默认值移到这里
        public string Category
        {
            get => _category; // 读取私有字段
            set
            {
                // 只有值变化时才触发通知（避免无效刷新）
                if (_category != value)
                {
                    _category = value;
                    OnPropertyChanged(); // 通知UI：Category属性已修改
                }
            }
        }

        // ===== INotifyPropertyChanged 接口实现（保留）=====
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


     

    }

}