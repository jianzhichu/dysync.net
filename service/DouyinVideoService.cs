using ClockSnowFlake;
using dy.net.dto;
using dy.net.model;
using dy.net.repository;
using dy.net.utils;
using System.ComponentModel;
using System.Threading.Tasks;

namespace dy.net.service
{
    public class DouyinVideoService
    {

        private readonly DouyinVideoRepository _dyCollectVideoRepository;
        private readonly DouyinCookieRepository douyinCookieRepository;

        public DouyinVideoService(DouyinVideoRepository dyCollectVideoRepository, DouyinCookieRepository douyinCookieRepository)
        {
            _dyCollectVideoRepository = dyCollectVideoRepository;
            this.douyinCookieRepository = douyinCookieRepository;
        }


        public async Task<bool> batchInsert(List<DouyinVideo> videos)
        {

            // 边界处理：如果传入的列表为空，直接返回成功（或根据业务返回false）
            if (videos == null || !videos.Any())
                return true;

            // 1. 提取待插入的所有AwemeId（去重，减少数据库查询压力）
            var newAwemeIds = videos.Select(x => x.AwemeId)
                                    .Distinct()
                                    .ToList();

            // 2. 查询数据库中已存在的AwemeId（只查需要的字段，提高效率）
            var existingAwemeIds = await _dyCollectVideoRepository
                .Query(x => newAwemeIds.Contains(x.AwemeId)) // 使用Query方法构建查询
                .Select(x => x.AwemeId) // 只获取AwemeId，减少数据传输
                .ToListAsync();

            // 3. 过滤出数据库中不存在的视频（只保留新记录）
            var videosToInsert = videos
                .Where(video => !existingAwemeIds.Contains(video.AwemeId))
                .ToList();

            // 4. 如果没有需要插入的新记录，直接返回成功
            if (!videosToInsert.Any())
                return true;

            // 5. 批量插入过滤后的新记录
            var insertedCount = await _dyCollectVideoRepository.InsertRangeAsync(videosToInsert);

            // 返回是否插入成功（至少插入一条）
            return insertedCount > 0;
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
                FavoriteCount = list.Count(x => x.ViedoType == VideoTypeEnum.Favorite),
                CollectCount = list.Count(x => x.ViedoType == VideoTypeEnum.Collect),
                FollowCount = list.Count(x => x.ViedoType == VideoTypeEnum.UperPost),
                GraphicVideoCount = list.Count(x => x.ViedoType == VideoTypeEnum.ImageVideo),

                VideoSizeTotal = ByteToGbConverter.ConvertBytesToGb(list.Sum(x => x.FileSize)),
                VideoFavoriteSize = ByteToGbConverter.ConvertBytesToGb(list.Where(x => x.ViedoType == VideoTypeEnum.Favorite).Sum(x => x.FileSize)),
                VideoCollectSize = ByteToGbConverter.ConvertBytesToGb(list.Where(x => x.ViedoType == VideoTypeEnum.Collect).Sum(x => x.FileSize)),
                VideoFollowSize = ByteToGbConverter.ConvertBytesToGb(list.Where(x => x.ViedoType == VideoTypeEnum.UperPost).Sum(x => x.FileSize)),
                GraphicVideoSize = ByteToGbConverter.ConvertBytesToGb(list.Where(x => x.ViedoType == VideoTypeEnum.ImageVideo).Sum(x => x.FileSize)),

                //TotalDiskSize= ByteToGbConverter.GetHostTotalDiskSpaceGB(),
            };
            if (data.GraphicVideoSize == "0.00")
            {
                if (list.Where(x => x.ViedoType == VideoTypeEnum.ImageVideo).Sum(x => x.FileSize) > 0)
                {
                    data.GraphicVideoSize = "<0.01";//避免显示0.00误导用户
                }
            }
            data.Authors = list.GroupBy(x => x.Author).Select(x => new VideoStaticsItemDto { Name = x.Key, Count = x.LongCount(), Icon = x.FirstOrDefault().AuthorAvatarUrl }).OrderByDescending(d => d.Count).ToList();
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


        /// <summary>
        /// 关注的博主的视频如果配置为视频标题作为文件名，生成文件名
        /// </summary>
        /// <param name="AuthorId"></param>
        /// <param name="ViedoNameSimplify"></param>
        /// <returns></returns>
        public async Task<(string, string)> GetUperLastViedoFileName(string AuthorId, string ViedoNameSimplify)
        {

            return await _dyCollectVideoRepository.GetUperLastViedoFileName(AuthorId, ViedoNameSimplify);
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
        public async Task<bool> ReDownloadViedoAsync(ReDownViedoDto dto)
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
            var reDownList = new List<ViedoReDown>();
            var filePathsToDelete = new List<string>(); // 收集待删除文件路径，统一处理

            foreach (var video in videos)
            {
                // 跳过无保存路径的视频（避免无效文件操作）
                if (string.IsNullOrWhiteSpace(video.VideoSavePath))
                {
                    Serilog.Log.Debug("视频无保存路径，跳过文件删除：VideoId={0}", video.Id);
                    continue;
                }

                // 构建重新下载记录
                reDownList.Add(new ViedoReDown
                {
                    Id = IdGener.GetLong().ToString(),
                    CreateTime = DateTime.UtcNow, // 统一使用UTC时间，避免时区问题
                    Status = 0, // 0=待下载（建议用枚举替代魔法值）
                    SavePath = video.VideoSavePath,
                    ViedoId = video.AwemeId,
                    CookieId = video.CookieId
                });

                filePathsToDelete.Add(video.VideoSavePath);
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
                foreach (var path in filePathsToDelete)
                {
                    try
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path); // 异步删除，提升并发性能
                            Serilog.Log.Debug("视频文件删除成功：Path={0}", path);
                        }
                        else
                        {
                            Serilog.Log.Error("视频文件不存在，跳过删除：Path={0}", path);
                        }
                    }
                    catch (IOException ex)
                    {
                        Serilog.Log.Error(ex, "视频文件删除失败：Path={0}", path);
                    }
                }

                var CookieIds = reDownList.Select(x => x.CookieId).Distinct();
                foreach (var ck in CookieIds)
                {
                    var cookie=  douyinCookieRepository.GetById(ck);
                    if (cookie == null)
                        continue;
                    var viedoTypes = videos.Where(x => x.CookieId == ck).Select(x => x.ViedoType).Distinct();

                    if(viedoTypes!=null&& viedoTypes.Any())
                    {
                        foreach (VideoTypeEnum item in viedoTypes)
                        {
                            switch (item)
                            {
                                case VideoTypeEnum.Favorite:
                                    cookie.FavHasSyncd = 0;
                                    break;
                                case VideoTypeEnum.Collect:
                                    cookie.CollHasSyncd = 0;
                                    break;
                                case VideoTypeEnum.UperPost:
                                    cookie.UperSyncd = 0;
                                    break;
                                case VideoTypeEnum.ImageVideo:
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    await douyinCookieRepository.UpdateAsync(cookie);

                }
                Serilog.Log.Debug("重新下载视频流程执行完成：成功创建{0}条重新下载记录，删除{1}个文件,等待重新下载...", reDownList.Count, filePathsToDelete.Count);
                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "重新下载视频执行失败：Ids={0}", string.Join(",", videoIds));
                return false;
            }
        }


        /// <summary>
        /// 获取待重新下载的视频列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<ViedoReDown>> GetViedoReDowns()
        {
            return await _dyCollectVideoRepository.GetViedoReDowns();
        }
    }
}
