namespace dy.net.model.dto
{
    public class DouyinCookieSwitchDto
    {
        public string Id { get; set; }

        public int Status { get; set; }
    }

    public class DouyinCollectCateSwitchDto
    {
        public string Id { get; set; }

        public bool Sync { get; set; }

        public string SaveFolder { get; set; }
    }
}
