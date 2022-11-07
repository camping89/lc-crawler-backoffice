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
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace LC.Crawler.BackOffice.MessageQueue.Consumers;

public class CrawlerDataReceiveConsumer : IDistributedEventHandler<CrawlResultEto>, ITransientDependency
{
    private readonly ProductManagerAladin _productManagerAladin;
    private readonly ArticleManangerAladin _articleManangerAladin;

    private readonly ProductManagerLongChau _productManagerLongChau;
    private readonly ArticleManangerLongChau _articleManangerLongChau;

    private readonly ProductManagerSieuThiSongKhoe _productManagerSieuThiSongKhoe;
    private readonly ArticleManangerSieuThiSongKhoe _articleManangerSieuThiSongKhoe;

    private readonly ArticleManangerSucKhoeDoiSong _articleManangerSucKhoeDoiSong;

    private readonly ArticleManangerBlogSucKhoe _articleManangerBlogSucKhoe;

    private readonly ArticleManangerSucKhoeGiaDinh _articleManangerSucKhoeGiaDinh;

    private readonly ArticleManangerAloBacSi _articleManangerAloBacSi;

    private readonly ArticleManangerSongKhoeMedplus _articleManangerSongKhoeMedplusi;

    private readonly IObjectMapper _objectMapper;

    private readonly CrawlerDataManager _crawlerDataManager;

    public CrawlerDataReceiveConsumer(IObjectMapper objectMapper,
        ArticleManangerLongChau articleManangerLongChau,
        ProductManagerLongChau productManagerLongChau,
        ProductManagerAladin productManagerAladin,
        ArticleManangerSucKhoeDoiSong articleManangerSucKhoeDoiSong,
        ArticleManangerBlogSucKhoe articleManangerBlogSucKhoe,
        ArticleManangerSucKhoeGiaDinh articleManangerSucKhoeGiaDinh,
        ArticleManangerAloBacSi articleManangerAloBacSi,
        ProductManagerSieuThiSongKhoe productManagerSieuThiSongKhoe,
        ArticleManangerSieuThiSongKhoe articleManangerSieuThiSongKhoe,
        ArticleManangerSongKhoeMedplus articleManangerSongKhoeMedplusi,
        CrawlerDataManager crawlerDataManager, ArticleManangerAladin articleManangerAladin)
    {
        _objectMapper = objectMapper;
        _articleManangerLongChau = articleManangerLongChau;
        _productManagerLongChau = productManagerLongChau;
        _productManagerAladin = productManagerAladin;
        _articleManangerSucKhoeDoiSong = articleManangerSucKhoeDoiSong;
        _articleManangerBlogSucKhoe = articleManangerBlogSucKhoe;
        _articleManangerSucKhoeGiaDinh = articleManangerSucKhoeGiaDinh;
        _articleManangerAloBacSi = articleManangerAloBacSi;
        _productManagerSieuThiSongKhoe = productManagerSieuThiSongKhoe;
        _articleManangerSieuThiSongKhoe = articleManangerSieuThiSongKhoe;
        _articleManangerSongKhoeMedplusi = articleManangerSongKhoeMedplusi;
        _crawlerDataManager = crawlerDataManager;
        _articleManangerAladin = articleManangerAladin;
    }

    public async Task HandleEventAsync(CrawlResultEto eventData)
    {
        if (eventData.EcommercePayloads is { Products: { } })
        {
            var url = eventData.EcommercePayloads.Url;
            if (url.Contains(PageDataSourceConsts.LongChauUrl))
            {
                await _crawlerDataManager.SaveCrawlerDataEcomAsync(PageDataSource.LongChau,
                    eventData.EcommercePayloads);
                await _productManagerLongChau.ProcessingDataAsync(eventData.EcommercePayloads);
            }

            if (url.Contains(PageDataSourceConsts.AladinUrl))
            {
                await _crawlerDataManager.SaveCrawlerDataEcomAsync(PageDataSource.Aladin, eventData.EcommercePayloads);
                await _productManagerAladin.ProcessingDataAsync(eventData.EcommercePayloads);
            }

            if (url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl))
            {
                await _crawlerDataManager.SaveCrawlerDataEcomAsync(PageDataSource.SieuThiSongKhoe,
                    eventData.EcommercePayloads);
                await _productManagerSieuThiSongKhoe.ProcessingDataAsync(eventData.EcommercePayloads);
            }
        }

        //Handle articles
        if (eventData.ArticlePayloads is { ArticlesPayload: { } })
        {
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
        }
    }
}