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
        /// 收藏夹Id
        /// </summary>
        [SugarColumn(Length =60,IsNullable =false)]
        public string XId { get; set; }

        /// <summary>
        /// 封面
        /// </summary>
        public string CoverUrl { get; set; }
        /// <summary>
        /// 保存文件夹
        /// </summary>
        public string SaveFolder { get; set; }
        /// <summary>
        /// 是否开启同步
        /// </summary>
        public bool Sync { get; set; }

        /// <summary>
        /// 收藏夹类型（0：自定义收藏夹，1：合集，2：短剧）
        /// </summary>
        public int CateType { get; set; }

        public DateTime CreateTime  { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}
