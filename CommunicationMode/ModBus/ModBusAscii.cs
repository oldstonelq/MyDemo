using CommunicationMode.Connection_Mode;
using CommunicationMode.Connection_Mode.Base;
using System;
using System.IO.Ports;
using System.Text;

namespace CommunicationMode.Modbus.ModBus
{
    /// <summary>
    /// Modbus ASCII 客户端（基于 SerialMode 扩展，兼容 C# 7.0+）
    /// 支持功能码：01(读线圈)、02(读离散输入)、03(读保持寄存器)、04(读输入寄存器)
    ///            05(写单个线圈)、06(写单个寄存器)、15(写多个线圈)、16(写多个寄存器)
    /// </summary>
    public class ModbusAsciiClient : SerialMode
    {
        /// <summary>
        /// 从站地址（默认1）
        /// </summary>
        public byte SlaveAddress { get; set; } = 1;

        /// <summary>
        /// 构造函数（复用基类串口参数）
        /// </summary>
        public ModbusAsciiClient(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
            : base(portName, baudRate, parity, dataBits, stopBits)
        {
            // Modbus ASCII 通常用无校验（Parity.None），依赖 LRC 校验
            // 超时建议设置为 200ms 以上（ASCII 传输效率较低）
            base.CommTimeout = 200;
        }

        #region 核心方法：ASCII 编码/解码与 LRC 校验
        /// <summary>
        /// 计算 LRC 校验码（纵向冗余校验）
        /// </summary>
        private byte CalculateLrc(byte[] data)
        {
            int lrc = 0;
            foreach (byte b in data)
            {
                lrc += b;
            }
            // 取低8位的补码
            return (byte)(-lrc & 0xFF);
        }

        /// <summary>
        /// 字节数组转 ASCII 字符串（每个字节→2个十六进制字符，大写）
        /// </summary>
        private string BytesToAscii(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:X2}", b); // 格式化为两位十六进制（大写）
            }
            return sb.ToString();
        }

        /// <summary>
        /// ASCII 字符串转字节数组（2个字符→1个字节，忽略非十六进制字符）
        /// </summary>
        /// <returns>值元组(转换是否成功, 转换后的字节数组)</returns>
        private (bool IsOk, byte[] Bytes) AsciiToBytes(string ascii)
        {
            try
            {
                // 过滤非十六进制字符
                StringBuilder filtered = new StringBuilder();
                foreach (char c in ascii)
                {
                    if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'))
                    {
                        filtered.Append(c);
                    }
                }

                // 长度必须为偶数（2个字符对应1个字节）
                if (filtered.Length % 2 != 0)
                    return (IsOk: false, Bytes: null);

                // 转换为字节数组
                byte[] bytes = new byte[filtered.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    string hex = filtered.ToString().Substring(i * 2, 2);
                    bytes[i] = Convert.ToByte(hex, 16);
                }
                return (IsOk: true, Bytes: bytes);
            }
            catch
            {
                return (IsOk: false, Bytes: null);
            }
        }

        /// <summary>
        /// 构建 Modbus ASCII 请求帧（: + 地址+功能码+数据的ASCII + LRC的ASCII + \r\n）
        /// </summary>
        private byte[] BuildRequestFrame(byte functionCode, byte[] data)
        {
            // 1. 拼接核心数据（从站地址 + 功能码 + 数据）
            int coreLength = 2 + data.Length; // 地址(1) + 功能码(1) + 数据(n)
            byte[] coreData = new byte[coreLength];
            coreData[0] = SlaveAddress;
            coreData[1] = functionCode;
            Buffer.BlockCopy(data, 0, coreData, 2, data.Length);

            // 2. 计算 LRC 并追加到核心数据
            byte lrc = CalculateLrc(coreData);
            byte[] coreWithLrc = new byte[coreLength + 1];
            Buffer.BlockCopy(coreData, 0, coreWithLrc, 0, coreLength);
            coreWithLrc[coreLength] = lrc;

            // 3. 转换为 ASCII 并添加帧首尾（:开头，\r\n结尾）
            string asciiFrame = ":" + BytesToAscii(coreWithLrc) + "\r\n";
            return Encoding.ASCII.GetBytes(asciiFrame);
        }

        /// <summary>
        /// 解析 Modbus ASCII 响应帧（校验格式 + LRC + 提取数据）
        /// </summary>
        /// <returns>值元组(解析是否成功, 消息, 提取的有效数据)</returns>
        private (bool IsOk, string Msg, byte[] Data) ParseResponseFrame(byte[] response, byte expectedFunctionCode)
        {
            // 1. 转换为字符串并去除首尾空白
            string asciiStr = Encoding.ASCII.GetString(response).Trim();

            // 2. 校验帧格式（必须以:开头，以\r\n结尾，中间为ASCII十六进制）
            if (!asciiStr.StartsWith(":") || !asciiStr.EndsWith("\r\n"))
                return (IsOk: false, Msg: "响应帧格式错误（缺少:或\r\n）", Data: null);

            // 3. 提取核心ASCII（去除:和\r\n）
            string coreAscii = asciiStr.Substring(1, asciiStr.Length - 3); // 去掉开头:和结尾\r\n（3个字符）

            // 4. 转换为字节数组（使用值元组的命名字段访问）
            var asciiToBytesResult = AsciiToBytes(coreAscii);
            if (!asciiToBytesResult.IsOk)
                return (IsOk: false, Msg: "ASCII转字节失败（格式错误）", Data: null);
            byte[] coreData = asciiToBytesResult.Bytes;

            // 5. 校验长度（至少 3 字节：地址1 + 功能码1 + LRC1）
            if (coreData.Length < 3)
                return (IsOk: false, Msg: "响应数据长度不足", Data: null);

            // 6. 校验 LRC（最后1字节是LRC，前面是数据）
            byte[] dataWithoutLrc = new byte[coreData.Length - 1];
            Buffer.BlockCopy(coreData, 0, dataWithoutLrc, 0, dataWithoutLrc.Length);
            byte receivedLrc = coreData[coreData.Length - 1];
            byte calculatedLrc = CalculateLrc(dataWithoutLrc);
            if (receivedLrc != calculatedLrc)
                return (
                    IsOk: false,
                    Msg: string.Format("LRC校验失败（接收：0x{0:X2}，计算：0x{1:X2}）", receivedLrc, calculatedLrc),
                    Data: null
                );

            // 7. 校验从站地址
            if (dataWithoutLrc[0] != SlaveAddress)
                return (
                    IsOk: false,
                    Msg: string.Format("从站地址不匹配（预期：{0}，实际：{1}）", SlaveAddress, dataWithoutLrc[0]),
                    Data: null
                );

            // 8. 校验功能码（异常时最高位为1）
            byte responseFunctionCode = dataWithoutLrc[1];
            if ((responseFunctionCode & 0x80) != 0)
            {
                byte errorCode = dataWithoutLrc.Length >= 3 ? dataWithoutLrc[2] : (byte)0;
                string errorMsg = GetModbusErrorMsg(errorCode);
                return (
                    IsOk: false,
                    Msg: string.Format("Modbus异常（功能码：{0:X2}，异常码：{1:X2} - {2}）",
                        expectedFunctionCode, errorCode, errorMsg),
                    Data: null
                );
            }
            if (responseFunctionCode != expectedFunctionCode)
                return (
                    IsOk: false,
                    Msg: string.Format("功能码不匹配（预期：{0:X2}，实际：{1:X2}）",
                        expectedFunctionCode, responseFunctionCode),
                    Data: null
                );

            // 9. 提取有效数据（去除地址、功能码）
            byte[] data = new byte[dataWithoutLrc.Length - 2]; // 总长度 - 地址1 - 功能码1
            if (data.Length > 0)
                Buffer.BlockCopy(dataWithoutLrc, 2, data, 0, data.Length);

            return (IsOk: true, Msg: "解析成功", Data: data);
        }

        /// <summary>
        /// Modbus 异常码说明（兼容低版本）
        /// </summary>
        private string GetModbusErrorMsg(byte errorCode)
        {
            switch (errorCode)
            {
                case 0x01: return "非法功能（设备不支持该功能码）";
                case 0x02: return "非法数据地址（寄存器/线圈地址超出范围）";
                case 0x03: return "非法数据值（写入值超出设备允许范围）";
                case 0x04: return "服务器设备故障（写入失败）";
                case 0x05: return "确认（仅用于特殊场景）";
                case 0x06: return "服务器设备忙（请重试）";
                default: return string.Format("未知异常（0x{0:X2}）", errorCode);
            }
        }

        /// <summary>
        /// 字节序转换（大端转小端）
        /// </summary>
        private ushort ReverseBytes(ushort value)
        {
            return (ushort)(((value & 0xFF) << 8) | ((value >> 8) & 0xFF));
        }
        #endregion

        #region Modbus ASCII 读操作实现
        /// <summary>
        /// 03 功能码：读保持寄存器
        /// </summary>
        /// <returns>值元组(是否成功, 消息, 寄存器值数组)</returns>
        public (bool IsOk, string Msg, ushort[] Values) ReadHoldingRegisters(ushort startAddress, ushort count)
        {
            return ReadRegisters(0x03, startAddress, count);
        }

        /// <summary>
        /// 04 功能码：读输入寄存器
        /// </summary>
        /// <returns>值元组(是否成功, 消息, 寄存器值数组)</returns>
        public (bool IsOk, string Msg, ushort[] Values) ReadInputRegisters(ushort startAddress, ushort count)
        {
            return ReadRegisters(0x04, startAddress, count);
        }

        /// <summary>
        /// 读寄存器通用方法
        /// </summary>
        private (bool IsOk, string Msg, ushort[] Values) ReadRegisters(byte functionCode, ushort startAddress, ushort count)
        {
            if (count < 1 || count > 125)
                return (IsOk: false, Msg: "读取数量必须为1-125", Values: null);

            // 构建数据段：起始地址（2字节，大端）+ 数量（2字节，大端）
            byte[] data = new byte[4];
            ushort startAddrBigEndian = ReverseBytes(startAddress);
            Buffer.BlockCopy(BitConverter.GetBytes(startAddrBigEndian), 0, data, 0, 2);
            ushort countBigEndian = ReverseBytes(count);
            Buffer.BlockCopy(BitConverter.GetBytes(countBigEndian), 0, data, 2, 2);

            // 发送请求
            byte[] requestFrame = BuildRequestFrame(functionCode, data);
            var sendResult = base.SendAndReceive(requestFrame);
            if (!sendResult.IsOk)
                return (IsOk: false, Msg: string.Format("发送失败：{0}", sendResult.Msg), Values: null);

            // 解析响应（命名字段访问，替代Item1/Item2/Item3）
            var parseResult = ParseResponseFrame(sendResult.ReceiveByte, functionCode);
            if (!parseResult.IsOk)
                return (IsOk: false, Msg: parseResult.Msg, Values: null);

            // 校验数据长度（1字节计数 + n*2字节寄存器值）
            if (parseResult.Data.Length != 1 + count * 2)
                return (
                    IsOk: false,
                    Msg: string.Format("数据长度不匹配（预期：{0}字节，实际：{1}字节）",
                        1 + count * 2, parseResult.Data.Length),
                    Values: null
                );

            // 转换为寄存器值数组
            ushort[] values = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                byte[] regBytes = new byte[2];
                Buffer.BlockCopy(parseResult.Data, 1 + i * 2, regBytes, 0, 2);
                ushort regValue = BitConverter.ToUInt16(regBytes, 0);
                values[i] = ReverseBytes(regValue);
            }

            return (IsOk: true, Msg: string.Format("成功读取{0}个寄存器", count), Values: values);
        }

        /// <summary>
        /// 01 功能码：读线圈状态
        /// </summary>
        /// <returns>值元组(是否成功, 消息, 线圈状态数组)</returns>
        public (bool IsOk, string Msg, bool[] Values) ReadCoils(ushort startAddress, ushort count)
        {
            return ReadDiscretes(0x01, startAddress, count);
        }

        /// <summary>
        /// 02 功能码：读离散输入
        /// </summary>
        /// <returns>值元组(是否成功, 消息, 离散输入状态数组)</returns>
        public (bool IsOk, string Msg, bool[] Values) ReadDiscreteInputs(ushort startAddress, ushort count)
        {
            return ReadDiscretes(0x02, startAddress, count);
        }

        /// <summary>
        /// 读离散量通用方法
        /// </summary>
        private (bool IsOk, string Msg, bool[] Values) ReadDiscretes(byte functionCode, ushort startAddress, ushort count)
        {
            if (count < 1 || count > 2000)
                return (IsOk: false, Msg: "读取数量必须为1-2000", Values: null);

            // 构建数据段：起始地址（2字节，大端）+ 数量（2字节，大端）
            byte[] data = new byte[4];
            ushort startAddrBigEndian = ReverseBytes(startAddress);
            Buffer.BlockCopy(BitConverter.GetBytes(startAddrBigEndian), 0, data, 0, 2);
            ushort countBigEndian = ReverseBytes(count);
            Buffer.BlockCopy(BitConverter.GetBytes(countBigEndian), 0, data, 2, 2);

            // 发送请求
            byte[] requestFrame = BuildRequestFrame(functionCode, data);
            var sendResult = base.SendAndReceive(requestFrame);
            if (!sendResult.IsOk)
                return (IsOk: false, Msg: string.Format("发送失败：{0}", sendResult.Msg), Values: null);

            // 解析响应（命名字段访问）
            var parseResult = ParseResponseFrame(sendResult.ReceiveByte, functionCode);
            if (!parseResult.IsOk)
                return (IsOk: false, Msg: parseResult.Msg, Values: null);

            // 校验数据长度（1字节计数 + n字节数据）
            int expectedBytes = 1 + (count + 7) / 8;
            if (parseResult.Data.Length != expectedBytes)
                return (
                    IsOk: false,
                    Msg: string.Format("数据长度不匹配（预期：{0}字节，实际：{1}字节）",
                        expectedBytes, parseResult.Data.Length),
                    Values: null
                );

            // 转换为bool数组
            bool[] values = new bool[count];
            byte byteCount = parseResult.Data[0];
            for (int i = 0; i < count; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                values[i] = (parseResult.Data[1 + byteIndex] & (1 << bitIndex)) != 0;
            }

            return (IsOk: true, Msg: string.Format("成功读取{0}个离散量", count), Values: values);
        }
        #endregion

        #region Modbus ASCII 写操作实现
        /// <summary>
        /// 06 功能码：写单个保持寄存器
        /// </summary>
        /// <returns>值元组(是否成功, 消息)</returns>
        public (bool IsOk, string Msg) WriteSingleRegister(ushort address, ushort value)
        {
            // 数据段：地址（2字节，大端）+ 值（2字节，大端）
            byte[] data = new byte[4];
            ushort addrBigEndian = ReverseBytes(address);
            Buffer.BlockCopy(BitConverter.GetBytes(addrBigEndian), 0, data, 0, 2);
            ushort valueBigEndian = ReverseBytes(value);
            Buffer.BlockCopy(BitConverter.GetBytes(valueBigEndian), 0, data, 2, 2);

            // 发送请求
            byte[] requestFrame = BuildRequestFrame(0x06, data);
            var sendResult = base.SendAndReceive(requestFrame);
            if (!sendResult.IsOk)
                return (IsOk: false, Msg: string.Format("发送失败：{0}", sendResult.Msg));

            // 解析响应
            var parseResult = ParseResponseFrame(sendResult.ReceiveByte, 0x06);
            if (!parseResult.IsOk)
                return (IsOk: false, Msg: parseResult.Msg);

            return (IsOk: true, Msg: string.Format("寄存器{0}写入成功（值：{1}）", address, value));
        }

        /// <summary>
        /// 16 功能码：写多个保持寄存器
        /// </summary>
        /// <returns>值元组(是否成功, 消息)</returns>
        public (bool IsOk, string Msg) WriteMultipleRegisters(ushort startAddress, ushort[] values)
        {
            if (values == null || values.Length == 0 || values.Length > 123)
                return (IsOk: false, Msg: "写入数量必须为1-123");

            // 数据段：起始地址（2）+ 数量（2）+ 字节数（1）+ 数据（n*2）
            int dataLength = 5 + values.Length * 2;
            byte[] data = new byte[dataLength];
            ushort startAddrBigEndian = ReverseBytes(startAddress);
            Buffer.BlockCopy(BitConverter.GetBytes(startAddrBigEndian), 0, data, 0, 2);
            ushort countBigEndian = ReverseBytes((ushort)values.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(countBigEndian), 0, data, 2, 2);
            data[4] = (byte)(values.Length * 2);

            // 填充数据（大端）
            for (int i = 0; i < values.Length; i++)
            {
                ushort valueBigEndian = ReverseBytes(values[i]);
                Buffer.BlockCopy(BitConverter.GetBytes(valueBigEndian), 0, data, 5 + i * 2, 2);
            }

            // 发送请求
            byte[] requestFrame = BuildRequestFrame(0x10, data);
            var sendResult = base.SendAndReceive(requestFrame);
            if (!sendResult.IsOk)
                return (IsOk: false, Msg: string.Format("发送失败：{0}", sendResult.Msg));

            // 解析响应
            var parseResult = ParseResponseFrame(sendResult.ReceiveByte, 0x10);
            if (!parseResult.IsOk)
                return (IsOk: false, Msg: parseResult.Msg);

            // 校验写入数量
            if (parseResult.Data.Length < 4)
                return (IsOk: false, Msg: "响应数据不完整");
            ushort responseCount = ReverseBytes(BitConverter.ToUInt16(parseResult.Data, 2));
            if (responseCount != values.Length)
                return (
                    IsOk: false,
                    Msg: string.Format("写入数量不匹配（预期：{0}，实际：{1}）",
                        values.Length, responseCount)
                );

            return (IsOk: true, Msg: string.Format("从地址{0}开始，成功写入{1}个寄存器", startAddress, values.Length));
        }

        /// <summary>
        /// 05 功能码：写单个线圈
        /// </summary>
        /// <returns>值元组(是否成功, 消息)</returns>
        public (bool IsOk, string Msg) WriteSingleCoil(ushort address, bool value)
        {
            // 数据段：地址（2字节，大端）+ 值（2字节，0xFF00=1，0x0000=0）
            byte[] data = new byte[4];
            ushort addrBigEndian = ReverseBytes(address);
            Buffer.BlockCopy(BitConverter.GetBytes(addrBigEndian), 0, data, 0, 2);
            ushort coilValue = value ? (ushort)0xFF00 : (ushort)0x0000;
            ushort coilValueBigEndian = ReverseBytes(coilValue);
            Buffer.BlockCopy(BitConverter.GetBytes(coilValueBigEndian), 0, data, 2, 2);

            // 发送请求
            byte[] requestFrame = BuildRequestFrame(0x05, data);
            var sendResult = base.SendAndReceive(requestFrame);
            if (!sendResult.IsOk)
                return (IsOk: false, Msg: string.Format("发送失败：{0}", sendResult.Msg));

            // 解析响应
            var parseResult = ParseResponseFrame(sendResult.ReceiveByte, 0x05);
            if (!parseResult.IsOk)
                return (IsOk: false, Msg: parseResult.Msg);

            return (IsOk: true, Msg: string.Format("线圈{0}写入成功（状态：{1}）", address, value));
        }

        /// <summary>
        /// 15 功能码：写多个线圈
        /// </summary>
        /// <returns>值元组(是否成功, 消息)</returns>
        public (bool IsOk, string Msg) WriteMultipleCoils(ushort startAddress, bool[] values)
        {
            if (values == null || values.Length == 0 || values.Length > 1968)
                return (IsOk: false, Msg: "写入数量必须为1-1968");

            // 转换bool数组为字节
            int byteCount = (values.Length + 7) / 8;
            byte[] dataBytes = new byte[byteCount];
            for (int i = 0; i < values.Length; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                if (values[i])
                    dataBytes[byteIndex] |= (byte)(1 << bitIndex);
            }

            // 数据段：起始地址（2）+ 数量（2）+ 字节数（1）+ 数据（n字节）
            byte[] data = new byte[5 + byteCount];
            ushort startAddrBigEndian = ReverseBytes(startAddress);
            Buffer.BlockCopy(BitConverter.GetBytes(startAddrBigEndian), 0, data, 0, 2);
            ushort countBigEndian = ReverseBytes((ushort)values.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(countBigEndian), 0, data, 2, 2);
            data[4] = (byte)byteCount;
            Buffer.BlockCopy(dataBytes, 0, data, 5, byteCount);

            // 发送请求
            byte[] requestFrame = BuildRequestFrame(0x0F, data);
            var sendResult = base.SendAndReceive(requestFrame);
            if (!sendResult.IsOk)
                return (IsOk: false, Msg: string.Format("发送失败：{0}", sendResult.Msg));

            // 解析响应
            var parseResult = ParseResponseFrame(sendResult.ReceiveByte, 0x0F);
            if (!parseResult.IsOk)
                return (IsOk: false, Msg: parseResult.Msg);

            // 校验写入数量
            if (parseResult.Data.Length < 4)
                return (IsOk: false, Msg: "响应数据不完整");
            ushort responseCount = ReverseBytes(BitConverter.ToUInt16(parseResult.Data, 2));
            if (responseCount != values.Length)
                return (
                    IsOk: false,
                    Msg: string.Format("写入数量不匹配（预期：{0}，实际：{1}）",
                        values.Length, responseCount)
                );

            return (IsOk: true, Msg: string.Format("从地址{0}开始，成功写入{1}个线圈", startAddress, values.Length));
        }
        #endregion
    }
}