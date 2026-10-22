using dy.net.extension;
using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.utils;
using SqlSugar;

namespace dy.net.repository
{
    public class DouyinVideoRepository : BaseRepository<DouyinVideo>
    {
        // 注入SQLSugar客户端
        public DouyinVideoRepository(ISqlSugarClient db) : base(db)
        {
        }




        public async Task<List<DouyinVideoTopDto>> GetTopsOrderBySyncTime(int top)
        {
            return await Db.Queryable<DouyinVideo>().Select(x => new DouyinVideoTopDto { Id = x.Id, Title = x.VideoTitle, Time = x.SyncTime.ToString("yyyy-MM-dd HH:mm:ss") }).Take(top).OrderByDescending(x => x.Time).ToListAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<(List<DouyinVideo> list, int totalCount)> GetPagedAsync(DouyinVideoPageRequestDto dto)
        {
            DateTime? start, end;
            GetDateBetween(dto.Dates, out start, out end);

            DateTime? start2, end2;
            GetDateBetween(dto.Dates2, out start2, out end2);


            VideoTypeEnum? enumviedoType = null;
            if (!string.IsNullOrEmpty(dto.ViedoType) && dto.ViedoType != "*" && dto.ViedoType != "4")
            {
                enumviedoType = dto.ViedoType.ToVideoTypeEnum();
            }

            var where = this.Db.Queryable<DouyinVideo>()
                //.WhereIF(!string.IsNullOrWhiteSpace(title), x => x.VideoTitle.Contains(title))
                .WhereIF(!string.IsNullOrWhiteSpace(dto.Title), x => x.VideoTitle.Contains(dto.Title))
                .WhereIF(!string.IsNullOrWhiteSpace(dto.Author), x => x.Author.Contains(dto.Author))
                .WhereIF(!string.IsNullOrWhiteSpace(dto.Tag), x => x.Tag1 == dto.Tag)
                .WhereIF(start.HasValue, x => x.SyncTime >= start.Value)
                .WhereIF(end.HasValue, x => x.SyncTime <= end.Value)
                .WhereIF(start2.HasValue, x => x.CreateTime >= start2.Value)
                .WhereIF(end2.HasValue, x => x.CreateTime <= end2.Value)
                .WhereIF(enumviedoType.HasValue, x => x.ViedoType == enumviedoType)
                .WhereIF(!string.IsNullOrWhiteSpace(dto.CookieId),x=>x.CookieId==dto.CookieId)
                .WhereIF(dto.ViedoType == "4", x => x.IsMergeVideo == 1);


            var totalCount = await where.CountAsync();
            List<DouyinVideo> list = new List<DouyinVideo>();
            if (string.IsNullOrWhiteSpace(dto.SortField))
                list = await where.OrderByDescending(x => x.SyncTime).Skip((dto.PageIndex - 1) * dto.PageSize).Take(dto.PageSize).ToListAsync();
            else
            {

                list = await where.OrderBy($"{dto.SortField} {dto.SortOrder}").Skip((dto.PageIndex - 1) * dto.PageSize).Take(dto.PageSize).ToListAsync();
            }
            if (list.Any())
            {
                var users = await this.Db.Queryable<DouyinCookie>().ToListAsync();
                foreach (var item in list)
                {
                    var user = users.FirstOrDefault(x => x.Id == item.CookieId);
                    if (user != null)
                    {
                        item.DyUser = user.UserName;
                    }
                }
            }
            return (list, totalCount);
        }

        public async Task<(List<DouyinVideo> list, int totalCount)> GetAuthorVideosPagedAsync(
            string authorId,
            string authorName,
            int pageIndex,
            int pageSize,
            string title = null,
            string tag1 = null)
        {
            var query = BuildAuthorVideoQuery(authorId, authorName);
            if (query == null) return (new List<DouyinVideo>(), 0);

            query = query
                .WhereIF(!string.IsNullOrWhiteSpace(title), x => x.VideoTitle.Contains(title))
                .WhereIF(!string.IsNullOrWhiteSpace(tag1), x => x.Tag1 == tag1);

            var totalCount = await query.CountAsync();
            var list = await query
                .OrderBy(x => x.CreateTime, OrderByType.Desc)
                .OrderBy(x => x.SyncTime, OrderByType.Desc)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (list, totalCount);
        }

        public async Task<List<AuthorVideoTagDto>> GetAuthorVideoTagsAsync(string authorId, string authorName)
        {
            var query = BuildAuthorVideoQuery(authorId, authorName);
            if (query == null) return new List<AuthorVideoTagDto>();

            var grouped = await query
                .Where(x => x.Tag1 != null && x.Tag1 != "")
                .GroupBy(x => x.Tag1)
                .Select(x => new { Name = x.Tag1, Count = SqlFunc.AggregateCount(x.Id) })
                .ToListAsync();

            return grouped
                .Select(x => new AuthorVideoTagDto
                {
                    Name = x.Name,
                    Count = x.Count
                })
                .OrderBy(x => x.Name)
                .ToList();
        }

        /// <summary>
        /// 「我的抖音」信息流：只返回本地有视频文件、可以直接播放的记录
        /// </summary>
        public async Task<(List<DouyinVideo> list, int totalCount)> GetFeedPagedAsync(DouyinFeedRequestDto dto)
        {
            var skip = (dto.PageIndex - 1) * dto.PageSize;

            if (string.Equals(dto.Order, "random", StringComparison.OrdinalIgnoreCase))
            {
                // 跨数据库的随机排序没法稳定翻页，所以只取 id 在内存里按种子洗牌，再取这一页的完整记录
                var ids = await BuildFeedQuery(dto).Select(x => x.Id).ToListAsync();
                var pageIds = DouyinFeedTypes.Shuffle(ids, dto.Seed).Skip(skip).Take(dto.PageSize).ToList();
                if (pageIds.Count == 0)
                    return (new List<DouyinVideo>(), ids.Count);

                var rows = await GetByIds(pageIds);
                var byId = rows.ToDictionary(x => x.Id);
                var ordered = pageIds.Where(byId.ContainsKey).Select(id => byId[id]).ToList();
                return (ordered, ids.Count);
            }

            var query = BuildFeedQuery(dto);
            var totalCount = await query.CountAsync();
            query = string.Equals(dto.Order, "publish", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.CreateTime, OrderByType.Desc).OrderBy(x => x.Id, OrderByType.Desc)
                : query.OrderBy(x => x.SyncTime, OrderByType.Desc).OrderBy(x => x.Id, OrderByType.Desc);
            var list = await query.Skip(skip).Take(dto.PageSize).ToListAsync();
            return (list, totalCount);
        }

        /// <summary>
        /// 各类型可播放视频的数量
        /// </summary>
        public async Task<Dictionary<VideoTypeEnum, int>> GetFeedTypeCountsAsync(string cookieId, string authorId = null)
        {
            var grouped = await BuildFeedQuery(new DouyinFeedRequestDto { CookieId = cookieId, AuthorId = authorId })
                .GroupBy(x => x.ViedoType)
                .Select(x => new { Type = x.ViedoType, Count = SqlFunc.AggregateCount(x.Id) })
                .ToListAsync();
            // 类型列允许为空，空值和 0 会映射到同一个枚举值，所以再合并一次
            return grouped
                .GroupBy(x => x.Type)
                .ToDictionary(x => x.Key, x => x.Sum(item => item.Count));
        }

        public async Task<int> GetFeedAuthorCountAsync(string cookieId)
        {
            return await BuildFeedQuery(new DouyinFeedRequestDto { CookieId = cookieId })
                .Where(x => x.AuthorId != null && x.AuthorId != "")
                .Select(x => x.AuthorId)
                .Distinct()
                .CountAsync();
        }

        /// <summary>
        /// 按博主汇总本地可播放的视频。博主数量有限，全部取回后由调用方排序、搜索和分页
        /// </summary>
        public async Task<List<DouyinFeedAuthorDto>> GetFeedAuthorsAsync(string cookieId)
        {
            return await BuildFeedQuery(new DouyinFeedRequestDto { CookieId = cookieId })
                .Where(x => x.AuthorId != null && x.AuthorId != "")
                .GroupBy(x => x.AuthorId)
                .Select(x => new DouyinFeedAuthorDto
                {
                    AuthorId = x.AuthorId,
                    Name = SqlFunc.AggregateMax(x.Author),
                    Count = SqlFunc.AggregateCount(x.Id),
                    LatestSyncTime = SqlFunc.AggregateMax(x.SyncTime),
                    // id 是递增的雪花 id，最大的就是最近同步的那条，用它取头像和封面
                    SampleVideoId = SqlFunc.AggregateMax(x.Id)
                })
                .ToListAsync();
        }

        private ISugarQueryable<DouyinVideo> BuildFeedQuery(DouyinFeedRequestDto dto)
        {
            // 一个分类最多对应两种视频类型（收藏 = 默认收藏夹 + 自定义收藏夹）
            var types = DouyinFeedTypes.ToVideoTypes(dto.Type);
            var firstType = types?[0] ?? default;
            var secondType = types != null && types.Length > 1 ? types[1] : firstType;
            return Db.Queryable<DouyinVideo>()
                // 只下载了图片或音频的记录没有视频文件，播放不了
                .Where(x => x.VideoSavePath != null && x.VideoSavePath.EndsWith(".mp4"))
                .WhereIF(types != null, x => x.ViedoType == firstType || x.ViedoType == secondType)
                .WhereIF(!string.IsNullOrWhiteSpace(dto.CookieId), x => x.CookieId == dto.CookieId)
                .WhereIF(!string.IsNullOrWhiteSpace(dto.AuthorId), x => x.AuthorId == dto.AuthorId)
                .WhereIF(!string.IsNullOrWhiteSpace(dto.Keyword),
                    x => x.VideoTitle.Contains(dto.Keyword) || x.Author.Contains(dto.Keyword));
        }

        private ISugarQueryable<DouyinVideo> BuildAuthorVideoQuery(string authorId, string authorName)
        {
            var query = Db.Queryable<DouyinVideo>();
            if (!string.IsNullOrWhiteSpace(authorId))
                query = query.Where(x => x.AuthorId == authorId);
            else if (!string.IsNullOrWhiteSpace(authorName))
                query = query.Where(x => x.Author == authorName);
            else
                return null;

            return query;
        }

        private static void GetDateBetween(List<string> dates, out DateTime? start, out DateTime? end)
        {
            start = null;
            end = null;
            if (dates != null && dates.Count == 2)
            {
                start = Convert.ToDateTime(dates[0]);
                end = Convert.ToDateTime(dates[1]);
            }
            else if (dates != null && dates.Count == 1)
            {
                start = Convert.ToDateTime(dates[0]);
            }
        }


        /// <summary>
        /// 对于重复标题进行处理
        /// </summary>
        /// <param name="AuthorId"></param>
        /// <param name="ViedoNameSimplify"></param>
        /// <returns></returns>
        public (string, string) GetUperLastViedoFileName(string AuthorId, string ViedoNameSimplify)
        {

            var video = this.Db.Queryable<DouyinVideo>().Where(x => x.AuthorId == AuthorId && x.ViedoType == VideoTypeEnum.dy_follows)
                 .Where(x => x.VideoTitleSimplify == ViedoNameSimplify)
                 .OrderByDescending(x => x.CreateTime).First();

            if (video != null)
            {
                if (string.IsNullOrWhiteSpace(video.VideoTitleSimplifyPrefix))
                {
                    return (ViedoNameSimplify, "001");
                }

                else
                {
                    //VideoTitleSimplifyPrefix的规则是从001开始
                    var prefixNumber = video.VideoTitleSimplifyPrefix.TrimEnd('-');
                    if (int.TryParse(prefixNumber, out int number))
                    {
                        number += 1;
                        return (ViedoNameSimplify, number.ToString("D3"));
                    }
                    else
                    {
                        return (ViedoNameSimplify, "001");
                    }
                }
            }
            else
            {
                return (ViedoNameSimplify, "");
            }
        }

        /// <summary>
        /// 根据视频ID列表获取视频信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<List<DouyinVideo>> GetByIds(List<string> ids)
        {
            // 使用 Queryable 方法构建查询
            return await this.Db.Queryable<DouyinVideo>()
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
        }

        /// <summary>
        /// 记录要重新下载的视频
        /// </summary>
        /// <param name="downs"></param>
        /// <returns></returns>
        public bool InsertReDowns(List<DouyinReDownload> downs)
        {
            if (downs != null)
            {
                return Db.Insertable(downs).ExecuteCommand() > 0;

            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 更新视频重新下载状态
        /// </summary>
        /// <param name="videoId">视频ID（对应ViedoReDown.Id）</param>
        /// <param name="status">状态值（1时会同步更新下载时间）</param>
        /// <returns>是否更新成功（影响行数>0）</returns>
        public async Task<bool> UpdateReDownStatus(string videoId, int status)
        {
            // 校验必填参数（避免无效数据库操作）
            if (string.IsNullOrWhiteSpace(videoId))
            {
                Serilog.Log.Error("更新重新下载状态失败：videoId为空");
                return false;
            }

            // 构建更新条件（统一Where条件，避免重复代码）
            var updateable = Db.Updateable<DouyinReDownload>()
                .Where(it => it.Id == videoId)
                .SetColumns(it => new DouyinReDownload
                {
                    Status = status,
                    UpdateTime = DateTime.Now
                });

            if (status == 1)
            {
                updateable = updateable.SetColumns(it => it.DownTime == DateTime.Now);

            }
            int affectedRows = await updateable.ExecuteCommandAsync();

            // 可选：记录更新结果日志
            if (affectedRows <= 0)
            {
                Serilog.Log.Information("更新重新下载状态无匹配数据：videoId={0}, status={1}", videoId, status);
            }

            return affectedRows > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<DouyinReDownload>> GetViedoReDowns()
        {
            return await this.Db.Queryable<DouyinReDownload>()
                .Where(x => x.Status == 0 || x.Status == 2)
                .ToListAsync();
        }
    }
}
