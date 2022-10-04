using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace LC.Crawler.BackOffice.Medias
{
    public class MediaManager : DomainService
    {
        private readonly IMediaRepository _mediaRepository;

        public MediaManager(IMediaRepository mediaRepository)
        {
            _mediaRepository = mediaRepository;
        }

        public async Task<Media> CreateAsync(
        string name, string contentType, string url, string description)
        {
            var media = new Media(
             GuidGenerator.Create(),
             name, contentType, url, description
             );

            return await _mediaRepository.InsertAsync(media);
        }

        public async Task<Media> UpdateAsync(
            Guid id,
            string name, string contentType, string url, string description
        )
        {
            var queryable = await _mediaRepository.GetQueryableAsync();
            var query = queryable.Where(x => x.Id == id);

            var media = await AsyncExecuter.FirstOrDefaultAsync(query);

            media.Name = name;
            media.ContentType = contentType;
            media.Url = url;
            media.Description = description;

            return await _mediaRepository.UpdateAsync(media);
        }

    }
}