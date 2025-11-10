// ---------------------------------------------------------------------------------
// File: TextLoggerTool.cs
// Description: 文本日志工具类
// Author: [刘晴]
// Create Date: 2025-11-10
// Last Modified: 2025-11-10
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tools.FileHelp;

namespace Tools.LogHelper
{
    /// <summary>
    /// 文本日志工具类
    /// </summary>
    public  class TextLoggerTool
    {
        /// <summary>
        /// 指定文件夹下写入日志文件（年份月—年份月日期）
        /// </summary>
        /// <param name="Info">写入信息</param>
        /// <param name="DirectoryPath">文件夹路径</param>
        /// <returns>捕获异常，返回失败，反之返回成功</returns>
        public static (bool IsOk,string Msg) WriteLog(string Info,string DirectoryPath)
        {
            try
            {
                string filePath = DirectoryPath + "\\" + DateTime.Now.ToString("yyyyMM");
                DirectoryTool.DirectoryExist(filePath, true);
                FileStream fileStream;
                fileStream = new FileStream(filePath + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log", FileMode.Append);
                StreamWriter streamWriter = new StreamWriter(fileStream);
                streamWriter.WriteLine(Info);
                streamWriter.Close();
                fileStream.Close();
                return (true ,"");
            }
            catch (Exception ex)
            {
                return (false , ex .Message);
            }
        }
    }
}
