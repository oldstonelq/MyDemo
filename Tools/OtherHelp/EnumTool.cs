// ---------------------------------------------------------------------------------
// File: EnumTool.cs
// Description: 枚举转换工具类
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tools.OtherHelp
{
    /// <summary>
    /// 枚举工具类
    /// </summary>
    public class EnumTool
    {
        /// <summary>
        /// 将字符串转换为指定的枚举类型（不区分大小写）
        /// 转换失败时抛出异常
        /// </summary>
        /// <typeparam name="TEnum">目标枚举类型</typeparam>
        /// <param name="value">要转换的字符串</param>
        /// <returns>转换后的枚举值</returns>
        /// <exception cref="ArgumentException">转换失败时抛出</exception>
        public static TEnum Parse<TEnum>(string value) where TEnum : struct, Enum
        {
            if (TryParse(value, out TEnum result))
            {
                return result;
            }

            throw new ArgumentException(
                $"无法将字符串 '{value}' 转换为枚举类型 '{typeof(TEnum).Name}'",
                nameof(value)
            );
        }
        /// <summary>
        /// 尝试将字符串转换为指定的枚举类型（不区分大小写）
        /// 转换失败时返回false，不抛出异常
        /// </summary>
        /// <typeparam name="TEnum">目标枚举类型</typeparam>
        /// <param name="value">要转换的字符串</param>
        /// <param name="result">转换成功的枚举值</param>
        /// <returns>是否转换成功</returns>
        public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct, Enum
        {
            // 检查输入合法性
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default;
                return false;
            }

            // 尝试转换（不区分大小写）
            return Enum.TryParse(value, ignoreCase: true, out result) &&
                   Enum.IsDefined(typeof(TEnum), result);
        }
        /// <summary>
        /// 将字符串转换为指定的枚举类型，转换失败时返回默认值
        /// </summary>
        /// <typeparam name="TEnum">目标枚举类型</typeparam>
        /// <param name="value">要转换的字符串</param>
        /// <param name="defaultValue">转换失败时的默认值</param>
        /// <returns>转换后的枚举值或默认值</returns>
        public static TEnum ParseOrDefault<TEnum>(string value, TEnum defaultValue) where TEnum : struct, Enum
        {
            return TryParse(value, out TEnum result) ? result : defaultValue;
        }
        /// <summary>
        /// 将整数转换为指定的枚举类型
        /// 转换失败时抛出异常
        /// </summary>
        /// <typeparam name="TEnum">目标枚举类型</typeparam>
        /// <param name="value">要转换的整数</param>
        /// <returns>转换后的枚举值</returns>
        /// <exception cref="ArgumentException">转换失败时抛出</exception>
        public static TEnum FromInt<TEnum>(int value) where TEnum : struct, Enum
        {
            if (TryFromInt(value, out TEnum result))
            {
                return result;
            }

            throw new ArgumentException(
                $"整数 '{value}' 不是枚举类型 '{typeof(TEnum).Name}' 的有效值",
                nameof(value)
            );
        }
        /// <summary>
        /// 尝试将整数转换为指定的枚举类型
        /// 转换失败时返回false
        /// </summary>
        /// <typeparam name="TEnum">目标枚举类型</typeparam>
        /// <param name="value">要转换的整数</param>
        /// <param name="result">转换成功的枚举值</param>
        /// <returns>是否转换成功</returns>
        public static bool TryFromInt<TEnum>(int value, out TEnum result) where TEnum : struct, Enum
        {
            result = (TEnum)Enum.ToObject(typeof(TEnum), value);
            return Enum.IsDefined(typeof(TEnum), result);
        }
        /// <summary>
        /// 获取枚举值的描述特性
        /// </summary>
        /// <param name="enumValue">枚举值</param>
        /// <returns></returns>
        public static string GetEnumDescription(Enum enumValue)
        {
            FieldInfo fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                DescriptionAttribute[] attributes =
                    (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes.Length > 0)
                    return attributes[0].Description;
            }

            return enumValue.ToString();
        }
        /// <summary>
        /// 获取枚举值的描述特性
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <param name="enumValue">枚举值</param>
        /// <returns>描述文本，若没有描述则返回枚举值的字符串形式</returns>
        public static string GetEnumDescription<TEnum>(TEnum enumValue)
            where TEnum : struct, Enum
        {
            // 获取枚举值对应的字段信息
            FieldInfo fieldInfo = typeof(TEnum).GetField(enumValue.ToString());
            if (fieldInfo == null)
            {
                return enumValue.ToString();
            }

            // 获取字段上的DescriptionAttribute特性
            var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false
            );

            // 如果有描述特性，则返回描述文本，否则返回枚举值的字符串形式
            return attributes.Length > 0 ? attributes[0].Description : enumValue.ToString();
        }
        /// <summary>
        /// 根据描述获取对应的枚举值
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <param name="description">枚举的描述文本</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns>对应的枚举值</returns>
        /// <exception cref="ArgumentException">当枚举类型无效或未找到匹配值时抛出</exception>
        public static TEnum GetEnumFromDescription<TEnum>(string description, bool ignoreCase = true)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException(nameof(description), "描述文本不能为空");
            }

            // 遍历枚举的所有值
            foreach (TEnum enumValue in Enum.GetValues(typeof(TEnum)))
            {
                // 获取当前枚举值的描述
                string enumDescription = GetEnumDescription(enumValue);

                // 比较描述是否匹配
                if (string.Equals(enumDescription, description,
                    ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                {
                    return enumValue;
                }
            }

            // 如果没有找到匹配的枚举值
            throw new ArgumentException(
                $"枚举类型 {typeof(TEnum).Name} 中未找到与描述 '{description}' 匹配的值",
                nameof(description)
            );
        }


        /// <summary>
        /// 尝试根据描述获取对应的枚举值
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <param name="description">枚举的描述文本</param>
        /// <param name="result">输出的枚举值</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns>是否获取成功</returns>
        public static bool TryGetEnumFromDescription<TEnum>(string description, out TEnum result, bool ignoreCase = true)
            where TEnum : struct, Enum
        {
            result = default;
            if (string.IsNullOrEmpty(description))
            {
                return false;
            }

            foreach (TEnum enumValue in Enum.GetValues(typeof(TEnum)))
            {
                string enumDescription = GetEnumDescription(enumValue);
                if (string.Equals(enumDescription, description,
                    ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                {
                    result = enumValue;
                    return true;
                }
            }

            return false;
        }

    }
}
