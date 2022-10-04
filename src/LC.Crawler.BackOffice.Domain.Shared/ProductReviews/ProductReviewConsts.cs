namespace LC.Crawler.BackOffice.ProductReviews
{
    public static class ProductReviewConsts
    {
        private const string DefaultSorting = "{0}Name asc";

        public static string GetDefaultSorting(bool withEntityName)
        {
            return string.Format(DefaultSorting, withEntityName ? "ProductReview." : string.Empty);
        }

    }
}