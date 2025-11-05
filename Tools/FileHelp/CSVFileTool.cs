using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.FileHelp
{
    /// <summary>
    /// CSV文件工具类
    /// </summary>
    public  class CSVFileTool
    {
        /// <summary>
        /// CSV文件写入单行
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        /// <param name="Mess">需要写入的信息内容</param>
        /// <param name="Filemode">文件打开模式</param>
        /// <returns>成功时返回ok,失败时返回错误内容</returns>
        public static string WriteCSV(string FilePath, string Mess, FileMode Filemode)
        {
            try
            {
                FileStream fs = new FileStream(FilePath, Filemode);
                StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("gb2312"));
                sw.WriteLine(Mess);
                sw.Close();
                fs.Close();
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// CSV文件写入多行
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        /// <param name="Mess">写入信息内容</param>
        /// <param name="Filemode">文件打开模式</param>
        /// <returns>成功时返回ok,失败时返回错误内容</returns>
        public static string WriteCSV(string FilePath, string[] Mess, FileMode Filemode)
        {
            try
            {
                FileStream fs = new FileStream(FilePath, Filemode);
                StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("gb2312"));
                for (int i = 0; i < Mess.Length; i++)
                {
                    sw.WriteLine(Mess[i]);
                }
                sw.Close();
                fs.Close();
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 读取CSV文件内容
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        /// <returns>返回字符串数组</returns>
        public static string[] ReadCSV(string FilePath)
        {
            try
            {
                StreamReader sr = new StreamReader(FilePath, Encoding.GetEncoding("gb2312"));
                List<string> mAllLine = new List<string>();
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    mAllLine.Add(line);
                }
                sr.Close();
                sr.Dispose();
                return mAllLine.ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
