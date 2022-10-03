namespace LC.Crawler.BackOffice.Categories
{
    public static class CategoryConsts
    {
        private const string DefaultSorting = "{0}Name asc";

        public static string GetDefaultSorting(bool withEntityName)
        {
            return string.Format(DefaultSorting, withEntityName ? "Category." : string.Empty);
        }

    }
}