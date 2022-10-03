using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.CrawlerAccounts;
using LC.Crawler.BackOffice.CrawlerCredentials;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.MessageQueue.Consumers.Etos;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace LC.Crawler.BackOffice.MessageQueue.Consumers;

public class CrawlerDataReceiveConsumer : IDistributedEventHandler<CrawlResultEto>, ITransientDependency
{
    private readonly DataSourceManager _dataSourceManager;
    private readonly CrawlerCredentialManager _crawlerCredentialManager;
    private readonly CrawlerAccountManager _crawlerAccountManager;
    private readonly IArticleLongChauRepository _articleLongChauRepository;
    private readonly IObjectMapper _objectMapper;

    public CrawlerDataReceiveConsumer(DataSourceManager dataSourceManager,
        CrawlerCredentialManager crawlerCredentialManager, CrawlerAccountManager crawlerAccountManager,
        IObjectMapper objectMapper,
        IArticleLongChauRepository articleLongChauRepository)
    {
        _dataSourceManager = dataSourceManager;
        _crawlerCredentialManager = crawlerCredentialManager;
        _crawlerAccountManager = crawlerAccountManager;
        _objectMapper = objectMapper;
        _articleLongChauRepository = articleLongChauRepository;
    }

    public async Task HandleEventAsync(CrawlResultEto eventData)
    {
        if (eventData.Items.Any())
        {
            var articles = _objectMapper.Map<List<CrawlPayload>, List<Article>>(eventData.Items);
            //TODO: Dựa vào urlsite trả về từ Crawler để xác định lưu vào DB nào
            if (true) // long chau
            {
                await _articleLongChauRepository.InsertManyAsync(articles);
            }
        }

        if (eventData.Credential is not null)
        {
            var crawlerCredential =
                _objectMapper.Map<CredentialEto, CrawlerCredential>(eventData.Credential.CrawlerCredential);
            await _crawlerCredentialManager.ResetCrawlerInformation(crawlerCredential);

            var crawlerAccount = _objectMapper.Map<AccountEto, CrawlerAccount>(eventData.Credential.CrawlerAccount);
            await _crawlerAccountManager.UpdateAccountStatus(crawlerAccount);
        }
    }

}