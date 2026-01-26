using AppLauncher.Services;
using AppLauncher.Services.Log;
using AppLauncher.Utils;
using AppLauncher.ViewModels;
using AppLauncher.Views;
using DryIoc;
using Prism.Container.DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AppLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {

        //public void OnInitialized(IContainerProvider containerProvider)
        //{
        //    // 方式二 加载默认界面 好像不管用
        //    //var regionManager = containerProvider.Resolve<IRegionManager>();
        //    //regionManager.RegisterViewWithRegion("MainRegion", typeof(ShortcutView));
        //}
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<CategoryService, CategoryService>();
            containerRegistry.RegisterSingleton<ShortcutStore, ShortcutStore>();
            containerRegistry.RegisterSingleton<SettingsService,SettingsService>();
            containerRegistry.RegisterSingleton<MainWindowViewModel>();

          

            // 注册界面
            containerRegistry.Register<addCategory>(); // 👈 关键
            containerRegistry.RegisterForNavigation<ShortcutView>();
            containerRegistry.RegisterForNavigation<VisionView>();

            // 注册日志
            containerRegistry.RegisterSingleton<ILoggerService, SerilogLoggerService>();

        }


        // 在App.xaml.cs的OnExit方法中添加
        protected override void OnExit(ExitEventArgs e)
        {
            // 清理YOLO检测的临时图片
            var tempFiles = Directory.GetFiles(Path.GetTempPath(), "yolo_detect_*.png");
            foreach (var file in tempFiles)
            {
                try { File.Delete(file); } catch { }
            }
            base.OnExit(e);
        }
    }
}
