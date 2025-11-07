// ---------------------------------------------------------------------------------
// File: SerialMode.cs
// Description: 日置万用表7276
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HiokiMultimeter.TCPMode
{
    public class Multimeter_7276
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
        /// 获取连接状态
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
        public Multimeter_7276(string ip, int port)
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

                    mConnected = MetInit();
                }
                catch (Exception)
                {
                    mConnected = false;
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }
        }
        /// <summary>
        /// 外部触发时,需要设置触发的次数（初始化）
        /// </summary>
        /// <returns></returns>
        public bool MetInit()
        {
            try
            {
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(IPaddress, Port);
                if (pingReply.Status != IPStatus.Success)
                {
                    ping.Dispose();
                    throw new Exception("connect failure");
                }
                else
                {
                    //ping.Dispose();
                    mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    mSocket.ReceiveTimeout = 1000;
                    mSocket.Connect(IPaddress, Port);

                    mSocket.Send(ASCIIEncoding.ASCII.GetBytes("DATA:CLE\r\n"));
                    mSocket.Send(ASCIIEncoding.ASCII.GetBytes("*CLS\r\n"));
                    //mSocket.Send(ASCIIEncoding.ASCII.GetBytes("*RST\r\n"));
                    Thread.Sleep(500);
                    mSocket.Send(ASCIIEncoding.ASCII.GetBytes("CONF:VOLT:DC 10\r\n"));
                    mSocket.Send(ASCIIEncoding.ASCII.GetBytes("VOLT:DC:NPLC 0.2\r\n"));//测试速度

                    if (TCPSendByte(ASCIIEncoding.ASCII.GetBytes("*IDN?\r\n")) == "") //读取ID号，用于判断是否联机
                    {
                        throw new Exception("connect failure");
                    }

                    mSocket.Send(Encoding.ASCII.GetBytes(":TRIG:SOUR EXT\r\n"));
                    mSocket.Send(Encoding.ASCII.GetBytes(":INIT:CONT ON\r\n"));

                    mSocket.Send(Encoding.ASCII.GetBytes(":IO:EOM:PULS 0.05\r\n"));//设置脉冲模式 宽度设置 50ms
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 修改电压表量程
        /// </summary>
        /// <param name="range">量程，单位为V</param>
        /// <param name="ms">延时毫秒数</param>
        public void SendRange(float range, int ms)
        {
            mSocket.Send(ASCIIEncoding.ASCII.GetBytes("CONF:VOLT:DC " + range + "\r\n"));
            mSocket.Send(ASCIIEncoding.ASCII.GetBytes("VOLT:DC:NPLC 0.2\r\n"));//测试速度
                                                                               // mSocket.Send(Encoding.ASCII.GetBytes(":TRIG:SOUR EXT\r\n"));
            mSocket.Send(Encoding.ASCII.GetBytes(":INIT:CONT ON\r\n"));
            Thread.Sleep(ms);

        }
        /// <summary>
        /// 外部触发时,需要触发
        /// </summary>
        public void Test()
        {
            if (Connected == false)
            {
                return;
            }
            mSocket.Send(Encoding.ASCII.GetBytes("DATA:CLE\r\n"));
            mSocket.Send(Encoding.ASCII.GetBytes("*CLS\r\n"));
        }
        /// <summary>
        /// 发送触发命令
        /// </summary>
        /// 
        public void SendTrig()
        {
            if (Connected == false)
            {
                return;
            }
            mSocket.Send(Encoding.ASCII.GetBytes("*TRG\r\n"));
        }
        /// <summary>
        /// 发送并接收数据
        /// </summary>
        /// <param name="mSendbyte"></param>
        /// <returns></returns>
        public string TCPSendByte(byte[] mSendbyte)
        {
            try
            {
                string retString = "";
                int count = 0;
                if (mSocket != null && mSocket.Connected == true)
                {
                    //PublicPara.AddRealTimeMessage("发送命令:");
                    mSocket.Send(mSendbyte);
                    while (true)
                    {
                        byte[] array = new byte[102400];
                        //PublicPara.AddRealTimeMessage("等待接收");
                        int mLength = mSocket.Receive(array);
                        if (mLength <= 0)
                        {
                            break;
                        }
                        else
                        {
                            //PublicPara.AddRealTimeMessage("等待次数："+ (count + 1));
                            byte[] receiveByte = new byte[mLength];
                            Array.Copy(array, receiveByte, mLength);
                            retString += ASCIIEncoding.ASCII.GetString(receiveByte);
                            if (retString.Contains("\r\n") == true)
                            {
                                break;
                            }
                            else
                            {
                                Thread.Sleep(100);
                                if (count++ > 50)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                }
                return retString;
            }
            catch (Exception)
            {
                return "";
            }
        }
        /// <summary>
        /// 读值
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public string ReadData(int num = 0)
        {
            if (Connected == false)
            {
                return "";
            }

            int count = 0;
            string str = "";
            do
            {
                //PublicPara.AddRealTimeMessage($"{count + 1}发送读表指令");
                str += TCPSendByte(ASCIIEncoding.ASCII.GetBytes(":R?\r\n"));

                //PublicPara.AddRealTimeMessage($"{count + 1}电压匹配开始");
                Regex r = new Regex("[\\+\\-]\\d\\.\\d+E[\\+\\-]\\d{2}");//保证最少有N个符合条件的数据
                if (r.Matches(str).Count >= num)
                {
                    break;
                }

                Thread.Sleep(50);
                //PublicPara.AddRealTimeMessage($"{count + 1}电压匹配结束");
            } while (count++ < 5);

            ////找到第一个正号或负号的位置
            //int pos = str.IndexOfAny(new char[] { '+', '-' });
            ////将此字符之前的内容删除
            //if (pos >= 0) str = str.Substring(pos);
            //str = str.Replace("\r\n", "");

            //Console.WriteLine(str);
            return str;
        }
        /// <summary>
        /// 数据转换
        /// </summary>
        /// <param name="mValue"></param>
        /// <returns></returns>
        public double[] GetDataArr(string mValue)
        {
            if (string.IsNullOrEmpty(mValue)) return null;
            Regex r = new Regex("[\\+\\-]\\d\\.\\d+E[\\+\\-]\\d{2}");//保证最少有N个符合条件的数据
            var mArrValue = r.Matches(mValue);
            var res = new double[mArrValue.Count];
            //数据处理
            for (int i = 0; i < res.Length; i++)
            {
                if (double.TryParse(mArrValue[i].Value, out var mDblValue))
                {
                    res[i] = mDblValue;
                }
                else
                {
                    res[i] = 0;
                }
            }

            return res;
        }
    }
}
