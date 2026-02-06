using SqlSugar;

namespace dy.net.model.entity
{
    [SugarTable(TableName = "dy_follow")]
    public class DouyinFollowed
    {

        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; }

        /// <summary>
        /// 博主sec_uid
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 500)]
        public string SecUid { get; set; }

        [SugarColumn(IsNullable = false, Length = 200)]
        public string UperName { get; set; }

        [SugarColumn(IsNullable = true, Length = 1000)]
        public string UperAvatar { get; set; }
        /// <summary>
        /// 官方账号(企业认证)
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 200)]
        public string Enterprise { get; set; }
        /// <summary>
        /// 是否开启同步
        /// </summary>
        public bool OpenSync { get; set; }
        /// <summary>
        /// 是否全量同步
        /// </summary>
        public bool FullSync { get; set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastSyncTime { get; set; }

        /// <summary>
        /// 我的userId,关注者的抖音userid
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 200)]
        public string mySelfId { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 500)]
        public string Signature { get; set; }

        /// <summary>
        /// 同步文件保存路径
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 500)]
        public string SavePath { get; set; }

        /// <summary>
        /// 博主Id
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 100)]
        public string UperId { get; set; }

        /// <summary>
        /// 标记为非关注的用户-手动增加的
        /// </summary>
        public bool IsNoFollowed { get; set; }

        [SugarColumn(IsNullable = true, Length = 100)]
        public string DouyinNo { get; set; }
    }
}
