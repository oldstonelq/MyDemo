// ---------------------------------------------------------------------------------
// File: SerialMode.cs
// Description: 日置万用表3562
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HiokiMultimeter.TCPMode
{
    public class Multimeter_3562
    {
        /// <summary>
        /// TCP套接字对象，用于与万用表通信
        /// </summary>
        private Socket mSocket;
        /// <summary>
        /// 连接状态
        /// </summary>
        private bool mConnected = false;
        /// <summary>
        /// 获取万用表的连接状态
        /// </summary>
        public bool Connected
        {
            get { return mConnected; }
        }
        /// <summary>
        /// 万用表的IP地址
        /// </summary>
        private string IPaddress = string.Empty;
        /// <summary>
        /// 万用表的端口号
        /// </summary>
        private int Port = 0;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        public Multimeter_3562(string ip, int port)
        {
            this.IPaddress = ip;
            this.Port = port;
        }
        /// <summary>
        /// 初始化通讯线程
        /// </summary>
        public void Init()
        {
            if (!mConnected)
            {
                Thread th = new Thread(thread_Connect);
                th.Start();
            }
        }
        /// <summary>
        /// 连接线程
        /// </summary>
        private void thread_Connect()
        {
            while (true)
            {
                try
                {
                    if (mConnected == true)
                    {
                        continue;
                    }

                    Ping ping = new Ping();
                    PingReply pingReply = ping.Send(IPaddress, Port);
                    if (pingReply.Status != IPStatus.Success)
                    {
                        ping.Dispose();
                        throw new Exception("connect failure");
                    }
                    else
                    {
                        ping.Dispose();
                        mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        mSocket.ReceiveTimeout = 5000;
                        mSocket.Connect(IPaddress, Port);

                        if (TCPSendByte(ASCIIEncoding.ASCII.GetBytes("*IDN?\r\n"), 10) == "") //读取ID号，用于判断是否联机
                        {
                            throw new Exception("connect failure");
                        }
                        //设置外部触发和触发次数
                        mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":MEM:CLEA\r\n"));
                        mSocket.Send(ASCIIEncoding.ASCII.GetBytes("*RST\r\n"));
                        mSocket.Send(ASCIIEncoding.ASCII.GetBytes("*CLS\r\n"));
                        mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":FUNC RESISTANCE\r\n"));
                        mSocket.Send(ASCIIEncoding.ASCII.GetBytes("RES:RANG 3E-3\r\n"));
                        mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":SAMPLE:RATE SLOW\r\n"));
                        mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":TRIG:SOUR EXT\r\n"));
                        mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":INIT:CONT ON\r\n"));
                        mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":MEMory:STAT ON\r\n"));
                        mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":SYST:EOM:MODE PULS\r\n"));//设置脉冲模式
                        mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":SYST:EOM:PULS 0.05\r\n"));//脉冲宽度设置 50ms
                        mConnected = true;
                    }
                }
                catch (Exception)
                {
                    mConnected = false;
                }
                finally
                {
                    if (mSocket == null || mSocket.Connected == false)
                    {
                        mConnected = false;
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// 对表初始化
        /// </summary>
        public void MetInit()
        {
            if (mConnected == false)
            {
                return;
            }
            //设置外部触发和触发次数
            mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":MEM:CLEA\r\n"));
            mSocket.Send(ASCIIEncoding.ASCII.GetBytes("*CLS\r\n"));
            //mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":TRIG:SOUR EXT\r\n"));
            mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":INIT:CONT ON\r\n"));
            mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":MEMory:STAT ON\r\n"));
        }
        /// <summary>
        /// 清除数据缓存
        /// </summary>
        public void ClearMemory()
        {
            mSocket.Send(Encoding.ASCII.GetBytes(":MEM:CLEA\r\n"));
            Thread.Sleep(50);
        }
        /// <summary>
        /// 发送调零
        /// </summary>
        public void SendTrig()
        {
            mSocket.Send(Encoding.ASCII.GetBytes("*TRG\r\n"));
        }
        /// <summary>
        /// 退出清零模式
        /// </summary>
        public void ClearAdjust()
        {

            mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":ADJust:CLEAr\r\n"));
            Thread.Sleep(50);
        }

        /// <summary>
        /// 内阻表调零,外部硬件必须接法正确才能使用,说明书说调零时间会有点长,10s的等待
        /// </summary>
        /// <returns></returns>
        public bool StartAdjust()
        {
            // Thread.Sleep(1000 * 5);
            if (mConnected == false)
            {
                return false;
            }
            try
            {
                //退出清零模式
                mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":ADJust:CLEAr\r\n"));
                Thread.Sleep(2000);
                //返回0-正常，1-异常
                var res = TCPSendByte(ASCIIEncoding.ASCII.GetBytes(":ADJ?\r\n"), 5 * 1000);

                if (!res.StartsWith("0"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                //退出清零模式
                //mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":ADJust:CLEAr\r\n"));
            }
        }
        /// <summary>
        /// 发送并接收数据
        /// </summary>
        /// <param name="mSendbyte"></param>
        /// <param name="mWaitTime"></param>
        /// <returns></returns>
        public string TCPSendByte(byte[] mSendbyte, int mWaitTime)
        {
            try
            {
                string retString = "";
                if (mSocket != null && mSocket.Connected == true)
                {
                    mSocket.Send(mSendbyte);
                    Thread.Sleep(mWaitTime);
                    byte[] array = new byte[102400];
                    int mLength = mSocket.Receive(array);
                    if (mLength <= 0)
                    {

                    }
                    else
                    {
                        byte[] receiveByte = new byte[mLength];
                        Array.Copy(array, receiveByte, mLength);
                        retString = ASCIIEncoding.ASCII.GetString(receiveByte);
                    }
                }

                return retString;
            }
            catch (Exception)
            {
                mSocket.Dispose();
                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mSocket.ReceiveTimeout = 5000;
                mSocket.Connect(IPaddress, Port);
                return "";
            }
        }
        /// <summary>
        /// 读内存中一个值
        /// </summary>
        /// <returns></returns>
        public string ReadOneData()
        {
            if (mConnected == false)
            {
                return "";
            }
            int num = 0;
            var list = new List<string>();
            var mValue = "";
            do
            {
                var temp = TCPSendByte(Encoding.ASCII.GetBytes(":mem:data?\r\n"), 1000);    //读完缓存中未清空的多余数值
                temp = TCPSendByte(ASCIIEncoding.ASCII.GetBytes(":FETCH?\r\n"), 1500);
                if (temp.StartsWith("END"))
                {
                    if (mValue != "")
                    {
                        break;
                    }
                }
                else
                {
                    mSocket.Send(Encoding.ASCII.GetBytes(":MEM:CLEA\r\n"));
                    Thread.Sleep(50);
                    mValue = temp.Split(',')[0];
                    list.Add(temp);
                    break;
                }
            } while (num++ < 2);

            if (mValue.Contains("END"))
            {
                mSocket.Send(Encoding.ASCII.GetBytes(":MEM:CLEA\r\n"));
            }

            var retValue = "NG";

            //数据处理
            if (list.Count > 0)
            {
                foreach (var t in list)
                {
                    foreach (var item in t.Replace('\n', '\0').Split('\r'))
                    {
                        // if (arr.Length < 2) continue;
                        if (double.TryParse(item.Replace(" ", ""), out var mDblValue) && mDblValue < 9999)
                        {
                            retValue = (mDblValue * 1000).ToString("0.0000");
                            return retValue;
                        }
                    }
                }

            }

            return retValue;
        }
        /// <summary>
        /// 读取多个值
        /// </summary>
        /// <returns></returns>
        public List<string> ReadMultiple()
        {
            string mValue = "";
            var temp = "";
            int num = 0;
            List<string> list = new List<string>();
            do
            {
                temp = TCPSendByte(ASCIIEncoding.ASCII.GetBytes(":mem:data?\r\n"), 1000);
                //temp = TCPSendByte(ASCIIEncoding.ASCII.GetBytes(":FETCH?\r\n"), 1500);
                if (temp.StartsWith("END"))
                {
                    if (mValue != "")
                    {
                        break;
                    }
                }
                else
                {
                    mSocket.Send(Encoding.ASCII.GetBytes(":MEM:CLEA\r\n"));
                    Thread.Sleep(50);
                    mValue = temp;
                    var arr = mValue.Replace("\r\n", "@").Split('@');
                    list.AddRange(arr);
                    if (temp.Contains("END"))
                    {
                        break;
                    }
                }
            } while (num++ < 10);
            //1,  0.3326E-3, 1.00000E+10\r\n  2,  0.3011E-3, 1.00000E+10\r\n  3,  0.2992E-3, 1.00000E+10\r\nEND\r\n
            if (mValue.Contains("END"))
            {
                mSocket.Send(Encoding.ASCII.GetBytes(":MEM:CLEA\r\n"));
                mSocket.Send(Encoding.ASCII.GetBytes(":FETCH?\r\n"));
            }

            return list;
        }
        /// <summary>
        /// 读内存中所有值
        /// </summary>
        /// <returns></returns>
        public string ReadAllData()
        {
            if (mConnected == false)
            {
                return "";
            }
            string retValue = "";
            retValue = TCPSendByte(ASCIIEncoding.ASCII.GetBytes(":FETCH?\r\n"), 1500);

            if (retValue.Contains("END"))
            {
                mSocket.Send(ASCIIEncoding.ASCII.GetBytes(":MEM:CLEA\r\n"));
            }
            //Thread.Sleep(4000);
            return retValue;

        }
        /// <summary>
        /// 数据转换
        /// </summary>
        /// <param name="list"></param>
        /// <param name="chCount"></param>
        /// <returns></returns>
        public double[] GetDataArr(List<string> list, int chCount)
        {
            if (list == null || list.Count == 0) return null;

            var resArr = new double[chCount];
            for (int i = 0; i < resArr.Length; i++)
            {
                resArr[i] = -9999;
            }
            foreach (var t in list)
            {
                var arr = t.Split(',');
                if (arr.Length >= 2)
                {
                    if (int.TryParse(arr[0], out var chNum) && chNum <= chCount
                                                            && double.TryParse(arr[1].Replace(" ", ""), out var mDblValue) && mDblValue < 9999)
                    {
                        resArr[chNum - 1] = Math.Abs(mDblValue) * 1000;
                    }
                }
            }

            return resArr;
        }
    }
}
