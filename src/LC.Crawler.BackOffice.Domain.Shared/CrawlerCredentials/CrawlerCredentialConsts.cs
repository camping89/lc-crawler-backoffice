namespace LC.Crawler.BackOffice.CrawlerCredentials
{
    public static class CrawlerCredentialConsts
    {
        private const string DefaultSorting = "{0}DataSourceType asc";

        public static string GetDefaultSorting(bool withEntityName)
        {
            return string.Format(DefaultSorting, withEntityName ? "CrawlerCredential." : string.Empty);
        }

    }
}