using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Payloads;
using LC.Crawler.BackOffice.ProductReviews;
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

    private readonly ProductManagerAladin _productManagerAladin;
    private readonly ProductManagerLongChau _productManagerLongChau;
    private readonly ProductManagerSieuThiSongKhoe _productManagerSieuThiSongKhoe;
    
    private readonly ArticleManangerLongChau _articleManangerLongChau;
    private readonly ArticleManangerAladin _articleManangerAladin;
    private readonly ArticleManangerSieuThiSongKhoe _articleManangerSieuThiSongKhoe;
    private readonly ArticleManangerSongKhoeMedplus _articleManangerSongKhoeMedplus;
    private readonly ArticleManangerSucKhoeDoiSong _articleManangerSucKhoeDoiSong;
    private readonly ArticleManangerSucKhoeGiaDinh _articleManangerSucKhoeGiaDinh;
    private readonly ArticleManangerAloBacSi _articleManangerAloBacSi;
    private readonly ArticleManangerBlogSucKhoe _articleManangerBlogSucKhoe;
    
    private readonly WooManagerLongChau _wooManagerLongChau;
    private readonly MediaManagerLongChau _mediaManagerLongChau;

    private readonly WooManagerAladin _wooManagerAladin;
    
    private readonly WordpressManagerSieuThiSongKhoe _wordpressManagerSieuThiSongKhoe;

    private readonly WordpressManagerLongChau _wordpressManagerLongChau;
    private readonly WooManangerBase _wooManangerBase;
    private readonly WooApiConsumers _wooApiConsumers;
    private readonly IDataSourceRepository _dataSourceRepository;
    private readonly IArticleSucKhoeDoiSongRepository _articleSucKhoeDoiSongRepository;
    private readonly WordpressManagerBase _wordpressManagerBase;
    private readonly WordpressManagerSucKhoeDoiSong _wordpressManagerSucKhoeDoiSong;
    private readonly WooManagerSieuThiSongKhoe _wooManagerSieuThiSongKhoe;

    private readonly WordpressManagerAloBacSi _wordpressManagerAloBacSi;

    private readonly IProductReviewLongChauRepository _productReviewLongChauRepository;
    public MasterService(ProductManagerLongChau productManagerLongChau, WooManagerLongChau wooManagerLongChau, MediaManagerLongChau mediaManagerLongChau, WordpressManagerSieuThiSongKhoe wordpressManagerSieuThiSongKhoe, ArticleManangerLongChau articleManangerLongChau,
        WordpressManagerLongChau wordpressManagerLongChau,
        WooManagerAladin wooManagerAladin,
        ProductManagerSieuThiSongKhoe productManagerSieuThiSongKhoe,
        ArticleManangerSongKhoeMedplus articleManangerSongKhoeMedplus, WooManangerBase wooManangerBase, IDataSourceRepository dataSourceRepository, WooApiConsumers wooApiConsumers,
        IArticleSucKhoeDoiSongRepository articleSucKhoeDoiSongRepository,
        WordpressManagerBase wordpressManagerBase,
        WordpressManagerSucKhoeDoiSong wordpressManagerSucKhoeDoiSong,
        WooManagerSieuThiSongKhoe wooManagerSieuThiSongKhoe,
        WordpressManagerAloBacSi wordpressManagerAloBacSi,
        IProductReviewLongChauRepository productReviewLongChauRepository, ProductManagerAladin productManagerAladin, ArticleManangerAladin articleManangerAladin, ArticleManangerSieuThiSongKhoe articleManangerSieuThiSongKhoe, ArticleManangerSucKhoeDoiSong articleManangerSucKhoeDoiSong, ArticleManangerSucKhoeGiaDinh articleManangerSucKhoeGiaDinh, ArticleManangerAloBacSi articleManangerAloBacSi, ArticleManangerBlogSucKhoe articleManangerBlogSucKhoe)
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
        _wooManangerBase = wooManangerBase;
        _dataSourceRepository = dataSourceRepository;
        _wooApiConsumers = wooApiConsumers;
        _articleSucKhoeDoiSongRepository = articleSucKhoeDoiSongRepository;
        _wordpressManagerBase = wordpressManagerBase;
        _wordpressManagerSucKhoeDoiSong = wordpressManagerSucKhoeDoiSong;
        _wooManagerSieuThiSongKhoe = wooManagerSieuThiSongKhoe;
        _wordpressManagerAloBacSi = wordpressManagerAloBacSi;
        _productReviewLongChauRepository = productReviewLongChauRepository;
        _productManagerAladin = productManagerAladin;
        _articleManangerAladin = articleManangerAladin;
        _articleManangerSieuThiSongKhoe = articleManangerSieuThiSongKhoe;
        _articleManangerSucKhoeDoiSong = articleManangerSucKhoeDoiSong;
        _articleManangerSucKhoeGiaDinh = articleManangerSucKhoeGiaDinh;
        _articleManangerAloBacSi = articleManangerAloBacSi;
        _articleManangerBlogSucKhoe = articleManangerBlogSucKhoe;
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
        // await _wooManagerLongChau.DoSyncCategoriesAsync();
        await _wooManagerLongChau.DoSyncProductToWooAsync();
        
        
        // await _wooManagerAladin.DoSyncProductToWooAsync();
       // await _wooManagerSieuThiSongKhoe.DoSyncUpdateProduct();
       // await _wooManagerSieuThiSongKhoe.DoSyncProductToWooAsync();
    }

    public async Task DoSyncArticleToWooAsync()
    {
        //var suckhoeArticle = await _articleSucKhoeDoiSongRepository.GetAsync(x => x.Title == "Lợi ích không ngờ của hành tây với sức khỏe");
        //var text = _wordpressManagerSucKhoeDoiSong.DoSyncCategoriesAsync();
        // await _wordpressManagerLongChau.DoSyncCategoriesAsync();
        // await _wordpressManagerLongChau.DoSyncPostAsync();
        //await _wordpressManagerSucKhoeDoiSong.DoSyncPostAsync();
        //await _wordpressManagerAloBacSi.UpdateDataPostAsync();
    }

    public async Task DoSyncArticles()
    {
        await _wordpressManagerSucKhoeDoiSong.DoSyncPostAsync();
    }
    
    // public async Task DoSyncSongKhoeMedplusArticles()
    // {
    //     await _wordpressManagerSongKhoeMedplus.DoSyncToWordpress();
    // }

    public async Task SyncData()
    {
        using StreamReader file = File.OpenText(@"D:\longchau.txt");
        JsonSerializer serializer = new JsonSerializer();
        var crawlResultEtos = (CrawlEcommercePayload)serializer.Deserialize(file, typeof(CrawlEcommercePayload));
        if (crawlResultEtos != null)
        {
            await _productManagerLongChau.ProcessingDataAsync(crawlResultEtos);
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

    public async Task DeleteDuplicateWooProduct(string site)
    {
        var dataSource = await _dataSourceRepository.GetAsync(_ => _.Url.Contains(site));
        await _wooManangerBase.DeleteDuplicateWooProduct(dataSource);
    }
    
    public async Task TestProductApi(string site)
    {
        var dataSource = await _dataSourceRepository.GetAsync(_ => _.Url.Contains(site));
        await _wooApiConsumers.GetProductBrandApi(dataSource);
    }
    
    public async Task TestArticleApi(string site)
    {
        var dataSource = await _dataSourceRepository.GetAsync(_ => _.Url.Contains(site));
        await _wooApiConsumers.GetArticleBrandApi(dataSource);
    }

    public async Task TestSyncReviews()
    {
        await _wooManagerLongChau.DoSyncReviews();
    }
    
    // public async Task CountProductAndArticleByCategory()
    // {
    //     var productAladinResult = await _productManagerAladin.CountProductByCategory();
    //     PrintConsole(productAladinResult, "Product", "Aladin");
    //
    //     var productLongChauResult = await _productManagerLongChau.CountProductByCategory();
    //     PrintConsole(productLongChauResult, "Product", "LongChau");
    //     
    //     var productSieuThiSongKhoeResult = await _productManagerSieuThiSongKhoe.CountProductByCategory();
    //     PrintConsole(productSieuThiSongKhoeResult, "Product", "SieuThiSongKhoe");
    //     
    //     var articleAladinResult = await _articleManangerAladin.CountArticleByCategory();
    //     PrintConsole(articleAladinResult, "Article", "Aladin");
    //     
    //     var articleLongChauResult = await _articleManangerLongChau.CountArticleByCategory();
    //     PrintConsole(articleLongChauResult, "Article", "LongChau");
    //     
    //     var articleSieuThiSongKhoeResult = await _articleManangerSieuThiSongKhoe.CountArticleByCategory();
    //     PrintConsole(articleSieuThiSongKhoeResult, "Article", "SieuThiSucKhoe");
    //     
    //     var articleAloBacSiResult = await _articleManangerAloBacSi.CountArticleByCategory();
    //     PrintConsole(articleAloBacSiResult, "Article", "AloBacSi");
    //     
    //     var articleBlogSucKhoeResult = await _articleManangerBlogSucKhoe.CountArticleByCategory();
    //     PrintConsole(articleBlogSucKhoeResult, "Article", "BlogSucKhoe");
    //     
    //     var articleSongKhoeMedplusResult = await _articleManangerSongKhoeMedplus.CountArticleByCategory();
    //     PrintConsole(articleSongKhoeMedplusResult, "Article", "SongKhoeMedplus");
    //     
    //     var articleSucKhoeDoiSongResult = await _articleManangerSucKhoeDoiSong.CountArticleByCategory();
    //     PrintConsole(articleSucKhoeDoiSongResult, "Article", "SucKhoeDoiSong");
    //     
    //     var articleSucKhoeGiaDinhResult = await _articleManangerSucKhoeGiaDinh.CountArticleByCategory();
    //     PrintConsole(articleSucKhoeGiaDinhResult, "Article", "SucKhoeGiaDinh");
    //
    //     var lines = new List<string>();
    //     lines.Add("----------------------------Product Aladin----------------------");
    //     lines.AddRange(productAladinResult.Select(item => $"{item.Key} ----- {item.Value}").ToList());
    //     lines.Add("----------------------------Product LongChau----------------------");
    //     lines.AddRange(productLongChauResult.Select(item => $"{item.Key} ----- {item.Value}"));
    //     lines.Add("----------------------------Product SieuThiSongKhoe----------------------");
    //     lines.AddRange(productSieuThiSongKhoeResult.Select(item => $"{item.Key} ----- {item.Value}"));
    //     lines.Add("----------------------------Article Aladin----------------------");
    //     lines.AddRange(articleAladinResult.Select(item => $"{item.Key} ----- {item.Value}"));
    //     lines.Add("----------------------------Article LongChau----------------------");
    //     lines.AddRange(articleLongChauResult.Select(item => $"{item.Key} ----- {item.Value}"));
    //     lines.Add("----------------------------Article SieuThiSongKhoe----------------------");
    //     lines.AddRange(articleSieuThiSongKhoeResult.Select(item => $"{item.Key} ----- {item.Value}"));
    //     lines.Add("----------------------------Article AloBacSi----------------------");
    //     lines.AddRange(articleAloBacSiResult.Select(item => $"{item.Key} ----- {item.Value}"));
    //     lines.Add("----------------------------Article BlogSucKhoe----------------------");
    //     lines.AddRange(articleBlogSucKhoeResult.Select(item => $"{item.Key} ----- {item.Value}"));
    //     lines.Add("----------------------------Article SongKhoeMedplus----------------------");
    //     lines.AddRange(articleSongKhoeMedplusResult.Select(item => $"{item.Key} ----- {item.Value}"));
    //     lines.Add("----------------------------Article SucKhoeDoiSong----------------------");
    //     lines.AddRange(articleSucKhoeDoiSongResult.Select(item => $"{item.Key} ----- {item.Value}"));
    //     lines.Add("----------------------------Article SucKhoeGiaDinh----------------------");
    //     lines.AddRange(articleSucKhoeGiaDinhResult.Select(item => $"{item.Key} ----- {item.Value}"));
    //
    //     await File.WriteAllLinesAsync("category-result.txt", lines);
    // }

    private void PrintConsole(List<KeyValuePair<string, int>> result, string type, string site)
    {
        Console.WriteLine($"{type} {site} Categories Count: {result.Count}");
        Console.WriteLine($"{type} {site} Count: {result.Sum(_ => _.Value)}");
        foreach (var item in result)
        {
            Console.WriteLine($"{type} Count: {item.Key} -------------{item.Value}");
        }
    }

    public async Task CleanPostDuplicate()
    {
        while (true)
        {
            var dataSources = await _dataSourceRepository.GetListAsync();
            foreach (var dataSource in dataSources)
            {
                try
                {
                    Console.WriteLine($"Site {dataSource.Url}");
                    await _wordpressManagerBase.CleanDuplicatePostsAsync(dataSource);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    //throw;
                }
            }
            Console.WriteLine($"Chờ 1 tiếng chạy lại");
            await Task.Delay(TimeSpan.FromHours(1));
        }
    }
}
