using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using LC.Crawler.BackOffice.Permissions;
using LC.Crawler.BackOffice.Medias;

namespace LC.Crawler.BackOffice.Medias
{

    [Authorize(BackOfficePermissions.Medias.Default)]
    public class MediasAppService : ApplicationService, IMediasAppService
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly MediaManager _mediaManager;

        public MediasAppService(IMediaRepository mediaRepository, MediaManager mediaManager)
        {
            _mediaRepository = mediaRepository;
            _mediaManager = mediaManager;
        }

        public virtual async Task<PagedResultDto<MediaDto>> GetListAsync(GetMediasInput input)
        {
            var totalCount = await _mediaRepository.GetCountAsync(input.FilterText, input.Name, input.ContentType, input.Url, input.Description, input.IsDowloaded);
            var items = await _mediaRepository.GetListAsync(input.FilterText, input.Name, input.ContentType, input.Url, input.Description, input.IsDowloaded, input.Sorting, input.MaxResultCount, input.SkipCount);

            return new PagedResultDto<MediaDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<Media>, List<MediaDto>>(items)
            };
        }

        public virtual async Task<MediaDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<Media, MediaDto>(await _mediaRepository.GetAsync(id));
        }

        [Authorize(BackOfficePermissions.Medias.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _mediaRepository.DeleteAsync(id);
        }

        [Authorize(BackOfficePermissions.Medias.Create)]
        public virtual async Task<MediaDto> CreateAsync(MediaCreateDto input)
        {

            var media = await _mediaManager.CreateAsync(
            input.Name, input.ContentType, input.Url, input.Description, input.IsDowloaded
            );

            return ObjectMapper.Map<Media, MediaDto>(media);
        }

        [Authorize(BackOfficePermissions.Medias.Edit)]
        public virtual async Task<MediaDto> UpdateAsync(Guid id, MediaUpdateDto input)
        {

            var media = await _mediaManager.UpdateAsync(
            id,
            input.Name, input.ContentType, input.Url, input.Description, input.IsDowloaded
            );

            return ObjectMapper.Map<Media, MediaDto>(media);
        }
    }
}