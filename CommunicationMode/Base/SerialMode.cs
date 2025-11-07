// ---------------------------------------------------------------------------------
// File: SerialMode.cs
// Description: 串口通信基础实现
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace CommunicationMode.Base
{
    /// <summary>
    /// 串口通信基类（C# 7.3 兼容版：资源安全+线程安全+通信可靠）
    /// </summary>
    public abstract class SerialMode : IDisposable
    {
        /// <summary>
        /// 串口实例
        /// </summary>
        private SerialPort _serialPort;
        /// <summary>
        /// 线程同步锁（保护所有共享资源操作）
        /// </summary>
        private readonly object _lockObj = new object();
        /// <summary>
        /// 资源释放标志（避免重复释放）
        /// </summary>
        private bool _disposed = false;

        // -------------- 可配置串口参数（支持外部修改/读取） --------------
        /// <summary>
        /// 串口号（如 "COM3"）
        /// </summary>
        public string PortName { get; protected set; } = string.Empty;

        /// <summary>
        /// 波特率（默认 9600）
        /// </summary>
        public int BaudRate { get; protected set; } = 9600;

        /// <summary>
        /// 奇偶校验位（默认 None）
        /// </summary>
        public Parity Parity { get; protected set; } = Parity.None;

        /// <summary>
        /// 数据位（默认 8）
        /// </summary>
        public int DataBits { get; protected set; } = 8;

        /// <summary>
        /// 停止位（默认 1）
        /// </summary>
        public StopBits StopBits { get; protected set; } = StopBits.One;

        /// <summary>
        /// 报文结束符（默认 0：无结束符，需外部设置）
        /// </summary>
        public byte EndCode { get; set; } = 0;

        /// <summary>
        /// 通信超时时间（毫秒，默认 300ms）
        /// </summary>
        public int CommTimeout { get; set; } = 300;

        /// <summary>
        /// 连接状态（对外只读，volatile 确保多线程可见性）
        /// </summary>
        public bool IsConnected { get; private set; } = false;

        // -------------- 构造函数（C# 7.3 兼容：移除 switch 表达式） --------------
        /// <summary>
        /// 初始化串口参数
        /// </summary>
        public SerialMode(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            // 入参校验：串口号非空
            if (string.IsNullOrWhiteSpace(portName))
            {
                throw new ArgumentNullException(nameof(portName), "串口号不能为空");
            }
            PortName = portName;

            // 入参校验：支持的波特率（C# 7.3 不支持模式匹配 or，改用 || ）
            if (baudRate == 1200 || baudRate == 2400 || baudRate == 4800 || baudRate == 9600 ||
                baudRate == 19200 || baudRate == 38400 || baudRate == 57600 || baudRate == 115200)
            {
                BaudRate = baudRate;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(baudRate), "不支持的波特率");
            }

            // 入参校验：数据位（5-8）
            if (dataBits >= 5 && dataBits <= 8)
            {
                DataBits = dataBits;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(dataBits), "数据位必须为 5-8");
            }

            Parity = parity;
            StopBits = stopBits;
        }

        // -------------- 连接/断开核心方法 --------------
        /// <summary>
        /// 连接串口（返回详细结果）
        /// </summary>
        public virtual (bool IsOk, string Msg) ConnectServer()
        {
            lock (_lockObj)
            {
                try
                {
                    // 已连接则直接返回
                    if (IsConnected && _serialPort != null && _serialPort.IsOpen)
                    {
                        return (true, $"串口 {PortName} 已连接");
                    }

                    // 释放旧串口资源
                    DisposeSerialPort();

                    // 初始化串口并配置参数
                    _serialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits)
                    {
                        ReadTimeout = CommTimeout,  // 读取超时
                        WriteTimeout = CommTimeout, // 写入超时
                        DtrEnable = true,           // 适配硬件流控（可按需关闭）
                        RtsEnable = true            // 适配硬件流控（可按需关闭）
                    };

                    // 打开串口
                    _serialPort.Open();
                    IsConnected = true;
                    return (true, $"串口 {PortName} 连接成功");
                }
                catch (UnauthorizedAccessException)
                {
                    IsConnected = false;
                    return (false, $"串口 {PortName} 被占用（权限不足或已被其他程序打开）");
                }
                catch (System.IO.IOException)
                {
                    IsConnected = false;
                    return (false, $"串口 {PortName} 不存在或硬件故障");
                }
                catch (TimeoutException)
                {
                    IsConnected = false;
                    return (false, $"串口 {PortName} 连接超时");
                }
                catch (Exception ex)
                {
                    IsConnected = false;
                    return (false, $"串口 {PortName} 连接失败：{ex.Message}");
                }
            }
        }

        /// <summary>
        /// 连接串口并发送确认指令（适用于需握手的设备）
        /// </summary>
        public virtual (bool IsOk, string Msg) ConnectServer(byte[] confirmCmd)
        {
            // 先执行基础连接
            var connectResult = ConnectServer();
            if (!connectResult.IsOk)
            {
                return connectResult;
            }

            // 发送确认指令并验证响应
            if (confirmCmd == null || confirmCmd.Length == 0)
            {
                return (false, "确认指令不能为空");
            }

            var sendResult = SendAndReceive(confirmCmd);
            if (!sendResult.IsOk || sendResult.ReceiveByte == null)
            {
                DisConnectServer(); // 确认失败则断开连接
                return (false, $"连接确认失败：{sendResult.Msg}");
            }

            // 此处可扩展：根据设备协议验证 ReceiveByte 是否为合法响应
            return (true, $"串口 {PortName} 连接并确认成功");
        }

        /// <summary>
        /// 断开串口连接
        /// </summary>
        public virtual (bool IsOk, string Msg) DisConnectServer()
        {
            lock (_lockObj)
            {
                try
                {
                    if (!IsConnected || _serialPort == null || !_serialPort.IsOpen)
                    {
                        return (true, $"串口 {PortName} 已断开或未初始化");
                    }

                    // 关闭前清空缓冲区（避免残留数据干扰下次连接）
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();
                    _serialPort.Close();
                    IsConnected = false;
                    return (true, $"串口 {PortName} 断开成功");
                }
                catch (Exception ex)
                {
                    return (false, $"串口 {PortName} 断开失败：{ex.Message}");
                }
                finally
                {
                    DisposeSerialPort(); // 释放串口实例
                }
            }
        }

        // -------------- 数据收发核心方法 --------------
        /// <summary>
        /// 发送数据并接收响应（优化结束符判断与超时控制）
        /// </summary>
        /// <returns>(是否成功, 消息, 发送的字节, 接收的字节)</returns>
        public virtual (bool IsOk, string Msg, byte[] SendByte, byte[] ReceiveByte) SendAndReceive(byte[] sendByte)
        {
            byte[] receiveData = null;
            List<byte> receiveBuffer = new List<byte>();
            var startTime = DateTime.Now;

            // 入参校验
            if (sendByte == null || sendByte.Length == 0)
            {
                return (false, "发送数据不能为空", sendByte, receiveData);
            }

            lock (_lockObj)
            {
                try
                {
                    // 校验串口状态
                    if (!IsConnected || _serialPort == null || !_serialPort.IsOpen)
                    {
                        return (false, "串口未连接或已关闭", sendByte, receiveData);
                    }

                    // 清空旧数据（避免历史数据干扰当前通信）
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();

                    // 发送数据
                    _serialPort.Write(sendByte, 0, sendByte.Length);

                    // 循环接收数据（直到超时或检测到结束符）
                    while ((DateTime.Now - startTime).TotalMilliseconds < CommTimeout)
                    {
                        // 读取可用数据
                        if (_serialPort.BytesToRead > 0)
                        {
                            byte[] tempBuffer = new byte[_serialPort.BytesToRead];
                            int readCount = _serialPort.Read(tempBuffer, 0, tempBuffer.Length);
                            // 追加有效数据（避免空数据干扰）
                            if (readCount > 0)
                            {
                                receiveBuffer.AddRange(tempBuffer);
                            }

                            // 检测结束符（每次读取后立即检查，避免数据截断）
                            if (EndCode != 0 && receiveBuffer.Contains(EndCode))
                            {
                                break;
                            }
                        }
                        else
                        {
                            // 无数据时短暂休眠，降低CPU占用
                            Thread.Sleep(5);
                        }
                    }

                    // 处理接收结果
                    receiveData = receiveBuffer.ToArray();
                    if (receiveData.Length == 0)
                    {
                        return (false, "未接收到响应数据（可能超时或设备无反馈）", sendByte, receiveData);
                    }

                    // 若有结束符，截取到结束符（包含结束符，去除后续冗余数据）
                    if (EndCode != 0)
                    {
                        int endIndex = receiveBuffer.IndexOf(EndCode);
                        if (endIndex >= 0)
                        {
                            // C# 7.3 不支持 LINQ Take，改用数组拷贝
                            receiveData = new byte[endIndex + 1];
                            Array.Copy(receiveBuffer.ToArray(), receiveData, endIndex + 1);
                        }
                    }

                    return (true, "发送接收成功", sendByte, receiveData);
                }
                catch (TimeoutException)
                {
                    // 超时后返回已接收的部分数据（便于排查问题）
                    receiveData = receiveBuffer.ToArray();
                    return (false, $"通信超时（{CommTimeout}ms）", sendByte, receiveData);
                }
                catch (System.IO.IOException ex)
                {
                    return (false, $"串口读写失败（硬件故障或断开）：{ex.Message}", sendByte, receiveData);
                }
                catch (Exception ex)
                {
                    return (false, $"通信异常：{ex.Message}", sendByte, receiveData);
                }
            }
        }

        // -------------- 资源释放辅助方法 --------------
        /// <summary>
        /// 释放串口实例
        /// </summary>
        private void DisposeSerialPort()
        {
            if (_serialPort != null)
            {
                try
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.DiscardInBuffer();
                        _serialPort.DiscardOutBuffer();
                        _serialPort.Close();
                    }
                }
                catch
                {
                    // 忽略关闭时的轻微异常（如已关闭的串口）
                }
                finally
                {
                    _serialPort.Dispose();
                    _serialPort = null;
                }
            }
        }

        // -------------- IDisposable 实现（规范资源释放） --------------
        /// <summary>
        /// 主动释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // 告知GC无需调用析构函数
        }
        /// <summary>
        /// 释放资源核心逻辑
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            // 释放托管资源（主动调用时）
            if (disposing)
            {
                DisConnectServer();
            }

            // 释放非托管资源
            DisposeSerialPort();
            _disposed = true;
        }

        /// <summary>
        /// 析构函数（仅作为资源释放兜底）
        /// </summary>
        ~SerialMode()
        {
            Dispose(false);
        }
    }
}