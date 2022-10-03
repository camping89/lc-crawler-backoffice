namespace LC.Crawler.BackOffice.Articles
{
    public static class ArticleConsts
    {
        private const string DefaultSorting = "{0}Title asc";

        public static string GetDefaultSorting(bool withEntityName)
        {
            return string.Format(DefaultSorting, withEntityName ? "Article." : string.Empty);
        }

    }
}