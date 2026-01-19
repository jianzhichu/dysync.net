using dy.net.model.entity;

namespace dy.net.model.dto
{
    public class AppConfigImportDto
    {

        public List<DouyinFollowed> follows { get; set; }

        public AppConfig conf { get; set; }

        public List<DouyinCookie> cookies { get; set; }
    }
}
