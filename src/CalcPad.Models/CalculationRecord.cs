using System;

namespace CalcPad.Models
{
    /// <summary>
    /// 计算记录模型
    /// </summary>
    public class CalculationRecord
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 计算公式
        /// </summary>
        public string Expression { get; set; } = string.Empty;

        /// <summary>
        /// 计算结果
        /// </summary>
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否为错误结果
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// 备注(可选)
        /// </summary>
        public string? Note { get; set; }
    }
}
