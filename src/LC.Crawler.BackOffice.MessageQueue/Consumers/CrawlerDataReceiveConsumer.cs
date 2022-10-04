using System.Text;
using HtmlAgilityPack;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.CrawlerAccounts;
using LC.Crawler.BackOffice.CrawlerCredentials;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Medias;
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
    private readonly IMediaRepository _mediaRepository;
    private readonly IObjectMapper _objectMapper;

    public CrawlerDataReceiveConsumer(DataSourceManager dataSourceManager,
        CrawlerCredentialManager crawlerCredentialManager, CrawlerAccountManager crawlerAccountManager,
        IObjectMapper objectMapper,
        IArticleLongChauRepository articleLongChauRepository,
        IMediaRepository mediaRepository)
    {
        _dataSourceManager = dataSourceManager;
        _crawlerCredentialManager = crawlerCredentialManager;
        _crawlerAccountManager = crawlerAccountManager;
        _objectMapper = objectMapper;
        _articleLongChauRepository = articleLongChauRepository;
        _mediaRepository = mediaRepository;
    }

    public async Task HandleEventAsync(CrawlResultEto eventData)
    {
        if (eventData.Items.Any())
        {
            var articles = _objectMapper.Map<List<CrawlPayload>, List<Article>>(eventData.Items);
            foreach (var article in articles)
            {
                if (!string.IsNullOrEmpty(article.Content))
                {
                    var mediaUrls = article.Content.GetImageUrls();

                    if (mediaUrls.Any())
                    {
                        var medias = mediaUrls.Select(url => new Media()
                        {
                            Url = url
                        }).ToList();
                        await _mediaRepository.InsertManyAsync(medias);

                        article.Content = ReplaceImageUrls(article.Content, medias);
                        
                        foreach (var media in medias)
                        {
                            article.AddMedia(media.Id);
                        }
                    }
                }
            }
            // //TODO: Dựa vào urlsite trả về từ Crawler để xác định lưu vào DB nào
            switch (eventData.DomainSite)
            {
                case  DataSourceConsts.LongChauUrl : // long chau
                {
                    await _articleLongChauRepository.InsertManyAsync(articles);
                    break;
                }
                case  DataSourceConsts.AladinUrl : 
                {
                    await _articleLongChauRepository.InsertManyAsync(articles);
                    break;
                }
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

    private string ReplaceImageUrls(string contentHtml, List<Media> medias)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(contentHtml);
        foreach (var node in htmlDoc.DocumentNode.SelectNodes("//img"))
        {
            var src = node.Attributes[@"src"].Value;
            var media = medias.FirstOrDefault(x => x.Url.Equals(src));
            
            if (media != null)
            {
                node.Attributes.Add("@media-id", $"media/{media.Id}");
            }
        }

        var newHtml = htmlDoc.DocumentNode.WriteTo();
        return newHtml;
    }

}