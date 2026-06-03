using CalcPad.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CalcPad.Services
{
    /// <summary>
    /// 历史记录服务
    /// 负责计算历史的存储、加载和管理
    /// </summary>
    public class HistoryService
    {
        private readonly string _dataFilePath;
        private readonly object _fileLock = new object();
        private List<CalculationRecord> _records = new();

        /// <summary>
        /// 只读的历史记录列表
        /// </summary>
        public IReadOnlyList<CalculationRecord> Records => _records.AsReadOnly();

        /// <summary>
        /// 初始化历史记录服务
        /// </summary>
        /// <param name="dataDirectory">数据存储目录</param>
        public HistoryService(string dataDirectory)
        {
            _dataFilePath = Path.Combine(dataDirectory, "history.json");
            EnsureDataDirectory();
        }

        /// <summary>
        /// 异步加载历史记录
        /// </summary>
        public async Task LoadAsync()
        {
            if (!File.Exists(_dataFilePath))
            {
                _records = new List<CalculationRecord>();
                return;
            }

            await Task.Run(() =>
            {
                lock (_fileLock)
                {
                    var json = File.ReadAllText(_dataFilePath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    _records = JsonSerializer.Deserialize<List<CalculationRecord>>(json, options) 
                               ?? new List<CalculationRecord>();
                }
            });
        }

        /// <summary>
        /// 添加计算记录
        /// </summary>
        /// <param name="record">计算记录</param>
        public async Task AddRecordAsync(CalculationRecord record)
        {
            // 最新记录插入到开头
            _records.Insert(0, record);
            
            // 自动归档(超过最大数量时)
            if (_records.Count > 1000)
            {
                await ArchiveOldRecordsAsync();
            }

            await SaveAsync();
        }

        /// <summary>
        /// 删除指定记录
        /// </summary>
        /// <param name="id">记录ID</param>
        public async Task DeleteRecordAsync(Guid id)
        {
            _records.RemoveAll(r => r.Id == id);
            await SaveAsync();
        }

        /// <summary>
        /// 清空所有历史记录
        /// </summary>
        public async Task ClearAsync()
        {
            _records.Clear();
            await SaveAsync();
        }

        /// <summary>
        /// 搜索历史记录
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <returns>匹配的记录列表</returns>
        public IEnumerable<CalculationRecord> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return _records;

            return _records.Where(r => 
                r.Expression.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                r.Result.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (r.Note != null && r.Note.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// 异步保存到文件
        /// </summary>
        private async Task SaveAsync()
        {
            await Task.Run(() =>
            {
                lock (_fileLock)
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    
                    var json = JsonSerializer.Serialize(_records, options);
                    File.WriteAllText(_dataFilePath, json);
                }
            });
        }

        /// <summary>
        /// 确保数据目录存在
        /// </summary>
        private void EnsureDataDirectory()
        {
            var directory = Path.GetDirectoryName(_dataFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        /// <summary>
        /// 归档旧记录
        /// 保留最近500条,其余归档到备份文件
        /// </summary>
        private async Task ArchiveOldRecordsAsync()
        {
            // 保留最近500条
            var toKeep = _records.Take(500).ToList();
            var toArchive = _records.Skip(500).ToList();
            
            // 生成归档文件名
            var archivePath = _dataFilePath.Replace(".json", $".backup_{DateTime.Now:yyyyMMdd}.json");
            
            await Task.Run(() =>
            {
                var json = JsonSerializer.Serialize(toArchive);
                File.WriteAllText(archivePath, json);
            });

            _records = toKeep;
        }
    }
}
