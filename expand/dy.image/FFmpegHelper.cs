using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dy.image
{
    public class FFmpegHelper : IDisposable
    {
        #if DEBUG
                // Debug 环境，通常是 Windows
                private readonly string _ffmpegExecutablePath = "E:\\down\\ffmpeg\\bin\\ffmpeg.exe";
                private readonly string _ffprobeExecutablePath = "E:\\down\\ffmpeg\\bin\\ffprobe.exe";
        #else
            // Release 环境，通常是 Docker Linux
            private readonly string _ffmpegExecutablePath = "ffmpeg";
            private readonly string _ffprobeExecutablePath = "ffprobe";
        #endif


        private Process _ffmpegProcess;
        private CancellationTokenSource _cancellationTokenSource;

        // 视频参数
        public int VideoWidth { get; set; } = 1080;
        public int VideoHeight { get; set; } = 1920;
        public int OutputFrameRate { get; set; } = 30;
        public int ImageDisplayDurationSeconds { get; set; } = 2;

        // 编码参数
        public string VideoCodec { get; set; } = "libx264";
        public string VideoPreset { get; set; } = "medium";
        public int VideoCrf { get; set; } = 23;
        public string AudioCodec { get; set; } = "aac";
        public string AudioBitrate { get; set; } = "192k";

       
        /// <summary>
        /// 将多张图片和一个音频文件合成为视频（最终终极版）。
        /// </summary>
        public async Task<string> CreateVideoFromImagesAndAudioAsync(
            IEnumerable<string> imageFilePaths,
            string audioFilePath,
            string outputVideoPath,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default)
        {
            // 输入验证
            if (imageFilePaths == null || !imageFilePaths.Any())
                throw new ArgumentException("图片路径列表不能为空。", nameof(imageFilePaths));

            if (string.IsNullOrEmpty(audioFilePath) || !File.Exists(audioFilePath))
                throw new FileNotFoundException("音频文件未找到。", audioFilePath);

            if (string.IsNullOrEmpty(outputVideoPath))
                throw new ArgumentNullException(nameof(outputVideoPath));

            foreach (var imagePath in imageFilePaths)
            {
                if (!File.Exists(imagePath))
                    throw new FileNotFoundException("图片文件未找到。", imagePath);
            }

            var outputDirectory = Path.GetDirectoryName(outputVideoPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // 关键步骤 1: 创建临时目录并生成有序图片序列
            string tempImageDir = Path.Combine(AppContext.BaseDirectory, "temp", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempImageDir);

            var imageList = imageFilePaths.ToList();
            try
            {
                for (int i = 0; i < imageList.Count; i++)
                {
                    string sourcePath = imageList[i];
                    // 重命名为有规律的文件名，如 temp_001.jpg, temp_002.png
                    string extension = Path.GetExtension(sourcePath);
                    string destFileName = $"temp_{i + 1:D3}{extension}"; // D3 确保是3位数字，不足补0
                    string destPath = Path.Combine(tempImageDir, destFileName);
                    File.Copy(sourcePath, destPath);
                }

                // 关键步骤 2: 构建符合你成功经验的 FFmpeg 命令
                string imageSequencePattern = Path.Combine(tempImageDir, "temp_%03d" + Path.GetExtension(imageList[0]));
                double imageFps = Math.Round(1.0 / ImageDisplayDurationSeconds, 2); ; // 例如 1/3 = 0.333... fps

                // 音频滤镜：如果音频比图片长则截断，比图片短则循环
                string audioFilter = await GetAudioFilterAsync(audioFilePath, imageList.Count * ImageDisplayDurationSeconds);

                // 构建 FFmpeg 参数列表
                var arguments = new List<string>
                {
                    "-y", // 覆盖输出文件
                    // --- 输入图片序列的配置 ---
                    "-f", "image2",              // 明确指定输入为图片序列
                    "-vcodec", "webp",           // 强制使用 WebP 解码器
                    "-r", imageFps.ToString(CultureInfo.InvariantCulture), // 设置图片播放速度
                    $"-i", $"\"{imageSequencePattern}\"", // 图片序列的路径模式
                    // --- 输入音频的配置 ---
                    audioFilter,                 // 应用音频滤镜（可能为空）
                    $"-i", $"\"{audioFilePath}\"", // 音频文件路径
                    // --- 视频编码配置 ---
                    "-c:v", VideoCodec,          // 视频编码器 (如 libx264)
                    "-preset", VideoPreset,      // 编码预设 (如 medium)
                    $"-crf", $"{VideoCrf}",      // 视频质量因子
                    $"-s", $"{VideoWidth}x{VideoHeight}", // 输出视频分辨率
                    "-pix_fmt", "yuv420p",       // 像素格式，确保兼容性
                    // --- 音频编码配置 ---
                    "-c:a", AudioCodec,          // 音频编码器 (如 aac)
                    $"-b:a", $"{AudioBitrate}",  // 音频比特率 (如 192k)
                    // --- 输出文件 ---
                    $"\"{outputVideoPath}\""     // 最终输出视频路径
                };

                string args = string.Join(" ", arguments);
                Console.WriteLine($"执行FFmpeg命令: {_ffmpegExecutablePath} {args}");

                // 执行命令
                await ExecuteFFmpegAsync(args, progress, cancellationToken);

                if (File.Exists(outputVideoPath))
                {
                    return outputVideoPath;
                }
                else
                {
                    throw new InvalidOperationException("视频合成失败，未生成输出文件。");
                }
            }
            finally
            {
                // 关键步骤 3: 清理临时文件
                if (Directory.Exists(tempImageDir))
                {
                    Directory.Delete(tempImageDir, recursive: true);
                }
            }
        }

        /// <summary>
        /// 生成音频滤镜，用于循环或截断音频
        /// </summary>
        private async Task<string> GetAudioFilterAsync(string audioFilePath, double imageTotalDurationSeconds)
        {
            // 使用 ffprobe 获取音频时长
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = _ffprobeExecutablePath,
                    Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{audioFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (double.TryParse(output, out double audioDurationSeconds))
                    {
                        Console.WriteLine($"音频时长: {audioDurationSeconds:F2}s, 图片总时长: {imageTotalDurationSeconds:F2}s");
                        if (audioDurationSeconds < imageTotalDurationSeconds)
                        {
                            // 音频较短，需要循环
                            double loopCount = Math.Ceiling(imageTotalDurationSeconds / audioDurationSeconds);
                            return $"-filter_complex \"[1:a]loop={loopCount - 1}:size={Math.Round(audioDurationSeconds * 44100)}:start=0[a]\" -map \"[a]\"";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取音频时长失败，将不使用音频滤镜: {ex.Message}");
            }

            // 音频较长或获取时长失败，不使用滤镜（默认截断）
            return "";
        }

        /// <summary>
        /// 异步执行FFmpeg命令
        /// </summary>
        private async Task ExecuteFFmpegAsync(string arguments, IProgress<double> progress, CancellationToken cancellationToken)
        {
            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                throw new InvalidOperationException("已有一个FFmpeg进程正在运行。");
            }

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var startInfo = new ProcessStartInfo
            {
                FileName = _ffmpegExecutablePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };

            _ffmpegProcess = new Process { StartInfo = startInfo };

            _ffmpegProcess.ErrorDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;
                Console.WriteLine($"FFmpeg: {e.Data}");
            };

            try
            {
                _ffmpegProcess.Start();
                _ffmpegProcess.BeginErrorReadLine();

                using (_cancellationTokenSource.Token.Register(() =>
                {
                    if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
                    {
                        try { _ffmpegProcess.Kill(); } catch { }
                    }
                }))
                {
                    await _ffmpegProcess.WaitForExitAsync(_cancellationTokenSource.Token);
                }

                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    throw new OperationCanceledException("FFmpeg进程被用户取消。", _cancellationTokenSource.Token);
                }

                if (_ffmpegProcess.ExitCode != 0)
                {
                    throw new InvalidOperationException($"FFmpeg执行失败，退出码: {_ffmpegProcess.ExitCode}。请查看控制台输出获取详细错误信息。");
                }
            }
            finally
            {
                _ffmpegProcess?.Dispose();
                _ffmpegProcess = null;
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _ffmpegProcess?.Dispose();
        }
    }

}
