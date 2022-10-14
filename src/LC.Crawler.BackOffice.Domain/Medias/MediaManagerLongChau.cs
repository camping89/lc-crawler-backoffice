﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Services;

namespace LC.Crawler.BackOffice.Medias;

public class MediaManagerLongChau : DomainService
{
    private readonly IMediaLongChauRepository _mediaLongChauRepository;
    private readonly IBlobContainer _blobContainer;

    public MediaManagerLongChau(IMediaLongChauRepository mediaLongChauRepository, IBlobContainer blobContainer)
    {
        _mediaLongChauRepository = mediaLongChauRepository;
        _blobContainer = blobContainer;
    }

    public async Task<Media> CreateAsync(
        string name, string contentType, string url, string description, bool isDowloaded)
    {
        var media = new Media(
            GuidGenerator.Create(),
            name, contentType, url, description, isDowloaded
        );

        return await _mediaLongChauRepository.InsertAsync(media);
    }

    public async Task CreateManyAsync(List<Media> medias)
    {
        await _mediaLongChauRepository.InsertManyAsync(medias);
    }

    public async Task<Media> UpdateAsync(
        Guid id,
        string name, string contentType, string url, string description, bool isDowloaded
    )
    {
        var queryable = await _mediaLongChauRepository.GetQueryableAsync();
        var query = queryable.Where(x => x.Id == id);

        var media = await AsyncExecuter.FirstOrDefaultAsync(query);

        media.Name = name;
        media.ContentType = contentType;
        media.Url = url;
        media.Description = description;
        media.IsDowloaded = isDowloaded;

        return await _mediaLongChauRepository.UpdateAsync(media);
    }

    private async Task SaveMedia(Media media,string type = "product")
    {
        var fileExtension = Path.GetExtension(media.Url);
        var fileBytes = await FileExtendHelper.DownloadFile(media.Url);
        if (fileBytes != null)
        {
            var fileName = $"{Enum.GetName(PageDataSource.LongChau)}/{type}/{media.Id}{fileExtension}";
            await _blobContainer.SaveAsync(fileName, fileBytes);
                
            //Do save media
            media.Name = fileName;
            media.IsDowloaded = true;
            await _mediaLongChauRepository.UpdateAsync(media, true);
        }
    }
        
    private async Task SaveMultipleMedia(List<Media> medias)
    {
        foreach (var media in medias)
        {
            await SaveMedia(media);
        }
    }

    public async Task ProcessDownloadMediasAsync()
    {
        var medias = await _mediaLongChauRepository.GetListAsync(x => x.IsDowloaded == true);
        foreach (var partition in medias.Partition(100))
        {
            //await Task.Factory.StartNew(async () => await SaveMultipleMedia(partition.ToList()));
            await SaveMultipleMedia(partition.ToList());
        }
    }
        
        
    public async Task<Stream> GetFileStream(string fileName)
    {
        return await _blobContainer.GetAsync(fileName);
    }

}