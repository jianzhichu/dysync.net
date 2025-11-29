using dy.net.dto;
using dy.net.extension;
using dy.net.model;
using Quartz.Util;
using SqlSugar;
using System.Linq;

namespace dy.net.repository
{
    public class DouyinVideoRepository : BaseRepository<DouyinVideo>
    {
        // 注入SQLSugar客户端
        public DouyinVideoRepository(ISqlSugarClient db) : base(db)
        {
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
            if (!string.IsNullOrEmpty(dto.ViedoType) && dto.ViedoType != "*")
            {
                enumviedoType = dto.ViedoType.ToVideoTypeEnum();
            }
            var where = this.Db.Queryable<DouyinVideo>()
                //.WhereIF(!string.IsNullOrWhiteSpace(title), x => x.VideoTitle.Contains(title))
                .WhereIF(!string.IsNullOrWhiteSpace(dto.Title), x => x.VideoTitle.Contains(dto.Title))
                .WhereIF(!string.IsNullOrWhiteSpace(dto.Author), x => x.Author == dto.Author)
                .WhereIF(start.HasValue, x => x.SyncTime >= start.Value)
                .WhereIF(end.HasValue, x => x.SyncTime <= end.Value)
                .WhereIF(start2.HasValue, x => x.CreateTime >= start2.Value)
                .WhereIF(end2.HasValue, x => x.CreateTime <= end2.Value)
                .WhereIF(enumviedoType.HasValue, x => x.ViedoType == enumviedoType);


            var totalCount = await where.CountAsync();
            var list = await where.OrderByDescending(x => x.SyncTime).Skip((dto.PageIndex - 1) * dto.PageSize).Take(dto.PageSize).ToListAsync();
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
        public async Task<(string, string)> GetUperLastViedoFileName(string AuthorId, string ViedoNameSimplify)
        {

            var video = await this.Db.Queryable<DouyinVideo>().Where(x => x.AuthorId == AuthorId && x.ViedoType == VideoTypeEnum.UperPost)
                 .Where(x => x.VideoTitleSimplify == ViedoNameSimplify)
                 .OrderByDescending(x => x.CreateTime).FirstAsync();

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
        public bool InsertReDowns(List<ViedoReDown> downs)
        {
            if (downs != null)
            {
                return  Db.Insertable(downs).ExecuteCommand() > 0;

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
            var updateable = Db.Updateable<ViedoReDown>()
                .Where(it => it.Id == videoId)
                .SetColumns(it => new ViedoReDown
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
                Serilog.Log.Debug("更新重新下载状态无匹配数据：videoId={0}, status={1}", videoId, status);
            }

            return affectedRows > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<ViedoReDown>> GetViedoReDowns()
        {
            return await this.Db.Queryable<ViedoReDown>()
                .Where(x => x.Status == 0 || x.Status == 2)
                .ToListAsync();
        }
    }
}
