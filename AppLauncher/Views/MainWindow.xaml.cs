using AppLauncher.Models;
using AppLauncher.Services;
using AppLauncher.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace AppLauncher.Views

{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel Vm => (MainViewModel)DataContext;
        public MainWindow()
        {

            InitializeComponent();
        }

        private void Border_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy; // ⭐ 必须
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true; // ⭐ 必须
        }

        // 拖曳函数
        private void Border_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                foreach (var f in files)
                {


                    Vm.AddShortcutFromPath(f);
                }
            }

        }


        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // TODO：以后这里换成 Command
           
            var addCategory = new addCategory();
            addCategory.Owner = this;
            addCategory.ShowDialog();
        }

        // BtnSettings_Click 点击跳转到SettingsWindow.xaml界面
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        // 分类按钮
        private void Category_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm &&
                sender is Button btn)
            {
                vm.CurrentCategory = btn.Content?.ToString() ?? "全部";
            }
        }


        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menu &&
                menu.DataContext is CategoryItem category &&
                DataContext is MainViewModel vm)
            {
                if (MessageBox.Show(
                        $"确定删除分类「{category.Name}」？\n该分类下的项目将移到【全部】",
                        "确认",
                        MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;

                vm.DeleteCategory(category);
            }
        }


        // 重命名分类
        private void RenameCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menu)
                return;

            if (menu.DataContext is not CategoryItem category)
                return;
            // 获取老名
            var oldName = category.Name;
            // 弹输入框
            var input = Microsoft.VisualBasic.Interaction.InputBox(
                "请输入新的分类名称",
                "重命名分类",
                category.Name);

            if (string.IsNullOrWhiteSpace(input))
                return;

            // 不允许重名
            var categories = CategoryService.Instance.Categories;
            //categories的第一个下标的name是全部
               
     
            if (input == "全部")
            {
                MessageBox.Show("不能重命名为全部", "提示");
                return;
            }
            if (categories.Any(c => c != category && c.Name == input))
            {
                MessageBox.Show("已存在同名分类", "提示");
                return;
            }

            // 修改名称
            ShortcutStore.Instance.Update(oldName, input);

            // 修改名称（UI 自动刷新）
            category.Name = input;

            // 持久化保存
            CategoryService.Instance.Save();
         

        }

    }
}

