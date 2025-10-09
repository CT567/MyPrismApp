using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions; // 引入 Prism 区域导航核心命名空间

namespace PrismDemo.ViewModels
{
    // MainWindow 对应的 ViewModel，继承 Prism 的 BindableBase（属性通知）
    public class MainWindowViewModel : BindableBase
    {
        // 注入 Prism 的 IRegionManager（用于控制区域导航）
        private readonly IRegionManager _regionManager;

        // 构造函数：通过依赖注入获取 IRegionManager
        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            // 初始化导航命令：绑定导航逻辑
            NavigateCommand = new DelegateCommand<string>(NavigateToView);

            // （可选）启动时默认导航到 MotionDebugView（即“运动调试”界面）
            NavigateToView("MotionView");
        }

        // 导航命令：对应 MainWindow 中按钮的 Command="{Binding NavigateCommand}"
        public DelegateCommand<string> NavigateCommand { get; private set; }

        // 导航核心逻辑：根据 CommandParameter（如 "MotionView"）加载对应视图到 MainRegion
        private void NavigateToView(string viewName)
        {
            if (!string.IsNullOrEmpty(viewName))
            {
                // 关键API：RequestNavigate(区域名, 视图标识)
                // 区域名：MainRegion（与 MainWindow 中 ContentControl 的 RegionName 一致）
                // 视图标识：MotionView（与 Step 2 中注册的导航标识一致）
                _regionManager.RequestNavigate("MainRegion", viewName);
            }
        }

        // （可选）底部状态栏的 ShowSystemStatusCommand（根据需求实现）
        public DelegateCommand ShowSystemStatusCommand { get; } = new DelegateCommand(() =>
        {
            // 状态栏按钮逻辑（如弹窗显示状态）
            System.Windows.MessageBox.Show("系统状态：正常");
        });



    }
}