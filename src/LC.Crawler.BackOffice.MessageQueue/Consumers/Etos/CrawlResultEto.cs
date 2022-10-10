using LC.Crawler.BackOffice.Enums;
using Veek.DataProvider.Crawler.Client.Entities;
using Volo.Abp.EventBus;

namespace LC.Crawler.BackOffice.MessageQueue.Consumers.Etos;

[EventName("Veek.DataProvider.Social.CrawlDataResultEto")]
public class CrawlResultEto
{
    public List<CrawlPayload> Items { get; set; }
    public DataSourceType DataSourceType { get; set; }
    public SourceType SourceType { get; set; }
    public CrawlerCredentialEto Credential { get; set; }
    
    public string DomainSite { get; set; }
}