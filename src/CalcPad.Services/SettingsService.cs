using CalcPad.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CalcPad.Services
{
    /// <summary>
    /// 配置管理服务
    /// 负责应用配置的加载、保存和管理
    /// </summary>
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private readonly object _fileLock = new object();
        private AppSettings _settings;

        /// <summary>
        /// 当前配置
        /// </summary>
        public AppSettings Settings => _settings;

        /// <summary>
        /// 初始化配置服务
        /// </summary>
        /// <param name="dataDirectory">数据存储目录</param>
        public SettingsService(string dataDirectory)
        {
            _settingsFilePath = Path.Combine(dataDirectory, "settings.json");
            _settings = new AppSettings();
            EnsureDataDirectory();
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        public async Task LoadAsync()
        {
            if (!File.Exists(_settingsFilePath))
            {
                // 使用默认配置
                _settings = new AppSettings();
                await SaveAsync();
                return;
            }

            await Task.Run(() =>
            {
                lock (_fileLock)
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    _settings = JsonSerializer.Deserialize<AppSettings>(json, options) 
                                ?? new AppSettings();
                }
            });
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public async Task SaveAsync()
        {
            await Task.Run(() =>
            {
                lock (_fileLock)
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    var json = JsonSerializer.Serialize(_settings, options);
                    File.WriteAllText(_settingsFilePath, json);
                }
            });
        }

        /// <summary>
        /// 确保数据目录存在
        /// </summary>
        private void EnsureDataDirectory()
        {
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }
    }
}
