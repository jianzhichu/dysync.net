using System.Reflection;

namespace dy.net.utils
{
    public static class ObjectExtensions
    {
        public static void PrintAsTable(this object obj)
        {
            if (obj == null)
            {
                Console.WriteLine("对象为null");
                return;
            }

            PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (properties.Length == 0)
            {
                Console.WriteLine("没有找到公共属性");
                return;
            }

            // 计算最大宽度
            int maxNameLength = "属性名".Length;
            int maxValueLength = "值".Length;

            foreach (var prop in properties)
            {
                maxNameLength = Math.Max(maxNameLength, prop.Name.Length);

                object value = prop.GetValue(obj);
                string valueStr = value?.ToString() ?? "null";
                maxValueLength = Math.Max(maxValueLength, valueStr.Length);
            }

            // 构建表格
            string separator = "+" + new string('-', maxNameLength + 2) + "+" + new string('-', maxValueLength + 2) + "+";

            Serilog.Log.Debug(separator);
            Serilog.Log.Debug($"| {"属性名".PadRight(maxNameLength)} | {"值".PadRight(maxValueLength)} |");
            Serilog.Log.Debug(separator);

            foreach (var prop in properties)
            {
                string name = prop.Name;
                object value = prop.GetValue(obj);
                string valueStr = value?.ToString() ?? "null";

                // 处理过长的字符串
                if (valueStr.Length > maxValueLength)
                {
                    valueStr = valueStr.Substring(0, maxValueLength - 3) + "...";
                }

                Serilog.Log.Debug($"| {name.PadRight(maxNameLength)} | {valueStr.PadRight(maxValueLength)} |");
            }

            Serilog.Log.Debug(separator);
        }
    }
}
