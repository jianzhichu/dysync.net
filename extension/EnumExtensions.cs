using dy.net.model.dto;
using System.ComponentModel;
using System.Reflection;

namespace dy.net.extension
{
    public static class EnumExtensions
    {
        /// <summary>
        /// 获取枚举值的 Description 特性描述
        /// </summary>
        /// <param name="enumValue">枚举值</param>
        /// <returns>如果存在 Description 特性，则返回其描述；否则返回枚举值的名称</returns>
        public static string GetDescription(this Enum enumValue)
        {
            // 1. 获取枚举值对应的类型
            Type enumType = enumValue.GetType();

            // 2. 获取枚举值对应的成员信息
            MemberInfo[] memberInfo = enumType.GetMember(enumValue.ToString());

            if (memberInfo.Length > 0)
            {
                // 3. 检查该成员是否有 Description 特性
                object[] attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes.Length > 0)
                {
                    // 4. 如果有，返回 Description 的值
                    return ((DescriptionAttribute)attributes[0]).Description;
                }
            }

            // 5. 如果没有找到 Description 特性，返回枚举值本身的名称
            return enumValue.GetHashCode().ToString();
        }


        /// <summary>
        /// 将 int 值转换为 VideoTypeEnum。
        /// </summary>
        /// <param name="value">要转换的 int 值</param>
        /// <returns>转换成功则返回对应的 VideoTypeEnum 值，否则返回 null</returns>
        public static VideoTypeEnum? ToVideoTypeEnum(this int value)
        {
            if (Enum.IsDefined(typeof(VideoTypeEnum), value))
            {
                return (VideoTypeEnum)value;
            }
            return null;
        }

        public static VideoTypeEnum? ToVideoTypeEnum(this string value)
        {
            if (int.TryParse(value, out int intValue))
            {
                return ToVideoTypeEnum(intValue);
            }
            return null;
        }
    }
}
