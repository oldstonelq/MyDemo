// ---------------------------------------------------------------------------------
// File: IScanner.cs
// Description: 扫码枪接口，定义了所有扫码枪实现类必须实现的属性和方法
// Author: [作者姓名]
// Create Date: 2023-XX-XX
// Last Modified: 2023-XX-XX
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormLearn.Models.BarcodeScanner
{
    /// <summary>
    /// 扫码枪接口，定义了扫码枪的基本功能和状态
    /// </summary>
    public interface IScanner
    {
        /// <summary>
        /// 获取连接状态
        /// </summary>
        bool Connected { get; }
        /// <summary>
        /// 初始化扫码枪
        /// </summary>
        void Init();    
        /// <summary>
        /// 读取条形码或二维码数据
        /// </summary>
        /// <returns>扫描到的数据字符串</returns>
        string Read();

        /// <summary>
        /// 关闭扫码枪连接
        /// </summary>
        void Close();
    }
}
