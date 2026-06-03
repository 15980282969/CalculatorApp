using System;
using System.Threading;
using System.Threading.Tasks;

namespace CalcPad.Services
{
    /// <summary>
    /// 防抖服务
    /// 用于延迟执行操作,避免频繁触发
    /// </summary>
    public class DebounceService
    {
        private CancellationTokenSource? _cts;
        private readonly int _delayMs;

        /// <summary>
        /// 初始化防抖服务
        /// </summary>
        /// <param name="delayMs">延迟时间(毫秒),默认300ms</param>
        public DebounceService(int delayMs = 300)
        {
            _delayMs = delayMs;
        }

        /// <summary>
        /// 防抖执行
        /// 如果在延迟时间内再次调用,会取消之前的任务
        /// </summary>
        /// <param name="action">要执行的操作</param>
        public async Task DebounceAsync(Func<Task> action)
        {
            // 取消之前的任务
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                // 延迟执行
                await Task.Delay(_delayMs, _cts.Token);
                
                // 如果没有被取消,执行操作
                if (!_cts.Token.IsCancellationRequested)
                {
                    await action();
                }
            }
            catch (TaskCanceledException)
            {
                // 预期内的取消,忽略
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
