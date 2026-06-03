using System.Collections.Generic;

namespace CalcPad.Models
{
    /// <summary>
    /// 应用配置模型
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 主题: Dark 或 Light
        /// </summary>
        public string Theme { get; set; } = "Dark";

        /// <summary>
        /// 窗口宽度
        /// </summary>
        public double WindowWidth { get; set; } = 500;

        /// <summary>
        /// 窗口高度
        /// </summary>
        public double WindowHeight { get; set; } = 700;

        /// <summary>
        /// 窗口位置 - 顶部
        /// </summary>
        public double WindowTop { get; set; } = double.NaN;

        /// <summary>
        /// 窗口位置 - 左侧
        /// </summary>
        public double WindowLeft { get; set; } = double.NaN;

        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool Topmost { get; set; } = false;

        /// <summary>
        /// 历史记录保留天数
        /// </summary>
        public int HistoryRetentionDays { get; set; } = 30;

        /// <summary>
        /// 最大历史记录数量
        /// </summary>
        public int MaxHistoryCount { get; set; } = 1000;

        /// <summary>
        /// 防抖延迟(毫秒)
        /// </summary>
        public int DebounceDelay { get; set; } = 300;

        /// <summary>
        /// 计算后自动清空输入框
        /// </summary>
        public bool AutoClearInput { get; set; } = true;

        /// <summary>
        /// 已定义的变量（名称 -> 表达式）
        /// </summary>
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }
}
