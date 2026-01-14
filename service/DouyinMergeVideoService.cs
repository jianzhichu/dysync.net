using ClockSnowFlake;
using dy.net.dto;
using dy.net.utils;
using Serilog;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace dy.net.service
{

    /// <summary>
    /// 合并多张图片+音频为视频
    /// </summary>
    public class DouyinMergeVideoService
    {
        private readonly DouyinHttpClientService douyinHttpClientService;

        public DouyinMergeVideoService(DouyinHttpClientService douyinHttpClientService)
        {
            this.douyinHttpClientService = douyinHttpClientService;
        }

        // 1. 并发控制：限制同时运行的 FFmpeg 进程数（建议设为 1，FFmpeg 单进程更稳定）
        private readonly SemaphoreSlim _ffmpegSemaphore = new SemaphoreSlim(1, 1);
        // 重试次数配置
        private const int RetryCount = 3;
        // 重试间隔（毫秒）
        private const int RetryDelay = 1000;


        /// <summary>
        /// 多视频合成一个视频
        /// </summary>
        /// <param name="videoFilePaths"></param>
        /// <param name="audioPath"></param>
        /// <param name="savePath"></param>
        /// <param name="ck"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public async Task<string> MergeMultipleVideosAsync(
           List<string> videoFilePaths,
           string audioPath,
           string savePath,
           string ck,
           int width = 1080,
           int height = 1920)
        {
            string mergMusicPath = GetRandomMergeMusic();
            if (!string.IsNullOrWhiteSpace(audioPath))
            {
                var (SuccessPaths, _) = await DownloadMediaAsync(new List<string> { audioPath }, Path.GetDirectoryName(savePath), "audio_", "mp3", ck);
                if (SuccessPaths != null && SuccessPaths.Length > 0)
                {
                    mergMusicPath = SuccessPaths[0];
                }
            }
            var ffmpeg = new FFmpegHelper();
            return await ffmpeg.MergeMultipleVideosAsync(videoFilePaths, mergMusicPath, savePath, width, height);
        }




        private static string GetRandomMergeMusic()
        {
            var allowedExtensions = new HashSet<string> { ".mp3", ".wav" };
            string mergMusic = Path.Combine(AppContext.BaseDirectory, "mp3", "silent_10.mp3");//默认音频
            var customMusics = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "mp3"))
              .Where(filePath =>
                  allowedExtensions.Contains(Path.GetExtension(filePath).ToLowerInvariant()) &&
                  Path.GetFileNameWithoutExtension(filePath) != "silent_10")
              .ToList();

            if (customMusics != null && customMusics.Any()) 
            {
                if (customMusics.Count() == 1)
                    return customMusics.FirstOrDefault();

                // 使用加密级别的随机数生成器（RNGCryptoServiceProvider）获取高随机性的索引
                using (var rng = RandomNumberGenerator.Create())
                {
                    byte[] randomNumber = new byte[4];
                    rng.GetBytes(randomNumber);

                    // 将随机字节转换为非负整数，并取模得到有效索引
                    int randomIndex = Math.Abs(BitConverter.ToInt32(randomNumber, 0)) % customMusics.Count();

                    return customMusics.ToList()[randomIndex];
                }
            }

            return mergMusic;
        }


        /// <summary>
        /// 视频合成（优化后：解决 FFmpeg 进程冲突，支持容错重试）
        /// </summary>
        public async Task<bool> MergeToVideo(string cookie, string rootPath, MediaMergeRequest request,
            string outputVideoPath, string fileNamefolder, bool mergeImg2Viedo, bool downImage = false, bool downMp3 = false)
        {
            // 输入参数校验（避免无效执行）
            if (request == null || request.ImageUrls == null || request.ImageUrls.Count == 0)
            {
                Log.Error("图片URL列表为空，无法合成视频");
                return false;
            }
            //if (mergeImg2Viedo && (request.AudioUrls == null || request.AudioUrls.Count == 0))
            //{
            //    Log.Error("合成视频时音频URL列表为空");
            //    return false;
            //}

            string tempDir = null;
            try
            {
                // 创建唯一临时目录（避免并发冲突）
                tempDir = Path.Combine(rootPath, "temp", Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir); // 确保目录存在

                // 1. 下载图片
                var (rawImages, imageError) = await DownloadMediaAsync(
                    request.ImageUrls, Path.Combine(tempDir, "raw-images"), "image_", "webp", cookie);
                if (!string.IsNullOrEmpty(imageError) || rawImages == null || rawImages.Length == 0)
                {
                    Log.Error($"图片下载失败：{imageError ?? "未下载到任何图片"}");
                    return false;
                }
                // 保存下载的图片（如果需要）
                if (downImage)
                {
                    await SaveDownloadedFilesAsync(rawImages, fileNamefolder, "jpg",Path.GetFileNameWithoutExtension(outputVideoPath));
                }

                // 2. 下载音频
                string[] rawAudios = Array.Empty<string>();
                if (mergeImg2Viedo && request.AudioUrls != null && request.AudioUrls.Count > 0) 
                {
                    var (audios, audioError) = await DownloadMediaAsync(
                        request.AudioUrls, Path.Combine(tempDir, "raw-audios"), "audio_", "mp3", cookie);
                    if (!string.IsNullOrEmpty(audioError) || audios == null || audios.Length == 0)
                    {
                        Log.Error($"音频下载失败：{audioError ?? "未下载到任何音频"}");
                        return false;
                    }
                    rawAudios = audios;
                    // 保存下载的音频（如果需要）
                    if (downMp3)
                    {
                        var ext= Path.GetExtension(rawAudios[0]);
                        await SaveDownloadedFilesAsync(rawAudios, fileNamefolder, ext, Path.GetFileNameWithoutExtension(outputVideoPath));
                    }
                }

                // 不合成视频，直接返回成功
                if (!mergeImg2Viedo)
                {
                    Log.Debug($"根据系统配置设置不下载图文视频-[{outputVideoPath}],{(downImage?"下载图片":"不下载图片")},{(downMp3?"下载音频":"不下载音频")}");
                    return true;
                }

                // 3. 调整图片显示时长
                AdjustImageDuration(request);

                // 4. 容错重试：处理 FFmpeg 进程冲突等临时错误
                bool mergeSuccess = await RetryOnFfmpegConflictAsync(async () =>
                {
                    // 并发控制：等待前一个 FFmpeg 进程完成
                    await _ffmpegSemaphore.WaitAsync();
                    try
                    {
                        // 每次合成创建独立的 FFmpegHelper 实例（避免状态共享）
                        var ffmpegHelper = new FFmpegHelper();
                        // 配置视频参数（独立实例，无共享冲突）
                        ffmpegHelper.VideoWidth = request.VideoWidth > 0 ? request.VideoWidth : 1080;
                        ffmpegHelper.VideoHeight = request.VideoHeight > 0 ? request.VideoHeight : 1920;
                        ffmpegHelper.ImageDisplayDurationSeconds = request.ImageDurationPerSecond;
                        ffmpegHelper.OutputFrameRate = 30;

                        // 进度回调
                        var progress = new Progress<double>(p =>
                        {
                            Log.Debug($"视频合成进度：{p:F2}%");
                        });

                        // 超时控制：避免 FFmpeg 进程无限运行（300秒=5分钟）
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
                        // 执行合成（确保每个图片都参与）

                        if (rawAudios.Length == 0)
                        {
                            var musicPath = GetRandomMergeMusic();
                            rawAudios = new string[] { musicPath };
                            Log.Debug("版权原因无法下载音频，使用默认无声音频文件");
                        }

                        string resultPath = await ffmpegHelper.CreateVideoFromImagesAndAudioAsync(
                            rawImages,          // 所有下载的图片（确保无遗漏）
                            rawAudios[0],       // 取第一个音频（可根据需求调整）
                            outputVideoPath,
                            ffmpegHelper.VideoWidth,
                            ffmpegHelper.VideoHeight,
                            progress,
                            cts.Token);

                        Log.Debug($"视频合成成功！文件路径：{resultPath}");
                        return !string.IsNullOrEmpty(resultPath) && File.Exists(resultPath);
                    }
                    finally
                    {
                        // 释放信号量，允许下一个任务执行
                        _ffmpegSemaphore.Release();
                    }
                });

                return mergeSuccess;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "视频合成过程中发生未处理异常");
                return false;
            }
            finally
            {
                // 安全清理临时目录（确保 FFmpeg 进程已释放文件句柄）
                await SafeCleanTempDirAsync(tempDir);
            }
        }

        /// <summary>
        /// 保存下载的文件（图片/音频）到目标目录
        /// </summary>
        private static async Task SaveDownloadedFilesAsync(string[] sourcePaths, string targetFolder,  string defaultExt,string videoFileName)
        {
            if (sourcePaths == null || sourcePaths.Length == 0) return;
            Directory.CreateDirectory(targetFolder); // 确保目标目录存在

            // 异步保存（不阻塞主线程）
            await Task.WhenAll(sourcePaths.Select(async (sourcePath, index) =>
            {
                if (!File.Exists(sourcePath)) return;
                string extension = Path.GetExtension(sourcePath) ?? $".{defaultExt}";
                string destFileName = $"{videoFileName}{index + 1:D3}{extension}";
                string destPath = Path.Combine(targetFolder, destFileName);

                // 文件不存在时才复制，避免覆盖
                if (!File.Exists(destPath))
                {
                    await Task.Run(() => File.Copy(sourcePath, destPath, overwrite: false));
                    Log.Debug($"已保存文件：{destPath}");
                }
            }));
        }

        /// <summary>
        /// FFmpeg 进程冲突时重试
        /// </summary>
        private async Task<bool> RetryOnFfmpegConflictAsync(Func<Task<bool>> action)
        {
            int retryCount = 0;
            while (retryCount < RetryCount)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    // 识别 FFmpeg 进程冲突相关异常（根据实际异常信息调整条件）
                    if (ex.Message.Contains("FFmpeg 进程正在运行") ||
                        ex.Message.Contains("进程已在运行") ||
                        ex.Message.Contains("文件被另一个进程占用"))
                    {
                        retryCount++;
                        Log.Warning($"FFmpeg 进程冲突，第 {retryCount}/{RetryCount} 次重试... 异常信息：{ex.Message}");
                        await Task.Delay(RetryDelay * retryCount); // 重试间隔递增
                        continue;
                    }
                    // 非进程冲突异常，直接抛出
                    throw;
                }
            }
            Log.Error($"FFmpeg 进程冲突，重试 {RetryCount} 次后仍失败");
            return false;
        }

        /// <summary>
        /// 安全清理临时目录（避免文件被占用）
        /// </summary>
        private async Task SafeCleanTempDirAsync(string tempDir)
        {
            if (string.IsNullOrEmpty(tempDir) || !Directory.Exists(tempDir))
                return;

            try
            {
                // 延迟清理：给 FFmpeg 进程足够时间释放文件句柄（1秒）
                await Task.Delay(1000);
                Directory.Delete(tempDir, recursive: true);
                //Log.Debug($"临时目录已清理：{tempDir}");
            }
            catch (Exception ex)
            {
                // 清理失败时备份目录，避免占用磁盘空间
                string backupDir = $"{tempDir}_backup_{Guid.NewGuid().ToString("N")}";
                Directory.Move(tempDir, backupDir);
                Log.Error(ex, $"临时目录清理失败，已备份至：{backupDir}");
            }
        }
        /// <summary>
        /// 调整图片显示时长
        /// </summary>
        private static void AdjustImageDuration(MediaMergeRequest request)
        {
            if (request.ImageUrls.Count <= 3)
                request.ImageDurationPerSecond = 3;
            else if (request.ImageUrls.Count > 20)
                request.ImageDurationPerSecond = 2;
            // 中间数量保持原有配置
        }


        /// <summary>通用媒体下载方法</summary>
        private async Task<(string[] SuccessPaths, string ErrorMsg)> DownloadMediaAsync(
            List<string> urls, string saveDir, string prefix, string ext,string cookie)
        {
            var successPaths = new List<string>();
            for (var i = 0; i < urls.Count; i++)
            {
                var url = urls[i];
                var fileExt = ext ?? Path.GetExtension(url).TrimStart('.') ?? "png";
                var fileName = $"{prefix}{i + 1}.{fileExt}";
                var savePath = Path.Combine(saveDir, fileName);

                try
                {
                   var (Success, ActualSavePath) = await douyinHttpClientService.DownloadAsync(url, savePath, cookie);
                    if(Success)
                    {
                        successPaths.Add(ActualSavePath);
                    }
                    else
                    {
                        Serilog.Log.Error($"下载失败：{url} → {savePath}");
                    }
                   //Console.WriteLine($"下载成功：{url} → {savePath}");
                }
                catch (Exception ex)
                {
                    var error = $"下载失败：{url}，错误：{ex.Message}";
                    Serilog.Log.Error(error);
                    return (Array.Empty<string>(), error);
                }
            }
            return (successPaths.ToArray(), null);
        }

    }
}
