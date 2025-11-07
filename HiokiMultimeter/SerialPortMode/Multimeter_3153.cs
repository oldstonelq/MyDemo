// ---------------------------------------------------------------------------------
// File: Multimeter_3153.cs
// Description: 日置绝缘测试仪表3153
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
using System.Threading.Tasks;

namespace HiokiMultimeter.SerialPortMode
{
    public class Multimeter_3153
    {
        /*绝缘测试表3153通讯
         * 下发指令增加\r\n后缀，转换成ASCII编码下发
         *  *IDN? 查询版本号 返回版本信息 比如HIOKI，3153,0，V1.00  初次建立连接时查询版本号，正常返回则代表连接成功
         *  *CLS 清空寄存器 无返回
         *  *RST 恢复初始设置 无返回
         *  *ESR? 查询寄存器状态或内容 返回数字比特位 比如128 ，64，32， 
         *  *TST？ 自检 返回错误寄存器代号 比如3
         *  :ESR0? 查询寄存器0的状态或内容 返回错误比特位 比如1，2，4， 
         *  :HEADer 启用或禁用响应消息的表头 例:HEADer ON 无返回
         *  :HEADer? 查询是否启用响应消息的表头 返回 ON/OFF
         *  :SYSTem:Error? 查询 RS-232c 通信错误 返回错误比特位 比如 1，2，4
         *  :MODE?  模式查询 返回模式的代码 MWITH : 耐压测试模式 MINS : 绝缘电阻测试模式 AWI : 耐压→绝缘电阻自动测试模式 AIW : 绝缘电阻→耐压自动测试模式
         *  :MODE   设置测试模式 无返回 例如 设置为耐压测试模式:MODE MWITH
         *  :STATe? 查询状态 返回设备状态代码 WREADY 耐压模式测试准备就绪 IREADY 绝缘测试准备就绪
         *  :STARt 测试开始 无返回
         *  :STOP  停止测试 无返回
         *  :CONFigure:WITHstand:KIND 设置电压测试模式（频率）  例:CONFigure:WITHstand:KIND AC50  设置成AC 50Hz
         *  :CONFigure:WITHstand:KIND？查询电压测试模式 返回模式代码 AC50
         *  :CONFigure:WITHstand:VOLTage 设置耐压测试电压  例:CONFigure:WITHstand:VOLTage 1.00 设置为1kv
         *  :CONFigure:WITHstand:VOLTage? 查询耐压测试电压 返回设定电压值
         *  :CONFigure:WITHstand:CUPPer 设置耐压测试电流上限 例如:CONFigure:WITHstand:CUPPer 5.0 单位mA
         *  :CONFigure:WITHstand:CUPPer? 查询耐压测试电流上限 返回设置值 单位mA
         *  :CONFigure:WITHstand:CLOWer 设置耐压测试电流下限 例如 :CONFigure:WITHstand:CLOWer 1.0 单位mA
         *  :CONFigure:WITHstand:CLOWer? 查询耐压测试电流下限 返回设置值 单位mA
         *  :CONFigure:WITHstand:TIMer 设置耐压测试时间 例如 :CONFigure:WITHstand:CLOWer 30  单位s
         *  :CONFigure:WITHstand:TIMer? 查询耐压测试时间 返回设定值 单位s
         *  :CONFigure:WITHstand:UTIMer 设置耐压测试的上升时间 例如:CONFigure:WITHstand:UTIMer 10.0 单位s
         *  :CONFigure:WITHstand:UTIMer? 查询耐压测试的上升时间 返回设定值 单位s
         *  :CONFigure:WITHstand:DTIMer 设置耐压测试的下降时间 例如:CONFigure:WITHstand:DTIMer 5.0 单位s
         *  :CONFigure:WITHstand:DTIMer?查询耐压测试的下降时间 返回设定值 单位s
         *  :WITHstand:CLOWer? 查询耐压测试的下限值是否启用 返回 ON/OFF
         *  :WITHstand:CLOWer 启用或禁用耐压测试下限值 例如 :WITHstand:CLOWer OFF
         *  :WITHstand:TIMer 启用或禁用耐压测试时间  例如 :WITHstand:CLOWer ON
         *  :WITHstand:TIMer? 查询耐压测试时间是否启用 返回OFF/ON
         *  :WITHstand:UTIMer 启用或禁用耐压测试上升时间 例如 :WITHstand:UTIMer ON
         *  :WITHstand:UTIMer? 查询耐压测试上升时间是否启用 返回OFF/ON
         *  :WITHstand:DTIMer 启用或禁用耐压测试下降时间 例如 :WITHstand:DTIMer OFF
         *  :WITHstand:DTIMer? 查询耐压测试下降时间是否启用 返回 OFF/ON
         *  :MEASure:RESult:WITHstand? 查询耐压模式测试结果 返回电压 电流 测试时间 测试结果 时间类型
         *  :MEASure:WITHstand:VOLTage? 查询耐压测试电压测试结果 返回实测值 单位kv
         *  :MEASure:WITHstand:CURRent? 查询耐压模式电流测试值 返回实测值 单位mA
         *  :MEASure:WITHstand:TIMer? 查询耐压模式测试时间 返回实测值 单位s
         *  :MEMory:WITHstand:FILE? 查询耐压测试设置参数
         *  :MEMory:WITHstand:LOAD 加载配制方案 例如:MEMory:WITHstand:LOAD 1 加载1号配置方案
         *  :MEMory:WITHstand:SAVE 保存配置方案 例如:MEMory:WITHstand:SAVE 2 保存当前配置为2号方案
         *  :MEMory:WITHstand:CLEar 删除配置方案 例如:MEMory:WITHstand:CLEar 3 删除三号配置方案
         *  :CONFigure:INSulation:VOLTage 设置绝缘测试电压 范围50到1200V 例:CONFigure:INSulation:VOLTage 500 设置为500V
         *  :CONFigure:INSulation:VOLTage? 查询绝缘测试电压 返回设定电压值
         *  :CONFigure:INSulation:RUPPer 设置绝缘测试电阻上限 例如:CONFigure:INSulation:RUPPer 5.0 单位MΩ
         *  :CONFigure:INSulation:RUPPer? 查询绝缘测试电阻上限
         *  :CONFigure:INSulation:RLOWer 设置绝缘测试电阻下限 例如 :CONFigure:INSulation:RLOWer 10.0 单位MΩ
         *  :CONFigure:INSulation:RLOWer? 查询绝缘测试电阻下限 返回设置值 单位MΩ
         *  :CONFigure:INSulation:TIMer 设置绝缘测试时间 例如 :CONFigure:INSulation:TIMer 30  单位s
         *  :CONFigure:INSulation:TIMer? 查询绝缘测试时间 返回设定值 单位s
         *  :CONFigure:INSulation:DELay 设置绝缘测试延迟时间 例如:CONFigure:INSulation:DELay 3.0 设置为3s延迟
         *  :CONFigure:INSulation:DELay?查询绝缘测试延迟时间
         *  :INSulation:RUPPer 启用或禁用绝缘测试内阻上限
         *  :INSulation:RUPPer?查询是否启用绝缘测试内阻上限
         *  :INSulation:TIMer 启用或禁用绝缘测试时间 
         *  :INSulation:TIMer?查询是否启用绝缘测试时间
         *  :INSulation:DELay 启用或禁用绝缘测试延迟时间
         *  :INSulation:DELay?查询是否启用绝缘测试延迟时间
         *  :MEASure:RESult:INSulation? 查询绝缘测试结果，返回电压 内阻 测试时间 结果
         *  :MEASure:INSulation:VOLTage? 查询绝缘测试电压结果
         *  :MEASure:INSulation:RESistance? 查询绝缘测试内阻
         *  :MEASure:INSulation:TIMer? 查询绝缘测试时间
         *  :MEMory:INSulation:FILE? 查询绝缘测试设置参数
         *  :MEMory:INSulation:LOAD 加载绝缘测试配置方案
         *  :MEMory:INSulation:SAVE 保存绝缘测试配置方案
         *  :MEMory:INSulation:CLEar 删除绝缘测试配置方案
         *  :PROGram:EDIT:FILE 设置扫描模式
         *  :PROGram:EDIT:FILE?查询扫描模式配置 返回配置代号
         *  :PROGram:EDIT:STEP 设置测试步骤
         *  :PROGram:EDIT:STEP? 设置步骤查询
         *  :PROGram:LOAD:FILE 加载测试步骤
         *  :PROGram:RESult:FILE? 查询测试配置代号
         *  :MEASure:RESult:STEP? 查询相应代号设备配置
         * */
        /// <summary>
        /// 串口对象
        /// </summary>
        private SerialPort mSerialPort;
        /// <summary>
        /// 串口号
        /// </summary>
        private string PortName;
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
        /// 构造函数
        /// </summary>
        /// <param name="PortName">串口号</param>
        /// <param name="index"></param>
        public Multimeter_3153(string portName)
        {
            this.PortName = portName;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (!mConnected)
            {
                mSerialPort = new SerialPort(PortName, 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                Thread th = new Thread(thread_Connect);
                th.IsBackground = true;
                th.Start();
            }
        }
        /// <summary>
        /// 连接函数，查询版本号，查询成功则认为连接成功，清除记忆以及初始化
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

                    if (mSerialPort.IsOpen == false)
                    {
                        mSerialPort.Open();
                    }
                    mSerialPort.DiscardOutBuffer();
                    mSerialPort.DiscardInBuffer();
                    Send("*IDN?");
                    //byte[] asciiBytes = System.Text.Encoding.ASCII.GetBytes("*IDN?\r\n");
                    //mSerialPort.Write(asciiBytes, 0, asciiBytes.Length);

                    //mSerialPort.Write ("*IDN?CR\r\n");//查询版本号，有返回则认为通讯成功

                    //string message = "*IDN?"; // 要发送的字符串
                    //byte[] bytesToSend = System.Text.Encoding.ASCII.GetBytes(message); // 将字符串转换为字节数组
                    //string hexString = ByteArrayToHexString(bytesToSend); // 将字节数组转换为十六进制字符串

                    //mSerialPort.WriteLine(hexString);
                    Thread.Sleep(1000);
                    if (mSerialPort.BytesToRead <= 0)
                    {
                        throw new Exception("connect falure");
                    }
                    else
                    {
                        string reslut = Received();
                    }
                    //mSerialPort.WriteLine(":MEM:CLEA");
                    //mSerialPort.WriteLine("*RST");//初始化
                    //mSerialPort.WriteLine("*CLS");
                    //mSerialPort.WriteLine(":FUNC RESISTANCE");
                    //mSerialPort.WriteLine("RES:RANG 300E-3");
                    //mSerialPort.WriteLine(":SAMPLE:RATE SLOW");
                    //mSerialPort.WriteLine(":TRIG:SOUR EXT");
                    //mSerialPort.WriteLine(":INIT:CONT ON");
                    //mSerialPort.WriteLine(":MEMory:STAT ON");
                    Send("*CLS");
                    Send("*RST");
                    mConnected = true;
                    Thread.Sleep(3000);


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
        /// 指令发送
        /// </summary>
        /// <param name="data">要发送的命令字符</param>
        private void Send(string data)
        {
            try
            {
                data += Environment.NewLine;
                byte[] asciiBytes = System.Text.Encoding.ASCII.GetBytes(data);
                mSerialPort.Write(asciiBytes, 0, asciiBytes.Length);
            }
            catch (Exception)
            {

                //throw;
            }

        }
        /// <summary>
        /// 接收返回
        /// </summary>
        /// <returns></returns>
        private string Received()
        {
            byte[] tempByte = new byte[mSerialPort.BytesToRead];
            mSerialPort.Read(tempByte, 0, tempByte.Length);
            string reslut = Encoding.Default.GetString(tempByte);
            return reslut;
        }
        /// <summary>
        /// 读值
        /// </summary>
        /// <returns></returns>
        public string ReadValue(TestType Type)
        {
            if (mConnected == false)
            {
                return null;
            }
            string BarcodeValue = "";
            //int Count = 0;
            mSerialPort.DiscardInBuffer();
            mSerialPort.DiscardOutBuffer();
            switch (Type)
            {
                case TestType.耐压测试:
                    Send(":MEASure:RESult:WITHstand?");
                    break;
                case TestType.绝缘测试:
                    Send(":MEASure:RESult:INSulation?");
                    break;
                default:
                    break;
            }

            Thread.Sleep(100);
            while (true)
            {
                try
                {
                    if (mSerialPort.BytesToRead > 0)
                    {
                        byte[] tempByte = new byte[mSerialPort.BytesToRead];
                        mSerialPort.Read(tempByte, 0, tempByte.Length);
                        BarcodeValue += ASCIIEncoding.ASCII.GetString(tempByte);
                    }
                    else if (BarcodeValue.Contains("\r\n"))
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    Thread.Sleep(100);
                }
            }
            return BarcodeValue.Replace("\r\n", "").Trim();


        }
        /// <summary>
        /// 清除记忆
        /// </summary>
        public void ClearMEMory()
        {
            Send("*CLS");
        }
        /// <summary>
        /// 开始测试
        /// </summary>
        public void StartTest()
        {
            Send(":STARt");
        }
        /// <summary>
        /// 停止工作
        /// </summary>
        public void StopWork()
        {
            Send(":STOP");
        }
        /// <summary>
        /// 查询仪表状态
        /// </summary>
        /// <returns></returns>
        public string QueryState()
        {
            string retValue = "";
            mSerialPort.DiscardOutBuffer();
            mSerialPort.DiscardInBuffer();
            Send(":STATe?");
            Thread.Sleep(100);
            while (true)
            {
                try
                {
                    if (mSerialPort.BytesToRead > 0)
                    {
                        byte[] mbyte = new byte[mSerialPort.BytesToRead];
                        mSerialPort.Read(mbyte, 0, mbyte.Length);
                        retValue += ASCIIEncoding.ASCII.GetString(mbyte);
                    }
                    else if (retValue.Contains("\r\n"))
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                    //throw;
                }

            }
            return retValue.Replace("\r\n", "").Trim();
        }
        /// <summary>
        /// 切换模式
        /// </summary>
        /// <returns></returns>
        public void ChangeMode(TestType Type)
        {
            switch (Type)
            {
                case TestType.耐压测试:
                    Send(":MODE WI");
                    break;
                case TestType.绝缘测试:
                    Send(":MODE IW");
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 参数发送
        /// </summary>
        /// <param name="TestParas"></param>
        public void SendParas(TestParas TestParas)
        {
            #region 耐压测试参数
            Send($":CONFigure:WITHstand:KIND {TestParas.KIND}");
            Send($":CONFigure:WITHstand:VOLTage {TestParas.WVOLTage}");
            Send($":CONFigure:WITHstand:CUPPer {TestParas.CUPPer}");
            Send($":CONFigure:WITHstand:CLOWer {TestParas.CLOWer}");
            Send($":CONFigure:WITHstand:TIMer {TestParas.WTIMer}");
            Send($":CONFigure:WITHstand:UTIMer {TestParas.UTIMer}");
            Send($":CONFigure:WITHstand:DTIMer {TestParas.DTIMer}");
            if (TestParas.CLOWerEnable)
            {
                Send(":WITHstand:CLOWer ON");
            }
            else
            {
                Send(":WITHstand:CLOWer OFF");
            }
            if (TestParas.WTIMerEnable)
            {
                Send($":WITHstand:TIMer ON");
            }
            else
            {
                Send(":WITHstand:TIMer OFF");
            }
            if (TestParas.UTIMerEnable)
            {
                Send(":WITHstand:UTIMer ON");
            }
            else
            {
                Send(":WITHstand:UTIMer OFF");
            }
            if (TestParas.DTIMerEnable)
            {
                Send(":WITHstand:DTIMer ON");
            }
            else
            {
                Send(":WITHstand:DTIMer OFF");
            }
            #endregion

            //#region 绝缘测试参数
            //Send($":CONFigure:INSulation:VOLTage {TestParas.IVOLTage}");
            //Send($":CONFigure:INSulation:RUPPer {TestParas.RUPPer}");
            //Send($":CONFigure:INSulation:RLOWer {TestParas.RLOWer}");
            //Send($":CONFigure:INSulation:TIMer {TestParas.ITIMer}");
            //Send($":CONFigure:INSulation:DELay {TestParas.DELay}");
            //if (TestParas.RUPPerEnable)
            //{
            //    Send($":INSulation:RUPPer ON");
            //}
            //else
            //{
            //    Send($":INSulation:RUPPer OFF");
            //}
            //if (TestParas.ITIMerEnable)
            //{
            //    Send($":INSulation:TIMer ON");
            //}
            //else
            //{
            //    Send($":INSulation:TIMer OFF");
            //}
            //if (TestParas.DELayEnable)
            //{
            //    Send($":INSulation:DELay ON");
            //}
            //else
            //{
            //    Send($":INSulation:DELay OFF");
            //}
            //#endregion

        }
        /// <summary>
        /// 将字节数组转换为十六进制字符串的帮助方法
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 2);
            foreach (byte b in data)
            {
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            }
            return sb.ToString().ToUpper();
        }
    }

    /// <summary>
    /// 绝缘测试参数
    /// </summary>
    public class TestParas
    {
        /// <summary>
        /// 耐压测试类型
        /// </summary>
        public string KIND { get; set; }
        /// <summary>
        /// 耐压测试电压
        /// </summary>
        public string WVOLTage { get; set; }
        /// <summary>
        /// 耐压测试电流上限
        /// </summary>
        public string CUPPer { get; set; }
        /// <summary>
        /// 耐压测试电流下限
        /// </summary>
        public string CLOWer { get; set; }
        /// <summary>
        /// 耐压测试下限是否启用
        /// </summary>
        public bool CLOWerEnable { get; set; }
        /// <summary>
        /// 耐压测试时间
        /// </summary>
        public string WTIMer { get; set; }
        /// <summary>
        /// 耐压测试时间是否启用
        /// </summary>
        public bool WTIMerEnable { get; set; }
        /// <summary>
        /// 耐压测试上升时间
        /// </summary>
        public string UTIMer { get; set; }
        /// <summary>
        /// 耐压测试上升时间是否启用
        /// </summary>
        public bool UTIMerEnable { get; set; }
        /// <summary>
        /// 耐压测试下降时间
        /// </summary>
        public string DTIMer { get; set; }
        /// <summary>
        /// 耐压测试下降时间是否启用
        /// </summary>
        public bool DTIMerEnable { get; set; }
        /// <summary>
        /// 绝缘测试电压
        /// </summary>
        public string IVOLTage { get; set; }
        /// <summary>
        /// 绝缘测试电阻上限
        /// </summary>
        public string RUPPer { get; set; }
        /// <summary>
        /// 绝缘测试电阻上限是否启用
        /// </summary>
        public bool RUPPerEnable { get; set; }
        /// <summary>
        /// 绝缘测试电阻下限
        /// </summary>
        public string RLOWer { get; set; }
        /// <summary>
        /// 绝缘测试时间
        /// </summary>
        public string ITIMer { get; set; }
        /// <summary>
        /// 绝缘测试时间是否启用
        /// </summary>
        public bool ITIMerEnable { get; set; }
        /// <summary>
        /// 绝缘测试延迟时间
        /// </summary>
        public string DELay { get; set; }
        /// <summary>
        /// 绝缘测试延迟时间是否启用
        /// </summary>
        public bool DELayEnable { get; set; }
    }

    /// <summary>
    /// 万用表测试模式
    /// </summary>
    public enum TestType
    {
        耐压测试,
        绝缘测试
    }
}
