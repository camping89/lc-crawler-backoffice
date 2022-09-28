using System;
using LC.Crawler.BackOffice.Enums;

namespace LC.Crawler.BackOffice.CrawlerCredentials
{
    public class CrawlerCredentialCreateDto
    {
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public DataSourceType DataSourceType { get; set; } = ((DataSourceType[])Enum.GetValues(typeof(DataSourceType)))[0];
        public DateTime? CrawledAt { get; set; }
        public bool IsAvailable { get; set; }
        public Guid? CrawlerAccountId { get; set; }
        public Guid? CrawlerProxyId { get; set; }
    }
}