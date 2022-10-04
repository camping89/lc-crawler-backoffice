namespace LC.Crawler.BackOffice.ProductVariants
{
    public static class ProductVariantConsts
    {
        private const string DefaultSorting = "{0}SKU asc";

        public static string GetDefaultSorting(bool withEntityName)
        {
            return string.Format(DefaultSorting, withEntityName ? "ProductVariant." : string.Empty);
        }

    }
}