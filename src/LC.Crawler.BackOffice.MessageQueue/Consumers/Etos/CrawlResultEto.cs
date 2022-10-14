using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Payloads;
using Volo.Abp.EventBus;

namespace LC.Crawler.BackOffice.MessageQueue.Consumers.Etos;

[EventName("Veek.DataProvider.Social.CrawlDataResultEto")]
public class CrawlResultEto
{
    public CrawlEcommercePayload EcommercePayloads { get; set; }
    public CrawlArticlePayload ArticlePayloads { get; set; }
    public DataSourceType DataSourceType { get; set; }
    public SourceType SourceType { get; set; }
    public CrawlerCredentialEto Credential { get; set; }
    
}