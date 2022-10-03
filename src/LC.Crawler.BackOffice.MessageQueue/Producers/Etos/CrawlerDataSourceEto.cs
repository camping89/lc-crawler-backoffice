using LC.Crawler.BackOffice.MessageQueue.Consumers.Etos;
using Volo.Abp.EventBus;

namespace LC.Crawler.BackOffice.MessageQueue.Producers.Etos;

[EventName("LC.CrawlerDataSourceEto")]
public class CrawlerDataSourceEto
{
    public CrawlerDataSourceEto(string url, CrawlerCredentialEto credential)
    {
        Url = url;
        Credential = credential;
    }
    public string Url { get; set; }
    public CrawlerCredentialEto Credential { get; set; }
}