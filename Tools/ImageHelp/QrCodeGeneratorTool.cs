// ---------------------------------------------------------------------------------
// File: QrCodeGeneratorTool.cs
// Description: 二维码生成工具
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.ImageHelper
{
    /// <summary>
    /// 二维码生成器封装类
    /// 提供二维码生成、保存到文件以及转换为Base64字符串的功能
    /// 基于第三方QRCoder库实现
    /// </summary>
    public class QrCodeGeneratorTool
    {
        /// <summary>
        /// 生成二维码并返回Image对象
        /// </summary>
        /// <param name="textToEncode">要编码的文本内容</param>
        /// <param name="pixelSize">像素大小，默认为10</param>
        /// <param name="eccLevel">纠错级别，默认为中等级别(Q)</param>
        /// <returns>生成的二维码图像对象</returns>
        /// <exception cref="ArgumentException">当文本内容为空时抛出</exception>
        /// <exception cref="ArgumentOutOfRangeException">当像素大小小于1时抛出</exception>
        public Image GenerateQrCode(string textToEncode, int pixelSize = 10, QRCodeGenerator.ECCLevel eccLevel = QRCodeGenerator.ECCLevel.Q)
        {
            if (string.IsNullOrEmpty(textToEncode))
                throw new ArgumentException("文本内容不能为空", nameof(textToEncode));

            if (pixelSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pixelSize), "像素大小必须大于0");

            using (var generator = new QRCodeGenerator())
            using (var qrCodeData = generator.CreateQrCode(textToEncode, eccLevel))
            using (var qrCode = new QRCode(qrCodeData))
            {
                return qrCode.GetGraphic(pixelSize);
            }
        }

        /// <summary>
        /// 生成二维码并保存为文件
        /// </summary>
        /// <param name="textToEncode">要编码的文本内容</param>
        /// <param name="filePath">保存文件路径（包含文件名和扩展名）</param>
        /// <param name="pixelSize">像素大小，默认为10</param>
        /// <remarks>文件格式由文件扩展名决定，默认保存为PNG格式</remarks>
        public void SaveQrCodeToFile(string textToEncode, string filePath, int pixelSize = 10)
        {
            using (Image qrImage = GenerateQrCode(textToEncode, pixelSize))
            {
                qrImage.Save(filePath);
            }
        }

        /// <summary>
        /// 生成二维码并转换为Base64字符串
        /// </summary>
        /// <param name="textToEncode">要编码的文本内容</param>
        /// <param name="pixelSize">像素大小，默认为10</param>
        /// <returns>Base64编码的二维码图像字符串</returns>
        /// <remarks>返回的Base64字符串表示的是PNG格式的二维码图像</remarks>
        public string ConvertQrCodeToBase64(string textToEncode, int pixelSize = 10)
        {
            using (Image qrImage = GenerateQrCode(textToEncode, pixelSize))
            using (MemoryStream ms = new MemoryStream())
            {
                qrImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] imageBytes = ms.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }
    }
}
