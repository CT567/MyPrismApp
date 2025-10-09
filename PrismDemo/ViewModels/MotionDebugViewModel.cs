using Prism.Commands;
using Prism.Mvvm;
using PrismDemo.Models;
using PrismDemo.Services.Motion;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;

namespace PrismDemo.ViewModels
{
    public class MotionDebugViewModel : BindableBase
    {
        private readonly IMotion _motionService;
        private IAxis _selectedAxis;
        private string _selectedPlatform;
        private ushort _selectedCardId;
        private string _cardStatus;
        private string _axisStatus;
        private string _limitStatus;

        // 泛型集合属性 ObservableCollection
        public ObservableCollection<IAxis> Axes { get; } = new ObservableCollection<IAxis>();
        public ObservableCollection<string> Platforms { get; } = new ObservableCollection<string>();
        public ObservableCollection<ushort> CardIds { get; } = new ObservableCollection<ushort>();
        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();

        // 命令定义
        public DelegateCommand InitCardCommand { get; }
        public DelegateCommand HotResetCommand { get; }
        public DelegateCommand CoolResetCommand { get; }
        public DelegateCommand LoadIniCommand { get; }
        public DelegateCommand LoadEniCommand { get; }
        public DelegateCommand EnableAllAxesCommand { get; }
        public DelegateCommand JogNegativeCommand { get; }
        public DelegateCommand JogPositiveCommand { get; }
        public DelegateCommand HomeCommand { get; }
        public DelegateCommand ClearPosCommand { get; }
        public DelegateCommand EmgStopCommand { get; }
        public DelegateCommand EnableAxisCommand { get; }
        public DelegateCommand RelativeMoveCommand { get; }
        public DelegateCommand AbsoluteMoveCommand { get; }
        public DelegateCommand DecStopCommand { get; }

        public MotionDebugViewModel(IMotion motionService)
        {
            _motionService = motionService;
            //第一个参数OnHotReset：命令的核心执行逻辑（点击按钮后实际运行的代码）。
            //第二个参数CanExecuteCardOperation：执行条件判断方法，返回bool值：
            //返回true：命令可执行（按钮启用）。
            //返回false：命令不可执行（按钮禁用）。

            // 初始化命令
            InitCardCommand = new DelegateCommand(OnInitCard);
            HotResetCommand = new DelegateCommand(OnHotReset, CanExecuteCardOperation)
                .ObservesProperty(() => SelectedCardId);
            CoolResetCommand = new DelegateCommand(OnCoolReset);
            LoadIniCommand = new DelegateCommand(OnLoadIni, CanExecuteCardOperation)
                .ObservesProperty(() => SelectedCardId);
            LoadEniCommand = new DelegateCommand(OnLoadEni, CanExecuteCardOperation)
                .ObservesProperty(() => SelectedCardId);
            EnableAllAxesCommand = new DelegateCommand(OnEnableAllAxes, CanExecuteCardOperation)
                .ObservesProperty(() => SelectedCardId);

            // 单轴命令
            JogNegativeCommand = new DelegateCommand(OnJogNegative, CanExecuteAxisOperation)
                .ObservesProperty(() => SelectedAxis);
            JogPositiveCommand = new DelegateCommand(OnJogPositive, CanExecuteAxisOperation)
                .ObservesProperty(() => SelectedAxis);
            HomeCommand = new DelegateCommand(OnHome, CanExecuteAxisOperation)
                .ObservesProperty(() => SelectedAxis);
            ClearPosCommand = new DelegateCommand(OnClearPos, CanExecuteAxisOperation)
                .ObservesProperty(() => SelectedAxis);
            EmgStopCommand = new DelegateCommand(OnEmgStop, CanExecuteAxisOperation)
                .ObservesProperty(() => SelectedAxis);
            EnableAxisCommand = new DelegateCommand(OnEnableAxis, CanExecuteAxisOperation)
                .ObservesProperty(() => SelectedAxis);
            RelativeMoveCommand = new DelegateCommand(OnRelativeMove, CanExecuteAxisOperation)
                .ObservesProperty(() => SelectedAxis);
            AbsoluteMoveCommand = new DelegateCommand(OnAbsoluteMove, CanExecuteAxisOperation)
                .ObservesProperty(() => SelectedAxis);
            DecStopCommand = new DelegateCommand(OnDecStop, CanExecuteAxisOperation)
                .ObservesProperty(() => SelectedAxis);

            // 初始化测试数据
            InitTestData();
        }

        // 属性访问器
        public IAxis SelectedAxis
        {
            get => _selectedAxis;
            set
            {
                SetProperty(ref _selectedAxis, value);
                UpdateAxisStatus();
            }
        }

        public string SelectedPlatform
        {
            get => _selectedPlatform;
            set => SetProperty(ref _selectedPlatform, value);
        }

        public ushort SelectedCardId
        {
            get => _selectedCardId;
            set => SetProperty(ref _selectedCardId, value);
        }

        public string CardStatus
        {
            get => _cardStatus;
            set => SetProperty(ref _cardStatus, value);
        }

        public string AxisStatus
        {
            get => _axisStatus;
            set => SetProperty(ref _axisStatus, value);
        }

        public string LimitStatus
        {
            get => _limitStatus;
            set => SetProperty(ref _limitStatus, value);
        }

        // 命令执行方法
        private void OnInitCard()
        {
            try
            {
                var result = _motionService.InitCard();
                CardStatus = result ? "卡初始化成功" : "卡初始化失败";
                Log($"卡初始化：{(result ? "成功" : "失败")}");
            }
            catch (Exception ex)
            {
                Log($"初始化失败：{ex.Message}");
            }
        }

        private void OnHotReset()
        {
            var result = _motionService.HotReset(SelectedCardId);
            Log($"热复位 (卡号：{SelectedCardId})：{(result ? "成功" : "失败")}");
        }

        private void OnCoolReset()
        {
            var result = _motionService.CoolReset();
            Log($"冷复位：{(result ? "成功" : "失败")}");
        }

        private void OnLoadIni()
        {
            var dialog = new OpenFileDialog { Filter = "INI文件|*.ini" };
            if (dialog.ShowDialog() == true)
            {
                var result = _motionService.LoadINI(SelectedCardId, dialog.FileName);
                Log($"加载INI文件 {Path.GetFileName(dialog.FileName)}：{(result ? "成功" : "失败")}");
            }
        }

        private void OnLoadEni()
        {
            var dialog = new OpenFileDialog { Filter = "ENI文件|*.eni" };
            if (dialog.ShowDialog() == true)
            {
                // 修复了参数传递错误，添加了SelectedCardId参数
                var result = _motionService.LoadENI(SelectedCardId, dialog.FileName);
                Log($"加载ENI文件 {Path.GetFileName(dialog.FileName)}：{(result ? "成功" : "失败")}");
            }
        }

        private void OnEnableAllAxes()
        {
            var result = _motionService.EnableAllAxes(SelectedCardId);
            Log($"使能所有轴 (卡号：{SelectedCardId})：{(result ? "成功" : "失败")}");
            RefreshAxesStatus();
        }

        private void OnJogNegative()
        {
            // 实际项目中需要实现Jog逻辑
            Log($"轴 {SelectedAxis.Name} 负向点动");
        }

        private void OnJogPositive()
        {
            // 实际项目中需要实现Jog逻辑
            Log($"轴 {SelectedAxis.Name} 正向点动");
        }

        private void OnHome()
        {
            var result = _motionService.SingleAxisGoHome(SelectedAxis);
            Log($"轴 {SelectedAxis.Name} 回零：{(result ? "成功" : "失败")}");
        }

        private void OnClearPos()
        {
            var result = _motionService.SetSingleAxisPositionZero(SelectedAxis);
            Log($"轴 {SelectedAxis.Name} 清零：{(result ? "成功" : "失败")}");
        }

        private void OnEmgStop()
        {
            var result = _motionService.CardEmgStop(SelectedAxis);
            Log($"轴 {SelectedAxis.Name} 急停：{(result ? "成功" : "失败")}");
        }

        private void OnEnableAxis()
        {
            var result = SelectedAxis.IsEnabled
                ? _motionService.SingleAxisDisable(SelectedAxis)
                : _motionService.SingleAxisEnable(SelectedAxis);

            Log($"轴 {SelectedAxis.Name} {(SelectedAxis.IsEnabled ? "禁用" : "使能")}：{(result ? "成功" : "失败")}");
            RefreshAxesStatus();
        }

        private void OnRelativeMove()
        {
            // 实际项目中需要实现相对移动逻辑
            Log($"轴 {SelectedAxis.Name} 相对移动到 {SelectedAxis.TargetPosition}");
        }

        private void OnAbsoluteMove()
        {
            // 实际项目中需要实现绝对移动逻辑
            Log($"轴 {SelectedAxis.Name} 绝对移动到 {SelectedAxis.TargetPosition}");
        }

        private void OnDecStop()
        {
            var result = _motionService.SingleAxisDecStop(SelectedAxis);
            Log($"轴 {SelectedAxis.Name} 减速停止：{(result ? "成功" : "失败")}");
        }

        // 辅助方法
        private bool CanExecuteCardOperation() => SelectedCardId > 0;

        private bool CanExecuteAxisOperation() => SelectedAxis != null;

        private void Log(string message)
        {
            LogMessages.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
            // 限制日志数量
            while (LogMessages.Count > 100)
            {
                LogMessages.RemoveAt(LogMessages.Count - 1);
            }
        }

        private void UpdateAxisStatus()
        {
            if (SelectedAxis == null)
            {
                AxisStatus = "";
                LimitStatus = "";
                return;
            }

            AxisStatus = _motionService.GetAixsState(SelectedAxis);
            var posLimit = _motionService.GetSingleAxisPositiveLimit(SelectedAxis) ? "正向限位触发" : "正常";
            var negLimit = _motionService.GetSingleAxisNegativeLimit(SelectedAxis) ? "负向限位触发" : "正常";
            var homeLimit = _motionService.GetSingleHomeLimit(SelectedAxis) ? "回零限位触发" : "正常";
            LimitStatus = $"正向：{posLimit}，负向：{negLimit}，回零：{homeLimit}";
        }

        /// <summary>
        /// 刷新所有轴的状态
        /// </summary>
        private void RefreshAxesStatus()
        {
            // 实际项目中需要从硬件刷新状态
            foreach (var axis in Axes)
            {
                RaisePropertyChanged(nameof(Axes));
            }
            UpdateAxisStatus();
        }

        private void InitTestData()
        {
            // 初始化卡号列表
            CardIds.Add(1);
            CardIds.Add(2);
            SelectedCardId = 1;

            // 初始化平台列表
            Platforms.Add("XYZ平台");
            Platforms.Add("SCARA机器人");
            Platforms.Add("Delta机器人");
            SelectedPlatform = "XYZ平台";

            // 添加测试轴
            Axes.Add(new LeiSaiAxis
            {
                CardID = 1,
                AxisID = 1,
                Name = "X轴",
                CurrentPosition = 0,
                TargetPosition = 0,
                MaxSpeed = 100,
                AccTime = 10,
                IsEnabled = false,
                IsHomed = false
            });

            Axes.Add(new LeiSaiAxis
            {
                CardID = 1,
                AxisID = 2,
                Name = "Y轴",
                CurrentPosition = 0,
                TargetPosition = 0,
                MaxSpeed = 100,
                AccTime = 10,
                IsEnabled = false,
                IsHomed = false
            });
        }
    }
}
