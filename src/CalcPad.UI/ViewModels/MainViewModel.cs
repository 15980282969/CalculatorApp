using CalcPad.Core;
using CalcPad.Models;
using CalcPad.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CalcPad.UI.ViewModels
{
    /// <summary>
    /// 主视图模型
    /// MVVM模式的核心,连接UI和业务逻辑
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly CalculationEngine _engine;
        private readonly HistoryService _historyService;
        private readonly SettingsService _settingsService;
        private readonly DebounceService _debounce;

        /// <summary>
        /// 公开SettingsService供View访问
        /// </summary>
        public SettingsService SettingsService => _settingsService;

        [ObservableProperty]
        private string _inputExpression = "";

        [ObservableProperty]
        private string _currentExpression = "";

        [ObservableProperty]
        private string _currentResult = "";

        [ObservableProperty]
        private SolidColorBrush _resultColor = Brushes.White;

        [ObservableProperty]
        private bool _isTopmost = false;

        [ObservableProperty]
        private string _topmostIcon = "Pin";

        [ObservableProperty]
        private string _topmostTooltip = "窗口置顶";

        [ObservableProperty]
        private ObservableCollection<CalculationRecord> _historyRecords = new();

        [ObservableProperty]
        private ObservableCollection<VariableItem> _variables = new();

        [ObservableProperty]
        private bool _showVariables = false;

        [ObservableProperty]
        private bool _autoClearInput = true;

        /// <summary>
        /// 初始化视图模型
        /// </summary>
        public MainViewModel()
        {
            // 初始化服务
            _engine = new CalculationEngine();
            
            // 数据存储路径: %APPDATA%\CalcPad\data
            var dataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CalcPad",
                "data");
            
            _historyService = new HistoryService(dataDir);
            _settingsService = new SettingsService(dataDir);
            _debounce = new DebounceService(300); // 300ms防抖

            // 异步加载数据
            _ = InitializeAsync();
        }

        /// <summary>
        /// 异步初始化
        /// </summary>
        private async Task InitializeAsync()
        {
            try
            {
                // 加载配置
                await _settingsService.LoadAsync();
                IsTopmost = _settingsService.Settings.Topmost;
                AutoClearInput = _settingsService.Settings.AutoClearInput;

                // 恢复变量
                RestoreVariables();

                // 加载历史记录
                await _historyService.LoadAsync();
                foreach (var record in _historyService.Records)
                {
                    HistoryRecords.Add(record);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 恢复持久化的变量
        /// </summary>
        private void RestoreVariables()
        {
            var savedVariables = _settingsService.Settings.Variables;
            if (savedVariables == null || savedVariables.Count == 0)
                return;

            // 重新设置所有变量（会自动计算和建立依赖关系）
            foreach (var kvp in savedVariables)
            {
                try
                {
                    _engine.Variables.SetVariable(kvp.Key, kvp.Value);
                }
                catch
                {
                    // 忽略恢复失败的变量
                }
            }

            // 刷新UI显示
            RefreshVariables();
        }

        /// <summary>
        /// 保存当前变量到配置
        /// </summary>
        private async Task SaveVariablesAsync()
        {
            // 获取所有变量的表达式
            var variableExpressions = _engine.Variables.GetAllVariableExpressions();
            
            // 更新配置
            _settingsService.Settings.Variables.Clear();
            foreach (var kvp in variableExpressions)
            {
                _settingsService.Settings.Variables[kvp.Key] = kvp.Value;
            }
            
            // 保存到文件
            await _settingsService.SaveAsync();
        }

        /// <summary>
        /// 保存窗口尺寸
        /// </summary>
        public void SaveWindowSize()
        {
            // 通过事件传递给View，View中保存
            // 这里只更新Settings对象
            // 实际保存在View中调用
        }

        /// <summary>
        /// 保存窗口位置
        /// </summary>
        public void SaveWindowPosition()
        {
            // 通过事件传递给View，View中保存
        }

        /// <summary>
        /// 异步保存窗口设置
        /// </summary>
        public async Task SaveWindowSettingsAsync()
        {
            await _settingsService.SaveAsync();
        }

        partial void OnAutoClearInputChanged(bool value)
        {
            // 自动保存配置
            _settingsService.Settings.AutoClearInput = value;
            _ = _settingsService.SaveAsync();
        }
        partial void OnInputExpressionChanged(string value)
        {
            // 自动转换中文括号为英文括号
            if (!string.IsNullOrEmpty(value))
            {
                var converted = ConvertChineseBrackets(value);
                if (converted != value)
                {
                    InputExpression = converted;
                    _ = _debounce.DebounceAsync(AsyncCalculate);
                    return;
                }
            }
            
            // 防抖实时计算
            _ = _debounce.DebounceAsync(AsyncCalculate);
        }

        /// <summary>
        /// 转换中文全角括号为英文半角括号
        /// </summary>
        private string ConvertChineseBrackets(string expression)
        {
            if (string.IsNullOrEmpty(expression))
                return expression;

            return expression
                .Replace('\uFF08', '(')  // （ -> (
                .Replace('\uFF09', ')'); // ） -> )
        }

        /// <summary>
        /// 异步计算(防抖触发)
        /// </summary>
        private async Task AsyncCalculate()
        {
            if (string.IsNullOrWhiteSpace(InputExpression))
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CurrentExpression = "";
                    CurrentResult = "";
                    ResultColor = Brushes.White;
                });
                return;
            }

            // 检查表达式是否完整
            var trimmedInput = InputExpression.TrimEnd();
            if (IsIncompleteExpression(trimmedInput))
            {
                // 表达式未完成,不显示错误,只显示公式
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CurrentExpression = trimmedInput;
                    CurrentResult = "";
                    ResultColor = Brushes.White;
                });
                return;
            }

            var result = _engine.Evaluate(InputExpression);
            
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                CurrentExpression = InputExpression;
                
                if (result.IsSuccess)
                {
                    CurrentResult = result.Result;
                    ResultColor = Brushes.White;
                    
                    // 刷新变量显示
                    RefreshVariables();
                }
                else
                {
                    CurrentResult = result.ErrorMessage ?? "错误";
                    ResultColor = Brushes.IndianRed;
                }
            });
        }

        /// <summary>
        /// 检查表达式是否未完成
        /// </summary>
        private bool IsIncompleteExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression))
                return true;

            // 移除末尾空格后再检查
            var trimmed = expression.TrimEnd();
            
            // 检查是否以运算符结尾
            var lastChar = trimmed.Last();
            if (new[] { '+', '-', '*', '/', '%', '^' }.Contains(lastChar))
                return true;

            // 检查括号是否匹配
            int balance = 0;
            foreach (char c in trimmed)
            {
                if (c == '(') balance++;
                else if (c == ')') balance--;
            }
            if (balance != 0)
                return true;

            // 检查是否有连续运算符(除了负号)
            if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"[+\*/%^]{2,}"))
                return true;

            return false;
        }

        /// <summary>
        /// 执行计算并保存历史
        /// </summary>
        [RelayCommand]
        private async Task Calculate()
        {
            if (string.IsNullOrWhiteSpace(InputExpression))
                return;

            // 立即计算
            var result = _engine.Evaluate(InputExpression);
            
            // 创建记录
            var record = new CalculationRecord
            {
                Expression = InputExpression,
                Result = result.IsSuccess ? result.Result : result.ErrorMessage ?? "错误",
                CreatedAt = DateTime.Now,
                IsError = !result.IsSuccess
            };

            // 保存到服务
            await _historyService.AddRecordAsync(record);
            
            // 更新UI
            HistoryRecords.Insert(0, record);
            
            // 更新显示
            CurrentExpression = InputExpression;
            if (result.IsSuccess)
            {
                CurrentResult = result.Result;
                ResultColor = Brushes.White;
                
                // 刷新变量显示
                RefreshVariables();
                
                // 保存变量到配置
                _ = SaveVariablesAsync();
                
                // 如果启用了自动清空，则清空输入框
                if (AutoClearInput)
                {
                    InputExpression = "";
                }
            }
            else
            {
                CurrentResult = result.ErrorMessage ?? "错误";
                ResultColor = Brushes.IndianRed;
            }
        }

        /// <summary>
        /// 新建计算(清空输入)
        /// </summary>
        [RelayCommand]
        private void NewCalculation()
        {
            InputExpression = "";
            CurrentExpression = "";
            CurrentResult = "";
            ResultColor = Brushes.White;
        }

        /// <summary>
        /// 清空历史记录
        /// </summary>
        [RelayCommand]
        private async Task ClearHistory()
        {
            var result = MessageBox.Show(
                "确定要清空所有历史记录吗?",
                "确认",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _historyService.ClearAsync();
                HistoryRecords.Clear();
            }
        }

        /// <summary>
        /// 切换主题
        /// </summary>
        [RelayCommand]
        private async Task ToggleTheme()
        {
            _settingsService.Settings.Theme = 
                _settingsService.Settings.Theme == "Dark" ? "Light" : "Dark";
            
            await _settingsService.SaveAsync();
            
            MessageBox.Show("主题切换功能需要完整实现MaterialDesignThemes的动态切换", "提示");
        }

        /// <summary>
        /// 切换窗口置顶
        /// </summary>
        [RelayCommand]
        private async Task ToggleTopmost()
        {
            IsTopmost = !IsTopmost;
            
            // 更新图标和提示
            if (IsTopmost)
            {
                TopmostIcon = "PinOff";
                TopmostTooltip = "取消置顶";
            }
            else
            {
                TopmostIcon = "Pin";
                TopmostTooltip = "窗口置顶";
            }
            
            _settingsService.Settings.Topmost = IsTopmost;
            await _settingsService.SaveAsync();
        }
        /// <summary>
        /// 切换变量面板显示
        /// </summary>
        [RelayCommand]
        private void ToggleVariables()
        {
            ShowVariables = !ShowVariables;
        }

        /// <summary>
        /// 清空所有变量
        /// </summary>
        [RelayCommand]
        private Task ClearVariables()
        {
            _engine.Variables.Clear();
            Variables.Clear();
            
            // 保存变量到配置
            return SaveVariablesAsync();
        }

        /// <summary>
        /// 删除指定变量
        /// </summary>
        /// <param name="variableName">变量名</param>
        [RelayCommand]
        private Task DeleteVariable(string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
                return Task.CompletedTask;

            _engine.Variables.RemoveVariable(variableName);
            
            // 完全刷新变量列表（因为依赖变量可能已更新）
            RefreshVariables();
            
            // 保存变量到配置
            return SaveVariablesAsync();
        }

        /// <summary>
        /// 刷新变量列表显示
        /// </summary>
        private void RefreshVariables()
        {
            Variables.Clear();
            foreach (var kvp in _engine.Variables.OrderedVariables)
            {
                Variables.Add(new VariableItem
                {
                    Name = kvp.Key,
                    Value = kvp.Value
                });
            }
        }

        /// <summary>
        /// 变量项模型
        /// </summary>
        public partial class VariableItem : ObservableObject
        {
            [ObservableProperty]
            private string _name = "";

            [ObservableProperty]
            private double _value;
        }
    }
}
