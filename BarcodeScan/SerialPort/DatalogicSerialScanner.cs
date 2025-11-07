// ---------------------------------------------------------------------------------
// File: DatalogicSerialScanner.cs
// Description: 德利捷串口扫码枪实现类，实现串口通信的德利捷扫码枪功能
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;


namespace WindowsFormLearn.Models.BarcodeScanner
{
    /// <summary>
    /// 德利捷串口扫码枪实现类，实现IScanner接口
    /// </summary>
    public class DatalogicSerialScanner : IScanner
    {
        /// <summary>
        /// 串口对象
        /// </summary>
        private SerialPort mSerialPort=null;
        /// <summary>
        /// 串口名称
        /// </summary>
        public string PortName;
        /// <summary>
        /// 波特率
        /// </summary>
        private int BaudRate;
        /// <summary>
        /// 校验位
        /// </summary>
        private Parity Parity;
        /// <summary>
        /// 数据位
        /// </summary>
        private int DataBits;
        /// <summary>
        /// 停止位
        /// </summary>
        private StopBits StopBits;
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
        /// 构造函数，初始化德利捷串口扫码枪
        /// </summary>
        /// <param name="PortName">串口名称</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">校验位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        public DatalogicSerialScanner(string PortName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
           this.PortName = PortName;
           this.BaudRate = baudRate;
           this.Parity = parity;
           this.DataBits = dataBits;
           this.StopBits = stopBits;
        }

        /// <summary>
        /// 初始化扫码枪
        /// </summary>
        public void Init()
        {
            mSerialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
            Thread mth = new Thread(Connect);
            mth.Start();
        }
        /// <summary>
        /// 连接扫码枪的方法，在单独线程中运行
        /// </summary>
        private void Connect()
        {
            while (true)
            {
                try
                {
                    if (Connected == true)
                    {
                        continue;
                    }
                    if (mSerialPort.IsOpen == false)
                    {
                        mSerialPort.Open();
                    }
                    mSerialPort.DiscardInBuffer();
                    mSerialPort.DiscardOutBuffer();
                    mSerialPort.WriteLine("UT");
                    Thread.Sleep(3000);
                    if (mSerialPort.BytesToRead > 0)
                    {
                        mConnected = true;
                    }
                    else
                    {
                        mConnected = false;
                    }
                }
                catch (Exception)
                {
                    mConnected = false;
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 读取扫码枪数据的方法
        /// </summary>
        /// <returns>扫描到的数据字符串</returns>
        public string Read()
        {
            try
            {
                if (Connected == true && mSerialPort.IsOpen == true)
                {
                    if (mSerialPort.BytesToRead > 0)
                    {
                        string data = mSerialPort.ReadExisting();
                        return data;
                    }
                }
                return null;
            }
            catch (Exception)
            {
                mConnected = false;
                return null;
            }
        }
        /// <summary>
        /// 关闭扫码枪连接
        /// </summary>
        public void Close()
        {
            if (Connected == true && mSerialPort.IsOpen == true)
            {
                mSerialPort.Close();
            }
        }
    }
}
