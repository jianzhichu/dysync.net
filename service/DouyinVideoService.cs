using ClockSnowFlake;
using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.repository;
using dy.net.utils;
using Serilog;
using SqlSugar;

namespace dy.net.service
{
    public class DouyinVideoService
    {

        private readonly ISqlSugarClient sqlSugarClient;

        private readonly DouyinVideoRepository _dyCollectVideoRepository;
        private readonly DouyinCookieRepository douyinCookieRepository;

        public DouyinVideoService(DouyinVideoRepository dyCollectVideoRepository, DouyinCookieRepository douyinCookieRepository, ISqlSugarClient sqlSugarClient)
        {
            _dyCollectVideoRepository = dyCollectVideoRepository;
            this.douyinCookieRepository = douyinCookieRepository;
            this.sqlSugarClient = sqlSugarClient;
        }


        public async Task<bool> DeleteById(string Id)
        {
            return await _dyCollectVideoRepository.DeleteByIdAsync(Id);
        }

        public async Task<bool> BatchInsertOrUpdate(List<DouyinVideo> videos)
        {
            // 边界处理：传入列表为空直接返回成功
            if (videos == null || !videos.Any())
                return true;

            // 1. 提取所有AwemeId（无需去重，用户保证无重复）
            var allAwemeIds = videos.Select(v => v.AwemeId).ToList();

            // 2. 查询数据库中已存在的视频记录（用于后续更新）
            var existingVideos = await _dyCollectVideoRepository
                .Query(x => allAwemeIds.Contains(x.AwemeId))
                .ToListAsync();

            // 3. 分拆数据集：不存在的（插入）、已存在的（更新）
            var existingAwemeIdSet = existingVideos.Select(v => v.AwemeId).ToHashSet();
            var videosToInsert = videos
                .Where(v => !existingAwemeIdSet.Contains(v.AwemeId))
                .ToList();
            var videosToUpdate = videos
                .Where(v => existingAwemeIdSet.Contains(v.AwemeId))
                .ToList();

            // 4. 事务包裹：确保插入/更新原子性
            var transaction = await _dyCollectVideoRepository.UseTranAsync(async () =>
           {
               int insertedCount = 0;
               int updatedCount = 0;

               // 5. 批量插入新记录
               if (videosToInsert.Any())
               {
                   insertedCount = await _dyCollectVideoRepository.InsertRangeAsync(videosToInsert);
               }

               // 6. 批量更新已存在记录（核心逻辑）
               if (videosToUpdate.Any())
               {
                   // 建立AwemeId与待更新数据的映射（O(1)匹配效率）
                   var updateMap = videosToUpdate.ToDictionary(v => v.AwemeId);

                   // 遍历已存在实体，赋值需要更新的字段
                   List<DouyinVideo> updates = new List<DouyinVideo>();
                   foreach (var existingVideo in existingVideos)
                   {
                       if (updateMap.TryGetValue(existingVideo.AwemeId, out var updateData))
                       {
                           existingVideo.VideoSavePath = updateData.VideoSavePath;
                           existingVideo.VideoCoverSavePath = updateData.VideoCoverSavePath;
                           existingVideo.ViedoType = updateData.ViedoType;
                       }
                   }
                   // 批量更新数据库
                   updatedCount = await _dyCollectVideoRepository.UpdateRangeAsync(existingVideos);
               }


               //foreach (var item in videos.Where(x=>x.ViedoType==VideoTypeEnum.dy_collects||x.ViedoType == VideoTypeEnum.dy_favorite).GroupBy(x => x.AuthorId))
               //{
                   
               //}

           }, ex =>
           {
               Serilog.Log.Error(ex, "批量插入/更新抖音视频失败，AwemeIds：{AwemeIds}", string.Join(",", allAwemeIds));
           });
            return transaction;
        }

        public async Task<bool> UpdateOne(DouyinVideo video)
        {
            return await _dyCollectVideoRepository.UpdateAsync(video);
        }

        public async Task<VideoStaticsDto> GetStatics()
        {

            List<DouyinVideo> list = await this._dyCollectVideoRepository.GetAllAsync();
            if (!list.Any())
                return new VideoStaticsDto();
            var Categories = list.GroupBy(x => x.Tag1).Select(x => new VideoStaticsItemDto { Name = x.Key, Count = x.LongCount() }).OrderByDescending(p => p.Count).ToList();
            Categories.Where(x => string.IsNullOrWhiteSpace(x.Name)).ToList().ForEach(x => x.Name = "其他");
            var data = new VideoStaticsDto
            {
                AuthorCount = list.Select(x => x.AuthorId).Distinct().Count(),
                CategoryCount = list.Select(x => x.Tag1).Distinct().Count(),
                VideoCount = list.Count,
                Categories = Categories,
                FavoriteCount = list.Count(x => x.ViedoType == VideoTypeEnum.dy_favorite),
                CollectCount = list.Count(x => x.ViedoType == VideoTypeEnum.dy_collects || x.ViedoType == VideoTypeEnum.dy_custom_collect),
                FollowCount = list.Count(x => x.ViedoType == VideoTypeEnum.dy_follows),
                GraphicVideoCount = list.Count(x => x.IsMergeVideo == 1),
                MixCount = list.Count(x => x.ViedoType == VideoTypeEnum.dy_mix),
                SeriesCount = list.Count(x => x.ViedoType == VideoTypeEnum.dy_series),
                VideoSizeTotal = DouyinFileUtils.ConvertBytesToGb(list.Sum(x => x.FileSize)),
                VideoFavoriteSize = DouyinFileUtils.ConvertBytesToGb(list.Where(x => x.ViedoType == VideoTypeEnum.dy_favorite).Sum(x => x.FileSize)),
                VideoCollectSize = DouyinFileUtils.ConvertBytesToGb(list.Where(x => x.ViedoType == VideoTypeEnum.dy_collects || x.ViedoType == VideoTypeEnum.dy_custom_collect).Sum(x => x.FileSize)),
                VideoFollowSize = DouyinFileUtils.ConvertBytesToGb(list.Where(x => x.ViedoType == VideoTypeEnum.dy_follows).Sum(x => x.FileSize)),
                VideoMixSize = DouyinFileUtils.ConvertBytesToGb(list.Where(x => x.ViedoType == VideoTypeEnum.dy_mix).Sum(x => x.FileSize)),
                VideoSeriesSize = DouyinFileUtils.ConvertBytesToGb(list.Where(x => x.ViedoType == VideoTypeEnum.dy_series).Sum(x => x.FileSize)),
                GraphicVideoSize = DouyinFileUtils.ConvertBytesToGb(list.Where(x => x.IsMergeVideo == 1).Sum(x => x.FileSize)),

                //TotalDiskSize= ByteToGbConverter.GetHostTotalDiskSpaceGB(),
            };
            if (data.GraphicVideoSize == "0.00")
            {
                if (list.Where(x => x.IsMergeVideo == 1).Sum(x => x.FileSize) > 0)
                {
                    data.GraphicVideoSize = "<0.01";//避免显示0.00误导用户
                }
            }
            if (data.VideoFavoriteSize == "0.00")
            {
                data.VideoFavoriteSize = "<0.01";//避免显示0.00误导用户
            }

            if (data.VideoCollectSize == "0.00")
            {
                data.VideoCollectSize = "<0.01";//避免显示0.00误导用户
            }
            if (data.VideoFollowSize == "0.00")
            {
                data.VideoFollowSize = "<0.01";//避免显示0.00误导用户
            }
            if (data.VideoMixSize == "0.00")
            {
                data.VideoMixSize = "<0.01";//避免显示0.00误导用户
            }

            if (data.VideoSeriesSize == "0.00")
            {
                data.VideoSeriesSize = "<0.01";//避免显示0.00误导用户
            }
            data.Authors = list.GroupBy(x => x.Author).Select(x => new VideoStaticsItemDto
            {
                Name = x.Key,
                Count = x.LongCount(),
                Icon = x.LastOrDefault()?.AuthorAvatarUrl,
                UperId = x.LastOrDefault()?.AuthorId ?? x.LastOrDefault()?.DyUserId
            }).OrderByDescending(d => d.Count).ToList();
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="awemeId"></param>
        /// <returns></returns>
        public async Task<DouyinVideo> GetByAwemeId(string awemeId)
        {
            return await _dyCollectVideoRepository.GetFirstAsync(x => x.AwemeId == awemeId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<(List<DouyinVideo> list, int totalCount)> GetPagedAsync(DouyinVideoPageRequestDto dto)
        {
            return await _dyCollectVideoRepository.GetPagedAsync(dto);
        }

        public async Task<List<DouyinVideo>> GetAllAsync()
        {
            return await _dyCollectVideoRepository.GetAllAsync();
        }

        /// <summary>
        /// 关注的博主的视频如果配置为视频标题作为文件名，生成文件名
        /// </summary>
        /// <param name="AuthorId"></param>
        /// <param name="ViedoNameSimplify"></param>
        /// <returns></returns>
        public (string, string) GetUperLastViedoFileName(string AuthorId, string ViedoNameSimplify)
        {

            return _dyCollectVideoRepository.GetUperLastViedoFileName(AuthorId, ViedoNameSimplify);
        }

        /// <summary>
        /// 根据ID获取视频信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DouyinVideo> GetById(string id)
        {
            return await _dyCollectVideoRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// 重新下载选中的视频
        /// </summary>
        /// <param name="dto">重新下载请求DTO（包含待处理视频ID列表）</param>
        /// <returns>是否执行成功（true=流程执行完成，false=无有效数据或执行失败）</returns>
        /// <exception cref="ArgumentNullException">DTO或ID列表为空时抛出</exception>
        /// <exception cref="IOException">文件操作失败时抛出（可根据业务调整处理方式）</exception>
        public async Task<bool> ReDownloadViedoAsync(ReDownViedoDto dto, bool forever = false)
        {
            // 1. 严格参数校验（避免无效流程）
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "重新下载请求DTO不能为空");
            if (dto.Ids == null || !dto.Ids.Any())
            {
                Serilog.Log.Error("重新下载视频失败：待处理视频ID列表为空");
                return false;
            }

            // 2. 查询有效视频记录（去重+非空校验，避免无效处理）
            var videoIds = dto.Ids.Distinct().ToList(); // 去重，减少数据库查询和操作
            var videos = await _dyCollectVideoRepository.GetByIds(videoIds);
            if (videos == null || !videos.Any())
            {
                Serilog.Log.Debug("未查询到有效视频记录：Ids={0}", string.Join(",", videoIds));
                return false;
            }

            // 3. 构建重新下载记录（提前准备数据，避免事务内耗时操作）
            var reDownList = new List<DouyinReDownload>();
            var filePathsToDelete = new List<(string path, bool onlyImgOrMp3)>(); // 收集待删除文件路径，统一处理

            foreach (var video in videos)
            {
                // 跳过无保存路径的视频（避免无效文件操作）
                if (string.IsNullOrWhiteSpace(video.VideoSavePath))
                {
                    Serilog.Log.Debug("视频无保存路径，跳过文件删除：VideoId={0}", video.Id);
                    continue;
                }

                // 构建重新下载记录
                reDownList.Add(new DouyinReDownload
                {
                    Id = IdGener.GetLong().ToString(),
                    CreateTime = DateTime.UtcNow, // 统一使用UTC时间，避免时区问题
                    Status = 0, // 0=待下载（建议用枚举替代魔法值）
                    SavePath = video.VideoSavePath,
                    ViedoId = video.AwemeId,
                    CookieId = video.CookieId
                });

                filePathsToDelete.Add((video.VideoSavePath, video.OnlyImgOrOnlyMp3));
            }

            // 无有效重新下载记录时直接返回
            if (!reDownList.Any())
            {
                Serilog.Log.Debug("无有效重新下载记录需要创建：VideoIds={0}", string.Join(",", videoIds));
                return false;
            }

            try
            {
                // 4. 数据库操作（事务保证一致性：创建重新下载记录 + 删除原视频记录必须同时成功/失败）
                var transactionResult = await _dyCollectVideoRepository.UseTranAsync(async () =>
                {
                    // 4.1 批量插入重新下载记录（SqlSugar批量插入效率更高）
                    _dyCollectVideoRepository.InsertReDowns(reDownList);
                    // 4.2 批量删除原视频记录（使用视频实际存在的ID，避免无效删除）
                    var actualDeleteIds = videos.Select(v => v.Id).ToList();
                    var deleteCount = await _dyCollectVideoRepository.DeleteByIdsAsync(actualDeleteIds); // 建议仓储层提供异步删除方法
                }, e =>
                {
                    Serilog.Log.Error(e, "数据库事务执行失败：Ids={0}", string.Join(",", videoIds));
                });

                // 5. 文件删除（非事务操作，失败不回滚数据库，可根据业务调整）
                // 采用异步文件操作，避免同步IO阻塞线程（需.NET 5+支持）
                foreach (var video in filePathsToDelete)
                {
                    try
                    {
                        if (File.Exists(video.path))
                        {
                            File.Delete(video.path); // 异步删除，提升并发性能
                            Serilog.Log.Debug("视频文件删除成功：Path={0}", video.path);

                            if (!video.onlyImgOrMp3)//如果是纯图片或纯音频文件，则不删除所在文件夹
                            {
                                //检查这个路径所在文件夹是否还有其他视频文件，如果没有则删除这个文件夹
                                var dir = Path.GetDirectoryName(video.path);

                                bool hasMp4File = Directory.EnumerateFiles(dir, "*.mp4", SearchOption.TopDirectoryOnly).Any(); // 只要存在一个MP4文件就返回true；
                                if (!hasMp4File)
                                {
                                    Directory.Delete(dir, true);
                                }
                            }
                        }
                        else
                        {
                            Serilog.Log.Error("视频文件不存在，跳过删除：Path={0}", video);
                        }
                    }
                    catch (IOException ex)
                    {
                        Serilog.Log.Error(ex, "视频文件删除失败：Path={0}", video);
                    }
                }

                //var CookieIds = reDownList.Select(x => x.CookieId).Distinct();
                //foreach (var ck in CookieIds)
                //{
                //    var cookie = douyinCookieRepository.GetById(ck);
                //    if (cookie == null)
                //        continue;
                //    var viedoTypes = videos.Where(x => x.CookieId == ck).Select(x => x.ViedoType).Distinct();

                //    //if (viedoTypes != null && viedoTypes.Any())
                //    //{
                //    //    foreach (VideoTypeEnum item in viedoTypes)
                //    //    {
                //    //        switch (item)
                //    //        {
                //    //            case VideoTypeEnum.dy_favorite:
                //    //                cookie.FavHasSyncd = 0;
                //    //                break;
                //    //            case VideoTypeEnum.dy_collects:
                //    //                cookie.CollHasSyncd = 0;
                //    //                break;
                //    //            case VideoTypeEnum.dy_follows:
                //    //                cookie.UperSyncd = 0;
                //    //                break;
                //    //            case VideoTypeEnum.ImageVideo:
                //    //                break;
                //    //            default:
                //    //                break;
                //    //        }
                //    //    }
                //    //}
                //    await douyinCookieRepository.UpdateAsync(cookie);

                //}
                if (!forever)
                    Serilog.Log.Debug("重新下载视频流程执行完成：成功创建{0}条重新下载记录，删除{1}个文件,等待重新下载...", reDownList.Count, filePathsToDelete.Count);
                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "重新下载视频执行失败：Ids={0}", string.Join(",", videoIds));
                return false;
            }
        }



        public async Task<List<DouyinVideoTopDto>> GetLastSyncTop(int top = 5)
        {
            return await _dyCollectVideoRepository.GetTopsOrderBySyncTime(top);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<VideoChartItemDto>> GetChartData(int day = 7)
        {

            var date = DateTime.Now.AddDays(-day);
            var list = await _dyCollectVideoRepository.GetListAsync(x => x.SyncTime > date);

            var resultData = list.GroupBy(x => x.SyncTime.ToString("yyyyMMdd")).Select(g => new VideoChartItemDto
            {
                Date = g.Key,
                Collect = g.Count(x => x.ViedoType == VideoTypeEnum.dy_collects || x.ViedoType == VideoTypeEnum.dy_custom_collect),
                Favorite = g.Count(x => x.ViedoType == VideoTypeEnum.dy_favorite),
                Follow = g.Count(x => x.ViedoType == VideoTypeEnum.dy_follows),
                Graphic = g.Count(x => string.IsNullOrEmpty(x.FileHash)),
                Mix = g.Count(x => x.ViedoType == VideoTypeEnum.dy_mix),
                Series = g.Count(x => x.ViedoType == VideoTypeEnum.dy_series),
            })
              .ToList();
            return resultData;
        }

        /// <summary>
        /// 删除无效记录（记录存在，用户手动把目录下的视频删了的情况，视频记录依然存在）
        /// </summary>
        /// <returns></returns>
        public async Task<List<DeleteInvalidVideoDto>> DeleteInvalidVideo()
        {
            var videos = await _dyCollectVideoRepository.GetAllAsync();
            List<DeleteInvalidVideoDto> vList = new List<DeleteInvalidVideoDto>();

            List<string> douyinVideoIds = new List<string>();
            foreach (var v in videos)
            {
                if (!File.Exists(v.VideoSavePath))
                {
                    douyinVideoIds.Add(v.Id);
                    vList.Add(new DeleteInvalidVideoDto { AwId = v.AwemeId, Title = v.VideoTitle, Path = v.VideoSavePath });
                }
            }

            if (douyinVideoIds.Any())
            {
                await _dyCollectVideoRepository.DeleteByIdsAsync(douyinVideoIds);
            }

            return vList;
        }

        /// <summary>
        /// 根据博主ID获取视频列表
        /// </summary>
        /// <param name="uperUid"></param>
        /// <returns></returns>
        internal async Task<List<DouyinVideo>> GetByAuthorId(string uperUid)
        {
            return await _dyCollectVideoRepository.GetListAsync(x => x.DyUserId == uperUid);
        }


        internal async Task<int> AddDeleteVideo(List<DouyinVideo> videos)
        {
            var deletes = videos.Select(video => new DouyinVideoDelete
            {
                ViedoId = video.AwemeId,
                VideoTitle = video.VideoTitle,
                VideoSavePath = video.VideoSavePath,
                Id = IdGener.GetLong().ToString(),
                DeleteTime = DateTime.Now
            })?.ToList();

            return await sqlSugarClient.Insertable<DouyinVideoDelete>(deletes).ExecuteCommandAsync();
        }

        /// <summary>
        /// 彻底删除视频
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        public async Task<bool> RealDeleteVideos(List<string> Ids)
        {
            if (Ids == null || !Ids.Any())
                return false;
            var videos = await _dyCollectVideoRepository.GetListAsync(x => Ids.Contains(x.Id));

            if (videos != null && videos.Count > 0)
            {
                if (videos.Count <= 30)
                {
                    var result = await ReDownloadViedoAsync(new ReDownViedoDto { Ids = videos.Select(x => x.Id)?.ToList() }, true);
                    if (result)
                    {
                        //加入删除逻辑
                        var deletes = await AddDeleteVideo(videos);
                        Serilog.Log.Debug($"批量永久删除博主{videos.FirstOrDefault()?.Author}，共{deletes}条记录");
                        return true;
                    }
                    else
                    {
                        Serilog.Log.Error($"批量删除{videos.FirstOrDefault()?.Author}视频失败");
                        return false;
                    }
                }
                else
                {
                    Task.Run(async () =>
                    {
                        var result = await ReDownloadViedoAsync(new ReDownViedoDto { Ids = videos.Select(x => x.Id)?.ToList() }, true);
                        if (result)
                        {
                            //加入删除逻辑
                            var deletes = await AddDeleteVideo(videos);
                            Serilog.Log.Debug($"批量永久删除博主{videos.FirstOrDefault()?.Author}，{deletes}条记录");
                        }
                        else
                        {
                            Serilog.Log.Error($"批量删除{videos.FirstOrDefault()?.Author}视频失败");
                        }
                    });
                    return true;
                }
            }
            else
            {
                Serilog.Log.Error($"没有查询到可删除的视频");
                return false;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HandOldFolderVideos()
        {
            // 1. 查询目标数据
            var list = await _dyCollectVideoRepository.GetListAsync(x => x.ViedoType == VideoTypeEnum.dy_favorite || x.ViedoType == VideoTypeEnum.dy_collects);
            // 缓存已处理的「Tag1+下一级文件夹」组合（避免重复移动）
            var processedFolderPairs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in list)
            {
                // 跳过空值（Tag1/Author为空）
                if (string.IsNullOrEmpty(item.Tag1) || string.IsNullOrEmpty(item.Author))
                {
                    Log.Debug($"跳过：Tag1/Author为空，ItemId={item.Id}");
                    continue;
                }

                try
                {
                    // 2. 标准化路径 + 拆分路径层级（核心：精准定位Tag1和其下一级文件夹）
                    string oldVideoPath = item.VideoSavePath;
                    // 统一分隔符为/，方便拆分层级
                    string standardPath = oldVideoPath.Replace('\\', '/').Trim('/');
                    string[] pathSegments = standardPath.Split('/'); // 拆分结果：["app","collect","校园教育","期末复习xxx","文件.mp4"]

                    // 2.1 找到Tag1在路径中的索引（比如"校园教育"的索引是2）
                    int tag1Index = Array.IndexOf(pathSegments, item.Tag1);
                    if (tag1Index == -1 || tag1Index + 1 >= pathSegments.Length - 1)
                    {
                        Log.Debug($"跳过：无Tag1下一级文件夹，Path={oldVideoPath}，Tag1={item.Tag1}");
                        continue;
                    }

                    // 2.2 解析核心路径（关键！）
                    string tag1FolderRelative = string.Join("/", pathSegments.Take(tag1Index + 1)); // Tag1根目录（相对）：app/collect/校园教育
                                                                                                    // Tag1根目录完整路径（如 D:/app/collect/校园教育 或 /app/collect/校园教育）
                    string tag1RootFolderFull = Path.GetFullPath(
                        Path.Combine(Path.GetPathRoot(oldVideoPath) ?? "",
                        tag1FolderRelative.Replace('/', Path.DirectorySeparatorChar))
                    );
                    string tag1NextLevelFolderName = pathSegments[tag1Index + 1]; // Tag1下一级文件夹名：期末复习xxx
                                                                                  // Tag1下一级文件夹完整路径：app/collect/校园教育/期末复习xxx
                    string tag1NextLevelFolderFull = Path.GetFullPath(
                        Path.Combine(tag1RootFolderFull, tag1NextLevelFolderName)
                    );

                    // 2.3 拼接新路径（替换Tag1为Author，保留下一级文件夹名）
                    // 新Author根目录完整路径：app/collect/张三
                    string authorRootFolderFull = tag1RootFolderFull.Replace(item.Tag1, item.Author);
                    // 新的下一级文件夹完整路径：app/collect/张三/期末复习xxx
                    string authorNextLevelFolderFull = Path.GetFullPath(
                        Path.Combine(authorRootFolderFull, tag1NextLevelFolderName)
                    );

                    // 2.4 防重复处理（Tag1下一级文件夹已处理则跳过）
                    string folderPairKey = $"{tag1NextLevelFolderFull}|{authorNextLevelFolderFull}";
                    if (processedFolderPairs.Contains(folderPairKey))
                    {
                        Log.Debug($"跳过：下一级文件夹已处理，Key={folderPairKey}");
                        continue;
                    }

                    // 3. 核心判断：检查Tag1的下一级文件夹是否有文件（而非Tag1根目录）
                    if (!Directory.Exists(tag1NextLevelFolderFull))
                    {
                        Log.Warning($"跳过：Tag1下一级文件夹不存在，Path={tag1NextLevelFolderFull}");
                        continue;
                    }
                    string[] nextLevelFiles = Directory.GetFiles(tag1NextLevelFolderFull); // 非递归，只查该文件夹下的文件
                    if (nextLevelFiles.Length == 0)
                    {
                        Log.Debug($"跳过：Tag1下一级文件夹无文件，Path={tag1NextLevelFolderFull}");
                        processedFolderPairs.Add(folderPairKey);
                        continue;
                    }

                    // 4. 移动Tag1的下一级文件夹（保留文件夹名，整体移动到Author目录下）
                    // 4.1 确保新Author目录存在
                    Directory.CreateDirectory(authorRootFolderFull);

                    // 4.2 目标文件夹已存在则跳过（如需覆盖，可删除此行+添加Directory.Delete(authorNextLevelFolderFull, true)）
                    if (Directory.Exists(authorNextLevelFolderFull))
                    {
                        Log.Debug($"跳过：目标下一级文件夹已存在，Path={authorNextLevelFolderFull}");
                        processedFolderPairs.Add(folderPairKey);
                        continue;
                    }

                    // 4.3 移动整个下一级文件夹（保留名称和内部所有文件）
                    Directory.Move(tag1NextLevelFolderFull, authorNextLevelFolderFull);
                    Log.Debug($"移动Tag1下一级文件夹成功：{tag1NextLevelFolderFull} → {authorNextLevelFolderFull}");

                    // 5. 关键新增：检查Tag1根目录是否为空，为空则删除
                    if (Directory.Exists(tag1RootFolderFull))
                    {
                        // 检查Tag1根目录下是否还有任何文件/文件夹
                        bool isTag1RootEmpty = !Directory.EnumerateFileSystemEntries(tag1RootFolderFull).Any();
                        if (isTag1RootEmpty)
                        {
                            Directory.Delete(tag1RootFolderFull, false); // false=仅删除空目录，避免误删
                            Log.Debug($"删除空Tag1根目录：{tag1RootFolderFull}");
                        }
                        else
                        {
                            Log.Debug($"Tag1根目录非空，不删除：{tag1RootFolderFull}");
                        }
                    }

                    // 标记已处理
                    processedFolderPairs.Add(folderPairKey);

                    // 6. 更新当前Item的视频路径（替换Tag1为Author，保留后续层级）
                    string newVideoPath = oldVideoPath.Replace(item.Tag1, item.Author);
                    item.VideoSavePath = newVideoPath;
                    Log.Debug($"更新Item路径：{oldVideoPath} → {newVideoPath}");
                }
                catch (IOException ex)
                {
                    Log.Error(ex, $"移动失败（IO异常），ItemId={item.Id}，Path={item.VideoSavePath}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Error(ex, $"删除/移动失败（权限不足），ItemId={item.Id}，Path={item.VideoSavePath}");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"处理失败（未知错误），ItemId={item.Id}，Path={item.VideoSavePath}");
                }
            }

            // 批量更新数据库
            if (list.Any())
            {
                await BatchInsertOrUpdate(list);
                Log.Debug($"批量更新数据库完成，共处理{list.Count}条数据");
            }

            return true;
        }

    }
}
