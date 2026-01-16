using dy.net.dto;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dy.net.utils
{
    public class FFmpegHelper : IDisposable
    {
        private string _ffmpegExecutablePath;
        private string _ffprobeExecutablePath;

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
        /// 将单个视频转封装为统一编码/分辨率的临时视频（解决concat合并兼容问题）
        /// </summary>
        /// <param name="videoDto">待转封装的视频Dto</param>
        /// <param name="targetWidth">目标输出宽度</param>
        /// <param name="targetHeight">目标输出高度</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>统一参数后的临时视频路径</returns>
        private async Task<string> CreateUnifiedTempVideoAsync(
            DouyinDynamicVideoDto videoDto,
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
            //Log.Debug($"待执行的动态滤镜（适配不固定尺寸）：{videoFilter}");

            // 4. 构建FFmpeg参数（无需对滤镜做任何转义，保留原有编码配置）
            var arguments = new List<string>
    {
        "-y",
        "-hide_banner",
        "-loglevel", "error",
        "-i", videoDto.Path,
        "-vf", // 滤镜参数标识（独立参数）
        videoFilter, // 动态滤镜字符串（独立参数，ArgumentList自动处理逗号）
        "-c:v", VideoCodec,
        "-preset", VideoPreset,
        "-crf", $"{VideoCrf}",
        "-pix_fmt", "yuv420p",
        "-profile:v", "main",
        // 关键帧计算也适配动态目标宽，保留原有逻辑
        "-g", $"{(targetWidth < 1080 ? 60 : 120)}",
        "-sc_threshold", "0",
        "-c:a", AudioCodec,
        "-b:a", AudioBitrate,
        "-ac", "2",
        "-ar", "44100",
        "-strict", "-2",
        "-f", "mp4",
        "-movflags", "+faststart",
        tempVideoPath
    };

            try
            {
                // 5. 复用现有ExecuteFFmpegAsync（已使用ArgumentList，自动处理特殊字符）
                await ExecuteFFmpegAsync(arguments, null, cancellationToken);

                if (File.Exists(tempVideoPath))
                {
                    //Log.Debug($"视频转封装为统一参数成功，临时路径：{tempVideoPath}");
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
            List<DouyinDynamicVideoDto> videoFilePaths,
            string audioPath,
            string savePath,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default)
        {
            List<DouyinDynamicVideoDto> validVideoList = new List<DouyinDynamicVideoDto>();
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

            //Log.Debug($"动态计算最优分辨率：{targetWidth}x{targetHeight}（适配所有视频）");

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

                // 后续编码参数、执行逻辑均不变（使用基准分辨率）
                arguments.AddRange(new[]
                {
            "-c:v", VideoCodec, "-preset", VideoPreset, "-crf", $"{VideoCrf}",
            "-pix_fmt", "yuv420p", "-profile:v", "main"
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

                // 执行FFmpeg命令
                await ExecuteFFmpegAsync(arguments, progress, cancellationToken);

                // 验证输出文件
                if (File.Exists(savePath))
                {
                    Log.Debug($"视频合成成功:{savePath}");
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