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


        private string _ffmpegExecutablePath;
        private string _ffprobeExecutablePath ;

        public FFmpegHelper()
        {
            if (Appsettings.Get("deploy") == "fn")
            {
                _ffmpegExecutablePath = "/usr/bin/ffmpeg";
                _ffprobeExecutablePath = "/usr/bin/ffprobe";
            }
            else
            {
                #if DEBUG
                // Debug 环境，通常是 Windows
                  _ffmpegExecutablePath = "E:\\down\\ffmpeg\\bin\\ffmpeg.exe";
                  _ffprobeExecutablePath = "E:\\down\\ffmpeg\\bin\\ffprobe.exe";
            #else
                // Release 环境，通常是 Docker Linux
                  _ffmpegExecutablePath = "ffmpeg";
                  _ffprobeExecutablePath = "ffprobe";
            #endif
            }
        }

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
                int imageCount = imageList.Count; // 要循环的图片总数（对应loop的size参数）
                double singleLoopDuration = imageCount * ImageDisplayDurationSeconds;

                if (audioDurationSeconds > singleLoopDuration)
                {
                    loopCount = (int)Math.Ceiling(audioDurationSeconds / singleLoopDuration);
                    //Console.WriteLine($"图片序列将循环 {loopCount} 次以匹配音频时长");
                }
                // H264编码要求宽高必须为偶数 20260103
                if (VideoWidth % 2 != 0)
                {
                    VideoWidth++;
                }
                if (VideoHeight % 2 != 0)
                {
                    VideoHeight++;
                }

                var arguments = new List<string>
                        {
                            "-y", // 覆盖输出文件

                            // 图片序列输入：保留原有逻辑，修复帧率含义（-r 是输出帧率，输入用-framerate更准确）
                            "-f", "image2",
                            "-framerate", imageFps.ToString(CultureInfo.InvariantCulture), // 输入帧率（图片播放速度）
                            "-start_number", "0", // 序列起始编号
                            "-i", imageSequencePattern, // 图片序列模板（无需引号，后续用ProcessStartInfo传参更安全）

                            // 音频输入
                            "-i", audioFilePath,

                            // 滤镜修复：补充size参数（核心！解决Number of frames to loop is not set报错）
                            // loop=循环次数:size=要循环的帧数；loop=-1表示无限循环（配合-shortest更简洁）
                            "-filter_complex", $"[0:v]loop=loop={loopCount - 1}:size={imageCount}[v]",

                            // 流映射
                            "-map", "[v]",
                            "-map", "1:a",

                            // 视频编码：保留你的变量，确保兼容性
                            "-c:v", VideoCodec,
                            "-preset", VideoPreset,
                            "-crf", $"{VideoCrf}",
                            "-s", $"{VideoWidth}x{VideoHeight}",
                            "-pix_fmt", "yuv420p", // 兼容所有播放器
                            "-profile:v", "main",

                            // 音频编码：标准化参数，避免无效值
                            "-c:a", AudioCodec,
                            "-b:a", $"{AudioBitrate}",
                            "-ac", "2", // 立体声
                            "-ar", "44100", // 标准采样率

                            // 封装优化
                            "-f", "mp4",
                            "-movflags", "+faststart",

                            // 同步参数：确保视频时长匹配音频
                            "-shortest",

                            // 输出路径
                            outputVideoPath
                        };

                // 执行命令
                await ExecuteFFmpegAsync(arguments, progress, cancellationToken);
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
        private async Task ExecuteFFmpegAsync(List<string> arguments, IProgress<double> progress, CancellationToken cancellationToken)
        {
            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                throw new InvalidOperationException("已有一个FFmpeg进程正在运行。");
            }

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var startInfo = new ProcessStartInfo
            {
                FileName = _ffmpegExecutablePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };

            foreach (var arg in arguments)
            {
                startInfo.ArgumentList.Add(arg);
            }
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
                    throw new InvalidOperationException($"FFmpeg执行失败，退出码: {_ffmpegProcess.ExitCode}。请查看控制台输出获取详细错误信息。执行命令：ffmpeg {string.Join(" ", arguments)}");
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
