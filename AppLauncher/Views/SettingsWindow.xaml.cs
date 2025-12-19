using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Windows.Forms; // 需要引用 System.Windows.Forms
using AppLauncher.Services;

namespace AppLauncher.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            var current = SettingsService.Instance.Settings.JsonStoreDirectory;
            TxtDir.Text = current ?? string.Empty;
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "选择用于存储快捷项的根目录（将在该目录下创建 AppLauncher 子文件夹）",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TxtDir.Text = dlg.SelectedPath;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var newDir = string.IsNullOrWhiteSpace(TxtDir.Text) ? null : TxtDir.Text.Trim();    
            string message = SettingsService.Instance.SetJsonStoreDirectory(newDir);
            if (!message.Equals("yes"))
            {
                System.Windows.MessageBox.Show(message);
                return;
            }
            System.Windows.MessageBox.Show("设置已保存，需要重载快捷项以应用新路径。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
