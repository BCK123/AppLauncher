using AppLauncher.Services;
using AppLauncher.Core.Log;
using AppLauncher.Utils.Utils.DataBase;
using AppLauncher.Utils.Utils.StartupUtil;
using AppLauncher.ViewModels;
using AppLauncher.Views;
using DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
using System.IO;
using System.Linq;
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


        // 应用启动时执行
        protected override void OnInitialized()
        {
            base.OnInitialized();

            var logger = Container.Resolve<ILoggerService>();
            try
            {
                logger.Info("正在清理临时文件夹中的图片缓存...");
                // 清理缓存
                Clean.DeleteImageCache();
            }
            catch
            {
                logger.Warn("处理图片缓存出现异常！");
            }

           
            // 初始化数据库
            DbInitializer.Initialize();
        }

     
    }
}
