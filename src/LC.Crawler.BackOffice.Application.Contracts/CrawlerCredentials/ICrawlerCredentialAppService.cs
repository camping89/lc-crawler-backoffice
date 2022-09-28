using LC.Crawler.BackOffice.Shared;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.CrawlerCredentials
{
    public interface ICrawlerCredentialsAppService : IApplicationService
    {
        Task<PagedResultDto<CrawlerCredentialWithNavigationPropertiesDto>> GetListAsync(GetCrawlerCredentialsInput input);

        Task<CrawlerCredentialWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id);

        Task<CrawlerCredentialDto> GetAsync(Guid id);

        Task<PagedResultDto<LookupDto<Guid?>>> GetCrawlerAccountLookupAsync(LookupRequestDto input);

        Task<PagedResultDto<LookupDto<Guid?>>> GetCrawlerProxyLookupAsync(LookupRequestDto input);

        Task DeleteAsync(Guid id);

        Task<CrawlerCredentialDto> CreateAsync(CrawlerCredentialCreateDto input);

        Task<CrawlerCredentialDto> UpdateAsync(Guid id, CrawlerCredentialUpdateDto input);
    }
}