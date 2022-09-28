using LC.Crawler.BackOffice.CrawlerAccounts;
using LC.Crawler.BackOffice.CrawlerProxies;

using System;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.CrawlerCredentials
{
    public class CrawlerCredentialWithNavigationProperties
    {
        public CrawlerCredential CrawlerCredential { get; set; }

        public CrawlerAccount CrawlerAccount { get; set; }
        public CrawlerProxy CrawlerProxy { get; set; }
        

        
    }
}