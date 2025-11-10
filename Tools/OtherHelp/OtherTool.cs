// ---------------------------------------------------------------------------------
// File: OtherTool.cs
// Description: 其他工具类
// Author: [刘晴]
// Create Date: 2025-11-10
// Last Modified: 2025-11-10
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tools.OtherHelp
{
    /// <summary>
    /// 其他工具类
    /// </summary>
    public  class OtherTool
    {
        /// <summary>
        /// 拷贝s类里面属性的值给d类里面同名属性(属性的类型以及名称必须相同)
        /// </summary>
        /// <typeparam name="D">拷贝的类型</typeparam>
        /// <typeparam name="S"> 被拷贝的类型</typeparam>
        /// <param name="s">被拷贝实例</param>
        /// <returns>返回拷贝类型实例</returns>
        public static D Mapper<D, S>(S s)
        {
            D d = Activator.CreateInstance<D>();
            try
            {
                var Types = s.GetType();//获得传入类型
                var Typed = typeof(D);
                foreach (PropertyInfo sp in Types.GetProperties())//获得传入类型的属性字段
                {
                    foreach (PropertyInfo dp in Typed.GetProperties())
                    {
                        if (dp.Name == sp.Name)//判断属性名是否相同
                        {
                            dp.SetValue(d, sp.GetValue(s, null), null);//获得s对象属性的值复制给d对象的属性
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return d;
        }
        /// <summary>
        /// 检测程序是否已经在运行
        /// </summary>
        /// <returns>True： 在运行 false: 不在运行</returns>
        public static bool SoftwareIsRuning()
        {
            bool canCreateNew = false;
            Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out canCreateNew);
            if (!canCreateNew)
            {
                return true;
            }
            else
            {
                return false ;
            }
        }
        /// <summary>
        /// 程序自启动
        /// </summary>
        /// <param name="SoftWareName">软件在注册表中的备注名称</param>
        /// <param name="SoftWareFile">软件实际的启动路径</param>
        /// <returns>IsOk :是否设置成功，Msg;异常信息</returns>
        public static (bool IsOk,string Msg) SoftwareAutoRun(string SoftWareName, string SoftWareFile)
        {
            try
            {
                string RunFile = @"Software\Microsoft\Windows\CurrentVersion\Run";//程序自启动注册表路径
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser;
                Microsoft.Win32.RegistryKey run = key.CreateSubKey(RunFile);//注册表路径
                if (string.IsNullOrEmpty(SoftWareName))
                {
                    return (false, "程序默认名为空");
                }
                if (string.IsNullOrEmpty(SoftWareFile))
                {
                    return (false, "程序的实际启动路径为空");
                }
                run.SetValue(SoftWareName, SoftWareFile);//设置注册表里面的备注名字以及实际选中的程序路径
                return (true, "");
            }
            catch (Exception ex)
            {
                return (true, ex .Message);
            }
        }
    }
}
