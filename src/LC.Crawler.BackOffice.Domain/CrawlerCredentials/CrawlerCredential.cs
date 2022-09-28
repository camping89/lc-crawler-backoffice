using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using LC.Crawler.BackOffice.Enums;

namespace LC.Crawler.BackOffice.CrawlerCredentials
{
    public class CrawlerCredential : FullAuditedEntity<Guid>, IHasConcurrencyStamp
    {
        public virtual DataSourceType DataSourceType { get; set; }

        public virtual DateTime? CrawledAt { get; set; }

        public virtual bool IsAvailable { get; set; }
        public Guid? CrawlerAccountId { get; set; }
        public Guid? CrawlerProxyId { get; set; }

        public string ConcurrencyStamp { get; set; }

        public CrawlerCredential()
        {

        }

        public CrawlerCredential(Guid id, Guid? crawlerAccountId, Guid? crawlerProxyId, DataSourceType dataSourceType, bool isAvailable, DateTime? crawledAt = null)
        {
            ConcurrencyStamp = Guid.NewGuid().ToString("N");
            Id = id;
            DataSourceType = dataSourceType;
            IsAvailable = isAvailable;
            CrawledAt = crawledAt;
            CrawlerAccountId = crawlerAccountId;
            CrawlerProxyId = crawlerProxyId;
        }

    }
}