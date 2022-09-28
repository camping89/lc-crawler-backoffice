namespace LC.Crawler.BackOffice.CrawlerAccounts
{
    public static class CrawlerAccountConsts
    {
        private const string DefaultSorting = "{0}Username asc";

        public static string GetDefaultSorting(bool withEntityName)
        {
            return string.Format(DefaultSorting, withEntityName ? "CrawlerAccount." : string.Empty);
        }

    }
}