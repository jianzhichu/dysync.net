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
        public static string ConvertBytesToGb(long bytes, int decimalPlaces = 2)
        {
            // 1GB = 1024 * 1024 * 1024 = 1073741824 Byte
            double gb = (double)bytes / 1073741824;

            // 四舍五入保留指定小数位数
            return Math.Round(gb, decimalPlaces).ToString("F2");
        }


        /// <summary>
        /// 获取宿主机所有本地固定磁盘的总空间（GB）
        /// </summary>
        /// <returns>总空间字符串（如 "1408.35 GB"），失败时返回错误信息</returns>
        public static string GetHostTotalDiskSpaceGB()
        {
            try
            {
                // 1. 获取所有驱动器
                DriveInfo[] allDrives = DriveInfo.GetDrives();

                // 2. 筛选：仅保留“就绪的本地固定磁盘”（DriveType.Fixed）
                var fixedDrives = allDrives.Where(drive =>
                    drive.IsReady &&
                    drive.DriveType == DriveType.Fixed
                ).ToList();

                if (!fixedDrives.Any())
                {
                    return "未找到可用的本地固定磁盘";
                }

                // 3. 累加所有固定磁盘的总空间（字节）
                long totalBytes = fixedDrives.Sum(drive => drive.TotalSize);

                // 4. 转换为GB并保留2位小数
                return ConvertBytesToGb(totalBytes);
            }
            catch (UnauthorizedAccessException)
            {
                return "权限不足，无法访问部分磁盘";
            }
            catch (Exception ex)
            {
                return $"获取宿主机总磁盘空间失败：{ex.Message}";
            }
        }
    }
}
