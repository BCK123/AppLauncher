using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AppLauncher.Models
{
    public class ShortcutItem : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
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

        public string TargetPath { get; set; } = "";
        public string? Arguments { get; set; }
        public string? IconPath { get; set; } // 可选：缓存 icon 文件路径或目标 exe 路径
        public string? RawSourcePath { get; set; } // 用户拖入的原始路径（.lnk 的路径或 exe 路径）
                                                   // ⭐ 新增
        public string Category { get; set; } = "全部";

        // ===== INotifyPropertyChanged =====
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
