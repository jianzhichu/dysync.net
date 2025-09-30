using System.ComponentModel.DataAnnotations;

namespace dy.net.dto
{
    public class VideoPageRequestDTO: PageRequestDto
    {
        // 3. 引用类型（string）如果允许为null，显式声明为 string?
        public string? Tag { get; set; }

        public string? Author { get; set; }

        public string? ViedoType { get; set; }

        // 4. 泛型集合（List<string>）允许为null，声明为 List<string>?
        public List<string>? Dates { get; set; }
    }


    public class PageRequestDto
    {

        public int PageIndex { get; set; }

        public int PageSize { get; set; } = 10;
    }
}
