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
    private readonly CrawlerCredentialManager _crawlerCredentialManager;
    private readonly IDataSourceRepository _dataSourceRepository;
    private readonly IObjectMapper _objectMapper;

    public CrawlerDataSourceProducer(IDistributedEventBus distributedEventBus, CrawlerCredentialManager crawlerCredentialManager,
        IObjectMapper objectMapper,
        IDataSourceRepository dataSourceRepository)
    {
        _distributedEventBus = distributedEventBus;
        _crawlerCredentialManager = crawlerCredentialManager;
        _objectMapper = objectMapper;
        _dataSourceRepository = dataSourceRepository;
    }

    public async Task InitCrawlerDataSourceQueueAsync()
    {
        var feedDataSources = await _dataSourceRepository.GetListAsync(x=>x.IsActive);
        
        if (feedDataSources.Any())
        {
            foreach (var dataSource in feedDataSources)
            {
                var crawlerDataSourceEto = new CrawlerDataSourceEto
                {
                    Id = Guid.NewGuid(),
                    CreatedAtUtc = DateTime.UtcNow,
                    Items = new List<CrawlerDataSourceItem>()
                    {
                        new()
                        {
                            Url = dataSource.Url,
                            SourceType = SourceType.LC,
                            DataSourceType = DataSourceType.Website
                        }
                    }
                };

                await _distributedEventBus.PublishAsync(crawlerDataSourceEto);
            }
        }
    }

}