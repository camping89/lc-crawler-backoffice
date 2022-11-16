using System.Text;
using HtmlAgilityPack;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.CrawlerData;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.MessageQueue.Consumers.Etos;
using LC.Crawler.BackOffice.Payloads;
using LC.Crawler.BackOffice.Products;
using LC.Crawler.BackOffice.WooCommerces;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace LC.Crawler.BackOffice.MessageQueue.Consumers;

public class CrawlerDataReceiveConsumer : IDistributedEventHandler<CrawlResultEto>, ITransientDependency
{
    private readonly ProductManagerAladin  _productManagerAladin;
    private readonly WooManagerAladin      _wooManagerAladin;
    private readonly ArticleManangerAladin _articleManangerAladin;

    private readonly ProductManagerLongChau  _productManagerLongChau;
    private readonly WooManagerLongChau      _wooManagerLongChau;
    private readonly ArticleManangerLongChau _articleManangerLongChau;

    private readonly ProductManagerSieuThiSongKhoe  _productManagerSieuThiSongKhoe;
    private readonly WooManagerSieuThiSongKhoe      _wooManagerSieuThiSongKhoe;
    private readonly ArticleManangerSieuThiSongKhoe _articleManangerSieuThiSongKhoe;

    private readonly ArticleManangerSucKhoeDoiSong _articleManangerSucKhoeDoiSong;

    private readonly ArticleManangerBlogSucKhoe _articleManangerBlogSucKhoe;

    private readonly ArticleManangerSucKhoeGiaDinh _articleManangerSucKhoeGiaDinh;

    private readonly ArticleManangerAloBacSi _articleManangerAloBacSi;

    private readonly ArticleManangerSongKhoeMedplus _articleManangerSongKhoeMedplusi;

    private readonly IObjectMapper _objectMapper;

    private readonly CrawlerDataManager _crawlerDataManager;

    private readonly ILogger<CrawlerDataReceiveConsumer> _logger;


    public CrawlerDataReceiveConsumer(IObjectMapper objectMapper,
        ArticleManangerLongChau                     articleManangerLongChau,
        ProductManagerLongChau                      productManagerLongChau,
        ProductManagerAladin                        productManagerAladin,
        ArticleManangerSucKhoeDoiSong               articleManangerSucKhoeDoiSong,
        ArticleManangerBlogSucKhoe                  articleManangerBlogSucKhoe,
        ArticleManangerSucKhoeGiaDinh               articleManangerSucKhoeGiaDinh,
        ArticleManangerAloBacSi                     articleManangerAloBacSi,
        ProductManagerSieuThiSongKhoe               productManagerSieuThiSongKhoe,
        ArticleManangerSieuThiSongKhoe              articleManangerSieuThiSongKhoe,
        ArticleManangerSongKhoeMedplus              articleManangerSongKhoeMedplusi,
        CrawlerDataManager                          crawlerDataManager, ArticleManangerAladin articleManangerAladin,
        ILogger<CrawlerDataReceiveConsumer>         logger,
        WooManagerAladin                            wooManagerAladin,
        WooManagerLongChau                          wooManagerLongChau,
        WooManagerSieuThiSongKhoe                   wooManagerSieuThiSongKhoe)
    {
        _objectMapper                    = objectMapper;
        _articleManangerLongChau         = articleManangerLongChau;
        _productManagerLongChau          = productManagerLongChau;
        _productManagerAladin            = productManagerAladin;
        _articleManangerSucKhoeDoiSong   = articleManangerSucKhoeDoiSong;
        _articleManangerBlogSucKhoe      = articleManangerBlogSucKhoe;
        _articleManangerSucKhoeGiaDinh   = articleManangerSucKhoeGiaDinh;
        _articleManangerAloBacSi         = articleManangerAloBacSi;
        _productManagerSieuThiSongKhoe   = productManagerSieuThiSongKhoe;
        _articleManangerSieuThiSongKhoe  = articleManangerSieuThiSongKhoe;
        _articleManangerSongKhoeMedplusi = articleManangerSongKhoeMedplusi;
        _crawlerDataManager              = crawlerDataManager;
        _articleManangerAladin           = articleManangerAladin;
        _logger                          = logger;
        _wooManagerAladin                = wooManagerAladin;
        _wooManagerLongChau              = wooManagerLongChau;
        _wooManagerSieuThiSongKhoe       = wooManagerSieuThiSongKhoe;
    }

    public async Task HandleEventAsync(CrawlResultEto eventData)
    {
        try
        {
            _logger.LogInformation($"============== Start at {DateTime.UtcNow:dd-MM-yyyy HH:mm} UTC ===========");
            Console.WriteLine($"============== Start at {DateTime.UtcNow:dd-MM-yyyy HH:mm} UTC ===========");
            if (eventData.EcommercePayloads is { Products: { } })
            {
                _logger.LogInformation($"============== Processing data Ecom page {eventData.EcommercePayloads.Url} ===========");
                Console.WriteLine($"============== Processing data Ecom page {eventData.EcommercePayloads.Url} ===========");
                
                var url = eventData.EcommercePayloads.Url;
                if (url.Contains(PageDataSourceConsts.LongChauUrl))
                {
                    await _crawlerDataManager.SaveCrawlerDataEcomAsync(PageDataSource.LongChau,
                        eventData.EcommercePayloads);
                    await _wooManagerLongChau.DoChangeStatusWooAsync(eventData.EcommercePayloads.Products);
                    await _productManagerLongChau.ProcessingDataAsync(eventData.EcommercePayloads);
                }

                if (url.Contains(PageDataSourceConsts.AladinUrl))
                {
                    await _crawlerDataManager.SaveCrawlerDataEcomAsync(PageDataSource.Aladin, eventData.EcommercePayloads);
                    await _wooManagerAladin.DoChangeStatusWooAsync(eventData.EcommercePayloads.Products);
                    await _productManagerAladin.ProcessingDataAsync(eventData.EcommercePayloads);
                }

                if (url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl))
                {
                    await _crawlerDataManager.SaveCrawlerDataEcomAsync(PageDataSource.SieuThiSongKhoe,
                        eventData.EcommercePayloads);
                    await _wooManagerSieuThiSongKhoe.DoChangeStatusWooAsync(eventData.EcommercePayloads.Products);
                    await _productManagerSieuThiSongKhoe.ProcessingDataAsync(eventData.EcommercePayloads);
                }
                
                Console.WriteLine($"============== Processed data Ecom page {eventData.EcommercePayloads.Url} - {eventData.EcommercePayloads.Products.Count} products ===========");
            }

            //Handle articles
            if (eventData.ArticlePayloads is { ArticlesPayload: { } })
            {
                _logger.LogInformation($"============== Processing data Article page {eventData.ArticlePayloads.Url} ===========");
                Console.WriteLine($"============== Processing data Article page {eventData.ArticlePayloads.Url} ===========");
                var url = eventData.ArticlePayloads.Url;
                if (url.Contains(PageDataSourceConsts.AladinUrl))
                {
                    await _crawlerDataManager.SaveCrawlerDataArticleAsync(PageDataSource.Aladin, eventData.ArticlePayloads);
                    await _articleManangerAladin.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
                }


                if (url.Contains(PageDataSourceConsts.LongChauUrl))
                {
                    await _crawlerDataManager.SaveCrawlerDataArticleAsync(PageDataSource.LongChau,
                        eventData.ArticlePayloads);
                    await _articleManangerLongChau.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
                }

                if (url.Contains(PageDataSourceConsts.SucKhoeDoiSongUrl))
                {
                    await _crawlerDataManager.SaveCrawlerDataArticleAsync(PageDataSource.SucKhoeDoiSong,
                        eventData.ArticlePayloads);
                    await _articleManangerSucKhoeDoiSong.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
                }

                if (url.Contains(PageDataSourceConsts.BlogSucKhoeUrl))
                {
                    await _crawlerDataManager.SaveCrawlerDataArticleAsync(PageDataSource.BlogSucKhoe,
                        eventData.ArticlePayloads);
                    await _articleManangerBlogSucKhoe.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
                }

                if (url.Contains(PageDataSourceConsts.SucKhoeGiaDinhUrl))
                {
                    await _crawlerDataManager.SaveCrawlerDataArticleAsync(PageDataSource.SucKhoeGiaDinh,
                        eventData.ArticlePayloads);
                    await _articleManangerSucKhoeGiaDinh.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
                }

                if (url.Contains(PageDataSourceConsts.AloBacSiUrl))
                {
                    await _crawlerDataManager.SaveCrawlerDataArticleAsync(PageDataSource.AloBacSi,
                        eventData.ArticlePayloads);
                    await _articleManangerAloBacSi.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
                }

                if (url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl))
                {
                    await _crawlerDataManager.SaveCrawlerDataArticleAsync(PageDataSource.SieuThiSongKhoe,
                        eventData.ArticlePayloads);
                    await _articleManangerSieuThiSongKhoe.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
                }

                if (url.Contains(PageDataSourceConsts.SongKhoeMedplusUrl))
                {
                    await _crawlerDataManager.SaveCrawlerDataArticleAsync(PageDataSource.SongKhoeMedplus,
                        eventData.ArticlePayloads);
                    await _articleManangerSongKhoeMedplusi.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
                }
                
                _logger.LogInformation($"============== Processed data Article page {eventData.ArticlePayloads.Url} - {eventData.ArticlePayloads.ArticlesPayload.Count} articles ===========");
                Console.WriteLine($"============== Processed data Article page {eventData.ArticlePayloads.Url} - {eventData.ArticlePayloads.ArticlesPayload.Count} articles ===========");
            }
            
            _logger.LogInformation($"============== End at {DateTime.UtcNow:dd-MM-yyyy HH:mm} UTC ===========");
            Console.WriteLine($"============== End at {DateTime.UtcNow:dd-MM-yyyy HH:mm} UTC ===========");
        }
        catch (Exception e)
        {
            _logger.LogException(e);
            Console.WriteLine(e);
            Console.WriteLine("----------------------------------------");
        }
    }
}