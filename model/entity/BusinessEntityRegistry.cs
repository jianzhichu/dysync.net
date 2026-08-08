namespace dy.net.model.entity
{
    /// <summary>
    /// Explicit business schema allow-list. Quartz entities must never be passed to
    /// SqlSugar CodeFirst because their schema is owned by QuartzSchemaInitializer.
    /// </summary>
    public static class BusinessEntityRegistry
    {
        public static readonly Type[] Types =
        {
            typeof(AdminUserInfo),
            typeof(AppConfig),
            typeof(DouyinCollectCate),
            typeof(DouyinCookie),
            typeof(DouyinFollowed),
            typeof(DouyinReDownload),
            typeof(DouyinVideo),
            typeof(DouyinVideoDelete),
            typeof(DouyinVideoUp)
        };
    }
}
