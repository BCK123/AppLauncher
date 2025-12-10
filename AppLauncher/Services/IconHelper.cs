using System;
using System.Drawing; // 需安装System.Drawing.Common才能识别
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppLauncher.Services
{
    public static class IconHelper
    {
        // 从 exe/dll/快捷方式提取图标（返回48x48的ImageSource，失败返回null）
        public static ImageSource? GetIconImage(string path)
        {
            // 1. 前置校验：路径为空/文件不存在直接返回null
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return null;

            Icon? ico = null;
            try
            {
                // 提取关联图标（支持exe/dll/lnk等文件）
                ico = Icon.ExtractAssociatedIcon(path);
                if (ico == null)
                    return null;

                // 将System.Drawing.Icon转为WPF的ImageSource
                var hIcon = ico.Handle;
                var imageSource = Imaging.CreateBitmapSourceFromHIcon(
                    hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(48, 48)
                );

                // 标记为不可变，避免内存泄漏
                imageSource.Freeze();
                return imageSource;
            }
            // 捕获具体异常，方便调试（避免空catch吞错）
            catch (FileNotFoundException)
            {
                // 文件不存在（已前置校验，这里是兜底）
                return null;
            }
            catch (ArgumentException ex)
            {
                // 路径无效/文件无图标
                Console.WriteLine($"提取图标失败：路径无效 - {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                // 其他异常（如权限不足）
                Console.WriteLine($"提取图标异常：{ex.Message}");
                return null;
            }
            finally
            {
                // 手动释放Icon资源，避免句柄泄漏（关键！）
                ico?.Dispose();
            }
        }
    }
}