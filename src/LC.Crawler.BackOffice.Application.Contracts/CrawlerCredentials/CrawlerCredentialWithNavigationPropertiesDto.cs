using LC.Crawler.BackOffice.CrawlerAccounts;
using LC.Crawler.BackOffice.CrawlerProxies;

using System;
using Volo.Abp.Application.Dtos;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.CrawlerCredentials
{
    public class CrawlerCredentialWithNavigationPropertiesDto
    {
        public CrawlerCredentialDto CrawlerCredential { get; set; }

        public CrawlerAccountDto CrawlerAccount { get; set; }
        public CrawlerProxyDto CrawlerProxy { get; set; }

    }
}