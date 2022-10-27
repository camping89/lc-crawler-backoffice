using System;
using LC.Crawler.BackOffice.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace LC.Crawler.BackOffice.TrackingDataSources;

public class TrackingDataSource : AuditedEntity<Guid>
{
    public string Url { get; set; }
    public PageDataSource PageDataSource { get; set; }
    public CrawlType CrawlType { get; set; }
    public string Error { get; set; }
}