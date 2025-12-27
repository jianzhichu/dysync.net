namespace dy.net.utils
{
    public class DouyinFileUtils
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

        /// <summary>
        /// 极简版：计算多个文件的总大小（字节）
        /// </summary>
        /// <param name="filePaths">文件路径列表</param>
        /// <returns>总字节数</returns>
        public static long GetTotalFileSize(List<string> filePaths)
        {
            long totalSize = 0;
            foreach (var path in filePaths)
            {
                // 仅判断文件是否存在，存在则累加大小
                if (File.Exists(path))
                {
                    totalSize += new FileInfo(path).Length;
                }
            }
            return totalSize;
        }


        /// <summary>
        /// 检查指定文件夹是否有读取权限
        /// </summary>
        /// <param name="directoryPath">文件夹路径</param>
        /// <returns>有读权限返回true，否则返回false</returns>
        public static bool HasDirectoryReadPermission(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                return false;

            try
            {
                // 首先检查文件夹是否存在
                if (!Directory.Exists(directoryPath))
                    return false;

                // 尝试枚举文件夹内容（核心验证读权限）
                // EnumerateFileSystemEntries 会触发实际的读权限检查
                var entries = Directory.EnumerateFileSystemEntries(directoryPath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                // 明确捕获无权限异常
                return false;
            }
            catch (Exception ex)
            {
                // 其他异常（如路径无效、文件夹被占用等），视为无有效读权限
                Console.WriteLine($"检查读权限时发生非权限异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查指定文件夹是否有写入权限
        /// </summary>
        /// <param name="directoryPath">文件夹路径</param>
        /// <returns>有写权限返回true，否则返回false</returns>
        public static bool HasDirectoryWritePermission(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                return false;

            try
            {
                // 检查文件夹是否存在
                if (!Directory.Exists(directoryPath))
                    return false;

                // 核心验证：在文件夹内创建临时文件（最直接的写权限验证）
                string tempFileName = $"temp_perm_check_{Guid.NewGuid()}.tmp";
                string tempFilePath = Path.Combine(directoryPath, tempFileName);

                // 尝试创建临时文件
                using (FileStream fs = File.Create(tempFilePath, 1, FileOptions.DeleteOnClose))
                {
                    // FileOptions.DeleteOnClose 确保即使程序异常退出，临时文件也会被删除
                    return true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查写权限时发生非权限异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查指定文件夹是否同时有读写权限
        /// </summary>
        /// <param name="directoryPath">文件夹路径</param>
        /// <returns>同时有读写权限返回true，否则返回false</returns>
        public static bool HasDirectoryReadWritePermission(string directoryPath)
        {
            return HasDirectoryReadPermission(directoryPath) && HasDirectoryWritePermission(directoryPath);
        }
    }
}
