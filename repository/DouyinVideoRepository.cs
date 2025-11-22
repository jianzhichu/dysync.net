using dy.net.dto;
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
        /// 分页查询收藏视频
        /// </summary>
        /// <param name="pageIndex">页码（从1开始）</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="tag">可选标签过滤</param>
        /// <param name="author">可选作者过滤</param>
        /// <returns>分页结果（视频列表和总数）</returns>
        public async Task<(List<DouyinVideo> list, int totalCount)> GetPagedAsync(int pageIndex, int pageSize, string tag = null, string author = null,string viedoType=null, List<string> dates = null)
        {

            DateTime? start = null;
            DateTime? end =null;
            if(dates!=null && dates.Count==2)
            {
                start = Convert.ToDateTime(dates[0]);
                end = Convert.ToDateTime(dates[1]);
            }
            else if(dates!=null && dates.Count==1)
            {
                start = Convert.ToDateTime(dates[0]);
            }

            var where = this.Db.Queryable<DouyinVideo>()
                .WhereIF(!string.IsNullOrWhiteSpace(tag), x => x.Tag1 == tag)
                .WhereIF(!string.IsNullOrWhiteSpace(author), x => x.Author == author)
                .WhereIF(start.HasValue, x => x.SyncTime >= start.Value)
                .WhereIF(end.HasValue, x => x.SyncTime <= end.Value)
                .WhereIF(!string.IsNullOrWhiteSpace(viedoType) && viedoType != "*", x => x.ViedoType == viedoType);


            var totalCount = await where.CountAsync();
            var list = await where.OrderByDescending(x=>x.SyncTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            if (list.Any()) { 
                var users= await this.Db.Queryable<DouyinUserCookie>().ToListAsync();
                foreach (var item in list)
                {
                    var user= users.FirstOrDefault(x=>x.Id== item.CookieId);
                    if(user!=null)
                    {
                        item.DyUser = user.UserName;
                    }
                }
            }
            return (list, totalCount);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="AuthorId"></param>
        /// <param name="ViedoNameSimplify"></param>
        /// <returns></returns>
        public async  Task<(string, string)> GetUperLastViedoFileName(string AuthorId,string ViedoNameSimplify)
        {

           var video=  await this.Db.Queryable<DouyinVideo>().Where(x => x.AuthorId == AuthorId && x.ViedoType == "3")
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

    }
}
