// ---------------------------------------------------------------------------------
// File: FileGeneralTool.cs
// Description: 通用文件操作类
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.FileHelp
{
    /// <summary>
    /// 文件通用工具类
    /// </summary>
    public  class FileGeneralTool
    {
        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        /// <returns>true or false</returns>
        public static bool FileExist(string FilePath)
        {
            if (File.Exists(FilePath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        /// <returns>成功时返回ok,失败时返回错误内容</returns>
        public static string DeleteFile(string FilePath)
        {
            try
            {
                File.Delete(FilePath);
                return "ok";
            }
            catch (Exception ex)
            {
                return "删除文件时出错:" + ex.Message;
            }
        }
        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="Sourcefile">源文件路径</param>
        /// <param name="Destfile">目标文件路径</param>
        /// <returns>true or false</returns>
        public static bool CopyFile(string Sourcefile, string Destfile)
        {
            try
            {
                File.Copy(Sourcefile, Destfile);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 返回指定目录下所有指定类型文件名
        /// </summary>
        /// <param name="Path">源文件路径</param>
        /// <param name="Pattern">指定文件类型,eg: *.csv</param>
        /// <returns></returns>
        public static string[] GetFiles(string Path, string Pattern)
        {
            try
            {
                string[] mValue = Directory.GetFiles(Path, Pattern);
                return mValue;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
