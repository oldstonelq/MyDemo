using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Tools.FileHelp
{
    /// <summary>
    /// ini文件工具类
    /// </summary>
    public  class InIFileTool
    {
        #region ini文件操作api函数
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filepath);
        private static StringBuilder mystrb = new StringBuilder(1024);                                 //读取ini文件内容保存
        #endregion

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="filepath"></param>
        public static void WriteIniFile(string section, string key, string val, string filepath)
        {
            WritePrivateProfileString(section, key, val, filepath);
        }
        /// <summary>
        /// 读文件
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string ReadIniFile(string section, string key, string filepath)
        {
            GetPrivateProfileString(section, key, "", mystrb, 1024, filepath);
            return mystrb.ToString();
        }
    }
}
