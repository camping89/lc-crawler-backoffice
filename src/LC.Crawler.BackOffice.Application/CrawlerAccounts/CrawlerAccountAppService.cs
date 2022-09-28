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
using LC.Crawler.BackOffice.CrawlerAccounts;

namespace LC.Crawler.BackOffice.CrawlerAccounts
{

    [Authorize(BackOfficePermissions.CrawlerAccounts.Default)]
    public class CrawlerAccountsAppService : ApplicationService, ICrawlerAccountsAppService
    {
        private readonly ICrawlerAccountRepository _crawlerAccountRepository;
        private readonly CrawlerAccountManager _crawlerAccountManager;

        public CrawlerAccountsAppService(ICrawlerAccountRepository crawlerAccountRepository, CrawlerAccountManager crawlerAccountManager)
        {
            _crawlerAccountRepository = crawlerAccountRepository;
            _crawlerAccountManager = crawlerAccountManager;
        }

        public virtual async Task<PagedResultDto<CrawlerAccountDto>> GetListAsync(GetCrawlerAccountsInput input)
        {
            var totalCount = await _crawlerAccountRepository.GetCountAsync(input.FilterText, input.Username, input.Password, input.TwoFactorCode, input.AccountType, input.AccountStatus, input.Email, input.EmailPassword, input.IsActive);
            var items = await _crawlerAccountRepository.GetListAsync(input.FilterText, input.Username, input.Password, input.TwoFactorCode, input.AccountType, input.AccountStatus, input.Email, input.EmailPassword, input.IsActive, input.Sorting, input.MaxResultCount, input.SkipCount);

            return new PagedResultDto<CrawlerAccountDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<CrawlerAccount>, List<CrawlerAccountDto>>(items)
            };
        }

        public virtual async Task<CrawlerAccountDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<CrawlerAccount, CrawlerAccountDto>(await _crawlerAccountRepository.GetAsync(id));
        }

        [Authorize(BackOfficePermissions.CrawlerAccounts.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _crawlerAccountRepository.DeleteAsync(id);
        }

        [Authorize(BackOfficePermissions.CrawlerAccounts.Create)]
        public virtual async Task<CrawlerAccountDto> CreateAsync(CrawlerAccountCreateDto input)
        {

            var crawlerAccount = await _crawlerAccountManager.CreateAsync(
            input.Username, input.Password, input.TwoFactorCode, input.AccountType, input.AccountStatus, input.Email, input.EmailPassword, input.IsActive
            );

            return ObjectMapper.Map<CrawlerAccount, CrawlerAccountDto>(crawlerAccount);
        }

        [Authorize(BackOfficePermissions.CrawlerAccounts.Edit)]
        public virtual async Task<CrawlerAccountDto> UpdateAsync(Guid id, CrawlerAccountUpdateDto input)
        {

            var crawlerAccount = await _crawlerAccountManager.UpdateAsync(
            id,
            input.Username, input.Password, input.TwoFactorCode, input.AccountType, input.AccountStatus, input.Email, input.EmailPassword, input.IsActive, input.ConcurrencyStamp
            );

            return ObjectMapper.Map<CrawlerAccount, CrawlerAccountDto>(crawlerAccount);
        }
    }
}