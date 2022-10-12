using LC.Crawler.BackOffice.CrawlerAccounts;
using LC.Crawler.BackOffice.CrawlerCredentials;
using LC.Crawler.BackOffice.CrawlerProxies;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.MessageQueue.Consumers.Etos;
using LC.Crawler.BackOffice.MessageQueue.Producers.Etos;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace LC.Crawler.BackOffice.MessageQueue.Producers;

public class CrawlerDataSourceProducer : ITransientDependency
{
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly DataSourceManager _dataSourceManager;
    private readonly CrawlerCredentialManager _crawlerCredentialManager;
    private readonly IObjectMapper _objectMapper;

    public CrawlerDataSourceProducer(IDistributedEventBus distributedEventBus, CrawlerCredentialManager crawlerCredentialManager, DataSourceManager dataSourceManager,
        IObjectMapper objectMapper)
    {
        _distributedEventBus = distributedEventBus;
        _crawlerCredentialManager = crawlerCredentialManager;
        _dataSourceManager = dataSourceManager;
        _objectMapper = objectMapper;
    }

    public async Task InitCrawlerDataSourceQueueAsync()
    {
        var feedDataSources = await _dataSourceManager.GetDataSourcesAsync();
        var crawlerDataSourceEto = new CrawlerDataSourceEto();
        crawlerDataSourceEto.Items = feedDataSources.Select(x => new CrawlerDataSourceItem
        {
            Url = x.Url,
            SourceType = SourceType.LC,
            DataSourceType = DataSourceType.Website
        }).ToList();
        
        await _distributedEventBus.PublishAsync(crawlerDataSourceEto);
    }

}