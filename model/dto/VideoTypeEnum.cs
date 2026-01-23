namespace dy.net.model.dto
{
    public enum VideoTypeEnum
    {
        /// <summary>
        ///喜欢的
        /// </summary>
        //[Description("喜欢的")]
        dy_favorite = 1,
        /// <summary>
        ///收藏的
        /// </summary>
        //[Description("收藏的")]
        dy_collects = 2,
        /// <summary>
        /// 关注的
        /// </summary>
        //[Description("关注的")]
        dy_follows = 3,
        /// <summary>
        ///  图文视频，无用了，但是不能删...
        /// </summary>
        //[Description("图片视频")]
        ImageVideo = 4,

        /// <summary>
        /// 自定义收藏夹
        /// </summary>
        //[Description("收藏短剧")]
        dy_custom_collect = 5,
        /// <summary>
        /// 收藏合集
        /// </summary>
        //[Description("收藏合集")]
        dy_mix = 6,
        /// <summary>
        /// 收藏短剧
        /// </summary>
        //[Description("收藏短剧")]
        dy_series = 7,

    }

}
