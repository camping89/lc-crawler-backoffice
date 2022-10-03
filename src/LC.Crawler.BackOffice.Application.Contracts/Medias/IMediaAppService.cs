using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.Medias
{
    public interface IMediasAppService : IApplicationService
    {
        Task<PagedResultDto<MediaDto>> GetListAsync(GetMediasInput input);

        Task<MediaDto> GetAsync(Guid id);

        Task DeleteAsync(Guid id);

        Task<MediaDto> CreateAsync(MediaCreateDto input);

        Task<MediaDto> UpdateAsync(Guid id, MediaUpdateDto input);
    }
}