
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using PrismDemo.Services.Camera;
using PrismDemo.Services.Motion;
using PrismDemo.Services.Motion.LeiSai;
using PrismDemo.Views;
using System.Windows;


namespace PrismDemo
{
    /// <summary>
    /// 入口
    /// </summary>
    public partial class App : PrismApplication
    {
        /// <summary>
        /// 指定主界面
        /// </summary>
        /// <returns></returns>
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        /// <summary>
        /// 配置模块目录：决定模块的加载顺序
        /// </summary>
        /// <param name="moduleCatalog"></param>
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            // 一定要调用 base 方法，否则可能报错
            base.ConfigureModuleCatalog(moduleCatalog);

            // 添加 DeviceModule 模块（类似 WinForm 加载插件）
            //moduleCatalog.AddModule<Modules.DeviceModule.DeviceModule>();
        }

        /// <summary>
        /// 注册类型: 依赖注入
        /// </summary>
        /// <param name="containerRegistry"></param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //注册运动控制服务（接口 + 实现） 格式：containerRegistry.Register<接口类型, 实现类型>();
            containerRegistry.Register<IMotion, LeiSaiMotion>();

            // 注册MotionDebugView导航视图
            containerRegistry.RegisterForNavigation<MotionDebugView>("MotionView");

        }

        /// <summary>
        /// 初始化，导航到主界面
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            // 导航 MotionDebugView 到 MainRegion
            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RequestNavigate("MainRegion", "MotionView");
        }


    }
}