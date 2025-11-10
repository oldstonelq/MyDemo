// ---------------------------------------------------------------------------------
// File: StringCompressTool.cs
// Description: 字符串解压缩工具
// Author: [刘晴]
// Create Date: 2025-11-10
// Last Modified: 2025-11-10
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.OtherHelp
{
    /// <summary>
    /// 字符串解压缩工具类
    /// </summary>
    public  class StringCompressTool
    {
        /// <summary>
        /// 压缩字符串
        /// </summary>
        /// <param name="value">需要压缩的长字符串</param>
        /// <returns>返回压缩之后的字符串</returns>
        public static string Compress(string value)
        {
            try
            {
                string data = "";
                byte[] byteArray = Encoding.Default.GetBytes(value);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (GZipStream sw = new GZipStream(ms, CompressionMode.Compress))
                    {
                        sw.Write(byteArray, 0, byteArray.Length);
                    }
                    data = Convert.ToBase64String(ms.ToArray());
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 解压字符串
        /// </summary>
        /// <param name="value">需要解压的字符串</param>
        /// <returns>返回解压之后的字符串</returns>
        public static string Decompress(string value)
        {
            try
            {
                string data = "";
                byte[] bytes = Convert.FromBase64String(value);
                using (MemoryStream msReader = new MemoryStream())
                {
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                        {
                            byte[] buffer = new byte[1024];
                            int readLen = 0;
                            while ((readLen = zip.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                msReader.Write(buffer, 0, readLen);
                            }

                        }
                    }
                    data = Encoding.Default.GetString(msReader.ToArray());
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
