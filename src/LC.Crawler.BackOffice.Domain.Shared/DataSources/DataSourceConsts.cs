namespace LC.Crawler.BackOffice.DataSources
{
    public static class DataSourceConsts
    {
        private const string DefaultSorting = "{0}Url asc";

        public static string GetDefaultSorting(bool withEntityName)
        {
            return string.Format(DefaultSorting, withEntityName ? "DataSource." : string.Empty);
        }

    }
}