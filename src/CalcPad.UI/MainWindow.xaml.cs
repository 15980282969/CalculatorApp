using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Reflection;

namespace CalcPad.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // 设置DataContext
            DataContext = new ViewModels.MainViewModel();
            
            // 注册全局键盘快捷键
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            
            // 聚焦到输入框
            Loaded += async (s, e) =>
            {
                // 等待配置加载完成
                await Task.Delay(100);
                
                // 恢复窗口尺寸和位置
                if (DataContext is ViewModels.MainViewModel vm)
                {
                    var settings = vm.SettingsService.Settings;
                    
                    // 恢复窗口尺寸
                    if (settings.WindowWidth > 0 && settings.WindowHeight > 0)
                    {
                        // 确保不超过屏幕尺寸
                        var maxWidth = SystemParameters.WorkArea.Width;
                        var maxHeight = SystemParameters.WorkArea.Height;
                        
                        Width = Math.Min(settings.WindowWidth, maxWidth);
                        Height = Math.Min(settings.WindowHeight, maxHeight);
                    }
                    
                    // 恢复窗口位置
                    if (!double.IsNaN(settings.WindowTop) && !double.IsNaN(settings.WindowLeft))
                    {
                        // 确保窗口在可见区域内
                        if (settings.WindowLeft < SystemParameters.WorkArea.Right && 
                            settings.WindowTop < SystemParameters.WorkArea.Bottom)
                        {
                            Left = settings.WindowLeft;
                            Top = settings.WindowTop;
                        }
                    }
                }
            };
            
            // 注册窗口尺寸变化事件
            SizeChanged += MainWindow_SizeChanged;
            LocationChanged += MainWindow_LocationChanged;
            Closing += MainWindow_Closing;
        }

        /// <summary>
        /// 全局键盘快捷键处理
        /// </summary>
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 注：Ctrl+C/V/Z 已由 TextBox 和 WPF 内置处理，不需要全局拦截
            // 只处理特殊快捷键
            
            // F5 - 刷新/重新计算
            if (e.Key == Key.F5)
            {
                if (DataContext is ViewModels.MainViewModel vm)
                {
                    vm.CalculateCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// 复制结果按钮点击事件
        /// </summary>
        private void CopyResultButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm && !string.IsNullOrEmpty(vm.CurrentResult))
            {
                try
                {
                    Clipboard.SetText(vm.CurrentResult);
                    // 可以添加一个提示，例如 Toast 或者按钮动画
                }
                catch
                {
                    // 忽略剪贴板异常
                }
            }
        }

        /// <summary>
        /// 输入框键盘事件
        /// </summary>
        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Enter键触发计算
                if (DataContext is ViewModels.MainViewModel vm)
                {
                    vm.CalculateCommand.Execute(null);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                // Esc键清空输入
                if (DataContext is ViewModels.MainViewModel vm)
                {
                    vm.InputExpression = "";
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// 历史记录双击事件 - 回填公式
        /// </summary>
        private void HistoryList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.ListView listView && 
                listView.SelectedItem is CalcPad.Models.CalculationRecord record)
            {
                if (DataContext is ViewModels.MainViewModel vm)
                {
                    vm.InputExpression = record.Expression;
                }
            }
        }

        /// <summary>
        /// 设置按钮点击事件 - 切换设置面板显示
        /// </summary>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsExpander.Visibility == Visibility.Collapsed)
            {
                SettingsExpander.Visibility = Visibility.Visible;
                SettingsExpander.IsExpanded = true;
            }
            else
            {
                SettingsExpander.IsExpanded = !SettingsExpander.IsExpanded;
                if (!SettingsExpander.IsExpanded)
                {
                    SettingsExpander.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// 窗口尺寸变化事件
        /// </summary>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 延迟保存，避免频繁写入
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                if (WindowState == WindowState.Normal) // 只在正常状态下保存
                {
                    if (DataContext is ViewModels.MainViewModel vm)
                    {
                        vm.SettingsService.Settings.WindowWidth = Width;
                        vm.SettingsService.Settings.WindowHeight = Height;
                        await vm.SettingsService.SaveAsync();
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        /// <summary>
        /// 窗口位置变化事件
        /// </summary>
        private void MainWindow_LocationChanged(object? sender, EventArgs e)
        {
            // 延迟保存
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                if (WindowState == WindowState.Normal)
                {
                    if (DataContext is ViewModels.MainViewModel vm)
                    {
                        vm.SettingsService.Settings.WindowTop = Top;
                        vm.SettingsService.Settings.WindowLeft = Left;
                        await vm.SettingsService.SaveAsync();
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        /// <summary>
        /// 窗口关闭事件 - 保存最终尺寸和位置
        /// </summary>
        private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm)
            {
                // 保存最终尺寸
                if (WindowState == WindowState.Normal)
                {
                    vm.SettingsService.Settings.WindowWidth = Width;
                    vm.SettingsService.Settings.WindowHeight = Height;
                    vm.SettingsService.Settings.WindowTop = Top;
                    vm.SettingsService.Settings.WindowLeft = Left;
                }
                
                await vm.SettingsService.SaveAsync();
            }
        }

        /// <summary>
        /// 变量列表项双击事件 - 弹出对话框修改变量值
        /// </summary>
        private void VariableItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && 
                item.DataContext is ViewModels.MainViewModel.VariableItem variable &&
                DataContext is ViewModels.MainViewModel vm)
            {
                // 弹出对话框让用户输入新值
                var inputDialog = new TextBox
                {
                    Text = variable.Value.ToString("G15"),
                    Width = 200,
                    Margin = new Thickness(10)
                };

                var dialog = new Window
                {
                    Title = $"修改变量 {variable.Name}",
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    ResizeMode = ResizeMode.NoResize
                };

                var okButton = new Button
                {
                    Content = "确定",
                    Width = 80,
                    Margin = new Thickness(0, 0, 8, 0),
                    IsDefault = true
                };
                okButton.Click += (s, ev) =>
                {
                    if (double.TryParse(inputDialog.Text, out double newValue))
                    {
                        // 使用ViewModel的引擎来更新变量
                        var mainVm = (ViewModels.MainViewModel)DataContext;
                        var engineField = typeof(ViewModels.MainViewModel).GetField("_engine", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (engineField != null)
                        {
                            var engine = engineField.GetValue(mainVm) as CalcPad.Core.CalculationEngine;
                            if (engine != null)
                            {
                                engine.Variables.SetVariable(variable.Name, newValue);
                                
                                // 调用私有的RefreshVariables方法
                                var refreshMethod = typeof(ViewModels.MainViewModel).GetMethod("RefreshVariables", 
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                refreshMethod?.Invoke(mainVm, null);
                                
                                // 重新计算当前表达式
                                var calculateMethod = typeof(ViewModels.MainViewModel).GetMethod("AsyncCalculate", 
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                calculateMethod?.Invoke(mainVm, null);
                                
                                // 保存变量
                                var saveMethod = typeof(ViewModels.MainViewModel).GetMethod("SaveVariablesAsync", 
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                saveMethod?.Invoke(mainVm, null);
                            }
                        }
                        
                        dialog.Close();
                    }
                    else
                    {
                        MessageBox.Show(
                            "请输入有效的数值！", 
                            "错误", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Warning);
                    }
                };

                var cancelButton = new Button
                {
                    Content = "取消",
                    Width = 80,
                    IsCancel = true
                };

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(10),
                    Children = { okButton, cancelButton }
                };

                var contentPanel = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = $"请输入 {variable.Name} 的新值:",
                            Margin = new Thickness(10, 10, 10, 5),
                            FontWeight = FontWeights.SemiBold
                        },
                        inputDialog,
                        buttonPanel
                    }
                };

                dialog.Content = contentPanel;

                // 聚焦到输入框
                dialog.Loaded += (s, ev) =>
                {
                    inputDialog.Focus();
                    inputDialog.SelectAll();
                };

                // 显示模态对话框
                dialog.ShowDialog();
            }
        }
    }
}
