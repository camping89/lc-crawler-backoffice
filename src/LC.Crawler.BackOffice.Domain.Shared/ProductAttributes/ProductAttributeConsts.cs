namespace LC.Crawler.BackOffice.ProductAttributes
{
    public static class ProductAttributeConsts
    {
        private const string DefaultSorting = "{0}Slug asc";

        public static string GetDefaultSorting(bool withEntityName)
        {
            return string.Format(DefaultSorting, withEntityName ? "ProductAttribute." : string.Empty);
        }

    }
}