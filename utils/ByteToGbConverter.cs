namespace dy.net.utils
{
    public class ByteToGbConverter
    {
        /// <summary>
        /// 将字节数转换为GB（采用二进制换算：1GB = 1024^3 Byte）
        /// </summary>
        /// <param name="bytes">字节数（需使用long类型避免大数值溢出）</param>
        /// <param name="decimalPlaces">保留的小数位数</param>
        /// <returns>转换后的GB值</returns>
        public static double ConvertBytesToGb(long bytes, int decimalPlaces = 2)
        {
            // 1GB = 1024 * 1024 * 1024 = 1073741824 Byte
            double gb = (double)bytes / 1073741824;

            // 四舍五入保留指定小数位数
            return Math.Round(gb, decimalPlaces);
        }
    }
}
