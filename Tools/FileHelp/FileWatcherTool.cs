// ---------------------------------------------------------------------------------
// File: FileWatcherTool.cs
// Description: 文件监控类
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.FileHelp
{
    /// <summary>
    /// 目录文件监控类
    /// </summary>
    public class FileWatcherTool : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private bool _isDisposed;
        private bool _isMonitoring;

        /// <summary>
        /// 监控的目录路径
        /// </summary>
        public string WatchPath { get; }

        /// <summary>
        /// 监控的文件筛选模式（默认为"*.*"，监控所有文件）
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// 是否监控子目录
        /// </summary>
        public bool IncludeSubdirectories { get; set; }

        /// <summary>
        /// 当文件或目录被创建时触发
        /// </summary>
        public event EventHandler<FileSystemEventArgs> Created;

        /// <summary>
        /// 当文件或目录被删除时触发
        /// </summary>
        public event EventHandler<FileSystemEventArgs> Deleted;

        /// <summary>
        /// 当文件或目录被修改时触发
        /// </summary>
        public event EventHandler<FileSystemEventArgs> Changed;

        /// <summary>
        /// 当文件或目录被重命名时触发
        /// </summary>
        public event EventHandler<RenamedEventArgs> Renamed;

        /// <summary>
        /// 监控发生错误时触发
        /// </summary>
        public event EventHandler<ErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// 初始化文件监控器
        /// </summary>
        /// <param name="path">要监控的目录路径</param>
        /// <param name="filter">文件筛选模式（默认为"*.*"）</param>
        /// <param name="includeSubdirectories">是否监控子目录（默认为false）</param>
        public FileWatcherTool(string path, string filter = "*.*", bool includeSubdirectories = false)
        {
            // 验证目录是否存在
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path), "监控路径不能为空");

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"指定的目录不存在: {path}");

            WatchPath = path;
            Filter = filter;
            IncludeSubdirectories = includeSubdirectories;

            // 初始化文件系统监控器
            _watcher = new FileSystemWatcher(WatchPath, Filter)
            {
                IncludeSubdirectories = includeSubdirectories,
                EnableRaisingEvents = false, // 初始不启用事件
                NotifyFilter = NotifyFilters.Attributes
                             | NotifyFilters.CreationTime
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
                             | NotifyFilters.LastAccess
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Security
                             | NotifyFilters.Size
            };

            // 订阅内部事件
            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Changed += OnChanged;
            _watcher.Renamed += OnRenamed;
            _watcher.Error += OnError;
        }

        /// <summary>
        /// 开始监控
        /// </summary>
        public void StartMonitoring()
        {
            CheckDisposed();

            if (!_isMonitoring)
            {
                _watcher.EnableRaisingEvents = true;
                _isMonitoring = true;
            }
        }

        /// <summary>
        /// 停止监控
        /// </summary>
        public void StopMonitoring()
        {
            CheckDisposed();

            if (_isMonitoring)
            {
                _watcher.EnableRaisingEvents = false;
                _isMonitoring = false;
            }
        }

        /// <summary>
        /// 更改监控的文件筛选模式
        /// </summary>
        /// <param name="newFilter">新的筛选模式（如"*.txt"）</param>
        public void ChangeFilter(string newFilter)
        {
            CheckDisposed();

            if (string.IsNullOrWhiteSpace(newFilter))
                throw new ArgumentNullException(nameof(newFilter));

            Filter = newFilter;
            _watcher.Filter = newFilter;
        }

        /// <summary>
        /// 处理文件创建事件
        /// </summary>
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            // 使用BeginInvoke避免UI线程阻塞
            Created?.BeginInvoke(this, e, null, null);
        }

        /// <summary>
        /// 处理文件删除事件
        /// </summary>
        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Deleted?.BeginInvoke(this, e, null, null);
        }

        /// <summary>
        /// 处理文件修改事件
        /// </summary>
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Changed?.BeginInvoke(this, e, null, null);
        }

        /// <summary>
        /// 处理文件重命名事件
        /// </summary>
        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            Renamed?.BeginInvoke(this, e, null, null);
        }

        /// <summary>
        /// 处理错误事件
        /// </summary>
        private void OnError(object sender, ErrorEventArgs e)
        {
            ErrorOccurred?.BeginInvoke(this, e, null, null);
        }

        /// <summary>
        /// 检查对象是否已释放
        /// </summary>
        private void CheckDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(FileWatcherTool));
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否手动释放</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                // 停止监控并释放托管资源
                StopMonitoring();

                // 取消事件订阅
                _watcher.Created -= OnCreated;
                _watcher.Deleted -= OnDeleted;
                _watcher.Changed -= OnChanged;
                _watcher.Renamed -= OnRenamed;
                _watcher.Error -= OnError;

                _watcher.Dispose();
            }

            _isDisposed = true;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~FileWatcherTool()
        {
            Dispose(false);
        }

    }
}
