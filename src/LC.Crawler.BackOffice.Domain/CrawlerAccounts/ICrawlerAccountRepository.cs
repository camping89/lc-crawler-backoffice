using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Enums;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.CrawlerAccounts
{
    public interface ICrawlerAccountRepository : IRepository<CrawlerAccount, Guid>
    {
        Task<List<CrawlerAccount>> GetListAsync(
            string filterText = null,
            string username = null,
            string password = null,
            string twoFactorCode = null,
            AccountType? accountType = null,
            AccountStatus? accountStatus = null,
            string email = null,
            string emailPassword = null,
            bool? isActive = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default
        );

        Task<long> GetCountAsync(
            string filterText = null,
            string username = null,
            string password = null,
            string twoFactorCode = null,
            AccountType? accountType = null,
            AccountStatus? accountStatus = null,
            string email = null,
            string emailPassword = null,
            bool? isActive = null,
            CancellationToken cancellationToken = default);
    }
}