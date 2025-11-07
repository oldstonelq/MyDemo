// ---------------------------------------------------------------------------------
// File: SystemEventLoggerTool.cs
// Description: 对EventLog封装实现系统日志操作
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Tools.LogHelper
{
    /// <summary>
    /// 系统日置操作类
    /// </summary>
    public class SystemEventLoggerTool
    {
        private readonly string _logName;
        private readonly string _sourceName;
        private EventLog _eventLog;
        private bool _isDisposed;

        /// <summary>
        /// 初始化系统日志管理器
        /// </summary>
        /// <param name="logName">日志名称</param>
        /// <param name="sourceName">日志源名称</param>
        public SystemEventLoggerTool(string logName, string sourceName)
        {
            _logName = string.IsNullOrWhiteSpace(logName) ? "Application" : logName;
            _sourceName = string.IsNullOrWhiteSpace(sourceName) ? "Application" : sourceName;

            InitializeEventLog();
        }

        /// <summary>
        /// 初始化事件日志
        /// </summary>
        private void InitializeEventLog()
        {
            try
            {
                // 检查并创建日志源（需要管理员权限）
                if (!EventLog.SourceExists(_sourceName))
                {
                    EventLog.CreateEventSource(_sourceName, _logName);
                }

                _eventLog = new EventLog(_logName)
                {
                    Source = _sourceName
                };
            }
            catch (SecurityException ex)
            {
                throw new InvalidOperationException("创建日志源需要管理员权限", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("初始化事件日志失败", ex);
            }
        }

        /// <summary>
        /// 写入信息级别的日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void WriteInformation(string message)
        {
            WriteLog(message, EventLogEntryType.Information);
        }

        /// <summary>
        /// 写入警告级别的日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void WriteWarning(string message)
        {
            WriteLog(message, EventLogEntryType.Warning);
        }

        /// <summary>
        /// 写入错误级别的日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void WriteError(string message)
        {
            WriteLog(message, EventLogEntryType.Error);
        }

        /// <summary>
        /// 写入错误级别的日志（包含异常信息）
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="ex">异常对象</param>
        public void WriteError(string message, Exception ex)
        {
            var fullMessage = $"{message}\n异常信息: {ex.Message}\n堆栈跟踪: {ex.StackTrace}";
            WriteLog(fullMessage, EventLogEntryType.Error);
        }

        /// <summary>
        /// 写入事件日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="entryType">日志条目类型</param>
        private void WriteLog(string message, EventLogEntryType entryType)
        {
            CheckDisposed();

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message));

            try
            {
                _eventLog.WriteEntry(message, entryType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("写入事件日志失败", ex);
            }
        }

        /// <summary>
        /// 获取指定数量的最新日志条目
        /// </summary>
        /// <param name="count">要获取的条目数量</param>
        /// <returns>日志条目列表</returns>
        public List<EventLogEntry> GetLatestEntries(int count)
        {
            CheckDisposed();

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "数量必须大于零");

            return _eventLog.Entries.Cast<EventLogEntry>()
                .OrderByDescending(e => e.TimeWritten)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// 根据时间范围获取日志条目
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>日志条目列表</returns>
        public List<EventLogEntry> GetEntriesByTimeRange(DateTime startTime, DateTime endTime)
        {
            CheckDisposed();

            if (startTime > endTime)
                throw new ArgumentException("开始时间不能晚于结束时间");

            return _eventLog.Entries.Cast<EventLogEntry>()
                .Where(e => e.TimeWritten >= startTime && e.TimeWritten <= endTime)
                .OrderByDescending(e => e.TimeWritten)
                .ToList();
        }

        /// <summary>
        /// 根据日志类型获取日志条目
        /// </summary>
        /// <param name="entryType">日志类型</param>
        /// <returns>日志条目列表</returns>
        public List<EventLogEntry> GetEntriesByType(EventLogEntryType entryType)
        {
            CheckDisposed();

            return _eventLog.Entries.Cast<EventLogEntry>()
                .Where(e => e.EntryType == entryType)
                .OrderByDescending(e => e.TimeWritten)
                .ToList();
        }

        /// <summary>
        /// 清除当前日志中的所有条目
        /// </summary>
        public void ClearLog()
        {
            CheckDisposed();

            try
            {
                _eventLog.Clear();
            }
            catch (SecurityException ex)
            {
                throw new InvalidOperationException("清除日志需要管理员权限", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("清除日志失败", ex);
            }
        }

        /// <summary>
        /// 删除指定的日志源
        /// </summary>
        public void DeleteSource()
        {
            CheckDisposed();

            try
            {
                if (EventLog.SourceExists(_sourceName))
                {
                    EventLog.DeleteEventSource(_sourceName);
                }
            }
            catch (SecurityException ex)
            {
                throw new InvalidOperationException("删除日志源需要管理员权限", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("删除日志源失败", ex);
            }
        }

        /// <summary>
        /// 删除指定的日志
        /// </summary>
        public void DeleteLog()
        {
            CheckDisposed();

            try
            {
                if (EventLog.Exists(_logName))
                {
                    EventLog.Delete(_logName);
                }
            }
            catch (SecurityException ex)
            {
                throw new InvalidOperationException("删除日志需要管理员权限", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("删除日志失败", ex);
            }
        }

        /// <summary>
        /// 检查对象是否已释放
        /// </summary>
        private void CheckDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SystemEventLoggerTool));
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
                // 释放托管资源
                _eventLog?.Dispose();
            }

            _isDisposed = true;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~SystemEventLoggerTool()
        {
            Dispose(false);
        }
    }
}
