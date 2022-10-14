using System.IO;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Payloads;
using LC.Crawler.BackOffice.Products;
using LC.Crawler.BackOffice.WooCommerces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Volo.Abp.DependencyInjection;

namespace LC.Crawler.BackOffice.DevConsole;

public class MasterService : ITransientDependency
{
    public ILogger<MasterService> Logger { get; set; }

    private readonly ProductManagerLongChau _productManagerLongChau;
    private readonly WooManagerLongChau _wooManagerLongChau;
    private readonly MediaManagerLongChau _mediaManagerLongChau;

    public MasterService(ProductManagerLongChau productManagerLongChau, WooManagerLongChau wooManagerLongChau, MediaManagerLongChau mediaManagerLongChau)
    {
        _productManagerLongChau = productManagerLongChau;
        _wooManagerLongChau = wooManagerLongChau;
        _mediaManagerLongChau = mediaManagerLongChau;
        Logger = NullLogger<MasterService>.Instance;
    }

    public async Task ProcessLongChauDataAsync()
    {
        //var content = await File.ReadAllTextAsync(@"C:\Users\huynn\Downloads\LongChau.txt");
        // deserialize JSON directly from a file
        using StreamReader file = File.OpenText(@"C:\Users\huynn\Downloads\LongChau.txt");
        JsonSerializer serializer = new JsonSerializer();
        var crawlResultEtos = (CrawlEcommercePayload)serializer.Deserialize(file, typeof(CrawlEcommercePayload));
        if (crawlResultEtos != null)
        {
            await _productManagerLongChau.ProcessingDataAsync(crawlResultEtos);
        }
    }

    public async Task DownLoadMediaLongChauAsync()
    {
        await _mediaManagerLongChau.ProcessDownloadMediasAsync();
    }

    public async Task DoSyncProductToWooAsync()
    {
        await _wooManagerLongChau.DoSyncCategoriesAsync();
        await _wooManagerLongChau.DoSyncProductToWooAsync();
    }
}
