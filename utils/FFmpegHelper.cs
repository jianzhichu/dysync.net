using dy.net.model.dto;
using Serilog;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace dy.net.utils
{
    // ===== 新增：硬件加速类型枚举 =====
    public enum HardwareAccelerationType
    {
        None,
        Nvenc,
        Qsv,
        Amf,
        Vaapi
    }

    public class FFmpegHelper : IDisposable
    {
        private string _ffmpegExecutablePath;
        private string _ffprobeExecutablePath;

        // ===== 新增：硬件加速状态 =====
        private HardwareAccelerationType _hwAccelType = HardwareAccelerationType.None;
        private string _vaapiDevicePath;
        private static readonly object HardwareAccelerationDetectionLock = new object();
        private static bool _hardwareAccelerationDetectionCompleted;
        private static HardwareAccelerationType _detectedHardwareAccelerationType = HardwareAccelerationType.None;
        private static string _detectedVaapiDevicePath;
        private bool _useHardwareAcceleration = true;
        public bool UseHardwareAcceleration
        {
            get => _useHardwareAcceleration;
            set
            {
                if (_useHardwareAcceleration == value) return;
                _useHardwareAcceleration = value;
                DetectHardwareAcceleration();
            }
        }

        public FFmpegHelper() : this(true)
        {
        }

        public FFmpegHelper(bool useHardwareAcceleration)
        {
            _useHardwareAcceleration = useHardwareAcceleration;
            if (Appsettings.Get("deploy") == "fn")
            {
                _ffmpegExecutablePath = "/usr/bin/ffmpeg";
                _ffprobeExecutablePath = "/usr/bin/ffprobe";
            }
            else if (OperatingSystem.IsWindows())
            {
                // ===== 修正：Windows 环境（Debug/Release 都走这里） =====
                // 之前用 #if DEBUG 判断，导致 Debug 编译的 dll 丢进 Docker 后
                // 仍然使用 Windows 路径，造成 "No such file or directory"。
                // 改为直接判断操作系统，无论 Debug/Release、无论部署到哪都不会错。
#if DEBUG
                _ffmpegExecutablePath = "E:\\down\\ffmpeg\\bin\\ffmpeg.exe";
                _ffprobeExecutablePath = "E:\\down\\ffmpeg\\bin\\ffprobe.exe";
#else
                _ffmpegExecutablePath = "ffmpeg.exe";
                _ffprobeExecutablePath = "ffprobe.exe";
#endif
            }
            else
            {
                // ===== 修正：Linux / Docker / macOS 环境 =====
                _ffmpegExecutablePath = "ffmpeg";
                _ffprobeExecutablePath = "ffprobe";
            }

            // ===== 新增：构造函数末尾检测硬件加速 =====
            DetectHardwareAcceleration();
        }

        // ===== 新增：检测可用的硬件加速编码器（实测版，避免"能列出但用不了"） =====
        // 优先级：QSV > NVENC > AMD AMF > VAAPI > 软件编码
        private void DetectHardwareAcceleration()
        {
            if (!UseHardwareAcceleration)
            {
                _hwAccelType = HardwareAccelerationType.None;
                _vaapiDevicePath = null;
                return;
            }

            lock (HardwareAccelerationDetectionLock)
            {
                if (_hardwareAccelerationDetectionCompleted)
                {
                    _hwAccelType = _detectedHardwareAccelerationType;
                    _vaapiDevicePath = _detectedVaapiDevicePath;
                    return;
                }

                ProbeHardwareAcceleration();
                _detectedHardwareAccelerationType = _hwAccelType;
                _detectedVaapiDevicePath = _vaapiDevicePath;
                _hardwareAccelerationDetectionCompleted = true;
            }
        }

        private void ProbeHardwareAcceleration()
        {
            // In Linux containers, do not probe encoders unless GPU device nodes are exposed.
            if (OperatingSystem.IsLinux() && IsRunningInContainer() && !HasGpuDeviceExposed())
            {
                _hwAccelType = HardwareAccelerationType.None;
                _vaapiDevicePath = null;
                return;
            }

            if (TryEncodeWith("h264_qsv", new[] { "-preset", "medium", "-global_quality", "23" }))
            {
                _hwAccelType = HardwareAccelerationType.Qsv;
                return;
            }

            if (TryEncodeWith("h264_nvenc", new[] { "-preset", "p4", "-rc", "vbr", "-cq", "23", "-b:v", "0" }))
            {
                _hwAccelType = HardwareAccelerationType.Nvenc;
                return;
            }

            if (TryEncodeWith("h264_amf", new[] { "-quality", "balanced", "-rc", "cqp", "-qp_i", "23", "-qp_p", "23" }))
            {
                _hwAccelType = HardwareAccelerationType.Amf;
                return;
            }

            if (OperatingSystem.IsLinux())
            {
                var vaapiDevice = GetVaapiDevicePath();
                if (vaapiDevice != null && TryEncodeWith(
                    "h264_vaapi",
                    new[] { "-qp", "23" },
                    new[] { "-vaapi_device", vaapiDevice },
                    "format=nv12,hwupload"))
                {
                    _hwAccelType = HardwareAccelerationType.Vaapi;
                    _vaapiDevicePath = vaapiDevice;
                    return;
                }
            }

            _hwAccelType = HardwareAccelerationType.None;
            _vaapiDevicePath = null;
        }

        private static bool IsRunningInContainer()
        {
            return string.Equals(
                       Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
                       "true",
                       StringComparison.OrdinalIgnoreCase)
                   || File.Exists("/.dockerenv")
                   || File.Exists("/run/.containerenv");
        }

        private static bool HasGpuDeviceExposed()
        {
            try
            {
                if (Directory.Exists("/dev/dri") && Directory.GetFiles("/dev/dri", "renderD*").Length > 0)
                    return true;
            }
            catch (IOException)
            {
                // 设备目录不可读时按未暴露设备处理。
            }
            catch (UnauthorizedAccessException)
            {
                // 设备目录不可读时按未暴露设备处理。
            }

            return File.Exists("/dev/nvidia0") && File.Exists("/dev/nvidiactl");
        }

        private bool TryEncodeWith(string encoder, string[] extraArgs, string[] beforeInputArgs = null, string videoFilter = null)
        {
            try
            {
                var args = new List<string>
                {
                    "-hide_banner", "-loglevel", "error"
                };
                if (beforeInputArgs != null) args.AddRange(beforeInputArgs);
                args.AddRange(new[] { "-f", "lavfi", "-i", "color=red:s=256x256:d=1" });
                if (!string.IsNullOrWhiteSpace(videoFilter)) args.AddRange(new[] { "-vf", videoFilter });
                args.AddRange(new[] { "-c:v", encoder });
                args.AddRange(extraArgs);
                args.AddRange(new[] { "-f", "null", "-" });

                var psi = new ProcessStartInfo
                {
                    FileName = _ffmpegExecutablePath,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                foreach (var a in args) psi.ArgumentList.Add(a);

                using (var p = new Process { StartInfo = psi })
                {
                    var errorBuilder = new StringBuilder();
                    p.ErrorDataReceived += (_, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data)) errorBuilder.AppendLine(e.Data);
                    };
                    if (!p.Start()) return false;
                    p.BeginErrorReadLine();

                    // ===== 改动点 A：加 5 秒超时，避免探测卡死导致构造函数永久阻塞 =====
                    if (!p.WaitForExit(5000))
                    {
                        try { p.Kill(entireProcessTree: true); } catch { }
                        p.WaitForExit();
                        //Log.Information($"硬件编码器 {encoder} 探测超时（5秒）");
                        return false;
                    }

                    p.WaitForExit(); // Drain asynchronous stderr callbacks.
                    string err = errorBuilder.ToString();
                    if (p.ExitCode == 0) return true;
                    //Log.Information($"硬件编码器 {encoder} 探测失败（退出码 {p.ExitCode}）：{err.Trim()}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"硬件编码器 {encoder} 探测异常：{ex.Message}");
                return false;
            }
        }

        private static string GetVaapiDevicePath()
        {
            try
            {
                const string driDirectory = "/dev/dri";
                if (!Directory.Exists(driDirectory)) return null;
                return Directory.GetFiles(driDirectory, "renderD*")
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Information("查找 VAAPI render device 失败：{Message}", ex.Message);
                return null;
            }
        }

        // ===== 新增：根据硬件加速类型向参数列表追加视频编码参数 =====
        private void AppendVideoEncoderArgs(List<string> arguments)
        {
            switch (_hwAccelType)
            {
                case HardwareAccelerationType.Qsv:
                    arguments.AddRange(new[]
                    {
                        "-c:v", "h264_qsv",
                        "-preset", "medium",
                        "-global_quality", VideoCrf.ToString(CultureInfo.InvariantCulture)
                    });
                    break;

                case HardwareAccelerationType.Nvenc:
                    arguments.AddRange(new[]
                    {
                        "-c:v", "h264_nvenc",
                        "-preset", "p4",
                        "-rc", "vbr",
                        "-cq", VideoCrf.ToString(CultureInfo.InvariantCulture),
                        "-b:v", "0"
                    });
                    break;

                case HardwareAccelerationType.Amf:
                    arguments.AddRange(new[]
                    {
                        "-c:v", "h264_amf",
                        "-quality", "balanced",
                        "-rc", "cqp",
                        "-qp_i", VideoCrf.ToString(CultureInfo.InvariantCulture),
                        "-qp_p", VideoCrf.ToString(CultureInfo.InvariantCulture)
                    });
                    break;

                case HardwareAccelerationType.Vaapi:
                    if (!string.IsNullOrWhiteSpace(_vaapiDevicePath))
                        arguments.InsertRange(1, new[] { "-vaapi_device", _vaapiDevicePath });
                    arguments.AddRange(new[]
                    {
                        "-c:v", "h264_vaapi",
                        "-qp", VideoCrf.ToString(CultureInfo.InvariantCulture)
                    });
                    AppendVaapiUploadFilter(arguments);
                    break;

                default:
                    // 未检测到硬件加速，保持原有软件编码参数
                    arguments.AddRange(new[]
                    {
                        "-c:v", VideoCodec,
                        "-preset", VideoPreset,
                        "-crf", $"{VideoCrf}"
                    });
                    break;
            }
        }

        private static void AppendVaapiUploadFilter(List<string> arguments)
        {
            var videoFilterIndex = arguments.FindLastIndex(arg => arg == "-vf");
            if (videoFilterIndex >= 0 && videoFilterIndex + 1 < arguments.Count)
            {
                arguments[videoFilterIndex + 1] += ",format=nv12,hwupload";
                return;
            }

            var complexFilterIndex = arguments.FindLastIndex(arg => arg == "-filter_complex");
            if (complexFilterIndex >= 0 && complexFilterIndex + 1 < arguments.Count)
            {
                var graph = arguments[complexFilterIndex + 1];
                var outputLabelIndex = graph.LastIndexOf("[v]", StringComparison.Ordinal);
                if (outputLabelIndex >= 0)
                {
                    arguments[complexFilterIndex + 1] = graph.Insert(outputLabelIndex, ",format=nv12,hwupload");
                }
            }
        }

        private void AppendOutputPixelFormat(List<string> arguments)
        {
            // VAAPI 输出使用硬件帧，不应再强制指定软件 yuv420p。
            if (_hwAccelType != HardwareAccelerationType.Vaapi)
                arguments.InsertRange(arguments.Count - 1, new[] { "-pix_fmt", "yuv420p" });
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

      

        // ===== 改动点 B（路线 3 核心）：新增图片预处理方法 =====
        /// <summary>
        /// 图片预处理：等比缩放 + 黑边填充到目标尺寸，输出 jpg。
        /// 目的：把 scale/pad 从主合成命令里剥离，让 GPU 编码器被喂饱。
        /// 使用局部 Process 状态，不依赖 _ffmpegProcess，可安全并行调用。
        /// </summary>
        private async Task PreprocessImageAsync(
            string sourcePath,
            string destPath,
            int targetWidth,
            int targetHeight,
            CancellationToken cancellationToken)
        {

            var startInfo = new ProcessStartInfo
            {
                FileName = _ffmpegExecutablePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardErrorEncoding = Encoding.UTF8
            };

            var args = new[]
            {
                "-y", "-hide_banner", "-loglevel", "error",
                "-i", sourcePath,
                "-vf", $"scale={targetWidth}:{targetHeight}:force_original_aspect_ratio=decrease,pad={targetWidth}:{targetHeight}:(ow-iw)/2:(oh-ih)/2:black",
                "-frames:v", "1",   // 只输出一帧，防止动图/gif 输出多帧
                "-q:v", "2",        // jpg 质量，2 已接近无损
                destPath
            };
            foreach (var a in args) startInfo.ArgumentList.Add(a);

            using var process = new Process { StartInfo = startInfo };
            var stderrBuilder = new StringBuilder();
            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data)) stderrBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginErrorReadLine();

            using (cancellationToken.Register(() =>
            {
                try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch { }
            }))
            {
                await process.WaitForExitAsync(cancellationToken);
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"图片预处理失败：{sourcePath}\nstderr:\n{stderrBuilder}");
            }
        }


        public async Task<string> CreateVideoFromImagesAndAudioAsync(
       IEnumerable<DouyinMergeVideoDto> imageFilePaths,
       string audioFilePath,
       string outputVideoPath,
       int VideoWidth = 1080,
       int VideoHeight = 1920,
       IProgress<double> progress = null,
       CancellationToken cancellationToken = default)
        {
            Log.Information("硬件加速类型：" + _hwAccelType.ToString());
            // 输入验证
            if (imageFilePaths == null || !imageFilePaths.Any())
                throw new ArgumentException("图片路径列表不能为空。", nameof(imageFilePaths));

            if (string.IsNullOrEmpty(audioFilePath) || !File.Exists(audioFilePath))
                throw new FileNotFoundException("音频文件未找到。", audioFilePath);

            if (string.IsNullOrEmpty(outputVideoPath))
                throw new ArgumentNullException(nameof(outputVideoPath));

            foreach (var imagePath in imageFilePaths)
            {
                if (!File.Exists(imagePath.Path))
                    throw new FileNotFoundException("图片文件未找到。", imagePath.Path);
                // 验证图片宽高字段有效性
                if (imagePath.Width <= 0 || imagePath.Height <= 0)
                    throw new ArgumentException($"图片 {imagePath.Path} 的宽高必须大于0", nameof(imageFilePaths));
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
                // H264编码要求宽高为偶数
                if (VideoWidth % 2 != 0) VideoWidth++;
                if (VideoHeight % 2 != 0) VideoHeight++;

                // ===== 改动点 C（路线 3 核心）：并行预处理所有图片到目标尺寸 jpg =====
                // 并发度取 CPU 数的一半，上限 8，避免与后续主 FFmpeg 抢 CPU
                int maxConcurrency = Math.Max(2, Math.Min(Environment.ProcessorCount / 2, 8));
                using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

                var preprocessTasks = imageList.Select(async (img, i) =>
                {
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        string destFileName = $"temp_{i + 1:D3}.jpg";
                        string destPath = Path.Combine(tempImageDir, destFileName);
                        await PreprocessImageAsync(img.Path, destPath, VideoWidth, VideoHeight, cancellationToken);
                        return destPath;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }).ToArray();

                string[] preprocessedPaths = await Task.WhenAll(preprocessTasks);

                // 输出统一为 jpg
                string imageSequencePattern = Path.Combine(tempImageDir, "temp_%03d.jpg");
                double imageFps = Math.Round(1.0 / ImageDisplayDurationSeconds, 2);

                // 获取音频时长
                double audioDurationSeconds = imageList.Count * ImageDisplayDurationSeconds;
                try
                {
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
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"获取音频时长失败，将使用图片总时长 ({audioDurationSeconds:F2}s) 作为视频时长: {ex.Message}");
                }

                // 计算图片序列循环次数
                int loopCount = 1;
                int imageCount = imageList.Count;
                double singleLoopDuration = imageCount * ImageDisplayDurationSeconds;

                if (audioDurationSeconds > singleLoopDuration)
                {
                    loopCount = (int)Math.Ceiling(audioDurationSeconds / singleLoopDuration);
                }

                // ===== 改动点 D（路线 3 核心）：滤镜链去掉 scale/pad =====
                // 图片已预处理为目标尺寸，主合成命令里不再需要 scale/pad
                // 单图用 null 保持流映射一致；多图只做 loop（复制帧，CPU 开销极小）
                bool isSingleImage = imageCount == 1;
                string filterComplex = isSingleImage
                    ? "[0:v]null[v]"
                    : $"[0:v]loop=loop={loopCount - 1}:size={imageCount}[v]";

                var arguments = new List<string> { "-y" };

                if (isSingleImage)
                {
                    // ===== 改动点 E：单图输入改用预处理后的 jpg =====
                    arguments.AddRange(new[]
                    {
                        "-loop", "1",
                        "-framerate", OutputFrameRate.ToString(CultureInfo.InvariantCulture),
                        "-i", preprocessedPaths[0]
                    });
                }
                else
                {
                    arguments.AddRange(new[]
                    {
                        "-f", "image2",
                        "-framerate", imageFps.ToString(CultureInfo.InvariantCulture),
                        "-start_number", "1",
                        "-i", imageSequencePattern
                    });
                }

                arguments.AddRange(new[]
                {
                    // 音频输入
                    "-i", audioFilePath,

                    // 滤镜：尺寸适配 + 循环
                    "-filter_complex", filterComplex,

                    // 流映射
                    "-map", "[v]",
                    "-map", "1:a",
                });

                // ===== 视频编码参数（原 -c:v / -preset / -crf 三行替换为下面这一行，其他不动） =====
                AppendVideoEncoderArgs(arguments);

                arguments.AddRange(new[]
                {
                    // ===== 改动点 F：删掉 -s，图片已是目标尺寸 =====
                    // "-s", $"{VideoWidth}x{VideoHeight}",
                    "-profile:v", "main",

                    // 音频编码参数
                    "-c:a", AudioCodec,
                    "-b:a", $"{AudioBitrate}",
                    "-ac", "2",
                    "-ar", "44100",

                    // 封装优化
                    "-f", "mp4",
                    "-movflags", "+faststart",

                    // 同步参数
                    "-shortest",

                    // 输出路径
                    outputVideoPath
                });
                AppendOutputPixelFormat(arguments);

                // 执行FFmpeg命令
                await ExecuteFFmpegAsync(arguments, progress, cancellationToken);

                if (!File.Exists(outputVideoPath))
                {
                    throw new InvalidOperationException("视频合成失败，未生成输出文件。");
                }

                return outputVideoPath;
            }
            finally
            {
                // 清理临时目录（增加异常捕获，避免删除失败导致程序崩溃）
                if (Directory.Exists(tempImageDir))
                {
                    try
                    {
                        Directory.Delete(tempImageDir, recursive: true);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"清理临时目录失败: {ex.Message}");
                    }
                }
            }
        }





        /// <summary>
        /// 将单个视频转封装为统一编码/分辨率的临时视频（解决concat合并兼容问题）
        /// </summary>
        /// <param name="videoDto">待转封装的视频Dto</param>
        /// <param name="targetWidth">目标输出宽度</param>
        /// <param name="targetHeight">目标输出高度</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>统一参数后的临时视频路径</returns>
        private async Task<string> CreateUnifiedTempVideoAsync(
            DouyinMergeVideoDto videoDto,
            int targetWidth, // 外部传入的动态目标宽（已取所有视频的最大值，偶数）
            int targetHeight, // 外部传入的动态目标高（已取所有视频的最大值，偶数）
            CancellationToken cancellationToken)
        {
            // 1. 生成临时视频路径
            string tempVideoDir = Path.Combine(AppContext.BaseDirectory, "temp");
            if (!Directory.Exists(tempVideoDir))
            {
                Directory.CreateDirectory(tempVideoDir);
            }
            string tempVideoPath = Path.Combine(tempVideoDir, $"{Guid.NewGuid()}.mp4");

            // 2. 构建【动态适配不固定尺寸】的极简滤镜字符串（核心修改）
            // 无需写死尺寸，直接使用传入的动态目标分辨率（已保证为偶数）
            // scale=targetWidth:-2：宽度最大为动态目标宽，高度按原比例自动缩放，-2强制高度为偶数（满足H264）
            // pad=targetWidth:targetHeight：居中补黑边到动态目标分辨率，缩放后尺寸≤目标尺寸，无矛盾
            // 核心：动态双向限制滤镜（不写死任何尺寸，使用传入的targetWidth/targetHeight）
            // scale={0}:{1}:force_original_aspect_ratio=decrease：双向限制不超出动态目标尺寸，保持原比例
            // pad={0}:{1}：动态填充到目标尺寸，无尺寸矛盾，无硬编码
            string videoFilter = string.Format(CultureInfo.InvariantCulture,
                "scale={0}:{1}:force_original_aspect_ratio=decrease,pad={0}:{1}:(ow-iw)/2:(oh-ih)/2:black",
                targetWidth,  // 动态传入的目标宽度（无硬编码）
                targetHeight  // 动态传入的目标高度（无硬编码）
            );

            // 3. 打印滤镜字符串（用于调试，验证动态尺寸是否正确）
            //Log.Information($"待执行的动态滤镜（适配不固定尺寸）：{videoFilter}");

            // 4. 构建FFmpeg参数（无需对滤镜做任何转义，保留原有编码配置）
            var arguments = new List<string>
            {
                "-y",
                "-hide_banner",
                "-loglevel", "error",
                "-i", videoDto.Path,
                "-vf", // 滤镜参数标识（独立参数）
                videoFilter, // 动态滤镜字符串（独立参数，ArgumentList自动处理逗号）
            };

            // ===== 视频编码参数（原 -c:v / -preset / -crf 三行替换为下面这一行，其他不动） =====
            AppendVideoEncoderArgs(arguments);

            arguments.AddRange(new[]
            {
                "-profile:v", "main",
                // 关键帧计算也适配动态目标宽，保留原有逻辑
                "-g", $"{(targetWidth < 1080 ? 60 : 120)}",
                "-c:a", AudioCodec,
                "-b:a", AudioBitrate,
                "-ac", "2",
                "-ar", "44100",
                "-strict", "-2",
                "-f", "mp4",
                "-movflags", "+faststart",
                tempVideoPath
            });
            AppendOutputPixelFormat(arguments);

            try
            {
                // 5. 复用现有ExecuteFFmpegAsync（已使用ArgumentList，自动处理特殊字符）
                await ExecuteFFmpegAsync(arguments, null, cancellationToken);

                if (File.Exists(tempVideoPath))
                {
                    //Log.Information($"视频转封装为统一参数成功，临时路径：{tempVideoPath}");
                    return tempVideoPath;
                }
                else
                {
                    //Log.Error($"视频转封装失败，未生成临时文件，原路径：{videoDto.Path}");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"视频转封装异常，原路径：{videoDto.Path}");
                if (File.Exists(tempVideoPath))
                {
                    File.Delete(tempVideoPath);
                }
                return string.Empty;
            }
        }
        /// <summary>
        /// 合并多个视频文件为一个MP4视频（嵌入指定音频文件）【已优化：移除冗余width/height参数，自动提取基准分辨率】
        /// </summary>
        /// <param name="videoFilePaths">待合并的视频路径列表（按合并顺序排列）</param>
        /// <param name="audioPath">音频文件（可为null/空，为空则保留视频原音频（若有））</param>
        /// <param name="savePath">输出视频的保存路径</param>
        /// <param name="progress">进度回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>输出视频路径</returns>
        // 移除了 width = 1080 和 height = 1920 两个传入参数
        public async Task<string> MergeMultipleVideosAsync(
            List<DouyinMergeVideoDto> videoFilePaths,
            string audioPath,
            string savePath,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default)
        {
            List<DouyinMergeVideoDto> validVideoList = new List<DouyinMergeVideoDto>();
            List<string> tempUnifiedVideoPaths = new List<string>();

            // 输入验证
            if (videoFilePaths == null || !videoFilePaths.Any())
            {
                //Serilog.Log.Error("待合并视频列表为空。");
                return string.Empty;
            }

            // 筛选有效视频 + 修正Dto中的视频自身宽高为偶数（H264编码要求）
            foreach (var videoDto in videoFilePaths)
            {
                if (videoDto == null || string.IsNullOrEmpty(videoDto.Path) || !File.Exists(videoDto.Path))
                {
                    //Serilog.Log.Error($"视频文件未找到:{videoDto?.Path ?? "空路径"}");
                    continue;
                }

                // 修正视频自身宽高（无效值赋予类默认分辨率，偶数修正）
                videoDto.Width = videoDto.Width <= 0 ? this.VideoWidth : (videoDto.Width % 2 != 0 ? videoDto.Width + 1 : videoDto.Width);
                videoDto.Height = videoDto.Height <= 0 ? this.VideoHeight : (videoDto.Height % 2 != 0 ? videoDto.Height + 1 : videoDto.Height);

                validVideoList.Add(videoDto);
            }

            // 无有效视频直接返回
            if (!validVideoList.Any())
            {
                Serilog.Log.Error("无可用的有效视频文件，无法进行合并。");
                return string.Empty;
            }

            if (string.IsNullOrEmpty(savePath))
            {
                //Serilog.Log.Error("合并视频输出文件名为空");
                return string.Empty;
            }

            // 自动创建输出目录
            var saveDir = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(saveDir) && !Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }

            // 音频文件验证
            bool hasCustomAudio = !string.IsNullOrEmpty(audioPath) && File.Exists(audioPath);

            // 核心修改：提取第一个有效视频的分辨率作为基准输出分辨率（替代传入的width/height）
            // 进阶：动态计算最优分辨率（取所有有效视频的最大宽、最大高）
            int targetWidth = validVideoList.Max(v => v.Width);
            int targetHeight = validVideoList.Max(v => v.Height);
            // 再次确认偶数（双重保障，避免Dto修正逻辑遗漏）
            targetWidth = targetWidth % 2 != 0 ? targetWidth + 1 : targetWidth;
            targetHeight = targetHeight % 2 != 0 ? targetHeight + 1 : targetHeight;

            //Log.Information($"动态计算最优分辨率：{targetWidth}x{targetHeight}（适配所有视频）");

            // 步骤1：创建临时文件列表
            string tempListFile = Path.Combine(AppContext.BaseDirectory, "temp", $"{Guid.NewGuid()}.txt");
            var tempDir = Path.GetDirectoryName(tempListFile);
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            try
            {
                // 转封装所有有效视频为统一参数的临时视频（使用基准分辨率）
                List<string> unifiedVideoPaths = new List<string>();
                foreach (var videoDto in validVideoList)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        //Log.Error("视频合并流程被取消，终止临时视频转封装");
                        return string.Empty;
                    }

                    // 传入基准分辨率（替代原来的传入参数width/height）
                    string tempUnifiedVideo = await CreateUnifiedTempVideoAsync(videoDto, targetWidth, targetHeight, cancellationToken);
                    if (string.IsNullOrEmpty(tempUnifiedVideo) || !File.Exists(tempUnifiedVideo))
                    {
                        //Log.Error($"视频转封装失败，跳过该视频：{videoDto.Path}");
                        continue;
                    }

                    tempUnifiedVideoPaths.Add(tempUnifiedVideo);
                    unifiedVideoPaths.Add(tempUnifiedVideo);
                }

                if (!unifiedVideoPaths.Any())
                {
                    Log.Error("统一转码成功的视频为空，无法进行下一步合并操作");
                    return string.Empty;
                }

                // 生成FFmpeg识别的文件列表
                var fileListContent = new StringBuilder();
                foreach (var videoPath in unifiedVideoPaths)
                {
                    string escapedPath = videoPath.Replace("\\", "/").Replace("'", "\\'");
                    fileListContent.AppendLine($"file '{escapedPath}'");
                }
                File.WriteAllText(tempListFile, fileListContent.ToString(), new UTF8Encoding(false));

                // 构建FFmpeg合并参数（使用基准分辨率targetWidth/targetHeight）
                var arguments = new List<string>();

                if (hasCustomAudio)
                {
                    arguments.AddRange(new[]
                    {
                "-y", "-f", "concat", "-safe", "0",
                "-i", tempListFile, "-i", audioPath,
                "-map", "0:v", "-map", "1:a",
                "-shortest", "-filter:a", "apad=pad_len=0"
            });
                }
                else
                {
                    arguments.AddRange(new[]
                    {
                "-y", "-f", "concat", "-safe", "0",
                "-i", tempListFile,
                "-map", "0:v", "-map", "0:a?",
                "-shortest"
            });
                }

                // 核心：动态双向限制滤镜（不写死任何尺寸，使用传入的targetWidth/targetHeight）
                // scale={0}:{1}:force_original_aspect_ratio=decrease：双向限制不超出动态目标尺寸，保持原比例
                // pad={0}:{1}：动态填充到目标尺寸，无尺寸矛盾，无硬编码
                string videoFilter = string.Format(CultureInfo.InvariantCulture,
                    "scale={0}:{1}:force_original_aspect_ratio=decrease,pad={0}:{1}:(ow-iw)/2:(oh-ih)/2:black",
                    targetWidth,  // 动态传入的目标宽度（无硬编码）
                    targetHeight  // 动态传入的目标高度（无硬编码）
                );

                arguments.AddRange(new[] { "-vf", videoFilter });

                // ===== 视频编码参数（原 -c:v / -preset / -crf 三行替换为下面这一行，其他不动） =====
                AppendVideoEncoderArgs(arguments);

                // 后续编码参数、执行逻辑均不变（使用基准分辨率）
                arguments.AddRange(new[]
                {
            "-profile:v", "main"
        });
                arguments.AddRange(new[]
                {
            "-c:a", AudioCodec, "-b:a", AudioBitrate,
            "-ac", "2", "-ar", "44100"
        });

                arguments.AddRange(new[]
                {
            "-f", "mp4", "-movflags", "+faststart", savePath
        });
                AppendOutputPixelFormat(arguments);

                // 执行FFmpeg命令
                await ExecuteFFmpegAsync(arguments, progress, cancellationToken);

                // 验证输出文件
                if (File.Exists(savePath))
                {
                    Log.Information($"视频合成成功:{savePath}");
                    return savePath;
                }
                else
                {
                    Log.Error("视频合成失败，未生成输出文件。");
                    return string.Empty;
                }
            }
            finally
            {
                // 清理临时文件（原有逻辑不变）
                if (File.Exists(tempListFile))
                {
                    try { File.Delete(tempListFile); }
                    catch (IOException ex) { Log.Warning($"清理临时文件列表失败：{ex.Message}"); }
                }

                foreach (var tempVideoPath in tempUnifiedVideoPaths)
                {
                    if (File.Exists(tempVideoPath))
                    {
                        try { File.Delete(tempVideoPath); }
                        catch (IOException ex) { Log.Warning($"清理临时视频失败：{ex.Message}"); }
                    }
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

            _cancellationTokenSource?.Dispose();
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

            // ===== 新增：收集 stderr，便于报错时定位真实原因 =====
            var stderrBuilder = new System.Text.StringBuilder();

            _ffmpegProcess.ErrorDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;
                stderrBuilder.AppendLine(e.Data);   // ← 新增
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
                    throw new InvalidOperationException(
                        $"FFmpeg执行失败，退出码: {_ffmpegProcess.ExitCode}。\n" +
                        $"stderr:\n{stderrBuilder}\n" +   // ← 新增
                        $"执行命令：ffmpeg {string.Join(" ", arguments)}");
                }
            }
            finally
            {
                _ffmpegProcess?.Dispose();
                _ffmpegProcess = null;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
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
