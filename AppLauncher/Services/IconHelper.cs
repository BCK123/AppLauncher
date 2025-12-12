using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppLauncher.Services
{
    public static class IconHelper
    {
        // Windows原生API：获取文件信息（包括图标）
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        // 图标尺寸常量
        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0; // 大图标(32x32)
        private const uint SHGFI_SMALLICON = 0x1; // 小图标(16x16)
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

        // 存储文件信息的结构体
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;          // 图标句柄
            public int iIcon;             // 图标索引
            public uint dwAttributes;     // 文件属性
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;  // 显示名
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;     // 类型名
        }

        // 释放图标句柄的API
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// WPF原生提取文件图标（无System.Drawing依赖）
        /// </summary>
        /// <param name="filePath">文件路径（EXE、快捷方式、文件夹等）</param>
        /// <param name="isLargeIcon">是否返回大图标（32x32），false为16x16</param>
        /// <returns>WPF的ImageSource，失败返回null</returns>
        public static ImageSource? GetIconImage(string filePath, bool isLargeIcon = true)
        {
            // 基础校验
            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            try
            {
                SHFILEINFO shfi = new SHFILEINFO();
                uint flags = SHGFI_ICON | (isLargeIcon ? SHGFI_LARGEICON : SHGFI_SMALLICON);

                // 调用Windows API获取图标句柄
                IntPtr hIcon = SHGetFileInfo(
                    filePath,
                    FILE_ATTRIBUTE_NORMAL,
                    ref shfi,
                    (uint)Marshal.SizeOf(shfi),
                    flags);

                if (hIcon == IntPtr.Zero || shfi.hIcon == IntPtr.Zero)
                    return null;

                // 将原生图标句柄转换为WPF的ImageSource
                ImageSource iconSource = Imaging.CreateBitmapSourceFromHIcon(
                    shfi.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                // 释放图标句柄（避免内存泄漏）
                DestroyIcon(shfi.hIcon);

                // 冻结ImageSource提升性能
                iconSource.Freeze();
                return iconSource;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"提取图标失败：{ex.Message}");
                return null;
            }
        }

        // 简化重载（默认返回大图标）
        public static ImageSource? GetIconImage(string filePath)
        {
            return GetIconImage(filePath, true);
        }
    }
}