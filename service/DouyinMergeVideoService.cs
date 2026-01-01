using ClockSnowFlake;
using dy.net.dto;
using dy.net.utils;
using Serilog;

namespace dy.net.service
{

    /// <summary>
    /// 合并多张图片+音频为视频
    /// </summary>
    public class DouyinMergeVideoService
    {
        private readonly FFmpegHelper _fFmpegHelper;
        private readonly DouyinHttpClientService douyinHttpClientService;

        public DouyinMergeVideoService(FFmpegHelper fFmpegHelper,DouyinHttpClientService douyinHttpClientService)
        {
            _fFmpegHelper = fFmpegHelper;
            this.douyinHttpClientService = douyinHttpClientService;
        }

        // 1. 并发控制：限制同时运行的 FFmpeg 进程数（建议设为 1，FFmpeg 单进程更稳定）
        private readonly SemaphoreSlim _ffmpegSemaphore = new SemaphoreSlim(1, 1);
        // 重试次数配置
        private const int RetryCount = 3;
        // 重试间隔（毫秒）
        private const int RetryDelay = 1000;

        /// <summary>
        /// 视频合成
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="rootPath"></param>
        /// <param name="request"></param>
        /// <param name="outputVideoPath"></param>
        /// <param name="fileNamefolder"></param>
        /// <param name="mergeImg2Viedo"></param>
        /// <param name="downImage"></param>
        /// <param name="downMp3"></param>
        /// <returns></returns>
        //public async Task<bool> MergeToVideo(string cookie,string rootPath, MediaMergeRequest request,string outputVideoPath,string fileNamefolder,bool mergeImg2Viedo,bool downImage=false,bool downMp3=false)
        //{

        //    try
        //    {
        //        // 创建唯一临时目录（避免并发冲突）
        //        var tempDir = Path.Combine(rootPath, "temp", Guid.NewGuid().ToString());
        //        try
        //        {
        //            // 1. 下载图片
        //            var (rawImages, error) = await DownloadMediaAsync(request.ImageUrls, Path.Combine(tempDir, "raw-images"), "image_", "webp",cookie);
        //            if (!string.IsNullOrEmpty(error))
        //            {
        //                Serilog.Log.Error($"{error}");
        //                return false;
        //            }
        //            else
        //            {
        //                if(downImage)
        //                {
        //                    for (int i = 0; i < rawImages.Length; i++)
        //                    {
        //                        string sourcePath = rawImages[i];
        //                        // 重命名为有规律的文件名，如 temp_001.jpg, temp_002.png
        //                        string extension = Path.GetExtension(sourcePath);
        //                        string destFileName = $"temp_{i + 1:D3}{extension}"; // D3 确保是3位数字，不足补0
        //                        string destPath = Path.Combine(fileNamefolder, destFileName);
        //                        if (destPath.Contains("小可爱") || sourcePath.Contains("小可爱")) { 
        //                                Console.WriteLine("发现小可爱图片");
        //                        }
        //                        if (!File.Exists(destPath))
        //                            File.Copy(sourcePath, destPath);
        //                    }
        //                }
        //            }
        //                // 2. 下载音频
        //                var (rawAudios, audioError) = await DownloadMediaAsync(request.AudioUrls, Path.Combine(tempDir, "raw-audios"), "audio_", "mp3", cookie);
        //            if (!string.IsNullOrEmpty(audioError))
        //            {
        //                Serilog.Log.Error($"{audioError}");
        //                return false;
        //            }
        //            else
        //            {
        //                if(downMp3)
        //                {
        //                    for (int i = 0; i < rawAudios.Length; i++)
        //                    {
        //                        string sourcePath = rawAudios[i];
        //                        // 重命名为有规律的文件名，如 temp_001.mp3, temp_002.mp3
        //                        string extension = Path.GetExtension(sourcePath);
        //                        string destFileName = $"temp_{i + 1:D3}{extension}"; // D3 确保是3位数字，不足补0
        //                        string destPath = Path.Combine(fileNamefolder, destFileName);
        //                        if (!File.Exists(destPath))
        //                            File.Copy(sourcePath, destPath);
        //                    }
        //                }
        //            }
        //            if (!mergeImg2Viedo)
        //            {
        //                // 不合成视频，直接返回成功
        //                Serilog.Log.Debug($"不合成视频，直接返回");
        //                return true;
        //            }

        //            // 4. 合成视频
        //            //var outputVideoPath = Path.Combine(tempDir, "output", $"merged-video.{request.OutputFormat.ToLower()}");

        //            // 2. 创建帮助类实例
        //            // 在Docker容器内，FFmpeg通常在PATH中，所以直接用 "ffmpeg" 即可

        //            // 根据图片数量调整每张图片显示时长
        //            if (request.ImageUrls.Count <= 3)
        //            {
        //                request.ImageDurationPerSecond = 3;
        //            }
        //            if (request.ImageUrls.Count > 20)
        //            {
        //                request.ImageDurationPerSecond = 2;
        //            }
        //            // 3. （可选）自定义视频参数
        //            _fFmpegHelper.VideoWidth = 1080;
        //            _fFmpegHelper.VideoHeight = 1920;
        //            _fFmpegHelper.ImageDisplayDurationSeconds = request.ImageDurationPerSecond;
        //            _fFmpegHelper.OutputFrameRate = 30;

        //            // 4. 创建进度
        //            var progress = new Progress<double>(p =>
        //            {
        //                Console.WriteLine($"进度: {p:F2}%");
        //            });

        //            // 5. 执行合成任务
        //            using (var cancellationTokenSource = new CancellationTokenSource())
        //            {
        //                string resultPath = await _fFmpegHelper.CreateVideoFromImagesAndAudioAsync(
        //                    rawImages,
        //                    rawAudios[0],
        //                    outputVideoPath,
        //                    request.VideoWidth,
        //                    request.VideoHeight,
        //                    progress,
        //                    cancellationTokenSource.Token);

        //                //Console.WriteLine($"视频合成成功！文件已保存至: {resultPath}");
        //                Serilog.Log.Debug($"视频合成成功！文件已保存至: {resultPath}");
        //            }
        //        }
        //        finally
        //        {
        //            // 清理临时目录（无论成功失败）
        //            if (Directory.Exists(tempDir))
        //            {
        //                Directory.Delete(tempDir, recursive: true);
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Serilog.Log.Error($"{ex.StackTrace}");
        //        return false;
        //    }
        //}


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
                if (mergeImg2Viedo&& request.AudioUrls != null && request.AudioUrls.Count>0)
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
                        await SaveDownloadedFilesAsync(rawAudios, fileNamefolder, "mp3", Path.GetFileNameWithoutExtension(outputVideoPath));
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
                            rawAudios= new string[] { Path.Combine(AppContext.BaseDirectory,"mp3", "silent_10.mp3") };
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
        private void AdjustImageDuration(MediaMergeRequest request)
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
                    await douyinHttpClientService.DownloadAsync(url, savePath, cookie);
                    successPaths.Add(savePath);
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
