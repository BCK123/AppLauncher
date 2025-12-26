using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AppLauncher.Converters
{ /// <summary>
  /// 分类名称转是否禁用右键菜单的转换器
  /// 当名称为“全部”时返回Collapsed（隐藏），否则返回Visible（显示）
  /// </summary>
    public class VisibilityConverter : IValueConverter
    {
        // 正向转换（名称→可见性）
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 空值处理
            if (value == null) return Visibility.Collapsed;

            string categoryName = value.ToString();
            // 如果是“全部”，隐藏右键菜单；否则显示
            return categoryName == "全部" ? Visibility.Collapsed : Visibility.Visible;
        }

        // 反向转换（无需实现，返回原值即可）
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
