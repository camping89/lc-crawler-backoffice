using Volo.Abp.Application.Dtos;
using System;
using LC.Crawler.BackOffice.Enums;

namespace LC.Crawler.BackOffice.CrawlerCredentials
{
    public class GetCrawlerCredentialsInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }

        public DataSourceType? DataSourceType { get; set; }
        public DateTime? CrawledAtMin { get; set; }
        public DateTime? CrawledAtMax { get; set; }
        public bool? IsAvailable { get; set; }
        public Guid? CrawlerAccountId { get; set; }
        public Guid? CrawlerProxyId { get; set; }

        public GetCrawlerCredentialsInput()
        {

        }
    }
}