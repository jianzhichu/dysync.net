using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace dy.net.utils
{
  

    public static class DiskInfoHelper
    {/// <summary>
     /// 在Docker容器中获取Linux宿主机的本地固定磁盘总空间（GB）
     /// 前提：宿主机需挂载 /proc 到容器内的 /host/proc（启动时加 -v /proc:/host/proc:ro）
     /// </summary>
     /// <returns>总空间字符串（如 "1408.35 GB"），失败时返回错误信息</returns>
        public static string GetDockerHostTotalDiskSpaceGB()
        {
            try
            {
                // 1. 检查是否为Linux宿主机（Docker主要运行在Linux上）
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return "仅支持Linux宿主机（Docker容器内）";
                }

                // 2. 检查宿主机/proc是否已挂载到容器
                string hostProcMountsPath = "/app/db/mounts";
                if (!File.Exists(hostProcMountsPath))
                {
                    return "未检测到宿主机/proc挂载，请使用 -v /proc:/host/proc:ro 启动容器";
                }

                // 3. 从宿主机/proc/mounts筛选物理磁盘挂载点（排除虚拟文件系统）
                var physicalMounts = GetHostPhysicalMounts(hostProcMountsPath);
                if (!physicalMounts.Any())
                {
                    return "未找到宿主机的物理磁盘挂载点";
                }

                // 4. 计算所有物理磁盘的总空间（通过df命令获取挂载点的总空间）
                long totalBytes = 0;
                foreach (var mountPoint in physicalMounts)
                {
                    // 执行df命令获取宿主机挂载点的总空间（需容器内有df工具，或通过/proc/diskstats计算）
                    var (success, bytes) = GetMountPointTotalBytes(mountPoint);
                    if (success)
                    {
                        totalBytes += bytes;
                    }
                }

                if (totalBytes == 0)
                {
                    return "无法读取宿主机磁盘空间（可能权限不足）";
                }

                // 5. 转换为GB
                return ConvertBytesToGb(totalBytes);
            }
            catch (Exception ex)
            {
                return $"获取宿主机磁盘空间失败：{ex.Message}";
            }
        }

        /// <summary>
        /// 从宿主机/proc/mounts筛选物理磁盘挂载点（排除虚拟文件系统）
        /// </summary>
        private static List<string> GetHostPhysicalMounts(string hostProcMountsPath)
        {
            var physicalMounts = new List<string>();
            // 虚拟文件系统类型（排除这些类型，剩下的视为物理磁盘相关）
            var virtualFsTypes = new HashSet<string>
        {
            "tmpfs", "sysfs", "proc", "devtmpfs", "devpts", "cgroup", "cgroup2",
            "securityfs", "pstore", "debugfs", "hugetlbfs", "mqueue", "configfs",
            "fusectl", "overlay", "squashfs", "overlay2" // overlay/overlay2是Docker自身的存储驱动，需排除
        };

            foreach (var line in File.ReadAllLines(hostProcMountsPath))
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    string device = parts[0]; // 设备名（如/dev/sda1）
                    string mountPoint = parts[1]; // 挂载点（如/）
                    string fsType = parts[2]; // 文件系统类型

                    // 筛选条件：非虚拟文件系统 + 设备名以/dev/开头（物理设备）
                    if (!virtualFsTypes.Contains(fsType) && device.StartsWith("/dev/"))
                    {
                        physicalMounts.Add(mountPoint);
                    }
                }
            }

            return physicalMounts.Distinct().ToList();
        }

        /// <summary>
        /// 通过df命令获取宿主机挂载点的总空间（字节）
        /// （需容器内安装coreutils，或替换为解析/proc/diskstats的逻辑）
        /// </summary>
        private static (bool success, long totalBytes) GetMountPointTotalBytes(string hostMountPoint)
        {
            try
            {
                // 在容器内执行df命令，指定宿主机的挂载点（需宿主机路径在容器内可见，或通过/proc计算）
                // 注意：df命令返回的是1K-blocks，需转换为字节（*1024）
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "df",
                        Arguments = $"-P {hostMountPoint}", // -P 确保输出格式一致
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    return (false, 0);
                }

                // 解析df输出（第二行为数据行）
                var lines = output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length < 2)
                {
                    return (false, 0);
                }

                var dataParts = lines[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (dataParts.Length >= 2 && long.TryParse(dataParts[1], out long blocks))
                {
                    return (true, blocks * 1024); // 1K-blocks -> 字节
                }

                return (false, 0);
            }
            catch
            {
                return (false, 0);
            }
        }

        /// <summary>
        /// 字节转GB（1GB = 1024^3字节）
        /// </summary>
        private static string ConvertBytesToGb(long bytes)
        {
            if (bytes <= 0) return "磁盘空间计算错误";
            double gb = (double)bytes / (1024 * 1024 * 1024);
            return $"{gb:F2} GB";
        }
    }
}
