namespace dy.net.dto
{
    public class VideoStaticsItemDto
    {
        public string Name { get; set; }

        public long Count { get; set; }

        public string Color { get; set; }

        public string Icon { get; set; }

    }



    public class VideoStaticsDto
    {

        public long VideoCount { get; set; }

        public long AuthorCount { get; set; }

        public long CategoryCount { get; set; }

        public double ViedoSizeTotal { get; set; }

        public long CollectCount { get; set; }
        public long FavoriteCount { get; set; }

        public List<VideoStaticsItemDto> Authors { get; set; } = new List<VideoStaticsItemDto>();
        public List<VideoStaticsItemDto> Categories { get; set; } = new List<VideoStaticsItemDto>();
    }
}
