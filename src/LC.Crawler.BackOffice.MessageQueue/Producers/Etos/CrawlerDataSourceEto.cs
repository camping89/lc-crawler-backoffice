using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.MessageQueue.Consumers.Etos;
using Volo.Abp.EventBus;

namespace LC.Crawler.BackOffice.MessageQueue.Producers.Etos;


[EventName("Veek.DataProvider.Social.CrawlerDataSourceEto")]
public class CrawlerDataSourceEto
{
    public Guid                        Id         { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<CrawlerDataSourceItem> Items      { get; set; }
    public CrawlerCredentialEto        Credential { get; set; }
}

public class CrawlerDataSourceItem
{
    public string         Url            { get; set; }
    public SourceType     SourceType     { get; set; }
    public DataSourceType DataSourceType { get; set; }
    public DateTime StopDateTime { get; set; }
    public string SourceId { get; set; }
}
