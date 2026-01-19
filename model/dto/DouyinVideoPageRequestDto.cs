using System.ComponentModel.DataAnnotations;

namespace dy.net.model.dto
{
    public class DouyinVideoPageRequestDto : PageRequestDto
    {
        /// <summary>
        /// 标题查询。
        /// </summary>
        public string? Title { get; set; }

        public string? Author { get; set; }
        //public string? Name { get; set; }

        public string? ViedoType { get; set; }

        public List<string>? Dates { get; set; }
        public List<string>? Dates2 { get; set; }

        public string? Tag { get; set; }

    }


    public class PageRequestDto
    {

        public int PageIndex { get; set; }

        public int PageSize { get; set; } = 10;
    }


    public class FollowRequestDto: PageRequestDto
    {
        public string FollowUserName { get; set; }

        public string MySelfId { get; set; }
    }

    public class FollowUpdateDto
    {

        public string Id { get; set; }

        public bool OpenSync { get; set; }

        public bool FullSync { get; set; }

        public string SavePath { get; set; }

        public string UperId { get; set; }

    }



    public class DouyinCollectCateRequestDto : PageRequestDto
    {

        public string cookieId { get; set; }
        public VideoTypeEnum cateType { get; set; }
    }
}
