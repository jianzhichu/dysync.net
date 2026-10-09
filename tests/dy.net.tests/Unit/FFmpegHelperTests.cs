using dy.net;
using dy.net.model.dto;
using dy.net.utils;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Xunit;

namespace dy.net.tests.Unit;

/// <summary>
/// FFmpegHelper 单元测试（覆盖 919e067 引入的硬件加速探测与编码参数组装逻辑）。
/// 说明：
/// - 仅覆盖不依赖真实硬件编码器的路径：CI（ubuntu/windows）与 Apple Silicon 均无 QSV/NVENC，
///   构造探测确定收敛为软编回退（None）；
/// - QSV/NVENC 参数分支经反射设置 _hwAccelType 后验证（避免为本 PR 改动产品代码可见性，
///   相关设计问题见 issue #40 / #41）；
/// - FFmpegHelper 构造函数读取 Appsettings.Get("deploy")，测试先以内存配置初始化静态
///   Configuration（deploy 未配置 → 走操作系统分支，与 Docker fn 分支相对）。
/// </summary>
public class FFmpegHelperTests : IDisposable
{
    private readonly FFmpegHelper _helper;
    private readonly List<string> _tempFiles = new();

    public FFmpegHelperTests()
    {
        new Appsettings(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build());
        _helper = new FFmpegHelper();
    }

    public void Dispose()
    {
        _helper.Dispose();
        foreach (var f in _tempFiles)
        {
            try { if (File.Exists(f)) File.Delete(f); } catch { /* 清理失败不影响测试结果 */ }
        }
    }

    private string CreateTempFile()
    {
        var path = Path.GetTempFileName();
        _tempFiles.Add(path);
        return path;
    }

    // ===== 反射辅助：仅用于触达私有状态，不构成对产品代码的依赖契约 =====

    private static HardwareAccelerationType GetHwAccelType(FFmpegHelper helper)
        => (HardwareAccelerationType)GetHwAccelField().GetValue(helper);

    private static void SetHwAccelType(FFmpegHelper helper, HardwareAccelerationType type)
        => GetHwAccelField().SetValue(helper, type);

    private static FieldInfo GetHwAccelField()
        => typeof(FFmpegHelper).GetField("_hwAccelType", BindingFlags.NonPublic | BindingFlags.Instance);

    private static List<string> InvokeAppendVideoEncoderArgs(FFmpegHelper helper)
    {
        var args = new List<string>();
        typeof(FFmpegHelper)
            .GetMethod("AppendVideoEncoderArgs", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(helper, new object[] { args });
        return args;
    }

    #region 硬件探测回退

    [Fact]
    public void Constructor_ProbeFallsBackToSoftware_WithoutHardwareEncoder()
    {
        // CI 双平台与 Apple Silicon 均无 h264_qsv/h264_nvenc 可用编码器，
        // 探测应回退到软编（None），而不是抛异常或停留未知状态
        Assert.Equal(HardwareAccelerationType.None, GetHwAccelType(_helper));
    }

    [Fact]
    public void DetectHardwareAcceleration_ShortCircuits_WhenDisabled()
    {
        // 先探测成功（假的）再关闭开关并显式重新探测：应直接短路为 None、不再 spawn 进程。
        // 注：正常运行时该属性在构造后设置不生效（issue #41），此处经反射显式重调方法验证守卫分支本身
        SetHwAccelType(_helper, HardwareAccelerationType.Qsv);
        _helper.UseHardwareAcceleration = false;

        typeof(FFmpegHelper)
            .GetMethod("DetectHardwareAcceleration", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(_helper, null);

        Assert.Equal(HardwareAccelerationType.None, GetHwAccelType(_helper));
    }

    #endregion

    #region AppendVideoEncoderArgs 编码参数分支

    [Fact]
    public void AppendVideoEncoderArgs_SoftwareFallback_UsesLibx264CrfPreset()
    {
        SetHwAccelType(_helper, HardwareAccelerationType.None);

        var args = InvokeAppendVideoEncoderArgs(_helper);

        Assert.Equal(new[] { "-c:v", "libx264", "-preset", "medium", "-crf", "23" }, args);
    }

    [Fact]
    public void AppendVideoEncoderArgs_Qsv_UsesGlobalQualityInsteadOfCrf()
    {
        SetHwAccelType(_helper, HardwareAccelerationType.Qsv);

        var args = InvokeAppendVideoEncoderArgs(_helper);

        Assert.Equal(new[] { "-c:v", "h264_qsv", "-preset", "medium", "-global_quality", "23" }, args);
    }

    [Fact]
    public void AppendVideoEncoderArgs_Nvenc_UsesVbrCq()
    {
        SetHwAccelType(_helper, HardwareAccelerationType.Nvenc);

        var args = InvokeAppendVideoEncoderArgs(_helper);

        Assert.Equal(
            new[] { "-c:v", "h264_nvenc", "-preset", "p4", "-rc", "vbr", "-cq", "23", "-b:v", "0" },
            args);
    }

    [Fact]
    public void AppendVideoEncoderArgs_RespectsConfiguredCrf()
    {
        // VideoCrf 可配置，QSV/NVENC 分支应使用该值而非写死 23
        _helper.VideoCrf = 28;
        SetHwAccelType(_helper, HardwareAccelerationType.Nvenc);

        var args = InvokeAppendVideoEncoderArgs(_helper);

        Assert.Contains("-cq", args);
        Assert.Equal("28", args[args.IndexOf("-cq") + 1]);
    }

    #endregion

    #region MergeMultipleVideosAsync 入参与状态修正

    [Fact]
    public async Task MergeMultipleVideosAsync_ReturnsEmpty_WhenListNull()
    {
        Assert.Equal(string.Empty, await _helper.MergeMultipleVideosAsync(null, null, "out.mp4"));
    }

    [Fact]
    public async Task MergeMultipleVideosAsync_ReturnsEmpty_WhenListEmpty()
    {
        Assert.Equal(string.Empty, await _helper.MergeMultipleVideosAsync(new List<DouyinMergeVideoDto>(), null, "out.mp4"));
    }

    [Fact]
    public async Task MergeMultipleVideosAsync_ReturnsEmpty_WhenAllFilesMissing()
    {
        // 全部文件不存在：过滤后无有效视频，应静默返回空串而非抛异常
        var list = new List<DouyinMergeVideoDto>
        {
            new() { Path = "/not/exists/a.mp4", Width = 1080, Height = 1920 },
            new() { Path = "/not/exists/b.mp4", Width = 720, Height = 1280 },
        };

        Assert.Equal(string.Empty, await _helper.MergeMultipleVideosAsync(list, null, "out.mp4"));
    }

    [Fact]
    public async Task MergeMultipleVideosAsync_ReturnsEmpty_WhenSavePathEmpty()
    {
        // 存在有效文件但保存路径为空：应在转封装之前返回空串
        var path = CreateTempFile();
        var list = new List<DouyinMergeVideoDto> { new() { Path = path, Width = 1080, Height = 1920 } };

        Assert.Equal(string.Empty, await _helper.MergeMultipleVideosAsync(list, null, string.Empty));
    }

    [Fact]
    public async Task MergeMultipleVideosAsync_NormalizesOddDimensionsToEven()
    {
        // H264 要求宽高为偶数：入参 Dto 的奇数宽高在进入转封装前即被修正（+1）。
        // 垃圾临时文件会让 ffmpeg 转封装失败被跳过、整体返回空串，但 Dto 修正发生在失败之前，可观察
        var path = CreateTempFile();
        var dto = new DouyinMergeVideoDto { Path = path, Width = 1081, Height = 1919 };
        var list = new List<DouyinMergeVideoDto> { dto };

        var result = await _helper.MergeMultipleVideosAsync(list, null, Path.Combine(Path.GetTempPath(), "out.mp4"));

        Assert.Equal(string.Empty, result);
        Assert.Equal(1082, dto.Width);
        Assert.Equal(1920, dto.Height);
    }

    #endregion

    #region CreateVideoFromImagesAndAudioAsync 输入校验

    [Fact]
    public async Task CreateVideoFromImagesAndAudioAsync_Throws_WhenImagesNull()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _helper.CreateVideoFromImagesAndAudioAsync(null, "a.mp3", "out.mp4"));
    }

    [Fact]
    public async Task CreateVideoFromImagesAndAudioAsync_Throws_WhenImagesEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _helper.CreateVideoFromImagesAndAudioAsync(new List<DouyinMergeVideoDto>(), "a.mp3", "out.mp4"));
    }

    [Fact]
    public async Task CreateVideoFromImagesAndAudioAsync_Throws_WhenAudioMissing()
    {
        // 音频校验先于逐图校验：图片列表非空即可命中音频缺失分支
        var list = new List<DouyinMergeVideoDto> { new() { Path = "/any/img.jpg", Width = 1080, Height = 1920 } };

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _helper.CreateVideoFromImagesAndAudioAsync(list, "/not/exists/a.mp3", "out.mp4"));
    }

    [Fact]
    public async Task CreateVideoFromImagesAndAudioAsync_Throws_WhenImageDimensionsInvalid()
    {
        var audio = CreateTempFile();
        var image = CreateTempFile();
        var list = new List<DouyinMergeVideoDto> { new() { Path = image, Width = 0, Height = 1920 } };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _helper.CreateVideoFromImagesAndAudioAsync(list, audio, "out.mp4"));
    }

    #endregion
}
