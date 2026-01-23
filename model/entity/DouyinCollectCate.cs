using dy.net.model.dto;
using SqlSugar;

namespace dy.net.model.entity
{

    /// <summary>
    /// 收藏夹-自定义收藏夹、合集、短剧
    /// </summary>
    [SugarTable(TableName = "dy_collect_cate")]
    public class DouyinCollectCate
    {
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; }

        /// <summary>
        /// 关联cookieId
        /// </summary>
        public string CookieId { get; set; }

        /// <summary>
        /// 收藏夹名称
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = false)]
        public string Name { get; set; }
        /// <summary>
        /// 收藏夹Id、合集Id、短剧Id
        /// </summary>
        [SugarColumn(Length = 60, IsNullable = false)]
        public string XId { get; set; }

        /// <summary>
        /// 封面
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string CoverUrl { get; set; }
        /// <summary>
        /// 保存文件夹
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string SaveFolder { get; set; }
        /// <summary>
        /// 是否开启同步
        /// </summary>
        public bool Sync { get; set; }

        /// <summary>
        /// 收藏夹类型（5：自定义收藏夹，6：合集，7：短剧）
        /// </summary>
        public VideoTypeEnum CateType { get; set; }

        public DateTime CreateTime { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 是否已完结
        /// </summary>
        public bool IsEnd { get; set; }

        /// <summary>
        /// 总集数
        /// </summary>
        public int Total { get; set; }
    }
}
