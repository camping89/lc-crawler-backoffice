using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Payloads;
using LC.Crawler.BackOffice.Products;
using LC.Crawler.BackOffice.WooCommerces;
using LC.Crawler.BackOffice.Wordpress;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Volo.Abp.DependencyInjection;

namespace LC.Crawler.BackOffice.DevConsole;

public class MasterService : ITransientDependency
{
    public ILogger<MasterService> Logger { get; set; }

    private readonly ProductManagerLongChau _productManagerLongChau;
    private readonly ProductManagerSieuThiSongKhoe _productManagerSieuThiSongKhoe;
    private readonly ArticleManangerSongKhoeMedplus _articleManangerSongKhoeMedplus;
    private readonly WooManagerLongChau _wooManagerLongChau;

    private readonly ArticleManangerLongChau _articleManangerLongChau;
    private readonly MediaManagerLongChau _mediaManagerLongChau;

    private readonly WooManagerAladin _wooManagerAladin;
    
    private readonly WordpressManagerSieuThiSongKhoe _wordpressManagerSieuThiSongKhoe;

    private readonly WordpressManagerLongChau _wordpressManagerLongChau;
    public MasterService(ProductManagerLongChau productManagerLongChau, WooManagerLongChau wooManagerLongChau, MediaManagerLongChau mediaManagerLongChau, WordpressManagerSieuThiSongKhoe wordpressManagerSieuThiSongKhoe, ArticleManangerLongChau articleManangerLongChau,
        WordpressManagerLongChau wordpressManagerLongChau,
        WooManagerAladin wooManagerAladin,
        ProductManagerSieuThiSongKhoe productManagerSieuThiSongKhoe,
        ArticleManangerSongKhoeMedplus articleManangerSongKhoeMedplus)
    {
        _productManagerLongChau = productManagerLongChau;
        _wooManagerLongChau = wooManagerLongChau;
        _mediaManagerLongChau = mediaManagerLongChau;
        _wordpressManagerSieuThiSongKhoe = wordpressManagerSieuThiSongKhoe;
        _articleManangerLongChau = articleManangerLongChau;
        _wordpressManagerLongChau = wordpressManagerLongChau;
        _wooManagerAladin = wooManagerAladin;
        _productManagerSieuThiSongKhoe = productManagerSieuThiSongKhoe;
        _articleManangerSongKhoeMedplus = articleManangerSongKhoeMedplus;
        Logger = NullLogger<MasterService>.Instance;
    }

    public async Task ProcessDataAsync()
    {
         using StreamReader file = File.OpenText(@"C:\Users\huynn\Downloads\LongChau.txt");
         JsonSerializer serializer = new JsonSerializer();
         var crawlResultEtos = (CrawlEcommercePayload)serializer.Deserialize(file, typeof(CrawlEcommercePayload));
         if (crawlResultEtos != null)
         {
             await _productManagerLongChau.ProcessingDataAsync(crawlResultEtos);
         }
    }

    public async Task ProcessLongChauArticleDataAsync()
    {
        using StreamReader file = File.OpenText(@"C:\Users\huynn\Downloads\LongChauArticles.txt");
        
        JsonSerializer serializer = new JsonSerializer();
        var crawlResultEtos = (CrawlArticlePayload)serializer.Deserialize(file, typeof(CrawlArticlePayload));
        if (crawlResultEtos != null)
        {
            await _articleManangerLongChau.ProcessingDataAsync(crawlResultEtos.ArticlesPayload);
        }
        
    }

    public async Task DownLoadMediaAsync()
    {
        await _mediaManagerLongChau.ProcessDownloadMediasAsync();
    }
    //
    // public async Task DownLoadMediaSieuThiSucKhoeAsync()
    // {
    //     await _mediaManagerSieuThiSongKhoe.ProcessDownloadMediasAsync();
    // }
    //
    // public async Task DownLoadMediaSongKhoeMedplusAsync()
    // {
    //     await _mediaManagerSongKhoeMedplus.ProcessDownloadMediasAsync();
    // }

    public async Task DoSyncProductToWooAsync()
    {
        await _wooManagerLongChau.DoSyncProductToWooAsync();
        
        // await _wooManagerAladin.DoSyncCategoriesAsync();
        // await _wooManagerAladin.DoSyncProductToWooAsync();
    }

    public async Task DoSyncArticleToWooAsync()
    {
        await _wordpressManagerLongChau.DoSyncCategoriesAsync();
        await _wordpressManagerLongChau.DoSyncPostAsync();
    }

    public async Task DoSyncArticles()
    {
        await _wordpressManagerLongChau.DoSyncPostAsync();
    }
    
    // public async Task DoSyncSongKhoeMedplusArticles()
    // {
    //     await _wordpressManagerSongKhoeMedplus.DoSyncToWordpress();
    // }

    public async Task SyncData()
    {
        using StreamReader file = File.OpenText(@"D:\SieuThiSongKhoe.txt");
        JsonSerializer serializer = new JsonSerializer();
        var crawlResultEtos = (CrawlEcommercePayload)serializer.Deserialize(file, typeof(CrawlEcommercePayload));
        if (crawlResultEtos != null)
        {
            await _productManagerSieuThiSongKhoe.ProcessingDataAsync(crawlResultEtos);
        }
    }
    
    public async Task SyncSongKhoeMedplusData()
    {
        using StreamReader file = File.OpenText(@"D:\songkhoemedplus.txt");
        JsonSerializer serializer = new JsonSerializer();
        var crawlResultEtos = (CrawlArticlePayload)serializer.Deserialize(file, typeof(CrawlArticlePayload));
        if (crawlResultEtos != null)
        {
            await _articleManangerSongKhoeMedplus.ProcessingDataAsync(crawlResultEtos.ArticlesPayload.Take(100).ToList());
        }
    }
}
