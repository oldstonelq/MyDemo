// ---------------------------------------------------------------------------------
// File: KeyenceTcpScanner.cs
// Description: 基恩士TCP扫码枪实现类，实现TCP通信的基恩士扫码枪功能
// Author: [刘晴]
// Create Date: 2025-11-10
// Last Modified: 2025-11-10
// Vison:2.0
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
    /// 基恩士TCP扫码枪实现类，实现IScanner接口
    /// </summary>
    public class KeyenceTcpScanner : IScanner
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
        /// 连接线程
        /// </summary>
        Thread ConnectThread = null;
        /// <summary>
        /// 构造函数，初始化基恩士TCP扫码枪
        /// </summary>
        /// <param name="IP">扫码枪的IP地址</param>
        /// <param name="Port">扫码枪的端口号</param>
        public KeyenceTcpScanner(IPAddress IP, int Port)
        {
            mIP = IP;
            mPort = Port;
        }
        /// <summary>
        /// 初始化扫码枪
        /// </summary>
        public void Init()
        {
            if (ConnectThread == null || ConnectThread.IsAlive == false)
            {
                ConnectThread = new Thread(Connect);
                ConnectThread.Start();
            }
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
                        mSocket.ReceiveTimeout = 4000;
                        this.mConnected = true;
                    }
                }
                catch (Exception)
                {
                    // 连接失败时关闭Socket并更新连接状态
                    mSocket.Close();
                    this.mConnected = false;
                }
                finally
                {
                    Thread.Sleep(500);
                }
            }
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
            // 发送开始扫码指令并返回结果
            return SendData(Encoding.ASCII.GetBytes("LON\r"));
        }

        /// <summary>
        /// 向扫码枪发送数据并接收响应
        /// </summary>
        /// <param name="Bytes">要发送的字节数组</param>
        /// <returns>扫码枪返回的字符串数据</returns>
        private string SendData(byte[] Bytes)
        {
            string mReceiveData = string.Empty;
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
                mSocket.ReceiveBufferSize = 1024 * 256;
                byte[] array = new byte[1024 * 256];
                int mLength = 0;
                
                try
                {
                    // 接收数据
                    mLength = mSocket.Receive(array);
                }
                catch (Exception)
                {
                    // 接收异常时释放Socket资源并更新连接状态
                    mSocket.Dispose();
                    this.mConnected = false;
                }
                
                if (mLength <= 0)
                {
                    // 接收数据长度为0时释放Socket资源并更新连接状态
                    mSocket.Dispose();
                    this.mConnected = false;
                }
                else
                {
                    // 处理接收到的数据
                    byte[] mAllByte = new byte[mLength];
                    Array.Copy(array, mAllByte, mLength);
                    mReceiveData = Encoding.ASCII.GetString(mAllByte);
                }
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
