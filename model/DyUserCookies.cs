using SqlSugar;

namespace dy.net.model
{
    [SugarTable(TableName = "dy_cookie")]
    public class DyUserCookies
    {

        [SqlSugar.SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; }
        /// <summary>
        /// 用户账号-唯一就行，手机号啥的
        /// </summary>

        //public string UserId { get; set; }
        /// <summary>
        /// 用户描述
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户抖音Cookie
        /// </summary>
        [SugarColumn(Length =-1,IsNullable =true)]
        public string Cookies { get; set; }
        /// <summary>
        /// 存储路径
        /// </summary>
        [SugarColumn(Length =255,IsNullable =true)]
        public string SavePath { get; set; }

        public int  Status { get; set; }

        /// <summary>
        /// 同步喜欢的视频需要sec_user_id
        /// </summary>
        [SugarColumn(Length =500,IsNullable =true)]
        public string SecUserId { get; set; }

        /// <summary>
        /// 喜欢的视频存储路径
        /// </summary>
        [SugarColumn(Length =500,IsNullable =true)]
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
    }
}
