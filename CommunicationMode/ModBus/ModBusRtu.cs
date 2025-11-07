// ---------------------------------------------------------------------------------
// File: ModBusRtu.cs
// Description: ModBusRtu协议实现
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using CommunicationMode.Base;
using System;
using System.IO.Ports;

namespace CommunicationMode.Modbus
{
    /// <summary>
    /// Modbus RTU 客户端（基于 SerialMode 扩展，兼容 C# 7.0+）
    /// 支持功能码：01(读线圈)、02(读离散输入)、03(读保持寄存器)、04(读输入寄存器)
    ///            05(写单个线圈)、06(写单个寄存器)、15(写多个线圈)、16(写多个寄存器)
    /// </summary>
    public class ModbusRtuClient : SerialMode
    {
        /// <summary>
        /// 从站地址（默认1）
        /// </summary>
        public byte SlaveAddress { get; set; } = 1;

        /// <summary>
        /// 构造函数（复用基类串口参数）
        /// </summary>
        public ModbusRtuClient(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
            : base(portName, baudRate, parity, dataBits, stopBits)
        {
            // Modbus RTU 推荐校验位：奇校验(Even)或偶校验(Odd)，无校验时需硬件保证可靠性
            // 串口超时建议设置为：至少 3.5 个字符时间（如 9600波特率下约 3.5ms*11位=38.5ms，建议设为 100ms）
            base.CommTimeout = 100;
        }

        #region 核心方法：CRC16 校验（Modbus RTU 必须）
        /// <summary>
        /// 计算 Modbus RTU CRC16 校验码（小端模式，低字节在前）
        /// </summary>
        private ushort CalculateCrc16(byte[] data)
        {
            ushort crc = 0xFFFF; // 初始值
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= (ushort)data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001; // 多项式
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc; // 低字节在前，高字节在后
        }

        /// <summary>
        /// 构建 Modbus RTU 请求帧（从站地址 + 功能码 + 数据 + CRC）
        /// </summary>
        private byte[] BuildRequestFrame(byte functionCode, byte[] data)
        {
            // 1. 拼接核心数据（从站地址 + 功能码 + 数据）
            int coreLength = 2 + data.Length; // 从站地址(1) + 功能码(1) + 数据(n)
            byte[] coreData = new byte[coreLength];
            coreData[0] = SlaveAddress;
            coreData[1] = functionCode;
            Buffer.BlockCopy(data, 0, coreData, 2, data.Length);

            // 2. 计算 CRC16 并追加到帧尾（低字节在前）
            ushort crc = CalculateCrc16(coreData);
            byte[] frame = new byte[coreLength + 2];
            Buffer.BlockCopy(coreData, 0, frame, 0, coreLength);
            frame[coreLength] = (byte)(crc & 0xFF); // 低字节
            frame[coreLength + 1] = (byte)(crc >> 8); // 高字节

            return frame;
        }

        /// <summary>
        /// 解析 Modbus RTU 响应帧（校验 CRC + 提取数据 + 处理异常）
        /// </summary>
        /// <returns>值元组(是否成功, 消息, 提取的有效数据)</returns>
        private (bool IsOk, string Msg, byte[] Data) ParseResponseFrame(byte[] response, byte expectedFunctionCode)
        {
            // 1. 基础校验（至少 5 字节：从站地址1 + 功能码1 + 数据1 + CRC2）
            if (response == null || response.Length < 5)
                return (IsOk: false, Msg: "响应帧长度不足", Data: null);

            // 2. 校验从站地址
            if (response[0] != SlaveAddress)
                return (
                    IsOk: false,
                    Msg: string.Format("从站地址不匹配（预期：{0}，实际：{1}）", SlaveAddress, response[0]),
                    Data: null
                );

            // 3. 校验 CRC16
            byte[] coreData = new byte[response.Length - 2];
            Buffer.BlockCopy(response, 0, coreData, 0, coreData.Length);
            ushort receivedCrc = BitConverter.ToUInt16(response, response.Length - 2);
            ushort calculatedCrc = CalculateCrc16(coreData);
            if (receivedCrc != calculatedCrc)
                return (
                    IsOk: false,
                    Msg: string.Format("CRC校验失败（接收：0x{0:X4}，计算：0x{1:X4}）", receivedCrc, calculatedCrc),
                    Data: null
                );

            // 4. 校验功能码（异常时最高位为1）
            byte responseFunctionCode = response[1];
            if ((responseFunctionCode & 0x80) != 0)
            {
                // 异常帧：功能码 = 原功能码 + 0x80，后跟1字节异常码
                byte errorCode = response.Length >= 3 ? response[2] : (byte)0;
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

            // 5. 提取有效数据（去除从站地址、功能码、CRC）
            byte[] data = new byte[response.Length - 3]; // 总长度 - 地址1 - 功能码1 - CRC2
            if (data.Length > 0)
                Buffer.BlockCopy(response, 2, data, 0, data.Length);

            return (IsOk: true, Msg: "解析成功", Data: data);
        }

        /// <summary>
        /// Modbus 异常码说明（兼容低版本的 switch-case）
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
        /// 字节序转换（大端转小端，Modbus 寄存器默认大端）
        /// </summary>
        private ushort ReverseBytes(ushort value)
        {
            return (ushort)(((value & 0xFF) << 8) | ((value >> 8) & 0xFF));
        }
        #endregion

        #region Modbus RTU 读操作实现
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
        /// <returns>值元组(是否成功, 消息, 寄存器值数组)</returns>
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

            // 构建数据段：起始地址（2字节，大端）+ 数量（2字节，大端）
            byte[] data = new byte[4];
            ushort startAddrBigEndian = ReverseBytes(startAddress);
            Buffer.BlockCopy(BitConverter.GetBytes(startAddrBigEndian), 0, data, 0, 2);
            ushort countBigEndian = ReverseBytes(count);
            Buffer.BlockCopy(BitConverter.GetBytes(countBigEndian), 0, data, 2, 2);

            // 构建请求帧并发送（通过基类串口发送）
            byte[] requestFrame = BuildRequestFrame(functionCode, data);
            var sendResult = base.SendAndReceive(requestFrame);
            if (!sendResult.IsOk)
                return (IsOk: false, Msg: string.Format("发送失败：{0}", sendResult.Msg), Values: null);

            // 解析响应帧（使用命名字段访问，替代Item1/Item3）
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

            // 转换为寄存器值数组（大端转小端）
            ushort[] values = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                byte[] regBytes = new byte[2];
                Buffer.BlockCopy(parseResult.Data, 1 + i * 2, regBytes, 0, 2); // 跳过1字节计数
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
        /// <returns>值元组(是否成功, 消息, 离散输入状态数组)</returns>
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

            // 解析响应（命名字段访问，替代Item1/Item3）
            var parseResult = ParseResponseFrame(sendResult.ReceiveByte, functionCode);
            if (!parseResult.IsOk)
                return (IsOk: false, Msg: parseResult.Msg, Values: null);

            // 校验数据长度（1字节计数 + n字节数据，n = (count +7)/8）
            int expectedBytes = 1 + (count + 7) / 8;
            if (parseResult.Data.Length != expectedBytes)
                return (
                    IsOk: false,
                    Msg: string.Format("数据长度不匹配（预期：{0}字节，实际：{1}字节）",
                        expectedBytes, parseResult.Data.Length),
                    Values: null
                );

            // 转换为bool数组（每个bit对应一个线圈/离散输入）
            bool[] values = new bool[count];
            byte byteCount = parseResult.Data[0]; // 第一个字节是数据计数
            for (int i = 0; i < count; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                values[i] = (parseResult.Data[1 + byteIndex] & (1 << bitIndex)) != 0; // 跳过计数字节
            }

            return (IsOk: true, Msg: string.Format("成功读取{0}个离散量", count), Values: values);
        }
        #endregion

        #region Modbus RTU 写操作实现
        /// <summary>
        /// 06 功能码：写单个保持寄存器
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">写入值</param>
        /// <returns>值元组(是否成功, 消息)</returns>
        public (bool IsOk, string Msg) WriteSingleRegister(ushort address, ushort value)
        {
            // 构建数据段：地址（2字节，大端）+ 值（2字节，大端）
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

            // 解析响应（RTU响应帧与请求帧数据段一致）
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
        /// <returns>值元组(是否成功, 消息)</returns>
        public (bool IsOk, string Msg) WriteMultipleRegisters(ushort startAddress, ushort[] values)
        {
            // 参数校验
            if (values == null || values.Length == 0 || values.Length > 123)
                return (IsOk: false, Msg: "写入数量必须为1-123");

            // 构建数据段：起始地址（2）+ 数量（2）+ 字节数（1）+ 数据（n*2）
            int dataLength = 5 + values.Length * 2;
            byte[] data = new byte[dataLength];
            ushort startAddrBigEndian = ReverseBytes(startAddress);
            Buffer.BlockCopy(BitConverter.GetBytes(startAddrBigEndian), 0, data, 0, 2);
            ushort countBigEndian = ReverseBytes((ushort)values.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(countBigEndian), 0, data, 2, 2);
            data[4] = (byte)(values.Length * 2); // 字节数 = 数量 * 2

            // 填充寄存器值（大端）
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

            // 解析响应（响应含起始地址和写入数量）
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
        /// <param name="address">线圈地址</param>
        /// <param name="value">线圈状态（true=0xFF00，false=0x0000）</param>
        /// <returns>值元组(是否成功, 消息)</returns>
        public (bool IsOk, string Msg) WriteSingleCoil(ushort address, bool value)
        {
            // 构建数据段：地址（2字节，大端）+ 值（2字节，0xFF00=1，0x0000=0，大端）
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
        /// <param name="startAddress">起始地址</param>
        /// <param name="values">线圈状态数组（1-1968个）</param>
        /// <returns>值元组(是否成功, 消息)</returns>
        public (bool IsOk, string Msg) WriteMultipleCoils(ushort startAddress, bool[] values)
        {
            // 参数校验
            if (values == null || values.Length == 0 || values.Length > 1968)
                return (IsOk: false, Msg: "写入数量必须为1-1968");

            // 计算数据字节数（向上取整）
            int byteCount = (values.Length + 7) / 8;
            byte[] dataBytes = new byte[byteCount];

            // 转换bool数组为字节（每个bit对应一个线圈）
            for (int i = 0; i < values.Length; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                if (values[i])
                    dataBytes[byteIndex] |= (byte)(1 << bitIndex);
            }

            // 构建数据段：起始地址（2）+ 数量（2）+ 字节数（1）+ 数据（n字节）
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