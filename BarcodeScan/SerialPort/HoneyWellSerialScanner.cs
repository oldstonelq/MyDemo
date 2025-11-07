// ---------------------------------------------------------------------------------
// File: DatalogicSerialScanner.cs
// Description: 海康威视串口扫码枪实现类，实现串口通信的海康威视扫码枪功能
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
    /// HoneyWell串口扫码枪实现类
    /// </summary>
    public class HoneyWellSerialScanner : IScanner
    {
        /// <summary>
        /// 串口对象，用于与扫码枪通信
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
        /// 构造函数，初始化HoneyWell串口扫码枪
        /// </summary>
        /// <param name="portName">串口名称</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">奇偶校验位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        public HoneyWellSerialScanner(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
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
                    // 发送读扫码枪信息指令判断是否连接成功
                    byte[] mBytes = new byte[] { 0x16, 0x4D, 0x0D, 0x52, 0x45, 0x56, 0x49, 0x4E, 0x46, 0x2E };
                    mSerialPort.Write(mBytes, 0, mBytes.Length);
                    Thread.Sleep(500);
                    if (mSerialPort.BytesToRead > 0)
                    {
                        mConnected = true;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    if (mSerialPort.IsOpen)
                    {
                        mSerialPort.Close();
                    }
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
            byte[] mBytes = new byte[] { 0x16, 0x54, 0x0D };
            mSerialPort.Write(mBytes, 0, mBytes.Length);
            Thread.Sleep(200);
            
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
                        BarcodeValue = BarcodeValue.Replace("\r", "").Replace("\n", "").Trim();
                        break;
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
            
            // 扫描完成后停止扫码
            StopRead();
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
        /// <summary>
        /// 停止扫码操作
        /// </summary>
        public void StopRead()
        {
            if (Connected == false)
            {
                return;
            }
            mSerialPort.DiscardInBuffer();
            mSerialPort.DiscardOutBuffer();
            // 发送停止扫码指令
            byte[] mBytes = new byte[] { 0x16, 0x55, 0x0D };
            mSerialPort.Write(mBytes, 0, mBytes.Length);
        }
    }
}
