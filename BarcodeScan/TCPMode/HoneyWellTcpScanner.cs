// ---------------------------------------------------------------------------------
// File: DatalogicTcpScanner.cs
// Description: 海康威视TCP扫码枪实现类，实现TCP通信的海康威视扫码枪功能
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace BarcodeScan.TCPMode
{
    /// <summary>
    /// 海康威视TCP扫码枪实现类，实现IScanner接口
    /// </summary>
    public class HoneyWellTcpScanner : IScanner
    {
        /// <summary>
        /// Socket对象，用于与扫码枪进行TCP通信
        /// </summary>
        Socket mSocket = null;
        
        /// <summary>
        /// 扫码枪的IP地址
        /// </summary>
        IPAddress mIP;
        
        /// <summary>
        /// 扫码枪的端口号
        /// </summary>
        int mPort = 0;

        /// <summary>
        /// 连接状态
        /// </summary>
        private bool mConnected;
        /// <summary>
        /// 获取扫码枪的连接状态
        /// </summary>
        public bool Connected
        {
            get { return mConnected; }
        }

        /// <summary>
        /// 构造函数，初始化海康威视TCP扫码枪
        /// </summary>
        /// <param name="IP">扫码枪的IP地址</param>
        /// <param name="Port">扫码枪的端口号</param>
        public HoneyWellTcpScanner(IPAddress IP, int Port)
        {
            mIP = IP;
            mPort = Port;
        }
        /// <summary>
        /// 初始化扫码枪
        /// </summary>
        public void Init()
        {
            Thread ConnectThread = new Thread(Connect);
            ConnectThread.Start();
        }
        /// <summary>
        /// 连接扫码枪的方法，在独立线程中运行
        /// </summary>
        private void Connect()
        {
            while (true)
            {
                try
                {
                    if (mSocket == null || mSocket.Connected == false)
                    {
                        mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        mSocket.Connect(this.mIP, this.mPort);
                        mSocket.ReceiveTimeout = 3000;
                        
                        // 清空接收缓冲区
                        if (mSocket.Available > 0)
                        {
                            byte[] bytes = new byte[1024];
                            mSocket.Receive(bytes);
                        }
                        this.mConnected = true;
                    }
                }
                catch (Exception)
                {
                    // 连接失败时关闭并释放Socket资源
                    mSocket.Close();
                    mSocket.Dispose();
                    this.mConnected = false;
                }
                finally
                {
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// 读取扫码枪ID信息
        /// </summary>
        /// <returns>扫码枪ID信息</returns>
        public string ReadID()
        {
            // 发送读取ID指令
            return SendData(new byte[] { 0x16, 0x4D, 0x0D, 0x52, 0x45, 0x56, 0x49, 0x4E, 0x46, 0x2E });
        }

        /// <summary>
        /// 开始扫码操作
        /// </summary>
        /// <returns>扫描到的条码字符串，如果未连接则返回空字符串</returns>
        public string Read()
        {
            if (Connected == false)
            {
                return "";
            }
            // 发送开始扫码指令
            return SendData(new byte[] { 0x16, 0x54, 0x0D });
        }

        /// <summary>
        /// 停止扫码操作
        /// </summary>
        public void StopRead()
        {
            if (Connected == false)
            {
                return;
            }
            // 发送停止扫码指令
            SendData(new byte[] { 0x16, 0x55, 0x0D });
        }

        /// <summary>
        /// 向扫码枪发送数据并接收响应
        /// </summary>
        /// <param name="Bytes">要发送的字节数组</param>
        /// <returns>扫码枪返回的字符串数据</returns>
        private string SendData(byte[] Bytes)
        {
            string mReceiveData = string.Empty;
            try
            {
                if (mSocket != null && mSocket.Connected == true)
                {
                    // 清空接收缓冲区
                    if (mSocket.Available > 0)
                    {
                        byte[] tempByte = new byte[1024 * 256];
                        int tempInt = mSocket.Receive(tempByte);
                    }
                    
                    // 发送数据
                    mSocket.Send(Bytes);
                    
                    // 如果是停止扫码指令，直接返回空字符串
                    if (Bytes[1] == 0x55)
                    {
                        return "";
                    }
                    
                    try
                    {
                        // 等待接收数据
                        Thread.Sleep(200);
                        byte[] array = new byte[1024];
                        int mLength = mSocket.Receive(array);
                        if (mLength <= 0)
                        {
                            throw new Exception();
                        }
                        byte[] mAllByte = new byte[mLength];
                        Array.Copy(array, mAllByte, mLength);
                        mReceiveData = Encoding.ASCII.GetString(mAllByte);
                    }
                    catch (Exception)
                    {
                        // 异常处理，此处为空实现
                    }
                    
                    // 如果是扫码指令且未接收到数据，则发送停止扫码指令
                    if (Bytes[1] == 0x54 && string.IsNullOrEmpty(mReceiveData))
                    {
                        mSocket.Send(new byte[] { 0x16, 0x55, 0x0D });
                    }
                }
            }
            catch (Exception)
            {
                // 异常处理，此处为空实现
            }
            return mReceiveData;
        }
        /// <summary>
        /// 关闭扫码枪连接
        /// </summary>
        public void Close()
        {
            if (Connected == true && mSocket.Connected == true)
            {
                mSocket.Close();
            }
        }
    }
}
