using ClockSnowFlake;
using dy.net.dto;
using dy.net.model;
using dy.net.repository;
using dy.net.utils;
using Newtonsoft.Json;
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
                    List<DouyinVideo> updates= new List<DouyinVideo>();
                    foreach (var existingVideo in existingVideos)
                    {
                        if (updateMap.TryGetValue(existingVideo.AwemeId, out var updateData))
                        {
                            existingVideo.VideoSavePath = updateData.VideoSavePath;
                            existingVideo.VideoCoverSavePath = updateData.VideoCoverSavePath;
                        }
                    }
                    // 批量更新数据库
                    updatedCount = await _dyCollectVideoRepository.UpdateRangeAsync(existingVideos);
                }

            }, ex =>
            {
                Serilog.Log.Error(ex, "批量插入/更新抖音视频失败，AwemeIds：{AwemeIds}", string.Join(",", allAwemeIds));
            });
            return transaction;
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

                // 5. 文件夹删除（非事务操作，失败不回滚数据库，可根据业务调整）
                // 关键：先通过文件路径获取父文件夹，再删除整个文件夹（含子内容）
                foreach (var filePath in filePathsToDelete)
                {
                    try
                    {
                        // 1. 验证文件路径有效性
                        if (string.IsNullOrWhiteSpace(filePath))
                        {
                            Serilog.Log.Error("文件路径为空，跳过文件夹删除");
                            continue;
                        }

                        // 2. 获取文件对应的父文件夹路径（无论文件是否存在，只要路径合法就能拿到父目录）
                        string parentDirPath = Path.GetDirectoryName(filePath);
                        if (string.IsNullOrWhiteSpace(parentDirPath))
                        {
                            Serilog.Log.Error("无法获取父文件夹路径，文件路径无效：Filepath={0}", filePath);
                            continue;
                        }

                        // 3. 验证父文件夹是否存在
                        if (Directory.Exists(parentDirPath))
                        {
                             Directory.Delete(parentDirPath, recursive: true);
                            Serilog.Log.Debug("文件夹删除成功（含子内容）：ParentDir={0}，关联文件路径={1}", parentDirPath, filePath);
                        }
                        else
                        {
                            Serilog.Log.Error("父文件夹不存在，跳过删除：ParentDir={0}，关联文件路径={1}", parentDirPath, filePath);
                        }
                    }
                    catch (IOException ex)
                    {
                        Serilog.Log.Error(ex, "文件夹删除失败（IO异常）：Filepath={0}，父文件夹路径={1}", filePath, Path.GetDirectoryName(filePath));
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        // 单独捕获权限异常，更精准的日志提示
                        Serilog.Log.Error(ex, "文件夹删除失败（权限不足）：Filepath={0}，父文件夹路径={1}", filePath, Path.GetDirectoryName(filePath));
                    }
                    catch (Exception ex)
                    {
                        // 捕获所有其他异常，避免循环中断
                        Serilog.Log.Error(ex, "文件夹删除失败（未知异常）：Filepath={0}，父文件夹路径={1}", filePath, Path.GetDirectoryName(filePath));
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
