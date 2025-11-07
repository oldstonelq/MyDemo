// ---------------------------------------------------------------------------------
// File: IScanner.cs
// Description: 扫码枪接口，定义了所有扫码枪实现类必须实现的属性和方法
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeScan
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
