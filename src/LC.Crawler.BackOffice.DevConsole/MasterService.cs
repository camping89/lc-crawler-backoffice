using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Extensions;
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
using Volo.Abp.Domain.Repositories;
using WordPressPCL.Models;
using Guid = System.Guid;

namespace LC.Crawler.BackOffice.DevConsole;

public class MasterService : ITransientDependency
{
    public ILogger<MasterService> Logger { get; set; }

    private readonly ProductManagerAladin _productManagerAladin;
    private readonly ProductManagerLongChau _productManagerLongChau;
    private readonly ProductManagerSieuThiSongKhoe _productManagerSieuThiSongKhoe;
    private readonly IProductLongChauRepository _productLongChauRepository;

    private readonly IArticleLongChauRepository _articleLongChauRepository;
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
    private readonly WordpressManagerAladin _wordpressManagerAladin;

    private readonly WordpressManagerAloBacSi _wordpressManagerAloBacSi;
    private readonly WordpressManagerSongKhoeMedplus _wordpressManagerSongKhoeMedplus;
    private readonly WordpressManagerSucKhoeGiaDinh _wordpressManagerSucKhoeGiaDinh;
    private readonly WordpressManagerBlogSucKhoe _wordpressManagerBlogSucKhoe;

    private readonly IProductReviewLongChauRepository _productReviewLongChauRepository;
    private readonly IProductSieuThiSongKhoeRepository _productSieuThiSongKhoeRepository;
    private readonly IArticleSieuThiSongKhoeRepository _articleSieuThiSongKhoeRepository;
    private readonly IMediaSieuThiSongKhoeRepository _mediaSieuThiSongKhoeRepository;
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
        IProductReviewLongChauRepository productReviewLongChauRepository, ProductManagerAladin productManagerAladin, ArticleManangerAladin articleManangerAladin, ArticleManangerSieuThiSongKhoe articleManangerSieuThiSongKhoe, ArticleManangerSucKhoeDoiSong articleManangerSucKhoeDoiSong, ArticleManangerSucKhoeGiaDinh articleManangerSucKhoeGiaDinh, ArticleManangerAloBacSi articleManangerAloBacSi, ArticleManangerBlogSucKhoe articleManangerBlogSucKhoe,
        IProductLongChauRepository productLongChauRepository,
        IArticleLongChauRepository articleLongChauRepository,
        IProductSieuThiSongKhoeRepository productSieuThiSongKhoeRepository,
        IArticleSieuThiSongKhoeRepository articleSieuThiSongKhoeRepository,
        IMediaSieuThiSongKhoeRepository mediaSieuThiSongKhoeRepository,
        WordpressManagerAladin wordpressManagerAladin,
        WordpressManagerSongKhoeMedplus wordpressManagerSongKhoeMedplus,
        WordpressManagerSucKhoeGiaDinh wordpressManagerSucKhoeGiaDinh,
        WordpressManagerBlogSucKhoe wordpressManagerBlogSucKhoe)
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
        _productLongChauRepository = productLongChauRepository;
        _articleLongChauRepository = articleLongChauRepository;
        _productSieuThiSongKhoeRepository = productSieuThiSongKhoeRepository;
        _articleSieuThiSongKhoeRepository = articleSieuThiSongKhoeRepository;
        _mediaSieuThiSongKhoeRepository = mediaSieuThiSongKhoeRepository;
        _wordpressManagerAladin = wordpressManagerAladin;
        _wordpressManagerSongKhoeMedplus = wordpressManagerSongKhoeMedplus;
        _wordpressManagerSucKhoeGiaDinh = wordpressManagerSucKhoeGiaDinh;
        _wordpressManagerBlogSucKhoe = wordpressManagerBlogSucKhoe;
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
        await _wooManagerAladin.DoSyncProductToWooAsync();
        
        
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
        await _wordpressManagerAloBacSi.DoSyncPostAsync();
    }
    public async Task DoSyncArticlesLongChau()
    {
        await _wordpressManagerLongChau.DoSyncPostAsync();
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
    
    public async Task ResetLongChau()
    {
        var articles = await _articleLongChauRepository.GetListAsync(x => x.CreationTime >= DateTime.UtcNow.AddDays(-45));
        foreach (var article in articles)
        {
            article.ExternalId = null;
            await _articleLongChauRepository.UpdateAsync(article, true);
            Console.WriteLine("Update product: " + article.Id);
        }
    }

    public async Task DeleteProductNotImages()
    {
        var dataSources = await _dataSourceRepository.GetListAsync();
        foreach (var dataSource in dataSources.Where(x=>x.Configuration.ApiKey.IsNotNullOrEmpty()))
        {
            await _wooManangerBase.DeleteProductNotImages(dataSource);
        }
    }
    public async Task SyncCateLongChau()
    {
        await _wooManagerLongChau.DoSyncCategoriesAsync();
        await _wooManagerLongChau.UpdateCategoriesProduct();
    }

    public async Task DoResyncAladinProduct()
    {
        await _wooManagerAladin.DoReSyncProductToWooAsync();
        //await _wooManagerSieuThiSongKhoe.DoReSyncProductToWooAsync();
    }
    public async Task DoSyncSieuthisongkhoeProduct()
    {
        //await _wooManagerSieuThiSongKhoe.DoSyncCategoriesAsync();
        //await _wooManagerSieuThiSongKhoe.DoSyncProductToWooAsync();
        //await _wooManagerSieuThiSongKhoe.DoSyncReviews();

        await _wooManagerSieuThiSongKhoe.DoReSyncProductToWooAsync();
    }

    public async Task ResetData()
    {
        
        var listMedia = new List<Guid>();
        var products = await _productSieuThiSongKhoeRepository.GetListAsync(x => x.CreationTime >= new DateTime(2023, 2, 20));
        foreach (var product in products)
        {
            product.ExternalId = null;
            listMedia.AddRange(product.Medias.Select(x=>x.MediaId));
        }

        await _productSieuThiSongKhoeRepository.UpdateManyAsync(products);
        
        var articles = await _articleSieuThiSongKhoeRepository.GetListAsync(x => x.CreationTime >= new DateTime(2023, 2, 20));
        foreach (var article in articles)
        {
            article.ExternalId = null;
            listMedia.AddRange(article.Medias.Select(x=>x.MediaId));
        }

        await _articleSieuThiSongKhoeRepository.UpdateManyAsync(articles);

        
        foreach (var mediaId in listMedia.Distinct())
        {
            var media = await _mediaSieuThiSongKhoeRepository.FirstOrDefaultAsync(x => x.Id == mediaId);
            if (media != null)
            {
                media.ExternalId = null;
                media.ExternalUrl = null;
                await _mediaSieuThiSongKhoeRepository.UpdateAsync(media);
            }
        }
    }

    public async Task ChangeStatusPost()
    {
        var dataSources = await _dataSourceRepository.GetListAsync();
        foreach (var dataSource in dataSources)
        {
            Console.WriteLine($"Change status posts: {dataSource.Url}");
            await _wordpressManagerBase.DoChangeStatusPosts(dataSource, Status.Publish);
            await _wooManangerBase.ChangeStatus(dataSource);
        }
        
    }
    public async Task UpdateHtmlAllPosts()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

        var dataSources = await _dataSourceRepository.GetListAsync();
        foreach (var dataSource in dataSources)
        {
            try
            {
                if (dataSource.Url.Contains("aladin"))
                {
                    await _wordpressManagerAladin.DoSyncPostAsync();
                }
                if (dataSource.Url.Contains("alobacsi"))
                {
                    await _wordpressManagerAloBacSi.DoUpdatePostAsync();
                }
                if (dataSource.Url.Contains("nhathuoclongchau"))
                {
                    await _wordpressManagerLongChau.DoUpdatePostAsync();
                }
                if (dataSource.Url.Contains("sieuthisongkhoe"))
                {
                    await _wordpressManagerSieuThiSongKhoe.DoUpdatePostAsync();
                }
                if (dataSource.Url.Contains("songkhoe.medplus"))
                {
                    await _wordpressManagerSongKhoeMedplus.DoUpdatePostAsync();
                }
                if (dataSource.Url.Contains("suckhoedoisong"))
                {
                    await _wordpressManagerSucKhoeDoiSong.DoUpdatePostAsync();
                }
                if (dataSource.Url.Contains("suckhoegiadinh"))
                {
                    await _wordpressManagerSucKhoeGiaDinh.DoUpdatePostAsync();
                }
                if (dataSource.Url.Contains("blogsuckhoe"))
                {
                    await _wordpressManagerBlogSucKhoe.DoUpdatePostAsync();
                }
            }
            catch (Exception e)
            {
                continue;
            }
        }
    }
    
    
    public async Task SyncAllPosts()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

        var dataSources = await _dataSourceRepository.GetListAsync();
        foreach (var dataSource in dataSources)
        {
            try
            {
                // if (dataSource.Url.Contains("aladin"))
                // {
                //     Console.WriteLine($"Sync article {dataSource.Url}");
                //     await _wordpressManagerAladin.DoSyncCategoriesAsync();
                //     await _wordpressManagerAladin.DoSyncPostAsync();
                // }
                // if (dataSource.Url.Contains("alobacsi"))
                // {
                //     Console.WriteLine($"Sync article {dataSource.Url}");
                //     await _wordpressManagerAloBacSi.DoSyncCategoriesAsync();
                //     await _wordpressManagerAloBacSi.DoSyncPostAsync();
                // }
                // if (dataSource.Url.Contains("nhathuoclongchau"))
                // {
                //     Console.WriteLine($"Sync article {dataSource.Url}");
                //     await _wordpressManagerLongChau.DoSyncCategoriesAsync();
                //     await _wordpressManagerLongChau.DoSyncPostAsync();
                // }
                // if (dataSource.Url.Contains("sieuthisongkhoe"))
                // {
                //     Console.WriteLine($"Sync article {dataSource.Url}");
                //     await _wordpressManagerSieuThiSongKhoe.DoSyncCategoriesAsync();
                //     await _wordpressManagerSieuThiSongKhoe.DoSyncPostAsync();
                // }
                // if (dataSource.Url.Contains("songkhoe.medplus"))
                // {
                //     Console.WriteLine($"Sync article {dataSource.Url}");
                //     await _wordpressManagerSongKhoeMedplus.DoSyncCategoriesAsync();
                //     await _wordpressManagerSongKhoeMedplus.DoSyncPostAsync();
                // }
                // if (dataSource.Url.Contains("suckhoedoisong"))
                // {
                //     Console.WriteLine($"Sync article {dataSource.Url}");
                //     await _wordpressManagerSucKhoeDoiSong.DoSyncCategoriesAsync();
                //     await _wordpressManagerSucKhoeDoiSong.DoSyncPostAsync();
                // }
                // if (dataSource.Url.Contains("suckhoegiadinh"))
                // {
                //     Console.WriteLine($"Sync article {dataSource.Url}");
                //     await _wordpressManagerSucKhoeGiaDinh.DoSyncCategoriesAsync();
                //     await _wordpressManagerSucKhoeGiaDinh.DoSyncPostAsync();
                // }
                if (dataSource.Url.Contains("blogsuckhoe"))
                {
                    Console.WriteLine($"Sync article {dataSource.Url}");
                    await _wordpressManagerBlogSucKhoe.DoSyncCategoriesAsync();
                    await _wordpressManagerBlogSucKhoe.DoSyncPostAsync();
                }
            }
            catch (Exception e)
            {
                continue;
            }
        }
    }
    public async Task SyncProducts()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

        var dataSources = await _dataSourceRepository.GetListAsync();
        foreach (var dataSource in dataSources)
        {
            try
            {
                if (dataSource.Url.Contains("aladin"))
                {
                    Console.WriteLine($"Sync product {dataSource.Url}");
                    await _wooManagerAladin.DoSyncCategoriesAsync();
                    await _wooManagerAladin.DoSyncProductToWooAsync();
                }
               
                if (dataSource.Url.Contains("nhathuoclongchau"))
                {
                    Console.WriteLine($"Sync product {dataSource.Url}");
                    await _wooManagerLongChau.DoSyncCategoriesAsync();
                    await _wooManagerLongChau.DoSyncProductToWooAsync();
                }
                if (dataSource.Url.Contains("sieuthisongkhoe"))
                {
                    Console.WriteLine($"Sync product {dataSource.Url}");
                    await _wooManagerSieuThiSongKhoe.DoSyncCategoriesAsync();
                    await _wooManagerSieuThiSongKhoe.DoSyncProductToWooAsync();
                }
            }
            catch (Exception e)
            {
                continue;
            }
        }
    }
    
}
