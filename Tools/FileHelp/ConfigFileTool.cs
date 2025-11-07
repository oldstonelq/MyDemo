// ---------------------------------------------------------------------------------
// File: ConfigFileTool.cs
// Description: 配置帮助类，提供对配置文件的读取、修改和删除操作
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.FileHelp
{
    /// <summary>
    /// 配置帮助类，提供对配置文件的读取、修改和删除操作
    /// </summary>
    public class ConfigHelper
    {
        /// <summary>
        /// 配置文件路径
        /// </summary>
        private string ConfigPath = string.Empty;

        /// <summary>
        /// 获取配置文件中指定键的值
        /// </summary>
        /// <param name="key">要获取的配置键</param>
        /// <returns>配置键对应的值，如果键不存在则返回空字符串</returns>
        public string GetConfigKey(string key)
        {
            Configuration ConfigurationInstance = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap()
            {
                ExeConfigFilename = ConfigPath
            }, ConfigurationUserLevel.None);

            if (ConfigurationInstance.AppSettings.Settings[key] != null)
                return ConfigurationInstance.AppSettings.Settings[key].Value;
            else
                return string.Empty;
        }
        /// <summary>
        /// 获取配置文件中的所有键值对
        /// </summary>
        /// <returns>包含所有键值对的字典</returns>
        public Dictionary<string, string> GetAllKeyAndValue()
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            Configuration ConfigurationInstance = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap()
            {
                ExeConfigFilename = ConfigPath
            }, ConfigurationUserLevel.None);
            if (ConfigurationInstance.AppSettings.Settings.Count > 0)
            {
                foreach (string key in ConfigurationInstance.AppSettings.Settings.AllKeys)
                {
                    keyValuePairs[key] = ConfigurationInstance.AppSettings.Settings[key].Value;
                }
            }
            return keyValuePairs;
        }

        /// <summary>
        /// 设置配置文件中指定键的值，如果键不存在则添加
        /// </summary>
        /// <param name="key">要设置的配置键</param>
        /// <param name="vls">要设置的配置值</param>
        /// <returns>操作是否成功</returns>
        public bool SetConfigKey(string key, string vls)
        {
            try
            {
                Configuration ConfigurationInstance = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap()
                {
                    ExeConfigFilename = ConfigPath
                }, ConfigurationUserLevel.None);

                if (ConfigurationInstance.AppSettings.Settings[key] != null)
                    ConfigurationInstance.AppSettings.Settings[key].Value = vls;
                else
                    ConfigurationInstance.AppSettings.Settings.Add(key, vls);

                ConfigurationInstance.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 删除配置文件中指定的键值对
        /// </summary>
        /// <param name="key">要删除的配置键</param>
        /// <returns>操作是否成功</returns>
        public bool DeleteConfigKey(string key)
        {
            try
            {
                Configuration ConfigurationInstance = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap()
                {
                    ExeConfigFilename = ConfigPath
                }, ConfigurationUserLevel.None);

                if (ConfigurationInstance.AppSettings.Settings[key] != null)
                    ConfigurationInstance.AppSettings.Settings.Remove(key);
                ConfigurationInstance.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        public ConfigHelper(string configPath)
        {
            ConfigPath = configPath;
        }
        /// <summary>
        /// 不带参数的构造函数
        /// </summary>
        public ConfigHelper()
        {
            // 获取app.config的默认路径
            ConfigPath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
        }
    }
}
