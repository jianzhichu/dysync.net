using ClockSnowFlake;
using dy.net.dto;
using dy.net.model;
using dy.net.service;
using dy.net.utils;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace dy.net.job
{
    public class DouyinFollowedViedoSyncJob : DouyinBasicSyncJob
    {
        public DouyinFollowedViedoSyncJob(DouyinCookieService douyinCookieService, DouyinHttpClientService douyinHttpClientService, DouyinVideoService douyinVideoService, DouyinCommonService douyinCommonService, DouyinFollowService douyinFollowService, DouyinMergeVideoService douyinMergeVideoService) : base(douyinCookieService, douyinHttpClientService, douyinVideoService, douyinCommonService, douyinFollowService, douyinMergeVideoService)
        {
        }

        protected override string JobType => SystemStaticUtil.DY_FOLLOWEDS;

        protected override async Task<List<DouyinCookie>> GetValidCookies()
        {
            return  await douyinCookieService.GetAllOpendAsync(x => !string.IsNullOrWhiteSpace(x.UpSavePath));
        }

        protected override bool IsCookieValid(DouyinCookie cookie)
        {
            return !string.IsNullOrWhiteSpace(cookie.Cookies)&&!string.IsNullOrWhiteSpace(cookie.UpSavePath);
        }

        protected override async Task<DouyinVideoInfo> FetchVideoData(DouyinCookie cookie, string cursor, string uperUid = "")
        {
            return await douyinHttpClientService.SyncUpderPostVideos(count, cursor, uperUid, cookie.Cookies);
        }

        protected override bool ShouldContinueSync(DouyinCookie cookie, DouyinVideoInfo data, DouyinFollowed followed)
        {
            return data != null && data.HasMore == 1 && cookie.UperSyncd == 0 && followed.FullSync;
        }

        protected override string GetNextCursor(DouyinVideoInfo data)
        {
            return data?.MaxCursor ?? "0";
        }

        /// <summary>
        /// 关注用户特殊处理文件夹存储路径，用户可自定义保存路径
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="item"></param>
        /// <param name="followed"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        protected override string CreateSaveFolder(DouyinCookie cookie, Aweme item, AppConfig config, DouyinFollowed followed)
        {
            #region 默认使用UP主名称作为文件夹名称，若关注列表中有自定义保存路径则使用自定义路径
            var authorName = string.IsNullOrWhiteSpace(item.Author?.Nickname) ? "UnknownAuthor" : DouyinFileNameHelper.SanitizePath(item.Author.Nickname);
            var folder = Path.Combine(cookie.UpSavePath, authorName);
            if (!string.IsNullOrWhiteSpace(followed.SavePath))
            {
                folder = Path.Combine(cookie.UpSavePath, followed.SavePath);
            }
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            #endregion

            if (config.UperSaveTogether)
            {
                return folder;
            }
            else
            {
                var sampleName = DouyinFileNameHelper.GenerateFileName(item.Desc, item.AwemeId);
                var (existingName, _) = douyinVideoService.GetUperLastViedoFileName(item.Author.Uid, sampleName).Result;
                var fileNameFolder = string.IsNullOrWhiteSpace(existingName) ? sampleName : existingName;
                return Path.Combine(folder, fileNameFolder);
            }
        }
        /// <summary>
        /// 关注的视频，生成文件名称
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="item"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        protected override string GetVideoFileName(DouyinCookie cookie, Aweme item,AppConfig config)
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
                if(image != null){
                    FileHash = IdGener.GetGuid().ToLower().Replace("-","");//使用随机值，避免重复
                    Height = image.Height.ToString();
                    Width = image.Width.ToString();
                }
            }


            string fileName;
            if (config?.UperUseViedoTitle ?? false)//优先
            {
                var sampleName = DouyinFileNameHelper.GenerateFileName(item.Desc, item.AwemeId);
                var (existingName, _) = douyinVideoService.GetUperLastViedoFileName(item.Author.Uid, sampleName).Result;
                fileName = string.IsNullOrWhiteSpace(existingName) ? $"{sampleName}.{Format}" : $"{existingName}.{Format}";
            }
            else
            {

                if (!string.IsNullOrWhiteSpace(config.FullFollowedTitleTemplate))
                {
                    var fullName = VideoTitleGenerator.Generate(config.FullFollowedTitleTemplate, new VideoTitleDataTemplate
                    {
                        FileHash = FileHash,
                        Id = item.AwemeId,
                        ReleaseTime = DateTimeUtil.Convert10BitTimestamp(item.CreateTime),
                        Resolution = $"{Width}×{Height}",
                        VideoTitle = DouyinFileNameHelper.GenerateFileName(item.Desc, item.AwemeId),
                        Author = item.Author.Nickname
                    });

                    fileName= $"{DouyinFileNameHelper.SanitizePath(fullName)}.{Format}";
                }
                else
                {
                    fileName = $"{item.AwemeId}.{Format}";
                }
            }
            return fileName;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="item"></param>
        /// <param name="config"></param>
        /// <param name="imageType"></param>
        /// <returns></returns>
        protected override string GetNfoFileName(DouyinCookie cookie, Aweme item, AppConfig config, string imageType)
        {
            if (config.UperSaveTogether)
            {
                var videoFileName = GetVideoFileName(cookie, item,config);
                return $"{Path.GetFileNameWithoutExtension(videoFileName)}{imageType}";
            }
            else
            {
                return base.GetNfoFileName(cookie, item, config, imageType);
            }
        }

        protected override string GetAuthorAvatarBasePath(DouyinCookie cookie)
        {
            return Path.Combine(cookie.UpSavePath, "author");
        }

        protected override async Task HandleSyncCompletion(DouyinCookie cookie, int syncCount)
        {
            if (syncCount > 0)
            {
                Serilog.Log.Debug($"{JobType}-Cookie-[{cookie.UserName}],本次同步成功{syncCount}条视频");
                cookie.UperSyncd = 1;
                await douyinCookieService.UpdateAsync(cookie);
            }
            else
            {
                Serilog.Log.Debug($"{JobType}-Cookie-[{cookie.UserName}],本次没有查询到新的视频");
            }
        }

        protected override VideoEntityDifferences GetVideoEntityDifferences(DouyinCookie cookie, Aweme item)
        {
            var config = douyinCommonService.GetConfig();
            string simplifiedTitle = string.Empty;

            if (config?.UperUseViedoTitle ?? false)
            {
                simplifiedTitle = DouyinFileNameHelper.GenerateFileName(item.Desc, item.AwemeId);
            }

            return new VideoEntityDifferences
            {
                VideoType = VideoTypeEnum.UperPost,
                VideoTitleSimplify = simplifiedTitle
            };
        }
    }
}