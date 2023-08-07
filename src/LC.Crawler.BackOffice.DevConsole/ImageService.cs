using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.DependencyInjection;

namespace LC.Crawler.BackOffice.DevConsole;

public class ImageService : ITransientDependency
{

    private readonly IMediaBlogSucKhoeRepository _mediaBlogSucKhoeRepository;
    private readonly IMediaAladinRepository _mediaAladinRepository;
    private readonly IMediaLongChauRepository _mediaLongChauRepository;

    public ImageService(IMediaBlogSucKhoeRepository mediaBlogSucKhoeRepository, IMediaAladinRepository mediaAladinRepository, IMediaLongChauRepository mediaLongChauRepository)
    {
        _mediaBlogSucKhoeRepository = mediaBlogSucKhoeRepository;
        _mediaAladinRepository = mediaAladinRepository;
        _mediaLongChauRepository = mediaLongChauRepository;
    }


    public async Task UpdateUrl()
    {
        var medias = (await _mediaLongChauRepository.GetQueryableAsync()).Where(x=>x.ExternalUrl.Contains("batdongsan")).ToList();
        foreach (var media in medias)
        {
            if (media.ExternalUrl.IsNotNullOrEmpty())
            {
                media.ExternalUrl = media.ExternalUrl.Replace("batdongsanaumy.com", "uptodream.com");
                await _mediaLongChauRepository.UpdateAsync(media,true);
            }
        }

    }
    public async Task RedownLoadImages()
    {
        var medias = await _mediaBlogSucKhoeRepository.GetListAsync(x=>string.IsNullOrEmpty(x.ExternalId) == false);
        //await SaveMultipleMedia(medias);
        foreach (var partition in medias.Partition(100))
        {
            await Task.Factory.StartNew(async () => await SaveMultipleMedia(partition.ToList()));
        }
    }
    
    private async Task SaveMedia(Media media)
    {
        var fileExtension = Path.GetExtension(media.Url);
        var fileBytes = await FileExtendHelper.DownloadFile(media.Url);
        if (fileBytes != null && media.ExternalUrl.Contains("webp") == false)
        {
            var fileName = media.ExternalUrl.Replace("http://newspaper.batdongsanaumy.com/wp-content/", string.Empty).Replace("/", "\\");
            var filePath = "C:\\lc-media\\newspaper\\" + fileName;
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            await File.WriteAllBytesAsync(filePath, fileBytes);
            await Task.Delay(500);
            Console.WriteLine("Download done:" + media.ExternalId);
        }
    }
        
    private async Task SaveMultipleMedia(List<Media> medias)
    {
        foreach (var media in medias)
        {
            await SaveMedia(media);
        }
    }
}