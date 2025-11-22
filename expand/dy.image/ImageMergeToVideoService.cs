namespace dy.image
{

    /// <summary>
    /// 合并多张图片+音频为视频
    /// </summary>
    public class ImageMergeToVideoService
    {
        private readonly DownloadHelper _downloadHelper;
        private readonly FFmpegHelper _fFmpegHelper;
        public ImageMergeToVideoService(DownloadHelper downloadHelper, FFmpegHelper fFmpegHelper)
        {
            _downloadHelper = downloadHelper;
            _fFmpegHelper = fFmpegHelper;
        }
        public async Task<bool> MergeToVideo(string rootPath, MediaMergeRequest request,string outputVideoPath,string fileNamefolder)
        {

            try
            {
                // 创建唯一临时目录（避免并发冲突）
                var tempDir = Path.Combine(rootPath, "temp", Guid.NewGuid().ToString());
                try
                {
                    // 1. 下载图片
                    var (rawImages, error) = await DownloadMediaAsync(request.ImageUrls, Path.Combine(tempDir, "raw-images"), "image_", "webp");
                    if (!string.IsNullOrEmpty(error))
                    {
                        Serilog.Log.Error($"{error}");
                        return false;
                    }
                    else
                    {
                        for (int i = 0; i < rawImages.Count(); i++)
                        {
                            string sourcePath = rawImages[i];
                            // 重命名为有规律的文件名，如 temp_001.jpg, temp_002.png
                            string extension = Path.GetExtension(sourcePath);
                            string destFileName = $"temp_{i + 1:D3}{extension}"; // D3 确保是3位数字，不足补0
                            string destPath = Path.Combine(fileNamefolder, destFileName);
                            File.Copy(sourcePath, destPath);
                        }
                    }

                        // 2. 下载音频
                        var (rawAudios, audioError) = await DownloadMediaAsync(request.AudioUrls, Path.Combine(tempDir, "raw-audios"), "audio_", "mp3");
                    if (!string.IsNullOrEmpty(audioError))
                    {
                        Serilog.Log.Error($"{audioError}");
                        return false;
                    }
                    else
                    {
                        for (int i = 0; i < rawAudios.Count(); i++)
                        {
                            string sourcePath = rawAudios[i];
                            // 重命名为有规律的文件名，如 temp_001.mp3, temp_002.mp3
                            string extension = Path.GetExtension(sourcePath);
                            string destFileName = $"temp_{i + 1:D3}{extension}"; // D3 确保是3位数字，不足补0
                            string destPath = Path.Combine(fileNamefolder, destFileName);
                            File.Copy(sourcePath, destPath);
                        }
                    }


                    // 4. 合成视频
                    //var outputVideoPath = Path.Combine(tempDir, "output", $"merged-video.{request.OutputFormat.ToLower()}");

                    // 2. 创建帮助类实例
                    // 在Docker容器内，FFmpeg通常在PATH中，所以直接用 "ffmpeg" 即可

                    // 根据图片数量调整每张图片显示时长
                    if (request.ImageUrls.Count <= 3)
                    {
                        request.ImageDurationPerSecond = 5;
                    }
                    if (request.ImageUrls.Count > 20)
                    {
                        request.ImageDurationPerSecond = 2;
                    }
                    // 3. （可选）自定义视频参数
                    _fFmpegHelper.VideoWidth = 1080;
                    _fFmpegHelper.VideoHeight = 1920;
                    _fFmpegHelper.ImageDisplayDurationSeconds = request.ImageDurationPerSecond;
                    _fFmpegHelper.OutputFrameRate = 30;

                    // 4. 创建进度
                    var progress = new Progress<double>(p =>
                    {
                        Console.WriteLine($"进度: {p:F2}%");
                    });

                    // 5. 执行合成任务
                    using (var cancellationTokenSource = new CancellationTokenSource())
                    {
                        string resultPath = await _fFmpegHelper.CreateVideoFromImagesAndAudioAsync(
                            rawImages,
                            rawAudios[0],
                            outputVideoPath,
                            progress,
                            cancellationTokenSource.Token);

                        Console.WriteLine($"视频合成成功！文件已保存至: {resultPath}");
                    }
                }
                finally
                {
                    // 清理临时目录（无论成功失败）
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, recursive: true);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"{ex.StackTrace}");
                return false;
            }
        }

        /// <summary>通用媒体下载方法</summary>
        private async Task<(string[] SuccessPaths, string ErrorMsg)> DownloadMediaAsync(
            List<string> urls, string saveDir, string prefix, string ext)
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
                    await _downloadHelper.DownloadFileAsync(url, savePath);
                    successPaths.Add(savePath);
                   Console.WriteLine($"下载成功：{url} → {savePath}");
                }
                catch (Exception ex)
                {
                    var error = $"下载失败：{url}，错误：{ex.Message}";
                    Console.WriteLine(error);
                    return (Array.Empty<string>(), error);
                }
            }
            return (successPaths.ToArray(), null);
        }

    }
}
