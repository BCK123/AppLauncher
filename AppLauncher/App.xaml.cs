using AppLauncher.Services;
using AppLauncher.Services.Log;
using AppLauncher.ViewModels;
using AppLauncher.Views;
using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

            // 注册日志
            containerRegistry.RegisterSingleton<ILoggerService, SerilogLoggerService>();

        }

    }
}
