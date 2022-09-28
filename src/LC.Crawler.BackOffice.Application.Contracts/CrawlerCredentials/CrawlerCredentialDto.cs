using System;
using LC.Crawler.BackOffice.Enums;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.CrawlerCredentials
{
    public class CrawlerCredentialDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
    {
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public DataSourceType DataSourceType { get; set; }
        public DateTime? CrawledAt { get; set; }
        public bool IsAvailable { get; set; }
        public Guid? CrawlerAccountId { get; set; }
        public Guid? CrawlerProxyId { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}