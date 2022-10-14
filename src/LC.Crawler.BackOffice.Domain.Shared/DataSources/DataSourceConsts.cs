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

    public static class PageDataSourceConsts
    {
        
        public const string LongChauUrl = "nhathuoclongchau.com";
        public const string AladinUrl = "aladin.com.vn";
        public const string SieuThiSongKhoeUrl = "sieuthisongkhoe.com";
        public const string SucKhoeDoiSongUrl = "suckhoedoisong.vn";
        public const string SongKhoeMedplusUrl = "songkhoe.medplus.vn";
        public const string BlogSucKhoeUrl = "blogsuckhoe.com";
        public const string SucKhoeGiaDinhUrl = "suckhoegiadinh.com.vn";
        public const string AloBacSiUrl = "alobacsi.com";
    }
}