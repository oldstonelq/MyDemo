using CommunicationMode.Connection_Mode;
using System;

namespace CommunicationMode.Modbus
{
    /// <summary>
    /// ModbusTCP 客户端（兼容 C# 7.0 及以下版本）
    /// 支持功能码：01(读线圈)、02(读离散输入)、03(读保持寄存器)、04(读输入寄存器)
    ///            05(写单个线圈)、06(写单个寄存器)、15(写多个线圈)、16(写多个寄存器)
    /// </summary>
    public class ModbusTcpClient : SocketMode
    {
        /// <summary>
        /// 事务处理标识符（自增，确保每个请求唯一）
        /// </summary>
        private ushort _transactionId = 0;

        /// <summary>
        /// 单元标识符（默认 1，可修改）
        /// </summary>
        public byte UnitId { get; set; } = 1;

        /// <summary>
        /// 构造函数（复用基类 Socket 连接参数）
        /// </summary>
        public ModbusTcpClient(string ip, int port = 502) : base(ip, port)
        {
            // ModbusTCP 默认端口为 502
        }

        #region 核心方法：构建/解析 ModbusTCP 帧
        /// <summary>
        /// 构建 ModbusTCP 请求帧（MBAP头 + PDU）
        /// </summary>
        private byte[] BuildRequestFrame(byte functionCode, byte[] pduData)
        {
            // 1. 构建 MBAP 头（7字节）
            _transactionId++; // 事务ID自增
            byte[] mbapHeader = new byte[7];
            // 事务ID（2字节，大端）
            ushort transIdBigEndian = ReverseBytes(_transactionId);
            Buffer.BlockCopy(BitConverter.GetBytes(transIdBigEndian), 0, mbapHeader, 0, 2);
            // 协议ID（2字节，0=Modbus）
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)0), 0, mbapHeader, 2, 2);
            // 长度（2字节，大端：单元ID(1) + PDU长度）
            ushort length = (ushort)(1 + pduData.Length);
            ushort lengthBigEndian = ReverseBytes(length);
            Buffer.BlockCopy(BitConverter.GetBytes(lengthBigEndian), 0, mbapHeader, 4, 2);
            // 单元ID（1字节）
            mbapHeader[6] = UnitId;

            // 2. 构建 PDU（功能码 + 数据）
            byte[] pdu = new byte[1 + pduData.Length];
            pdu[0] = functionCode;
            Buffer.BlockCopy(pduData, 0, pdu, 1, pduData.Length);

            // 3. 合并 MBAP + PDU（低版本C#需手动拼接，避免LINQ Concat的简化写法）
            byte[] frame = new byte[mbapHeader.Length + pdu.Length];
            Buffer.BlockCopy(mbapHeader, 0, frame, 0, mbapHeader.Length);
            Buffer.BlockCopy(pdu, 0, frame, mbapHeader.Length, pdu.Length);
            return frame;
        }

        /// <summary>
        /// 解析 ModbusTCP 响应帧（提取 PDU 数据，处理异常）
        /// </summary>
        /// <returns>值元组(是否成功, 消息, 提取的PDU数据)</returns>
        private (bool IsOk, string Msg, byte[] Data) ParseResponseFrame(byte[] response, byte expectedFunctionCode)
        {
            // 1. 校验响应长度（至少 8 字节：7字节MBAP + 1字节功能码）
            if (response == null || response.Length < 8)
                return (IsOk: false, Msg: "响应数据长度不足", Data: null);

            // 2. 校验功能码（异常时功能码最高位为1）
            byte responseFunctionCode = response[7]; // MBAP后第1字节是功能码
            if ((responseFunctionCode & 0x80) != 0)
            {
                // 异常帧：功能码 = 原功能码 + 0x80，后跟异常码
                byte errorCode = (response.Length >= 9) ? response[8] : (byte)0;
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

            // 3. 提取 PDU 数据（功能码后的字节，低版本C#需手动复制，避免LINQ Skip）
            byte[] data = new byte[response.Length - 8]; // 跳过7字节MBAP + 1字节功能码
            Buffer.BlockCopy(response, 8, data, 0, data.Length);
            return (IsOk: true, Msg: "解析成功", Data: data);
        }

        /// <summary>
        /// Modbus 异常码说明（用传统switch替代switch表达式）
        /// </summary>
        private string GetModbusErrorMsg(byte errorCode)
        {
            switch (errorCode)
            {
                case 0x01:
                    return "非法功能（设备不支持该功能码）";
                case 0x02:
                    return "非法数据地址（寄存器/线圈地址超出范围）";
                case 0x03:
                    return "非法数据值（写入值超出设备允许范围）";
                case 0x04:
                    return "服务器设备故障（写入失败）";
                case 0x05:
                    return "确认（仅用于特殊场景）";
                case 0x06:
                    return "服务器设备忙（请重试）";
                default:
                    return string.Format("未知异常（{0:X2}）", errorCode);
            }
        }

        /// <summary>
        /// 字节序转换（Modbus使用大端模式，C#默认小端）
        /// </summary>
        private ushort ReverseBytes(ushort value)
        {
            return (ushort)(((value & 0xFF) << 8) | ((value >> 8) & 0xFF));
        }
        #endregion

        #region Modbus 功能实现（读操作）
        /// <summary>
        /// 03 功能码：读保持寄存器
        /// </summary>
        /// <param name="startAddress">起始地址（0x0000 - 0xFFFF）</param>
        /// <param name="count">读取数量（1 - 125）</param>
        /// <returns>值元组(是否成功, 消息, 寄存器值数组)</returns>
        public (bool IsOk, string Msg, ushort[] Values) ReadHoldingRegisters(ushort startAddress, ushort count)
        {
            return ReadRegisters(0x03, startAddress, count);
        }

        /// <summary>
        /// 04 功能码：读输入寄存器
        /// </summary>
        public (bool IsOk, string Msg, ushort[] Values) ReadInputRegisters(ushort startAddress, ushort count)
        {
            return ReadRegisters(0x04, startAddress, count);
        }

        /// <summary>
        /// 读寄存器通用方法（03/04功能码）
        /// </summary>
        private (bool IsOk, string Msg, ushort[] Values) ReadRegisters(byte functionCode, ushort startAddress, ushort count)
        {
            // 参数校验
            if (count < 1 || count > 125)
                return (IsOk: false, Msg: "读取数量必须为1-125", Values: null);

            // 构建 PDU 数据：起始地址（2字节）+ 数量（2字节）
            byte[] pduData = new byte[4];
            ushort startAddrBigEndian = ReverseBytes(startAddress);
            Buffer.BlockCopy(BitConverter.GetBytes(startAddrBigEndian), 0, pduData, 0, 2);
            ushort countBigEndian = ReverseBytes(count);
            Buffer.BlockCopy(BitConverter.GetBytes(countBigEndian), 0, pduData, 2, 2);

            // 构建请求帧并发送
            byte[] requestFrame = BuildRequestFrame(functionCode, pduData);
            var sendResult = SendAndReceive(requestFrame); // 复用基类的SendAndReceive
            if (!sendResult.IsOk)
                return (IsOk: false, Msg: string.Format("发送失败：{0}", sendResult.Msg), Values: null);

            // 解析响应（使用值元组的命名字段访问，替代Item1/Item3）
            var parseResult = ParseResponseFrame(sendResult.ReceiveByte, functionCode);
            if (!parseResult.IsOk)
                return (IsOk: false, Msg: parseResult.Msg, Values: null);

            // 校验数据长度（每个寄存器2字节）
            if (parseResult.Data.Length != count * 2)
                return (
                    IsOk: false,
                    Msg: string.Format("数据长度不匹配（预期：{0}字节，实际：{1}字节）",
                        count * 2, parseResult.Data.Length),
                    Values: null
                );

            // 转换为 ushort 数组（大端转小端）
            ushort[] values = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                byte[] regBytes = new byte[2];
                Buffer.BlockCopy(parseResult.Data, i * 2, regBytes, 0, 2);
                ushort regValue = BitConverter.ToUInt16(regBytes, 0);
                values[i] = ReverseBytes(regValue); // 转回小端
            }

            return (IsOk: true, Msg: string.Format("成功读取{0}个寄存器", count), Values: values);
        }

        /// <summary>
        /// 01 功能码：读线圈状态
        /// </summary>
        /// <param name="startAddress">起始地址（0x0000 - 0xFFFF）</param>
        /// <param name="count">读取数量（1 - 2000）</param>
        /// <returns>值元组(是否成功, 消息, 线圈状态数组)</returns>
        public (bool IsOk, string Msg, bool[] Values) ReadCoils(ushort startAddress, ushort count)
        {
            return ReadDiscretes(0x01, startAddress, count);
        }

        /// <summary>
        /// 02 功能码：读离散输入
        /// </summary>
        public (bool IsOk, string Msg, bool[] Values) ReadDiscreteInputs(ushort startAddress, ushort count)
        {
            return ReadDiscretes(0x02, startAddress, count);
        }

        /// <summary>
        /// 读离散量通用方法（01/02功能码）
        /// </summary>
        private (bool IsOk, string Msg, bool[] Values) ReadDiscretes(byte functionCode, ushort startAddress, ushort count)
        {
            // 参数校验
            if (count < 1 || count > 2000)
                return (IsOk: false, Msg: "读取数量必须为1-2000", Values: null);

            // 构建 PDU 数据：起始地址（2字节）+ 数量（2字节）
            byte[] pduData = new byte[4];
            ushort startAddrBigEndian = ReverseBytes(startAddress);
            Buffer.BlockCopy(BitConverter.GetBytes(startAddrBigEndian), 0, pduData, 0, 2);
            ushort countBigEndian = ReverseBytes(count);
            Buffer.BlockCopy(BitConverter.GetBytes(countBigEndian), 0, pduData, 2, 2);

            // 发送请求
            byte[] requestFrame = BuildRequestFrame(functionCode, pduData);
            var sendResult = SendAndReceive(requestFrame);
            if (!sendResult.IsOk)
                return (IsOk: false, Msg: string.Format("发送失败：{0}", sendResult.Msg), Values: null);

            // 解析响应（命名字段访问，替代Item1/Item3）
            var parseResult = ParseResponseFrame(sendResult.ReceiveByte, functionCode);
            if (!parseResult.IsOk)
                return (IsOk: false, Msg: parseResult.Msg, Values: null);

            // 校验数据长度（字节数 = (count + 7) / 8，向上取整，响应含1字节计数）
            int expectedBytes = (count + 7) / 8;
            if (parseResult.Data.Length != expectedBytes + 1)
                return (
                    IsOk: false,
                    Msg: string.Format("数据长度不匹配（预期：{0}字节，实际：{1}字节）",
                        expectedBytes + 1, parseResult.Data.Length),
                    Values: null
                );

            // 转换为 bool 数组（每个字节的8位对应8个离散量）
            bool[] values = new bool[count];
            byte byteCount = parseResult.Data[0]; // 第一个字节是数据计数
            for (int i = 0; i < count; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                values[i] = (parseResult.Data[byteIndex + 1] & (1 << bitIndex)) != 0;
            }

            return (IsOk: true, Msg: string.Format("成功读取{0}个离散量", count), Values: values);
        }
        #endregion

        #region Modbus 功能实现（写操作）
        /// <summary>
        /// 06 功能码：写单个保持寄存器
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">写入值</param>
        public (bool IsOk, string Msg) WriteSingleRegister(ushort address, ushort value)
        {
            // 构建 PDU 数据：地址（2字节）+ 值（2字节）
            byte[] pduData = new byte[4];
            ushort addrBigEndian = ReverseBytes(address);
            Buffer.BlockCopy(BitConverter.GetBytes(addrBigEndian), 0, pduData, 0, 2);
            ushort valueBigEndian = ReverseBytes(value);
            Buffer.BlockCopy(BitConverter.GetBytes(valueBigEndian), 0, pduData, 2, 2);

            // 发送请求
            byte[] requestFrame = BuildRequestFrame(0x06, pduData);
            var sendResult = SendAndReceive(requestFrame);
            if (!sendResult.IsOk)
                return (IsOk: false, Msg: string.Format("发送失败：{0}", sendResult.Msg));

            // 解析响应（命名字段访问）
            var parseResult = ParseResponseFrame(sendResult.ReceiveByte, 0x06);
            if (!parseResult.IsOk)
                return (IsOk: false, Msg: parseResult.Msg);

            return (IsOk: true, Msg: string.Format("寄存器{0}写入成功（值：{1}）", address, value));
        }

        /// <summary>
        /// 16 功能码：写多个保持寄存器
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="values">写入值数组（1-123个）</param>
        public (bool IsOk, string Msg) WriteMultipleRegisters(ushort startAddress, ushort[] values)
        {
            // 参数校验
            if (values == null || values.Length == 0 || values.Length > 123)
                return (IsOk: false, Msg: "写入数量必须为1-123");

            // 构建 PDU 数据：起始地址（2）+ 数量（2）+ 字节数（1）+ 数据（n*2）
            int dataLength = values.Length * 2;
            byte[] pduData = new byte[5 + dataLength];
            ushort startAddrBigEndian = ReverseBytes(startAddress);
            Buffer.BlockCopy(BitConverter.GetBytes(startAddrBigEndian), 0, pduData, 0, 2);
            ushort countBigEndian = ReverseBytes((ushort)values.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(countBigEndian), 0, pduData, 2, 2);
            pduData[4] = (byte)dataLength; // 字节数 = 数量 * 2

            // 填充数据（大端模式）
            for (int i = 0; i < values.Length; i++)
            {
                ushort valueBigEndian = ReverseBytes(values[i]);
                Buffer.BlockCopy(BitConverter.GetBytes(valueBigEndian), 0, pduData, 5 + i * 2, 2);
            }

            // 发送请求
            byte[] requestFrame = BuildRequestFrame(0x10, pduData);
            var sendResult = SendAndReceive(requestFrame);
            if (!sendResult.IsOk)
                return (IsOk: false, Msg: string.Format("发送失败：{0}", sendResult.Msg));

            // 解析响应（命名字段访问）
            var parseResult = ParseResponseFrame(sendResult.ReceiveByte, 0x10);
            if (!parseResult.IsOk)
                return (IsOk: false, Msg: parseResult.Msg);

            // 校验响应中的数量是否匹配
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
        /// <param name="address">线圈地址</param>
        /// <param name="value">线圈状态（true=0xFF00，false=0x0000）</param>
        public (bool IsOk, string Msg) WriteSingleCoil(ushort address, bool value)
        {
            // 构建 PDU 数据：地址（2字节）+ 值（2字节）
            byte[] pduData = new byte[4];
            ushort addrBigEndian = ReverseBytes(address);
            Buffer.BlockCopy(BitConverter.GetBytes(addrBigEndian), 0, pduData, 0, 2);
            ushort coilValue = value ? (ushort)0xFF00 : (ushort)0x0000;
            ushort coilValueBigEndian = ReverseBytes(coilValue);
            Buffer.BlockCopy(BitConverter.GetBytes(coilValueBigEndian), 0, pduData, 2, 2);

            // 发送请求
            byte[] requestFrame = BuildRequestFrame(0x05, pduData);
            var sendResult = SendAndReceive(requestFrame);
            if (!sendResult.IsOk)
                return (IsOk: false, Msg: string.Format("发送失败：{0}", sendResult.Msg));

            // 解析响应（命名字段访问）
            var parseResult = ParseResponseFrame(sendResult.ReceiveByte, 0x05);
            if (!parseResult.IsOk)
                return (IsOk: false, Msg: parseResult.Msg);

            return (IsOk: true, Msg: string.Format("线圈{0}写入成功（状态：{1}）", address, value));
        }

        /// <summary>
        /// 15 功能码：写多个线圈
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="values">线圈状态数组（1-1968个）</param>
        public (bool IsOk, string Msg) WriteMultipleCoils(ushort startAddress, bool[] values)
        {
            // 参数校验
            if (values == null || values.Length == 0 || values.Length > 1968)
                return (IsOk: false, Msg: "写入数量必须为1-1968");

            // 计算字节数（向上取整）
            int byteCount = (values.Length + 7) / 8;
            byte[] dataBytes = new byte[byteCount];

            // 转换 bool 数组为字节数组（每个bit对应一个线圈）
            for (int i = 0; i < values.Length; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                if (values[i])
                    dataBytes[byteIndex] |= (byte)(1 << bitIndex);
            }

            // 构建 PDU 数据：起始地址（2）+ 数量（2）+ 字节数（1）+ 数据（n字节）
            byte[] pduData = new byte[5 + byteCount];
            ushort startAddrBigEndian = ReverseBytes(startAddress);
            Buffer.BlockCopy(BitConverter.GetBytes(startAddrBigEndian), 0, pduData, 0, 2);
            ushort countBigEndian = ReverseBytes((ushort)values.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(countBigEndian), 0, pduData, 2, 2);
            pduData[4] = (byte)byteCount;
            Buffer.BlockCopy(dataBytes, 0, pduData, 5, byteCount);

            // 发送请求
            byte[] requestFrame = BuildRequestFrame(0x0F, pduData);
            var sendResult = SendAndReceive(requestFrame);
            if (!sendResult.IsOk)
                return (IsOk: false, Msg: string.Format("发送失败：{0}", sendResult.Msg));

            // 解析响应（命名字段访问）
            var parseResult = ParseResponseFrame(sendResult.ReceiveByte, 0x0F);
            if (!parseResult.IsOk)
                return (IsOk: false, Msg: parseResult.Msg);

            // 校验响应中的数量
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