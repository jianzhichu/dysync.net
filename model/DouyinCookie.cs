using dy.net.dto;
using Newtonsoft.Json;
using SqlSugar;

namespace dy.net.model
{
    [SugarTable(TableName = "dy_cookie")]
    public class DouyinCookie
    {

        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; }

        /// <summary>
        /// 用户抖音ID，对应我关注信息里面的myself_user_id
        /// </summary>
        [SugarColumn(Length =500,IsNullable =true)]
        public string MyUserId { get; set; }
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

        /// <summary>
        /// 是否已经同步过了（就是是否是第一次同步） 0-未同步 1-已同步
        /// 如果是0，则同步时会获取全部数据；如果是1，则同步时只获取最新的数据（也就是只查一次）
        /// 接口可以将该值改为0，下次开始同步就会重新获取全部数据
        /// </summary>
        public int CollHasSyncd { get; set; }
        public int FavHasSyncd { get; set; }
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
        [SugarColumn(Length = 500, IsNullable = true)]
        public string ImgSavePath { get; set; }


        //[SugarColumn(IsIgnore = true)]
        //public List<DouyinUpSecUserIdDto> UpSecUserIdsJson
        //{
        //    get
        //    {
        //        // 反序列化逻辑（保持不变）
        //        if (string.IsNullOrWhiteSpace(UpSecUserIds))
        //        {
        //            return new List<DouyinUpSecUserIdDto>();
        //        }

        //        try
        //        {
        //            return JsonConvert.DeserializeObject<List<DouyinUpSecUserIdDto>>(UpSecUserIds);
        //        }
        //        catch (JsonSerializationException ex)
        //        {
        //            // 日志记录（按需添加）
        //            // Logger.Error($"反序列化失败：{ex.Message}，原始值：{UpSecUserIds}");
        //            return new List<DouyinUpSecUserIdDto>();
        //        }
        //        catch (Exception ex)
        //        {
        //            // 日志记录（按需添加）
        //            // Logger.Error($"处理异常：{ex.Message}");
        //            return new List<DouyinUpSecUserIdDto>();
        //        }
        //    }
        //    set
        //    {
        //        // 序列化逻辑：将列表转为JSON字符串，赋值给UpSecUserIds
        //        try
        //        {
        //            // 若传入的列表为null，直接设为空字符串（避免序列化后出现"null"字符串）
        //            UpSecUserIds = value == null
        //                ? string.Empty
        //                : JsonConvert.SerializeObject(value);
        //        }
        //        catch (JsonSerializationException ex)
        //        {
        //            // 捕获序列化异常（如对象循环引用、不支持的类型等）
        //            // Logger.Error($"序列化失败：{ex.Message}，列表值：{value}");
        //            // 异常时默认设为空字符串，避免存储错误的JSON
        //            UpSecUserIds = string.Empty;
        //        }
        //        catch (Exception ex)
        //        {
        //            // 捕获其他未知异常
        //            // Logger.Error($"设置值异常：{ex.Message}");
        //            UpSecUserIds = string.Empty;
        //        }
        //    }
        //}

    }
}
