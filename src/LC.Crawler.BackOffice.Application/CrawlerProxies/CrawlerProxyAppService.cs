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
using LC.Crawler.BackOffice.CrawlerProxies;

namespace LC.Crawler.BackOffice.CrawlerProxies
{

    [Authorize(BackOfficePermissions.CrawlerProxies.Default)]
    public class CrawlerProxiesAppService : ApplicationService, ICrawlerProxiesAppService
    {
        private readonly ICrawlerProxyRepository _crawlerProxyRepository;
        private readonly CrawlerProxyManager _crawlerProxyManager;

        public CrawlerProxiesAppService(ICrawlerProxyRepository crawlerProxyRepository, CrawlerProxyManager crawlerProxyManager)
        {
            _crawlerProxyRepository = crawlerProxyRepository;
            _crawlerProxyManager = crawlerProxyManager;
        }

        public virtual async Task<PagedResultDto<CrawlerProxyDto>> GetListAsync(GetCrawlerProxiesInput input)
        {
            var totalCount = await _crawlerProxyRepository.GetCountAsync(input.FilterText, input.Ip, input.Port, input.Protocol, input.Username, input.Password, input.PingedAtMin, input.PingedAtMax, input.IsActive);
            var items = await _crawlerProxyRepository.GetListAsync(input.FilterText, input.Ip, input.Port, input.Protocol, input.Username, input.Password, input.PingedAtMin, input.PingedAtMax, input.IsActive, input.Sorting, input.MaxResultCount, input.SkipCount);

            return new PagedResultDto<CrawlerProxyDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<CrawlerProxy>, List<CrawlerProxyDto>>(items)
            };
        }

        public virtual async Task<CrawlerProxyDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<CrawlerProxy, CrawlerProxyDto>(await _crawlerProxyRepository.GetAsync(id));
        }

        [Authorize(BackOfficePermissions.CrawlerProxies.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _crawlerProxyRepository.DeleteAsync(id);
        }

        [Authorize(BackOfficePermissions.CrawlerProxies.Create)]
        public virtual async Task<CrawlerProxyDto> CreateAsync(CrawlerProxyCreateDto input)
        {

            var crawlerProxy = await _crawlerProxyManager.CreateAsync(
            input.Ip, input.Port, input.Protocol, input.Username, input.Password, input.IsActive, input.PingedAt
            );

            return ObjectMapper.Map<CrawlerProxy, CrawlerProxyDto>(crawlerProxy);
        }

        [Authorize(BackOfficePermissions.CrawlerProxies.Edit)]
        public virtual async Task<CrawlerProxyDto> UpdateAsync(Guid id, CrawlerProxyUpdateDto input)
        {

            var crawlerProxy = await _crawlerProxyManager.UpdateAsync(
            id,
            input.Ip, input.Port, input.Protocol, input.Username, input.Password, input.IsActive, input.PingedAt, input.ConcurrencyStamp
            );

            return ObjectMapper.Map<CrawlerProxy, CrawlerProxyDto>(crawlerProxy);
        }
    }
}