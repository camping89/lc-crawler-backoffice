using LC.Crawler.BackOffice.Shared;
using LC.Crawler.BackOffice.CrawlerProxies;
using LC.Crawler.BackOffice.CrawlerAccounts;
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
using LC.Crawler.BackOffice.CrawlerCredentials;

namespace LC.Crawler.BackOffice.CrawlerCredentials
{

    [Authorize(BackOfficePermissions.CrawlerCredentials.Default)]
    public class CrawlerCredentialsAppService : ApplicationService, ICrawlerCredentialsAppService
    {
        private readonly ICrawlerCredentialRepository _crawlerCredentialRepository;
        private readonly CrawlerCredentialManager _crawlerCredentialManager;
        private readonly IRepository<CrawlerAccount, Guid> _crawlerAccountRepository;
        private readonly IRepository<CrawlerProxy, Guid> _crawlerProxyRepository;

        public CrawlerCredentialsAppService(ICrawlerCredentialRepository crawlerCredentialRepository, CrawlerCredentialManager crawlerCredentialManager, IRepository<CrawlerAccount, Guid> crawlerAccountRepository, IRepository<CrawlerProxy, Guid> crawlerProxyRepository)
        {
            _crawlerCredentialRepository = crawlerCredentialRepository;
            _crawlerCredentialManager = crawlerCredentialManager; _crawlerAccountRepository = crawlerAccountRepository;
            _crawlerProxyRepository = crawlerProxyRepository;
        }

        public virtual async Task<PagedResultDto<CrawlerCredentialWithNavigationPropertiesDto>> GetListAsync(GetCrawlerCredentialsInput input)
        {
            var totalCount = await _crawlerCredentialRepository.GetCountAsync(input.FilterText, input.DataSourceType, input.CrawledAtMin, input.CrawledAtMax, input.IsAvailable, input.CrawlerAccountId, input.CrawlerProxyId);
            var items = await _crawlerCredentialRepository.GetListWithNavigationPropertiesAsync(input.FilterText, input.DataSourceType, input.CrawledAtMin, input.CrawledAtMax, input.IsAvailable, input.CrawlerAccountId, input.CrawlerProxyId, input.Sorting, input.MaxResultCount, input.SkipCount);

            return new PagedResultDto<CrawlerCredentialWithNavigationPropertiesDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<CrawlerCredentialWithNavigationProperties>, List<CrawlerCredentialWithNavigationPropertiesDto>>(items)
            };
        }

        public virtual async Task<CrawlerCredentialWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id)
        {
            return ObjectMapper.Map<CrawlerCredentialWithNavigationProperties, CrawlerCredentialWithNavigationPropertiesDto>
                (await _crawlerCredentialRepository.GetWithNavigationPropertiesAsync(id));
        }

        public virtual async Task<CrawlerCredentialDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<CrawlerCredential, CrawlerCredentialDto>(await _crawlerCredentialRepository.GetAsync(id));
        }

        public virtual async Task<PagedResultDto<LookupDto<Guid?>>> GetCrawlerAccountLookupAsync(LookupRequestDto input)
        {
            var query = (await _crawlerAccountRepository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    x => x.Username != null &&
                         x.Username.Contains(input.Filter));

            var lookupData = await query.PageBy(input.SkipCount, input.MaxResultCount).ToDynamicListAsync<CrawlerAccount>();
            var totalCount = query.Count();
            return new PagedResultDto<LookupDto<Guid?>>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<CrawlerAccount>, List<LookupDto<Guid?>>>(lookupData)
            };
        }

        public virtual async Task<PagedResultDto<LookupDto<Guid?>>> GetCrawlerProxyLookupAsync(LookupRequestDto input)
        {
            var query = (await _crawlerProxyRepository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    x => x.Ip != null &&
                         x.Ip.Contains(input.Filter));

            var lookupData = await query.PageBy(input.SkipCount, input.MaxResultCount).ToDynamicListAsync<CrawlerProxy>();
            var totalCount = query.Count();
            return new PagedResultDto<LookupDto<Guid?>>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<CrawlerProxy>, List<LookupDto<Guid?>>>(lookupData)
            };
        }

        [Authorize(BackOfficePermissions.CrawlerCredentials.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _crawlerCredentialRepository.DeleteAsync(id);
        }

        [Authorize(BackOfficePermissions.CrawlerCredentials.Create)]
        public virtual async Task<CrawlerCredentialDto> CreateAsync(CrawlerCredentialCreateDto input)
        {

            var crawlerCredential = await _crawlerCredentialManager.CreateAsync(
            input.CrawlerAccountId, input.CrawlerProxyId, input.DataSourceType, input.IsAvailable, input.CrawledAt
            );

            return ObjectMapper.Map<CrawlerCredential, CrawlerCredentialDto>(crawlerCredential);
        }

        [Authorize(BackOfficePermissions.CrawlerCredentials.Edit)]
        public virtual async Task<CrawlerCredentialDto> UpdateAsync(Guid id, CrawlerCredentialUpdateDto input)
        {

            var crawlerCredential = await _crawlerCredentialManager.UpdateAsync(
            id,
            input.CrawlerAccountId, input.CrawlerProxyId, input.DataSourceType, input.IsAvailable, input.CrawledAt, input.ConcurrencyStamp
            );

            return ObjectMapper.Map<CrawlerCredential, CrawlerCredentialDto>(crawlerCredential);
        }
    }
}