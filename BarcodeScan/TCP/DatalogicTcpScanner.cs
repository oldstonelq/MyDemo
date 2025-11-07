// ---------------------------------------------------------------------------------
// File: DatalogicTcpScanner.cs
// Description: 德利捷TCP扫码枪实现类，实现TCP通信的德利捷扫码枪功能
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


namespace WindowsFormLearn.Models.BarcodeScanner
{
    /// <summary>
    /// 德利捷TCP扫码枪实现类，实现IScanner接口，通过TCP协议与德利捷扫码枪通信
    /// </summary>
    public class DatalogicTcpScanner : IScanner
    {
        /// <summary>
        /// TCP套接字对象，用于与扫码枪通信
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
        /// 构造函数，初始化德利捷TCP扫码枪
        /// </summary>
        /// <param name="IP">扫码枪的IP地址</param>
        /// <param name="Port">扫码枪的端口号</param>
        public DatalogicTcpScanner(IPAddress IP, int Port)
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
        /// 连接扫码枪的方法，在单独线程中运行，自动重连
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
                        string mCode = SendData(Encoding.ASCII.GetBytes("UT"));
                        if (!string.IsNullOrEmpty(mCode))
                        {
                            this.mConnected = true;
                        }
                        else
                        {
                            throw new Exception("error");
                        }
                    }
                }
                catch (Exception ex)
                {
                    mSocket.Close();
                    this.mConnected = false;
                    Thread.Sleep(2000);
                }
                finally
                {
                    Thread.Sleep(500);
                }
            }
        }
        /// <summary>
        /// 读取扫码枪数据的方法
        /// </summary>
        /// <returns>扫描到的数据字符串，如果扫描失败则返回空字符串</returns>
        public string Read()
        {
            if (Connected == false)
            {
                return "";
            }
            //string mCode = SendData(Encoding.ASCII.GetBytes("UT\r\n"));
            string mCode = SendData(Encoding.ASCII.GetBytes("UT"));
            if (!mCode.Contains("\r") || mCode.Contains("NG") || mCode.StartsWith("\u0002"))
            {   //\r是结尾,不包含结尾直接返回空
                return string.Empty;
            }
            mCode = mCode.Replace("\r", "").Replace("\n", "");
            return mCode;
        }
        /// <summary>
        /// 停止读取数据的方法（目前未实现）
        /// </summary>
        public void StopRead()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 向扫码枪发送数据并接收返回结果的方法
        /// </summary>
        /// <param name="Bytes">要发送的字节数组</param>
        /// <returns>扫码枪返回的数据字符串</returns>
        private string SendData(byte[] Bytes)
        {
            string mReceiveData = string.Empty;
            if (mSocket != null && mSocket.Connected == true)
            {
                if (mSocket.Available > 0)
                {
                    byte[] tempByte = new byte[1024 * 256];
                    int tempInt = mSocket.Receive(tempByte);
                }
                mSocket.Send(Bytes);
                mSocket.ReceiveBufferSize = 1024 * 256;
                byte[] array = new byte[1024 * 256];
                int mLength = 0;
                try
                {
                    mLength = mSocket.Receive(array);
                }
                catch (Exception)
                {
                    mSocket.Dispose();
                    this.mConnected = false;
                }
                if (mLength <= 0)
                {
                    mSocket.Dispose();
                    this.mConnected = false;
                }
                else
                {
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
