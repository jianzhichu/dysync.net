using System.ComponentModel;

namespace dy.net.dto
{
    public enum VideoTypeEnum
    {
        [Description("喜欢的")]
        Favorite = 1,
        [Description("收藏的")]
        Collect = 2,
        [Description("关注的")]
        UperPost = 3,
        [Description("图片视频")]
        ImageVideo = 4
    }
}
