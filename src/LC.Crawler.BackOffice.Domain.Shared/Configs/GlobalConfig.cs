namespace LC.Crawler.BackOffice.Configs;

public class GlobalConfig
{
    //TODO Define config values
    
    public class Crawler
    {
        public const int Batch                   = 5;
        public const int CrawlingInterval        = 15;
        public const int DefaultCrawlingInterval = 60;
        public const int CrawlerAccountPerProxy  = 10;
        public const int SyncTimeOutInHours      = 24;

        public const int SyncTimeHours = 17;
        public const int ReSyncTimeHours = 17;
    }
}


