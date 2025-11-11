// ---------------------------------------------------------------------------------
// File: AssemblyTool.cs
// Description: 程序集信息工具
// Author: [刘晴]
// Create Date: 2025-11-11
// Last Modified: 2025-11-11
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Tools.OtherHelp
{
    /// <summary>
    /// Assembly工具类
    /// </summary>
    public class AssemblyTool
    {
        /// <summary>
        /// 获取当前程序的所有程序集
        /// </summary>
        /// <returns>返回当前程序的所有程序集</returns>
        public static Assembly[] GetAppAllAssembly()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }
        #region 1. 获取指定程序集的信息（核心方法）
        /// <summary>
        /// 获取程序集的指定特性值
        /// </summary>
        /// <typeparam name="T">特性类型（如 AssemblyTitleAttribute）</typeparam>
        /// <param name="assembly">目标程序集</param>
        /// <param name="defaultValue">默认值（特性不存在时返回）</param>
        /// <param name="valueSelector">特性值的选择器（从特性中提取需要的值）</param>
        /// <returns>特性值</returns>
        private static string GetAssemblyAttribute<T>(Assembly assembly, string defaultValue, Func<T, string> valueSelector)
            where T : Attribute
        {
            // 从程序集中获取指定特性（第一个匹配项）
            var attribute = assembly.GetCustomAttribute<T>();
            return attribute != null ? valueSelector(attribute) : defaultValue;
        }
        #endregion

        #region 2. 快捷方法（当前程序集）
        /// <summary>
        /// 获取当前程序集的标题（AssemblyTitle）
        /// </summary>
        /// <param name="defaultValue">默认返回</param>
        /// <returns>返回当前程序集的标题或者设定的默认值</returns>
        public static string GetCurrentTitle(string defaultValue = "未知标题")
        {
            var assembly = Assembly.GetExecutingAssembly();
            return GetAssemblyAttribute<AssemblyTitleAttribute>(assembly, defaultValue, attr => attr.Title);
        }

        /// <summary>
        /// 获取当前程序集的版本（AssemblyVersion）
        /// </summary>
        /// <param name="defaultValue">默认返回</param>
        /// <returns> 返回当前程序集的版本或者设定的默认值</returns>
        public static string GetCurrentVersion(string defaultValue = "1.0.0.0")
        {
            var assembly = Assembly.GetExecutingAssembly();
            return GetAssemblyAttribute<AssemblyVersionAttribute>(assembly, defaultValue, attr => attr.Version.ToString());
        }

        /// <summary>
        /// 获取当前程序集的文件版本（AssemblyFileVersion）
        /// </summary>
        /// <param name="defaultValue">默认返回</param>
        /// <returns> 返回当前程序集的文件版本或者设定的默认值</returns>
        public static string GetCurrentFileVersion(string defaultValue = "1.0.0.0")
        {
            var assembly = Assembly.GetExecutingAssembly();
            return GetAssemblyAttribute<AssemblyFileVersionAttribute>(assembly, defaultValue, attr => attr.Version);
        }

        /// <summary>
        /// 获取当前程序集的描述（AssemblyDescription）
        /// </summary>
        /// <param name="defaultValue">默认返回</param>
        /// <returns> 返回当前程序集的描述或者设定的默认值</returns>
        public static string GetCurrentDescription(string defaultValue = "无描述")
        {
            var assembly = Assembly.GetExecutingAssembly();
            return GetAssemblyAttribute<AssemblyDescriptionAttribute>(assembly, defaultValue, attr => attr.Description);
        }

        /// <summary>
        /// 获取当前程序集的公司（AssemblyCompany）
        /// </summary>
        /// <param name="defaultValue">默认返回</param>
        /// <returns> 返回当前程序集的公司或者设定的默认值</returns>
        public static string GetCurrentCompany(string defaultValue = "未知公司")
        {
            var assembly = Assembly.GetExecutingAssembly();
            return GetAssemblyAttribute<AssemblyCompanyAttribute>(assembly, defaultValue, attr => attr.Company);
        }

        /// <summary>
        /// 获取当前程序集的版权信息（AssemblyCopyright）
        /// </summary>
        /// <param name="defaultValue">默认返回</param>
        /// <returns> 返回当前程序集的版权信息或者设定的默认值</returns>
        public static string GetCurrentCopyright(string defaultValue = "无版权信息")
        {
            var assembly = Assembly.GetExecutingAssembly();
            return GetAssemblyAttribute<AssemblyCopyrightAttribute>(assembly, defaultValue, attr => attr.Copyright);
        }
        #endregion

        #region 3. 快捷方法（指定程序集）
        /// <summary>
        /// 获取指定程序集的标题
        /// </summary>
        /// <param name="assembly">目标程序集（如 Assembly.LoadFrom("xxx.dll")）</param>
        /// <param name="defaultValue">默认返回</param>
        /// <returns> 返回指定程序集的标题或者设定的默认值</returns> 
        public static string GetTitle(Assembly assembly, string defaultValue = "未知标题")
        {
            return GetAssemblyAttribute<AssemblyTitleAttribute>(assembly, defaultValue, attr => attr.Title);
        }

        /// <summary>
        /// 获取指定程序集的版本
        /// </summary>
        /// <param name="assembly">目标程序集（如 Assembly.LoadFrom("xxx.dll")）</param>
        /// <param name="defaultValue">默认返回</param>
        /// <returns> 返回指定程序集的版本或者设定的默认值</returns> 
        public static string GetVersion(Assembly assembly, string defaultValue = "1.0.0.0")
        {
            return GetAssemblyAttribute<AssemblyVersionAttribute>(assembly, defaultValue, attr => attr.Version.ToString());
        }
        #endregion

        #region 4. 其他常用场景
        /// <summary>
        /// 获取调用者程序集的信息（适用于类库）
        /// </summary>
        /// <param name="defaultValue">默认返回</param>
        /// <returns> 返回调用者程序集的信息或者设定的默认值</returns> 
        public static string GetCallingAssemblyTitle(string defaultValue = "未知标题")
        {
            var assembly = Assembly.GetCallingAssembly(); // 调用当前方法的程序集
            return GetAssemblyAttribute<AssemblyTitleAttribute>(assembly, defaultValue, attr => attr.Title);
        }

        /// <summary>
        /// 获取入口程序集的信息（如控制台/WinForm 应用的主程序集）
        /// </summary>
        /// <param name="defaultValue">默认返回</param>
        /// <returns> 返回入口程序集的信息或者设定的默认值</returns> 
        public static string GetEntryAssemblyTitle(string defaultValue = "未知标题")
        {
            var assembly = Assembly.GetEntryAssembly(); // 应用程序的入口程序集（可能为null，需判空）
            return assembly != null
                ? GetAssemblyAttribute<AssemblyTitleAttribute>(assembly, defaultValue, attr => attr.Title)
                : defaultValue;
        }
        #endregion

    }
}

