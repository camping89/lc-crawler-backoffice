using System.Text;
using HtmlAgilityPack;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Core;
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
    
    private readonly ProductManagerLongChau        _productManagerLongChau;
    private readonly ArticleManangerLongChau       _articleManangerLongChau;
    
    private readonly ProductManagerSieuThiSongKhoe _productManagerSieuThiSongKhoe;
    private readonly ArticleManangerSieuThiSongKhoe       _articleManangerSieuThiSongKhoe;
    
    private readonly ArticleManangerSucKhoeDoiSong _articleManangerSucKhoeDoiSong;
    
    private readonly ArticleManangerBlogSucKhoe    _articleManangerBlogSucKhoe;
    
    private readonly ArticleManangerSucKhoeGiaDinh _articleManangerSucKhoeGiaDinh;
    
    private readonly ArticleManangerAloBacSi       _articleManangerAloBacSi;

    private readonly ArticleManangerSongKhoeMedplus       _articleManangerSongKhoeMedplusi;
    
    private readonly IObjectMapper _objectMapper;

    public CrawlerDataReceiveConsumer(IObjectMapper objectMapper,
        ArticleManangerLongChau                     articleManangerLongChau,
        ProductManagerLongChau                      productManagerLongChau,
        ProductManagerAladin                        productManagerAladin,
        ArticleManangerSucKhoeDoiSong               articleManangerSucKhoeDoiSong,
        ArticleManangerBlogSucKhoe                  articleManangerBlogSucKhoe,
        ArticleManangerSucKhoeGiaDinh               articleManangerSucKhoeGiaDinh,
        ArticleManangerAloBacSi                     articleManangerAloBacSi, ProductManagerSieuThiSongKhoe productManagerSieuThiSongKhoe, ArticleManangerSieuThiSongKhoe articleManangerSieuThiSongKhoe, ArticleManangerSongKhoeMedplus articleManangerSongKhoeMedplusi)
    {
        _objectMapper                  = objectMapper;
        _articleManangerLongChau       = articleManangerLongChau;
        _productManagerLongChau        = productManagerLongChau;
        _productManagerAladin          = productManagerAladin;
        _articleManangerSucKhoeDoiSong = articleManangerSucKhoeDoiSong;
        _articleManangerBlogSucKhoe    = articleManangerBlogSucKhoe;
        _articleManangerSucKhoeGiaDinh = articleManangerSucKhoeGiaDinh;
        _articleManangerAloBacSi       = articleManangerAloBacSi;
        _productManagerSieuThiSongKhoe = productManagerSieuThiSongKhoe;
        _articleManangerSieuThiSongKhoe = articleManangerSieuThiSongKhoe;
        _articleManangerSongKhoeMedplusi = articleManangerSongKhoeMedplusi;
    }

    public async Task HandleEventAsync(CrawlResultEto eventData)
    {
        if (eventData.EcommercePayloads is { Products: { } })
        {
            // //TODO: Dựa vào urlsite trả về từ Crawler để xác định lưu vào DB nào
            // switch (eventData.EcommercePayloads.Url)
            // {
            //     case  PageDataSourceConsts.LongChauUrl : // long chau
            //     {
            //         await _productManagerLongChau.ProcessingDataAsync(eventData.EcommercePayloads);
            //         break;
            //     }
            //     case  PageDataSourceConsts.AladinUrl :
            //     {
            //         await _productManagerAladin.ProcessingDataAsync(eventData.EcommercePayloads);
            //         break;
            //     }
            // }
            //
            var url = eventData.EcommercePayloads.Url;
            if (url.Contains(PageDataSourceConsts.LongChauUrl))
            {
                await _productManagerLongChau.ProcessingDataAsync(eventData.EcommercePayloads);
            }
            
            if (url.Contains(PageDataSourceConsts.AladinUrl))
            {
                await _productManagerAladin.ProcessingDataAsync(eventData.EcommercePayloads);
            }
            
            if (url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl))
            {
                await _productManagerSieuThiSongKhoe.ProcessingDataAsync(eventData.EcommercePayloads);
            }
        }
        
        //Handle articles
        if (eventData.ArticlePayloads is { ArticlesPayload: { } })
        {
            // //TODO: Dựa vào urlsite trả về từ Crawler để xác định lưu vào DB nào
            
            // switch (url)
            // {
            //     case  PageDataSourceConsts.LongChauUrl : // long chau
            //     {
            //         await _articleManangerLongChau.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
            //         break;
            //     }
            //     case  PageDataSourceConsts.AladinUrl :
            //     {
            //         break;
            //     }
            //     case PageDataSourceConsts.SucKhoeDoiSongUrl:
            //         break;
            // }
            
            var url = eventData.ArticlePayloads.Url;
            if (url.Contains(PageDataSourceConsts.LongChauUrl))
            {
                await _articleManangerLongChau.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
            }
            
            if (url.Contains(PageDataSourceConsts.SucKhoeDoiSongUrl))
            {
                await _articleManangerSucKhoeDoiSong.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
            }
            
            if (url.Contains(PageDataSourceConsts.BlogSucKhoeUrl))
            {
                await _articleManangerBlogSucKhoe.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
            }
            
            if (url.Contains(PageDataSourceConsts.SucKhoeGiaDinhUrl))
            {
                await _articleManangerSucKhoeGiaDinh.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
            }
            
            if (url.Contains(PageDataSourceConsts.AloBacSiUrl))
            {
                await _articleManangerAloBacSi.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
            }
            
            if (url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl))
            {
                await _articleManangerSieuThiSongKhoe.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
            }
            
            if (url.Contains(PageDataSourceConsts.SongKhoeMedplusUrl))
            {
                await _articleManangerSongKhoeMedplusi.ProcessingDataAsync(eventData.ArticlePayloads.ArticlesPayload);
            }
        }
    }

}