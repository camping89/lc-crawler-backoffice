namespace LC.Crawler.BackOffice.ProductComments
{
    public static class ProductCommentConsts
    {
        private const string DefaultSorting = "{0}Name asc";

        public static string GetDefaultSorting(bool withEntityName)
        {
            return string.Format(DefaultSorting, withEntityName ? "ProductComment." : string.Empty);
        }

    }
}