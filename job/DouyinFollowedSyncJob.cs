using ClockSnowFlake;
using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.model.response;
using dy.net.service;
using dy.net.utils;

namespace dy.net.job
{
    public class DouyinFollowedSyncJob : DouyinBasicSyncJob
    {
        public DouyinFollowedSyncJob(DouyinCookieService douyinCookieService, DouyinHttpClientService douyinHttpClientService, DouyinVideoService douyinVideoService, DouyinCommonService douyinCommonService, DouyinFollowService douyinFollowService, DouyinMergeVideoService douyinMergeVideoService, DouyinCollectCateService douyinCollectCateService) : base(douyinCookieService, douyinHttpClientService, douyinVideoService, douyinCommonService, douyinFollowService, douyinMergeVideoService, douyinCollectCateService)
        {
        }

        protected override VideoTypeEnum VideoType => VideoTypeEnum.dy_follows;

        protected override async Task<List<DouyinCookie>> GetSyncCookies()
        {
            return await douyinCookieService.GetOpendCookiesAsync(x => !string.IsNullOrWhiteSpace(x.UpSavePath));
        }

        protected override async Task<DouyinVideoInfoResponse> FetchVideoData(DouyinCookie cookie, string cursor, DouyinFollowed followed, DouyinCollectCate cate)
        {
            return await douyinHttpClientService.SyncUpderPostVideos(count, cursor, followed.SecUid, cookie.Cookies);
        }

        //protected override bool ShouldContinueSync(DouyinCookie cookie, DouyinVideoInfoResponse data, DouyinFollowed followed,AppConfig config)
        //{
        //    return data != null && data.HasMore == 1 && cookie.UperSyncd == 0 && followed.FullSync;
        //}
        protected override string GetAuthorAvatarBasePath(DouyinCookie cookie)
        {
            return Path.Combine(cookie.UpSavePath, "author");
        }

        /// <summary>
        /// 关注用户特殊处理文件夹存储路径，用户可自定义保存路径
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="item"></param>
        /// <param name="followed"></param>
        /// <param name="cate"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        protected override string CreateSaveFolder(DouyinCookie cookie, Aweme item, AppConfig config, DouyinFollowed followed, DouyinCollectCate cate)
        {
            #region 默认使用UP主名称作为文件夹名称，若关注列表中有自定义保存路径则使用自定义路径
            // 1. 优先获取有效的作者名称（遵循原有优先级：followed.UperName > item.Author.Nickname > 默认值）
            var rawAuthorName = followed?.UperName ?? item?.Author?.Nickname;
            var authorName = string.IsNullOrWhiteSpace(rawAuthorName)
                ? "未知博主"
                : DouyinFileNameHelper.SanitizeLinuxFileName(rawAuthorName, "", true);
            // 2. 确定最终文件夹路径（遵循原有优先级：followed.SavePath > authorName > 基础路径）
            var targetFolderName = !string.IsNullOrWhiteSpace(followed?.SavePath) ? followed.SavePath : authorName;
            var rootFolder = Path.Combine(cookie.UpSavePath, targetFolderName);

            if (!Directory.Exists(rootFolder)) Directory.CreateDirectory(rootFolder);
            #endregion

            var sampleName = DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId, true);
            var (existingName, _) = douyinVideoService.GetUperLastViedoFileName(item.Author.Uid, sampleName);
            var fileNameFolder = string.IsNullOrWhiteSpace(existingName) ? sampleName : existingName;
            return Path.Combine(rootFolder, fileNameFolder);
        }
        /// <summary>
        /// 关注的视频，生成文件名称
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="item"></param>
        /// <param name="config"></param>
        /// <param name="cate"></param>
        /// <returns></returns>
        protected override string GetVideoFileName(DouyinCookie cookie, Aweme item, AppConfig config, DouyinCollectCate cate)
        {

            string Format = "mp4";
            string FileHash = "";
            string Height = "";
            string Width = "";

            if (item.Video != null && item.Video.BitRate != null)
            {
                var bitrate = item.Video.BitRate.FirstOrDefault();
                Format = bitrate.Format;
                FileHash = bitrate.PlayAddr.FileHash;
                Height = bitrate.PlayAddr.Height.ToString();
                Width = bitrate.PlayAddr.Width.ToString();
            }
            else
            {
                //图片合成视频，参数要自己写。
                var image = item.Images?.FirstOrDefault();
                if (image != null)
                {
                    FileHash = IdGener.GetGuid().ToLower().Replace("-", "");//使用随机值，避免重复
                    Height = image.Height.ToString();
                    Width = image.Width.ToString();
                }
            }


            string fileName;
            //if (config?.UperUseViedoTitle ?? false)//优先
            //{
            //    var sampleName = DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId);
            //    var (existingName, _) = douyinVideoService.GetUperLastViedoFileName(item.Author.Uid, sampleName);
            //    fileName = string.IsNullOrWhiteSpace(existingName) ? $"{sampleName}.{Format}" : $"{existingName}.{Format}";
            //}
            //else
            //{

            if (!string.IsNullOrWhiteSpace(config.FullFollowedTitleTemplate))
            {
                var fullName = VideoTitleGenerator.Generate(config.FullFollowedTitleTemplate, new VideoTitleDataTemplate
                {
                    FileHash = FileHash,
                    Id = item.AwemeId,
                    ReleaseTime = DateTimeUtil.Convert10BitTimestamp(item.CreateTime),
                    Resolution = $"{Width}×{Height}",
                    VideoTitle = DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId),
                    Author = item.Author.Nickname
                });

                fileName = $"{fullName}.{Format}";
            }
            else
            {
                fileName = $"{item.AwemeId}.{Format}";
            }
            //}
            return fileName;

        }



    }
}