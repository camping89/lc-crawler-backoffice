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
        var credentials = await _crawlerCredentialManager.GetValidCredentials();
        credentials = credentials.Where(properties => properties.CrawlerAccount.AccountType is AccountType.FacebookGroupPost)
            .ToList();
        
        var crawlerDataSources = ArrangeCrawlerDataSource(feedDataSources, credentials);

        foreach (var crawlerDataSourceEto in crawlerDataSources)
        {
            await _distributedEventBus.PublishAsync(crawlerDataSourceEto);
        }

        await _crawlerCredentialManager.UpdateMany(credentials.Select(nav => nav.CrawlerCredential).ToList());
    }

    private List<CrawlerDataSourceEto> ArrangeCrawlerDataSource(IEnumerable<DataSource> dataSources, List<CrawlerCredentialWithNavigationProperties> credentials)
    {
        var crawlerDataSourceEtos = new List<CrawlerDataSourceEto>();
        foreach (var dataSource in dataSources)
        {
            var credential = credentials.FirstOrDefault(nav => nav.CrawlerCredential.IsAvailable);
            if (credential != null)
            {
                var crawlerCredentialEto = _objectMapper.Map<CrawlerCredential, CredentialEto>(credential.CrawlerCredential);
                var crawlerAccountEto = _objectMapper.Map<CrawlerAccount, AccountEto>(credential.CrawlerAccount);
                var crawlerProxyEto = _objectMapper.Map<CrawlerProxy, ProxyEto>(credential.CrawlerProxy);
                
                var credentialEto = new CrawlerCredentialEto(crawlerCredentialEto, crawlerAccountEto, crawlerProxyEto);

                crawlerDataSourceEtos.Add(new CrawlerDataSourceEto(dataSource.Url, credentialEto));
                
                credential.CrawlerCredential.IsAvailable = false;
            }
        }

        return crawlerDataSourceEtos;
    }
}