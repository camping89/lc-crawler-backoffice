namespace LC.Crawler.BackOffice.CrawlerProxies
{
    public static class CrawlerProxyConsts
    {
        private const string DefaultSorting = "{0}Ip asc";

        public static string GetDefaultSorting(bool withEntityName)
        {
            return string.Format(DefaultSorting, withEntityName ? "CrawlerProxy." : string.Empty);
        }

    }
}