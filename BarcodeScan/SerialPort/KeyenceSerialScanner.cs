// ---------------------------------------------------------------------------------
// File: DatalogicSerialScanner.cs
// Description: 基恩士串口扫码枪实现类，实现串口通信的基恩士扫码枪功能
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison:1.0
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
    /// 基恩士串口扫码枪实现类，实现IScanner接口
    /// </summary>
    public class KeyenceSerialScanner : IScanner
    {
        /// <summary>
        /// 串口对象，用于与扫码枪通信
        /// </summary>
        private SerialPort mSerialPort;
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
        /// 构造函数，初始化基恩士串口扫码枪
        /// </summary>
        /// <param name="portName">串口名称</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">奇偶校验位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        public KeyenceSerialScanner(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            this.PortName = portName;
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
        /// 连接扫码枪的方法，在独立线程中运行
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
                    // 发送测试指令判断是否连接成功
                    mSerialPort.WriteLine("TEST\r");
                    Thread.Sleep(500);
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
                finally
                {
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// 开始扫码操作
        /// </summary>
        /// <returns>扫描到的条码字符串，如果未连接或扫描失败则返回空字符串</returns>
        public string Read()
        {
            if (Connected == false)
            {
                return "";
            }
            string BarcodeValue = "";
            int Count = 0;
            mSerialPort.DiscardInBuffer();
            mSerialPort.DiscardOutBuffer();
            // 发送开始扫码指令
            mSerialPort.WriteLine("LON\r");
            Thread.Sleep(100);
            
            // 循环读取条码数据，最多尝试30次
            while (true)
            {
                try
                {
                    if (mSerialPort.BytesToRead > 0)
                    {
                        byte[] tempByte = new byte[mSerialPort.BytesToRead];
                        mSerialPort.Read(tempByte, 0, tempByte.Length);
                        BarcodeValue += ASCIIEncoding.ASCII.GetString(tempByte);
                        
                        // 检查是否接收到完整的条码数据
                        if (BarcodeValue.Count(o => o.ToString().Contains("\r")) == 2)
                        {
                            BarcodeValue = BarcodeValue.Split('\r')[1];
                            // 检查是否为错误响应
                            if (BarcodeValue == "ERROR")
                            {
                                BarcodeValue = "";
                            }
                            break;
                        }
                    }
                    else if (Count++ > 30)
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                    // 异常处理，此处为空实现
                }
                finally
                {
                    Thread.Sleep(100);
                }
            }
            return BarcodeValue;
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
