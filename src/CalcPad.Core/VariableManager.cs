using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NCalc;

namespace CalcPad.Core
{
    /// <summary>
    /// 变量管理器
    /// 负责存储和管理用户定义的变量，支持依赖追踪和自动重新计算
    /// </summary>
    public class VariableManager
    {
        // 变量元数据
        private class VariableMetadata
        {
            public string Name { get; set; } = "";
            public string Expression { get; set; } = "";  // 原始表达式
            public double Value { get; set; }              // 计算后的值
            public HashSet<string> Dependencies { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);  // 依赖的变量
        }

        // 变量存储字典
        private readonly Dictionary<string, VariableMetadata> _variables = new Dictionary<string, VariableMetadata>(StringComparer.OrdinalIgnoreCase);

        // 变量定义顺序（用于UI显示）
        private readonly List<string> _definitionOrder = new List<string>();

        /// <summary>
        /// 只读的变量字典
        /// </summary>
        public IReadOnlyDictionary<string, double> Variables => 
            _variables.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);

        /// <summary>
        /// 获取按定义顺序排列的变量列表
        /// </summary>
        public IEnumerable<KeyValuePair<string, double>> OrderedVariables
        {
            get
            {
                return _definitionOrder
                    .Where(name => _variables.ContainsKey(name))
                    .Select(name => new KeyValuePair<string, double>(name, _variables[name].Value));
            }
        }

        /// <summary>
        /// 获取所有变量的表达式（用于持久化）
        /// </summary>
        public Dictionary<string, string> GetAllVariableExpressions()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in _definitionOrder)
            {
                if (_variables.TryGetValue(name, out var metadata))
                {
                    result[name] = metadata.Expression;
                }
            }
            return result;
        }

        /// <summary>
        /// 设置变量（存储表达式而非直接存储值）
        /// </summary>
        /// <param name="name">变量名</param>
        /// <param name="expression">表达式</param>
        public void SetVariable(string name, string expression)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("变量名不能为空", nameof(name));

            // 验证变量名格式
            if (!IsValidVariableName(name))
                throw new ArgumentException($"无效的变量名: {name}。变量名只能包含字母、数字和下划线，且不能以数字开头。", nameof(name));

            // 提取表达式中依赖的变量
            var dependencies = ExtractDependencies(expression);

            // 如果是新变量，添加到定义顺序列表
            bool isNewVariable = !_variables.ContainsKey(name);
            if (isNewVariable)
            {
                _definitionOrder.Add(name);
            }

            // 存储变量元数据
            _variables[name] = new VariableMetadata
            {
                Name = name,
                Expression = expression,
                Dependencies = dependencies
            };

            // 计算变量值
            RecalculateVariable(name);
            
            // 如果不是新变量（即修改变量），需要重新计算依赖该变量的其他变量
            if (!isNewVariable)
            {
                RecalculateDependentVariables(name);
            }
        }

        /// <summary>
        /// 设置变量值（直接赋值，用于简单数值）
        /// </summary>
        /// <param name="name">变量名</param>
        /// <param name="value">变量值</param>
        public void SetVariable(string name, double value)
        {
            SetVariable(name, value.ToString());
        }

        /// <summary>
        /// 获取变量值
        /// </summary>
        public double GetVariable(string name)
        {
            if (_variables.TryGetValue(name, out var metadata))
                return metadata.Value;

            throw new KeyNotFoundException($"变量 '{name}' 未定义");
        }

        /// <summary>
        /// 检查变量是否已定义
        /// </summary>
        public bool HasVariable(string name)
        {
            return _variables.ContainsKey(name);
        }

        /// <summary>
        /// 删除变量
        /// </summary>
        public bool RemoveVariable(string name)
        {
            if (_variables.Remove(name))
            {
                _definitionOrder.Remove(name);
                
                // 重新计算依赖该变量的其他变量
                RecalculateDependentVariables(name);
                
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清空所有变量
        /// </summary>
        public void Clear()
        {
            _variables.Clear();
            _definitionOrder.Clear();
        }

        /// <summary>
        /// 解析赋值表达式（如 "a=100"）
        /// </summary>
        public bool TryParseAssignment(string expression, out string? varName, out string? valueExpression)
        {
            varName = null;
            valueExpression = null;

            if (string.IsNullOrWhiteSpace(expression))
                return false;

            // 查找等号位置
            var equalIndex = expression.IndexOf('=');
            if (equalIndex <= 0 || equalIndex == expression.Length - 1)
                return false;

            // 提取变量名和值表达式
            varName = expression.Substring(0, equalIndex).Trim();
            valueExpression = expression.Substring(equalIndex + 1).Trim();

            // 验证变量名
            if (!IsValidVariableName(varName))
                return false;

            // 值表达式不能包含赋值符号（不支持连续赋值）
            if (valueExpression.Contains('='))
                return false;

            return true;
        }

        /// <summary>
        /// 重新计算指定变量及其依赖链
        /// </summary>
        private void RecalculateVariable(string name)
        {
            if (!_variables.TryGetValue(name, out var metadata))
                return;

            try
            {
                // 创建表达式并注册依赖变量的当前值
                var expr = new Expression(metadata.Expression);
                
                foreach (var depName in metadata.Dependencies)
                {
                    if (_variables.TryGetValue(depName, out var depMetadata))
                    {
                        expr.Parameters[depName] = depMetadata.Value;
                    }
                }

                // 计算表达式
                var result = expr.Evaluate();
                metadata.Value = Convert.ToDouble(result);
            }
            catch
            {
                // 计算失败，保持原值
            }
        }

        /// <summary>
        /// 重新计算所有依赖指定变量的变量
        /// </summary>
        private void RecalculateDependentVariables(string changedVariable)
        {
            // 找出所有依赖该变量的变量
            var dependentVars = _variables.Values
                .Where(v => v.Dependencies.Contains(changedVariable, StringComparer.OrdinalIgnoreCase))
                .Select(v => v.Name)
                .ToList();

            // 递归重新计算
            foreach (var varName in dependentVars)
            {
                RecalculateVariable(varName);
                // 递归处理依赖这个变量的其他变量
                RecalculateDependentVariables(varName);
            }
        }

        /// <summary>
        /// 提取表达式中依赖的变量名
        /// </summary>
        private HashSet<string> ExtractDependencies(string expression)
        {
            var dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            // 提取所有可能的变量名（字母开头的标识符）
            var matches = Regex.Matches(expression, @"\b[a-zA-Z_][a-zA-Z0-9_]*\b");
            
            foreach (Match match in matches)
            {
                var identifier = match.Value;
                
                // 跳过NCalc内置函数
                if (!IsNCalcFunction(identifier))
                {
                    dependencies.Add(identifier);
                }
            }

            return dependencies;
        }

        /// <summary>
        /// 验证变量名是否有效
        /// </summary>
        private bool IsValidVariableName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // 不能以数字开头
            if (char.IsDigit(name[0]))
                return false;

            // 只能包含字母、数字、下划线
            foreach (char c in name)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                    return false;
            }

            // 不能是NCalc内置函数名
            var reservedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "abs", "acos", "asin", "atan", "atan2", "ceiling", "cos", "cosh",
                "exp", "floor", "ieeeremainder", "log", "log10", "max", "min",
                "pow", "round", "sign", "sin", "sinh", "sqrt", "tan", "tanh",
                "truncate", "if", "in", "true", "false"
            };

            return !reservedNames.Contains(name);
        }

        /// <summary>
        /// 检查是否为NCalc内置函数
        /// </summary>
        private bool IsNCalcFunction(string name)
        {
            var builtInFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "abs", "acos", "asin", "atan", "atan2", "ceiling", "cos", "cosh",
                "exp", "floor", "ieeeremainder", "log", "log10", "max", "min",
                "pow", "round", "sign", "sin", "sinh", "sqrt", "tan", "tanh",
                "truncate", "if", "in", "true", "false"
            };

            return builtInFunctions.Contains(name);
        }

        /// <summary>
        /// 获取所有已定义变量名的集合
        /// </summary>
        public HashSet<string> GetVariableNames()
        {
            return new HashSet<string>(_variables.Keys, StringComparer.OrdinalIgnoreCase);
        }
    }
}
