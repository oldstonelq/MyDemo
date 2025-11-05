using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.FileHelp
{
    /// <summary>
    /// 目录工具类
    /// </summary>
    public  class DirectoryTool
    {
        /// <summary>
        /// 判断目录是否存在
        /// </summary>
        /// <param name="DirPath">目录路径</param>
        /// <param name="Create">当目录不存在时是否创建</param>
        /// <returns>true or false</returns>
        public static bool DirectoryExist(string DirPath, bool Create)
        {
            if (Directory.Exists(DirPath))
            {
                return true;
            }
            else
            {
                if (Create == true)
                {
                    try
                    {
                        Directory.CreateDirectory(DirPath);
                    }
                    catch (Exception)
                    {
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// 返回上一级路径
        /// </summary>
        /// <param name="Path">当前路径</param>
        /// <returns>上一级路径全名</returns>
        public static string GetParentDirectory(string Path)
        {
            return Directory.GetParent(Path).FullName;
        }
    }
}
