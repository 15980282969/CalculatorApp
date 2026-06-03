using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CalcPad.Core
{
    /// <summary>
    /// 表达式语法验证器
    /// </summary>
    public static class ExpressionValidator
    {
        /// <summary>
        /// 验证表达式语法是否正确
        /// </summary>
        /// <param name="expression">待验证的表达式</param>
        /// <param name="variableManager">变量管理器（可选）</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string expression, VariableManager? variableManager = null)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            try
            {
                // 检查是否为变量赋值表达式
                if (variableManager != null && variableManager.TryParseAssignment(expression, out var varName, out _))
                {
                    // 赋值表达式只需要验证等号右边的值表达式
                    var equalIndex = expression.IndexOf('=');
                    var valueExpression = expression.Substring(equalIndex + 1).Trim();
                    return CheckBasicSyntax(valueExpression) && CheckParentheses(valueExpression);
                }

                // 基础语法检查
                if (!CheckBasicSyntax(expression, variableManager))
                    return false;

                // 括号匹配检查
                if (!CheckParentheses(expression))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 基础语法检查
        /// </summary>
        private static bool CheckBasicSyntax(string expression, VariableManager? variableManager = null)
        {
            // 不允许包含危险字符(防止注入)
            var dangerousChars = new[] { ';', ':', '{', '}', '[', ']', '|', '\\', '@', '#', '$' };
            foreach (var ch in dangerousChars)
            {
                if (expression.Contains(ch))
                    return false;
            }

            // 检查变量引用是否已定义
            if (variableManager != null)
            {
                if (!CheckVariableReferences(expression, variableManager))
                    return false;
            }

            // 检查连续运算符(除了负号开头)
            // 允许: -5, 5*-3, 但不允许 5++3, 5**3
            var invalidPatterns = new[]
            {
                @"\+\+",     // ++
                @"--(?!\d)", // --后面不是数字(允许负号)
                @"\*\*",     // **
                @"//",       // //
                @"\*\+",     // *+
                @"\*-",      // *-
                @"\+/",      // +/
                @"-/",       // -/
                @"//\*",     // /*
                @"//\+",     // /+
                @"//\-"      // /-
            };

            foreach (var pattern in invalidPatterns)
            {
                if (Regex.IsMatch(expression, pattern))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 检查变量引用是否已定义
        /// </summary>
        private static bool CheckVariableReferences(string expression, VariableManager variableManager)
        {
            // 提取所有可能的变量名（字母开头的标识符）
            var matches = Regex.Matches(expression, @"\b[a-zA-Z_][a-zA-Z0-9_]*\b");
            
            foreach (Match match in matches)
            {
                var identifier = match.Value;
                
                // 跳过NCalc内置函数
                if (IsNCalcFunction(identifier))
                    continue;
                
                // 检查变量是否已定义
                if (!variableManager.HasVariable(identifier))
                {
                    // 未定义的变量，验证失败
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检查是否为NCalc内置函数
        /// </summary>
        private static bool IsNCalcFunction(string name)
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
        /// 括号匹配检查
        /// </summary>
        private static bool CheckParentheses(string expression)
        {
            int balance = 0;
            
            foreach (char c in expression)
            {
                if (c == '(')
                    balance++;
                else if (c == ')')
                    balance--;

                // 右括号多于左括号
                if (balance < 0)
                    return false;
            }

            // 最终必须平衡
            return balance == 0;
        }
    }
}
