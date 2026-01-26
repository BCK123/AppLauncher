using AppLauncher.Models;
using AppLauncher.Services;
using AppLauncher.Services.Log;
using AppLauncher.Utils;
using AppLauncher.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace AppLauncher.ViewModels
{
    //Prism 默认 只认识MainWindowViewModel 这个名字。
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        public ICommand ShowShortcutCommand { get; }
        public ICommand ShowVisionCommand { get; }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }
        public ICommand OpenSettingsCommand { get; }

        private readonly SettingsService _settingsService;
        public MainWindowViewModel(IRegionManager regionManager,SettingsService settingsService)
        {
            _regionManager = regionManager;
            _settingsService = settingsService;
            Monitor = new SystemMonitorService();

            OpenSettingsCommand = new RelayCommand(p => BtnSettings_Click());

            // 正常尺寸
            ShowShortcutCommand = new DelegateCommand(() =>
            {
                _regionManager.RequestNavigate("MainRegion", nameof(ShortcutView));

                Application.Current.MainWindow.WindowState = WindowState.Normal;
            });
            // 最大化窗口尺寸  
            ShowVisionCommand = new DelegateCommand(() =>
            {
                _regionManager.RequestNavigate("MainRegion", nameof(VisionView));

               //Application.Current.MainWindow.WindowState = WindowState.Maximized;
            });



            // 监控
            // UI 就绪后再启动
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                new Action(() => Monitor.Start()),
                System.Windows.Threading.DispatcherPriority.Background);

        }

        private void BtnSettings_Click()
        {
            var settingsWindow = new SettingsWindow(_settingsService);
            settingsWindow.ShowDialog();
        }

        public SystemMonitorService Monitor { get; }
    }

}
