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

        /// <summary>
        /// 通过文件头魔数（前几个字节）识别真实文件类型
        /// </summary>
        public static string GetFileExtensionByMagicNumber(Stream stream)
        {
            // 读取前16个字节（足够识别常见格式）
            byte[] magicBytes = new byte[16];
            int readBytes = stream.Read(magicBytes, 0, magicBytes.Length);
            //stream.Position = 0; // 读完重置指针

            if (readBytes < 4) return ""; // 字节数不足，兜底

            // 魔数匹配规则（核心：区分MP4视频和M4A音频）
            // MP4/M4A 基础魔数：0x00 0x00 0x00 ?? 0x66 0x74 0x79 0x70 (ftyp)
            if (magicBytes[4] == 0x66 && magicBytes[5] == 0x74 && magicBytes[6] == 0x79 && magicBytes[7] == 0x70)
            {
                // 读取ftyp后的字符，区分MP4/M4A
                if (readBytes >= 12)
                {
                    // MP4 特征：ftypmp42 / ftypisom / ftypmp41
                    string ftyp = System.Text.Encoding.ASCII.GetString(magicBytes, 8, 4);
                    if (ftyp == "mp42" || ftyp == "isom" || ftyp == "mp41")
                    {
                        // 进一步判断是否包含视频轨道（简化版：M4A只有音频，MP4可能含视频）
                        // 这里做简化区分：如果是纯音频则为m4a，否则为mp4
                        // （进阶版可解析MP4盒子，新手先按此规则）
                        return "m4a"; // 你原链接的真实类型
                                      // 若要严格区分视频MP4，可扩展：return "mp4";
                    }
                    // M4A 特征：ftypM4A / ftypM4B / ftypM4P
                    else if (ftyp == "M4A " || ftyp == "M4B " || ftyp == "M4P ")
                    {
                        return "m4a";
                    }
                }
                // 兜底：MP4/M4A统一先按Content-Type辅助，无则按mp4
                return "mp4";
            }
            // MP3 魔数：0xFF 0xFB 或 0x49 0x44 0x33 (ID3)
            else if ((magicBytes[0] == 0xFF && magicBytes[1] == 0xFB) ||
                     (magicBytes[0] == 0x49 && magicBytes[1] == 0x44 && magicBytes[2] == 0x33))
            {
                return "mp3";
            }
            // 其他常见格式（可扩展）
            else if (magicBytes[0] == 0xFF && magicBytes[1] == 0xD8) // JPG
            {
                return "jpg";
            }
            else if (magicBytes[0] == 0x89 && magicBytes[1] == 0x50 && magicBytes[2] == 0x4E && magicBytes[3] == 0x47) // PNG
            {
                return "png";
            }
            else
            {
                return ""; // 未知格式
            }
        }
    }
}
