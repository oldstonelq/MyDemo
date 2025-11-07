// ---------------------------------------------------------------------------------
// File: SocketMode.cs
// Description: TCP通信基础实现
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationMode.Connection_Mode
{
    /// <summary>
    /// 同步Socket连接设备的基类（优化版：资源安全+线程安全+连接可靠）
    /// </summary>
    public abstract class SocketMode : IDisposable
    {
        /// <summary>
        /// Socket通讯实例（私有，避免子类直接修改导致状态混乱）
        /// </summary>
        private Socket _socket = null;
        /// <summary>
        /// 端口号
        /// </summary>
        protected int _port = 10000;
        /// <summary>
        /// IP地址
        /// </summary>
        protected string _ip = string.Empty;
        /// <summary>
        /// 连接状态（volatile确保多线程可见性）
        /// </summary>
        private volatile bool _isConnected;
        /// <summary>
        /// 连接状态（对外只读）
        /// </summary>
        public bool Connected => _isConnected;
        /// <summary>
        /// 全局锁对象（所有操作共享同一把锁，确保线程安全）
        /// </summary>
        private readonly object _lockObj = new object(); 
        /// <summary>
        /// 释放标志（避免重复释放）
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// 连接超时时间（毫秒，可外部设置）
        /// </summary>
        public int ConnectTimeout { get; set; } = 3000;

        /// <summary>
        /// 发送超时时间（毫秒，可外部设置）
        /// </summary>
        public int SendTimeout { get; set; } = 3000;

        /// <summary>
        /// 接收超时时间（毫秒，可外部设置）
        /// </summary>
        public int ReceiveTimeout { get; set; } = 3000;

        /// <summary>
        /// 日志记录委托（预留日志扩展点，可外部注入）
        /// </summary>
        public Action<string, Exception> LogError { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        public SocketMode(string ip, int port)
        {
            _ip = ip ?? throw new ArgumentNullException(nameof(ip), "IP地址不能为null");
            if (port < 1 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port), "端口号必须在1-65535之间");
            _port = port;
        }

        /// <summary>
        /// 连接到服务端（同步连接，支持自定义超时）
        /// </summary>
        /// <returns>返回结果（IsOk：是否成功，Msg：提示/错误信息）</returns>
        public virtual (bool IsOk, string Msg) ConnectServer()
        {
            lock (_lockObj)
            {
                try
                {
                    if (_isConnected && _socket?.Connected == true)
                        return (true, "设备已连接");

                    DisposeSocket();

                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        SendTimeout = SendTimeout,
                        ReceiveTimeout = ReceiveTimeout
                    };

                    // 使用自定义超时时间
                    using (var cts = new CancellationTokenSource(ConnectTimeout))
                    {
                        var connectTask = Task.Run(() => _socket.Connect(_ip, _port), cts.Token);
                        connectTask.Wait(cts.Token);

                        if (_socket.Connected)
                        {
                            _isConnected = true;
                            return (true, "连接成功");
                        }
                        else
                        {
                            _isConnected = false;
                            DisposeSocket();
                            return (false, $"连接超时或失败（超时时间：{ConnectTimeout}ms）");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _isConnected = false;
                    DisposeSocket();
                    var errorMsg = $"连接超时（{ConnectTimeout}ms未响应）";
                    LogError?.Invoke(errorMsg, null); // 记录超时日志
                    return (false, errorMsg);
                }
                catch (SocketException ex)
                {
                    _isConnected = false;
                    DisposeSocket();
                    string errorMsg = string.Empty;
                    switch(ex.SocketErrorCode)
                    {
                        case SocketError.ConnectionRefused:
                            errorMsg = "连接被拒绝（目标主机未监听端口）";
                            break;
                        case SocketError.HostNotFound:
                        errorMsg = "找不到目标主机（IP地址无效或无法解析）";
                            break;
                        case SocketError.TimedOut:
                            errorMsg = "连接超时";
                            break;
                        default:
                            errorMsg = $"Socket错误：{ex.SocketErrorCode} - {ex.Message}";
                            break;// 补充默认错误信息
                    };
                    LogError?.Invoke(errorMsg, ex); // 记录Socket异常日志
                    return (false, errorMsg);
                }
                catch (Exception ex)
                {
                    _isConnected = false;
                    DisposeSocket();
                    var errorMsg = $"连接失败：{ex.Message}";
                    LogError?.Invoke(errorMsg, ex); // 记录通用异常日志
                    return (false, errorMsg);
                }
            }
        }
        /// <summary>
        /// 连接并发送确认指令（同步调整：已连接时返回true）
        /// </summary>
        public virtual (bool IsOk, string Msg) ConnectServer(byte[] confirmCmd)
        {
            // 先调用调整后的ConnectServer（已连接时直接返回true）
            var connectResult = ConnectServer();
            if (!connectResult.IsOk)
                return connectResult;

            // 若已连接，直接验证确认指令（或跳过，根据需求调整）
            if (confirmCmd == null || confirmCmd.Length == 0)
                return (false, "确认指令不能为空");

            var sendResult = SendAndReceive(confirmCmd);
            if (!sendResult.IsOk || sendResult.ReceiveByte == null)
            {
                DisConnectServer();
                return (false, $"连接确认失败：{sendResult.Msg}");
            }

            return (true, $"设备连接并确认成功（{_ip}:{_port}）");
        }
        /// <summary>
        /// 断开连接（公开方法，允许外部主动调用）
        /// </summary>
        /// <returns>返回结果（IsOk：是否成功，Msg：提示/错误信息）</returns>
        public virtual (bool IsOk, string Msg) DisConnectServer()
        {
            lock (_lockObj)
            {
                try
                {
                    if (!_isConnected || _socket == null)
                        return (true, "已断开连接或未初始化");

                    _socket.Shutdown(SocketShutdown.Both);
                    DisposeSocket();
                    _isConnected = false;
                    return (true, "断开连接成功");
                }
                catch (SocketException ex)
                {
                    var errorMsg = $"断开连接失败：{ex.SocketErrorCode} - {ex.Message}";
                    LogError?.Invoke(errorMsg, ex);
                    return (false, errorMsg);
                }
                catch (Exception ex)
                {
                    var errorMsg = $"断开连接失败：{ex.Message}";
                    LogError?.Invoke(errorMsg, ex);
                    return (false, errorMsg);
                }
            }
        }

        /// <summary>
        /// 发送数据并接收响应（优化线程安全和连接检测）
        /// </summary>
        protected virtual (bool IsOk, string Msg, byte[] SendByte, byte[] ReceiveByte) SendAndReceive(byte[] sendByte)
        {
            byte[] receiveByte = null;

            if (sendByte == null || sendByte.Length == 0)
                return (false, "发送数据为空", sendByte, receiveByte);

            lock (_lockObj)
            {
                try
                {
                    if (!_isConnected || _socket == null || !_socket.Connected)
                        return (false, "连接已断开或未初始化", sendByte, receiveByte);

                    if (!IsSocketConnected())
                    {
                        _isConnected = false;
                        return (false, "连接已失效（网络中断）", sendByte, receiveByte);
                    }

                    int actualSent = _socket.Send(sendByte);
                    if (actualSent != sendByte.Length)
                    {
                        var errorMsg = $"发送不完整（已发：{actualSent}/{sendByte.Length}字节）";
                        LogError?.Invoke(errorMsg, null);
                        return (false, errorMsg, sendByte, receiveByte);
                    }

                    using (var ms = new MemoryStream())
                    {
                        byte[] buffer = new byte[1024 * 10];
                        int readCount;

                        while ((readCount = _socket.Receive(buffer)) > 0)
                        {
                            ms.Write(buffer, 0, readCount);
                            // 【根据协议补充】结束标识判断
                        }

                        receiveByte = ms.ToArray();
                    }

                    if (receiveByte.Length == 0)
                    {
                        var errorMsg = "未接收到响应数据";
                        LogError?.Invoke(errorMsg, null);
                        return (false, errorMsg, sendByte, receiveByte);
                    }

                    return (true, "发送接收成功", sendByte, receiveByte);
                }
                catch (SocketException ex)
                {
                    _isConnected = false;
                    var errorMsg = $"Socket异常：{ex.SocketErrorCode} - {ex.Message}";
                    LogError?.Invoke(errorMsg, ex);
                    return (false, errorMsg, sendByte, receiveByte);
                }
                catch (Exception ex)
                {
                    var errorMsg = $"操作失败：{ex.Message}";
                    LogError?.Invoke(errorMsg, ex);
                    return (false, errorMsg, sendByte, receiveByte);
                }
            }
        }

        /// <summary>
        /// 检测Socket是否真正连接
        /// </summary>
        private bool IsSocketConnected()
        {
            if (_socket == null) return false;

            try
            {
                return !_socket.Poll(100, SelectMode.SelectRead) || _socket.Available > 0;
            }
            catch (SocketException ex)
            {
                LogError?.Invoke("检测连接状态时发生异常", ex);
                return false;
            }
        }

        /// <summary>
        /// 释放Socket资源
        /// </summary>
        private void DisposeSocket()
        {
            if (_socket != null)
            {
                try
                {
                    if (_socket.Connected)
                        _socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception ex)
                {
                    LogError?.Invoke("关闭Socket时发生异常", ex);
                }
                finally
                {
                    _socket.Close();
                    _socket.Dispose();
                    _socket = null;
                }
            }
        }

        /// <summary>
        /// 主动释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 资源释放核心逻辑
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                DisConnectServer();
            }

            _disposed = true;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~SocketMode()
        {
            Dispose(false);
        }
    }
}