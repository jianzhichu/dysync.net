using SqlSugar;

namespace dy.net.model.entity
{
    [SugarTable(TableName = "dy_cookie")]
    public class DouyinCookie
    {

        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; }

        /// <summary>
        /// 用户抖音ID，对应我关注信息里面的myself_user_id
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string MyUserId { get; set; }
        /// <summary>
        /// 用户描述
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户抖音Cookie
        /// </summary>
        [SugarColumn(Length = -1, IsNullable = true)]
        public string Cookies { get; set; }
        /// <summary>
        /// 存储路径（收藏视频的存储路径）
        /// </summary>
        [SugarColumn(Length = 255, IsNullable = true)]
        public string SavePath { get; set; }

        /// <summary>
        /// 是否按收藏夹文件夹来存储视频
        /// </summary>
        public bool UseCollectFolder { get; set; }

        /// <summary>
        /// 是否下载合集
        /// </summary>
        public bool DownMix { get; set; }
        /// <summary>
        /// 是否下载短剧
        /// </summary>
        public bool DownSeries { get; set; }
        /// <summary>
        /// 是否下载收藏夹视频
        /// </summary>
        public bool DownCollect { get; set; }
   
        /// <summary>
        /// 是否下载点赞（喜欢的）视频
        /// </summary>
        public bool DownFavorite { get; set; }
        /// <summary>
        /// 是否下载关注的博主的作品
        /// </summary>
        public bool DownFollowd { get; set; }
        /// <summary>
        /// 1 开启同步 0 关闭同步
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 同步喜欢的视频需要sec_user_id
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string SecUserId { get; set; }

        /// <summary>
        /// 喜欢的视频存储路径
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string FavSavePath { get; set; }

        ///// <summary>
        ///// 最新收藏夹的分页页码
        ///// </summary>
        //[SugarColumn(Length =500,IsNullable =true)]
        //public string CollectMaxCursor { get; set; }
        ///// <summary>
        ///// 最新我喜欢的分页页码
        ///// </summary>
        //[SugarColumn(Length = 500, IsNullable = true)]
        //public string FavoriteMaxCursor { get; set; }

        /// <summary>
        /// 是否已经同步过了（就是是否是第一次同步） 0-未同步 1-已同步
        /// 如果是0，则同步时会获取全部数据；如果是1，则同步时只获取最新的数据（也就是只查一次）
        /// 接口可以将该值改为0，下次开始同步就会重新获取全部数据
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int CollHasSyncd { get; set; }

        [SugarColumn(IsNullable = true)]
        public int FavHasSyncd { get; set; }

        [SugarColumn(IsNullable = true)]
        public int UperSyncd { get; set; }
        /// <summary>
        /// 关注的用户sec_user_id列表 json字符串存储
        /// </summary>
        //[SugarColumn(Length =-1,IsNullable =true)]
        //public string UpSecUserIds { get; set; }


        /// <summary>
        /// Up主发布的视频存储路径
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string UpSavePath { get; set; }


        /// <summary>
        /// 图片视频存储路径
        /// </summary>
        //[SugarColumn(Length = 500, IsNullable = true)]
        //public string ImgSavePath { get; set; }

        /// <summary>
        /// 抖音返回的状态码，主要用于判断Cookie是否有效
        /// 0 正常 8 - 用户未登录
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string StatusMsg { get; set; }

        /// <summary>
        /// 是否统一一个路径(savepath)
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "useSinglePath")]
        public bool UseSinglePath { get; set; } = true;//默认true

        /// <summary>
        /// 合集存储路径
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string MixPath { get; set; }

        /// <summary>
        /// 短剧存储路径
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string SeriesPath { get; set; }
    }
}
