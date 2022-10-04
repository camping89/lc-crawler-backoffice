namespace LC.Crawler.BackOffice.ArticleComments
{
    public static class ArticleCommentConsts
    {
        private const string DefaultSorting = "{0}Name asc";

        public static string GetDefaultSorting(bool withEntityName)
        {
            return string.Format(DefaultSorting, withEntityName ? "ArticleComment." : string.Empty);
        }

    }
}