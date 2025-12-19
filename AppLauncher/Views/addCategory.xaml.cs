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

using AppLauncher.Services;
namespace AppLauncher.Views
{
    /// <summary>
    /// addCategory.xaml 的交互逻辑
    /// </summary>
    public partial class addCategory : Window
    {
       
        public addCategory()
        {
            InitializeComponent();
           
        }
        private void BtnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            // 先去除首尾空格，避免空字符/纯空格的无效输入
            string categoryName = txtCategoryName.Text.Trim();

            // 输入校验：判断是否为空
            if (string.IsNullOrEmpty(categoryName))
            {
                MessageBox.Show("请输入分类名称！", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCategoryName.Focus(); // 聚焦回输入框，方便用户重新输入
                return;
            }
            try
            {
                // 构造 单实例 不需要构造函数注入
                CategoryService.Instance.Add(categoryName);
                DialogResult = true;   // ⭐ 通知调用方成功
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
   
    
    
    }
}
