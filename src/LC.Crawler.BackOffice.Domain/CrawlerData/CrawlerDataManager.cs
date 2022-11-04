using System;
using System.IO;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Payloads;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Services;

namespace LC.Crawler.BackOffice.CrawlerData;

public class CrawlerDataManager : DomainService
{
    private readonly IBlobContainer _blobContainer;

    public CrawlerDataManager(IBlobContainer blobContainer)
    {
        _blobContainer = blobContainer;
    }

    public async Task SaveCrawlerDataEcomAsync(PageDataSource pageDataSource,CrawlEcommercePayload ecommercePayload)
    {
        try
        {
            var fileName = $"DataCrawler/{Enum.GetName(pageDataSource)}/Ecom_{DateTime.UtcNow:dd-MM-yyyy}.json";
            var jsonData = JsonConvert.SerializeObject(ecommercePayload);
            await using var stream = GenerateStreamFromString(jsonData);
            await _blobContainer.SaveAsync(fileName, stream, true);
        }
        catch (Exception e)
        {
            Logger.LogException(e, LogLevel.Error);
        }
    }
    public async Task SaveCrawlerDataArticleAsync(PageDataSource pageDataSource,CrawlArticlePayload articlePayload)
    {
        try
        {
            var fileName = $"DataCrawler/{Enum.GetName(pageDataSource)}/Article_{DateTime.UtcNow:dd-MM-yyyy}.json";
            var jsonData = JsonConvert.SerializeObject(articlePayload);
            await using var stream = GenerateStreamFromString(jsonData);
            await _blobContainer.SaveAsync(fileName, stream, true);
        }
        catch (Exception e)
        {
            Logger.LogException(e, LogLevel.Error);
        }
    }
    
    public static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}