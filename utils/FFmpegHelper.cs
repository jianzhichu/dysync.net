using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dy.net.utils
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
            int VideoWidth = 1080,
            int Height = 1920,
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
                    string extension = Path.GetExtension(sourcePath);
                    string destFileName = $"temp_{i + 1:D3}{extension}";
                    string destPath = Path.Combine(tempImageDir, destFileName);
                    File.Copy(sourcePath, destPath);
                }

                string imageSequencePattern = Path.Combine(tempImageDir, "temp_%03d" + Path.GetExtension(imageList[0]));
                double imageFps = Math.Round(1.0 / ImageDisplayDurationSeconds, 2);

                // --- 修正点 1: 整合音频时长获取逻辑 ---
                // 我们不再需要 GetAudioFilterAsync，而是直接获取时长用于计算视频循环
                double audioDurationSeconds = imageList.Count * ImageDisplayDurationSeconds; // 默认值为图片总时长
                try
                {
                    // 使用 ffprobe 获取音频时长
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = _ffprobeExecutablePath,
                        Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{audioFilePath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = new Process { StartInfo = startInfo })
                    {
                        process.Start();
                        string output = await process.StandardOutput.ReadToEndAsync();
                        await process.WaitForExitAsync();

                        if (double.TryParse(output, out double duration))
                        {
                            audioDurationSeconds = duration;
                            //Console.WriteLine($"成功获取音频时长: {audioDurationSeconds:F2}s");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error($"获取音频时长失败，将使用图片总时长 ({audioDurationSeconds:F2}s) 作为视频时长: {ex.Message}");
                }

                // 计算图片序列需要循环的次数
                int loopCount = 1;
                double singleLoopDuration = imageList.Count * ImageDisplayDurationSeconds;
                if (audioDurationSeconds > singleLoopDuration)
                {
                    loopCount = (int)Math.Ceiling(audioDurationSeconds / singleLoopDuration);
                    //Console.WriteLine($"图片序列将循环 {loopCount} 次以匹配音频时长");
                }

                // --- 修正点 2: 重新构建 FFmpeg 参数列表 ---
                //var arguments = new List<string>
                //{
                //    "-y", // 覆盖输出文件

                //    // --- 所有输入文件放在最前面 ---
                //    // 图片序列输入
                //    "-f", "image2",
                //    "-r", imageFps.ToString(CultureInfo.InvariantCulture),
                //    "-i", $"\"{imageSequencePattern}\"",

                //    // 音频输入
                //    "-i", $"\"{audioFilePath}\"",

                //    // --- 修正点 3: 使用 filter_complex 对视频流进行循环 ---
                //    // [0:v] 表示第一个输入（图片序列）的视频流
                //    // loop={loopCount-1} 表示循环 (次数-1) 次
                //    // [v] 是处理后的视频流的别名
                //    "-filter_complex", $"\"[0:v]loop={loopCount - 1}[v]\"",

                //    // --- 修正点 4: 明确映射输出流 ---
                //    // 将处理后的视频流 [v] 映射到输出
                //    "-map", "\"[v]\"",
                //    // 将第二个输入（音频文件）的音频流映射到输出
                //    "-map", "\"1:a\"",

                //    // --- 视频编码配置 ---
                //    "-c:v", VideoCodec,
                //    "-preset", VideoPreset,
                //    "-crf", $"{VideoCrf}",
                //    "-s", $"{VideoWidth}x{VideoHeight}",
                //    "-pix_fmt", "yuv420p",

                //    // --- 音频编码配置 ---
                //    "-c:a", AudioCodec,
                //    "-b:a", $"{AudioBitrate}",

                //    // --- 修正点 5: 使用 -shortest 参数 ---
                //    // 确保视频和音频同时结束，即使循环次数计算得不完全精确
                //    "-shortest",

                //    // --- 输出文件 ---
                //    $"\"{outputVideoPath}\""
                //};

                //AI优化后。。20251129
                var arguments = new List<string>
                    {
                        "-y", // 覆盖输出文件

                        // 图片序列输入：恢复你原有的 -r，移除可能冲突的参数
                        "-f", "image2",
                        "-r", imageFps.ToString(CultureInfo.InvariantCulture), // 保留你原本的帧率参数
                        "-start_number", "0", // 仅增加：避免序列编号问题（不影响原有逻辑）
                        "-i", imageSequencePattern, // 关键修复：去掉引号（原代码的引号是核心失败原因）

                        // 音频输入：同样去掉引号
                        "-i", audioFilePath,

                        // 滤镜修复：恢复简单写法，避免复杂参数（适配旧版 FFmpeg）
                        "-filter_complex", $"[0:v]loop={loopCount - 1}[v]", // 去掉额外参数，和你原逻辑一致

                        // 流映射：去掉引号，恢复简单写法
                        "-map", "[v]",
                        "-map", "1:a",

                        // 视频编码：保留你的变量，只增加兼容性参数
                        "-c:v", VideoCodec, // 保留你原本的编码变量（之前能跑通，说明该编码支持）
                        "-preset", VideoPreset, // 保留原变量
                        "-crf", $"{VideoCrf}", // 保留原变量
                        "-s", $"{VideoWidth}x{VideoHeight}",
                        "-pix_fmt", "yuv420p", // 仅增加：兼容性关键（不影响原有逻辑）
                        "-profile:v", "main", // 仅增加：适配多数播放器（不冲突）

                        // 音频编码：保留你的变量，修复可能的无效值
                        "-c:a", AudioCodec, // 保留原变量
                        "-b:a", $"{AudioBitrate}", // 保留原变量
                        "-ac", "2", // 仅增加：避免单声道兼容问题
                        "-ar", "44100", // 仅增加：标准化采样率

                        // 封装优化：仅增加关键参数，不影响原有逻辑
                        "-f", "mp4", // 明确格式（之前可能自动识别，现在显式声明更稳定）
                        "-movflags", "+faststart", // 仅增加：解决部分播放器无法播放

                        // 保留你原有的同步参数
                        "-shortest",

                        // 输出路径：去掉引号（核心修复点）
                        outputVideoPath
                    };

                string args = string.Join(" ", arguments);
                //Console.WriteLine($"执行FFmpeg命令: {_ffmpegExecutablePath} {args}");

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
                // 清理临时目录
                if (Directory.Exists(tempImageDir))
                {
                    Directory.Delete(tempImageDir, recursive: true);
                }
            }
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
                //Console.WriteLine($"FFmpeg: {e.Data}");
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
