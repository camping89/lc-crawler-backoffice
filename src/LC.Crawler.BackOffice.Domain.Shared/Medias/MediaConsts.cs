namespace LC.Crawler.BackOffice.Medias
{
    public static class MediaConsts
    {
        private const string DefaultSorting = "{0}Name asc";

        public static string GetDefaultSorting(bool withEntityName)
        {
            return string.Format(DefaultSorting, withEntityName ? "Media." : string.Empty);
        }

    }
}