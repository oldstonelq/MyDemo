// ---------------------------------------------------------------------------------
// File: ImageConverTool.cs
// Description: 图片格式转换帮助
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Tools.ImageHelper
{
    /// <summary>
    /// 图片格式转换辅助类
    /// 提供将位图转换为多种常见图片格式的功能，支持JPEG、PNG、GIF、BMP、ICO、TIFF和WebP格式
    /// </summary>
    public class ImageConverTool
    {
        /// <summary>
        /// 需要转换的位图对象
        /// </summary>
        private Bitmap Bmp = null;

        /// <summary>
        /// 将位图转换为指定格式并保存到指定目录
        /// </summary>
        /// <param name="dir">保存路径</param>
        /// <param name="imageType">需要转换的图片格式</param>
        /// <returns>转换是否成功</returns>
        public bool ConvertImage(string dir, ImageType imageType)
        {
            // 确保目录存在
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string extension = GetImageExtension(imageType);
            string filePath = Path.Combine(dir, $"{Bmp.Tag}.{extension}");

            try
            {
                switch (imageType)
                {
                    case ImageType.Jpeg:
                        // 保存JPEG时设置质量
                        using (var encoderParameters = new EncoderParameters(1))
                        {
                            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
                            var codecInfo = GetEncoderInfo("image/jpeg");
                            Bmp.Save(filePath, codecInfo, encoderParameters);
                        }
                        break;
                    case ImageType.Png:
                        Bmp.Save(filePath, ImageFormat.Png);
                        break;
                    case ImageType.Gif:
                        Bmp.Save(filePath, ImageFormat.Gif);
                        break;
                    case ImageType.Bmp:
                        Bmp.Save(filePath, ImageFormat.Bmp);
                        break;
                    case ImageType.Ico:
                        using (Stream stream = File.Create(filePath))
                        {
                            using (Icon icon = Icon.FromHandle(Bmp.GetHicon()))
                            {
                                icon.Save(stream);
                            }
                        }
                        break;
                    case ImageType.Tiff:
                        // 保存为TIFF格式
                        using (var encoderParameters = new EncoderParameters(1))
                        {
                            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionLZW);
                            var codecInfo = GetEncoderInfo("image/tiff");
                            Bmp.Save(filePath, codecInfo, encoderParameters);
                        }
                        break;
                    case ImageType.WebP:
                        // WebP格式需要使用特定方法
                        SaveAsWebP(Bmp, filePath, 90);
                        break;
                    default:
                        throw new NotSupportedException($"不支持{imageType}格式的转换");
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取图片格式对应的文件扩展名
        /// </summary>
        /// <param name="imageType">图片格式枚举</param>
        /// <returns>文件扩展名（小写，不包含点号）</returns>
        private string GetImageExtension(ImageType imageType)
        {
            switch (imageType)
            {
                case ImageType.Jpeg:
                    return "jpg";
                case ImageType.WebP:
                    return "webp";
                default:
                    return imageType.ToString().ToLower();
            }
        }

        /// <summary>
        /// 获取指定MIME类型的图像编码器信息
        /// </summary>
        /// <param name="mimeType">图像MIME类型</param>
        /// <returns>对应MIME类型的图像编码器信息，如果找不到则返回null</returns>
        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == mimeType)
                {
                    return codec;
                }
            }
            return null;
        }

        /// <summary>
        /// 将图像保存为WebP格式
        /// </summary>
        /// <param name="image">图像对象</param>
        /// <param name="filePath">保存路径</param>
        /// <param name="quality">质量参数(0-100)</param>
        /// <exception cref="NotSupportedException">当系统不支持WebP格式编码时抛出</exception>
        private void SaveAsWebP(Image image, string filePath, int quality)
        {
            // WebP需要使用System.Drawing.Common 5.0+或第三方库
            // 这里使用System.Drawing的内置方法（需要适当的系统支持）
            using (var stream = File.Create(filePath))
            {
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                var codecInfo = GetEncoderInfo("image/webp");

                if (codecInfo == null)
                {
                    throw new NotSupportedException("系统不支持WebP格式编码");
                }

                image.Save(stream, codecInfo, encoderParameters);
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bmp">需要进行格式转换的位图对象</param>
        public ImageConverTool(Bitmap bmp)
        {
            Bmp = bmp;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="FilePath">需要进行格式转换的图片路径</param>
        public ImageConverTool(string FilePath)
        {
            Bmp = new Bitmap(FilePath);
        }
    }

    /// <summary>
    /// 图片文件类型枚举
    /// 包含常见的图像格式，可用于文件类型判断、转换等场景
    /// </summary>
    public enum ImageType
    {
        /// <summary>
        /// 联合图像专家组格式（JPEG）
        /// 支持有损压缩，广泛用于照片
        /// </summary>
        [Description("JPEG 图像 (*.jpg)")]
        Jpeg,

        /// <summary>
        /// 便携式网络图形格式
        /// 支持透明通道和无损压缩
        /// </summary>
        [Description("PNG 图像 (*.png)")]
        Png,

        /// <summary>
        /// 图形交换格式
        /// 支持动画和透明，颜色限制为256色
        /// </summary>
        [Description("GIF 图像 (*.gif)")]
        Gif,

        /// <summary>
        /// 标签图像文件格式
        /// 支持无损压缩，常用于印刷和高质量图像
        /// </summary>
        [Description("TIFF 图像 (*.tiff)")]
        Tiff,

        /// <summary>
        /// 位图格式
        /// 未压缩的图像格式，文件体积较大
        /// </summary>
        [Description("位图 (*.bmp)")]
        Bmp,

        /// <summary>
        /// Windows图标格式
        /// 通常用于应用程序图标
        /// </summary>
        [Description("图标 (*.ico)")]
        Ico,

        /// <summary>
        /// WebP格式
        /// 现代图像格式，提供更好的压缩率
        /// </summary>
        [Description("WebP 图像 (*.webp)")]
        WebP,
    }
}
