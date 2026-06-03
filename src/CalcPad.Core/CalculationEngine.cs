using NCalc;
using System;
using System.Collections.Concurrent;

namespace CalcPad.Core
{
    /// <summary>
    /// 表达式计算引擎
    /// 使用NCalc库进行数学表达式解析和计算
    /// </summary>
    public class CalculationEngine
    {
        // 表达式缓存(提升性能)
        private readonly ConcurrentDictionary<string, Expression> _expressionCache 
            = new ConcurrentDictionary<string, Expression>();
        
        private const int MaxCacheSize = 100;

        // 变量管理器
        private readonly VariableManager _variableManager = new VariableManager();

        /// <summary>
        /// 获取变量管理器
        /// </summary>
        public VariableManager Variables => _variableManager;

        /// <summary>
        /// 计算表达式
        /// </summary>
        /// <param name="expression">数学表达式</param>
        /// <returns>计算结果</returns>
        public CalculationResult Evaluate(string expression)
        {
            // 空表达式检查
            if (string.IsNullOrWhiteSpace(expression))
                return CalculationResult.Empty;

            // 长度限制(安全防护)
            if (expression.Length > 500)
                return CalculationResult.Error("错误: 表达式过长(最多500字符)");

            try
            {
                // 检查是否为变量赋值表达式
                if (_variableManager.TryParseAssignment(expression, out var varName, out _) && varName != null)
                {
                    return HandleVariableAssignment(expression, varName);
                }

                // 表达式验证
                if (!ExpressionValidator.IsValid(expression, _variableManager))
                    return CalculationResult.Error("错误: 表达式语法不正确");

                // 获取或编译表达式
                var expr = GetOrCompileExpression(expression);
                
                // 注册变量参数到NCalc
                RegisterVariables(expr);
                
                // 执行计算
                var result = expr.Evaluate();
                
                // 格式化结果
                var formattedResult = FormatResult(result);
                
                return CalculationResult.Success(expression, formattedResult);
            }
            catch (DivideByZeroException)
            {
                return CalculationResult.Error("错误: 除数不能为零");
            }
            catch (TimeoutException)
            {
                return CalculationResult.Error("错误: 计算超时");
            }
            catch (KeyNotFoundException ex)
            {
                return CalculationResult.Error($"错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                return CalculationResult.Error($"错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理变量赋值表达式
        /// </summary>
        private CalculationResult HandleVariableAssignment(string expression, string varName)
        {
            try
            {
                // 提取值表达式（等号右边的部分）
                var equalIndex = expression.IndexOf('=');
                var valueExpression = expression.Substring(equalIndex + 1).Trim();

                // 存储变量（保存表达式而非直接存储值）
                _variableManager.SetVariable(varName, valueExpression);

                // 清除缓存（因为变量已改变）
                ClearCache();

                // 获取计算后的值
                var value = _variableManager.GetVariable(varName);
                return CalculationResult.Success(expression, $"{varName} = {value:G15}");
            }
            catch (Exception ex)
            {
                return CalculationResult.Error($"错误: 变量赋值失败 - {ex.Message}");
            }
        }

        /// <summary>
        /// 注册变量到NCalc表达式
        /// </summary>
        private void RegisterVariables(Expression expr)
        {
            foreach (var kvp in _variableManager.Variables)
            {
                expr.Parameters[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// 获取或编译表达式(带缓存)
        /// </summary>
        private Expression GetOrCompileExpression(string expression)
        {
            // 尝试从缓存获取
            if (_expressionCache.TryGetValue(expression, out var cached))
                return cached;

            // 创建新表达式
            var expr = new Expression(expression);

            // 缓存控制(避免内存泄漏)
            if (_expressionCache.Count >= MaxCacheSize)
            {
                _expressionCache.Clear();
            }

            // 添加到缓存
            _expressionCache[expression] = expr;
            return expr;
        }

        /// <summary>
        /// 格式化计算结果
        /// </summary>
        private string FormatResult(object? result)
        {
            if (result == null)
                return "0";

            // 处理数字类型
            if (result is double d)
            {
                // 保留15位有效数字,避免浮点精度问题
                return d.ToString("G15");
            }

            if (result is decimal dec)
            {
                return dec.ToString("G29");
            }

            if (result is int i)
            {
                return i.ToString();
            }

            // 其他类型直接转字符串
            return result?.ToString() ?? "0";
        }

        /// <summary>
        /// 清空表达式缓存
        /// </summary>
        public void ClearCache()
        {
            _expressionCache.Clear();
        }
    }

    /// <summary>
    /// 计算结果记录
    /// </summary>
    public record CalculationResult(
        bool IsSuccess,
        string Expression,
        string Result,
        string? ErrorMessage = null)
    {
        /// <summary>
        /// 空结果
        /// </summary>
        public static CalculationResult Empty => 
            new(false, "", "", null);
        
        /// <summary>
        /// 成功结果
        /// </summary>
        public static CalculationResult Success(string expr, string result) => 
            new(true, expr, result, null);
        
        /// <summary>
        /// 错误结果
        /// </summary>
        public static CalculationResult Error(string message) => 
            new(false, "", "", message);
    }
}
