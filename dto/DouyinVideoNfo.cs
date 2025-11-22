namespace dy.net.dto
{

    // Actor.cs
    public class Actor
    {
        public string Name { get; set; }
        public string Role { get; set; } // 角色名称
        public string Thumb { get; set; } // 演员头像 URL 或路径
    }
    public class DouyinVideoNfo
    {

        /// 视频名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 封面图片路径或URL
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// 海报图片路径或URL
        /// </summary>
        public string Poster { get; set; }

        /// <summary>
        /// 作者/创作者
        /// </summary>
        public string Author { get; set; }

        // 新增的属性
        public DateTime? ReleaseDate { get; set; } // 可空，避免无发布时间时的默认值
        public IEnumerable<string> Genres { get; set; } // 分类标签集合（如 ["动作", "科幻"]）

        public List<Actor> Actors { get; set; }
    }
}
