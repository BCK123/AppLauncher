using AppLauncher.Models;
using AppLauncher.Services;
using AppLauncher.ViewModels;
using Prism.Ioc;
using Prism.Navigation.Regions;
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
        private readonly CategoryService _categoryService;

        private readonly IContainerProvider _container;
        private readonly SettingsService _settingsService;
        private readonly ShortcutStore _shortcutStore;
        private readonly IRegionManager _regionManager;


        public MainWindow(IRegionManager regionManager,IContainerProvider container, CategoryService categoryService, SettingsService settingsService, ShortcutStore shortcutStore)
        {
            _settingsService = settingsService;
            _categoryService = categoryService;
            _container = container;
            _shortcutStore = shortcutStore;

            InitializeComponent();
            _regionManager = regionManager;
            // 方式一 加载默认界面
            Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _regionManager.RequestNavigate("MainRegion", nameof(ShortcutView));
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // TODO：以后这里换成 Command
           
            var addCategory = _container.Resolve<addCategory>();
            addCategory.Owner = this;
            addCategory.ShowDialog();
        }

        // BtnSettings_Click 点击跳转到SettingsWindow.xaml界面
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_settingsService);
            settingsWindow.ShowDialog();
        }

        // 分类按钮
     

    }
}

