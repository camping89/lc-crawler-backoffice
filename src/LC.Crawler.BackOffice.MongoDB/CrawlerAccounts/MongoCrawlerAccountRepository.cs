using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;
using MongoDB.Driver.Linq;
using MongoDB.Driver;

namespace LC.Crawler.BackOffice.CrawlerAccounts
{
    public class MongoCrawlerAccountRepository : MongoDbRepository<BackOfficeMongoDbContext, CrawlerAccount, Guid>, ICrawlerAccountRepository
    {
        public MongoCrawlerAccountRepository(IMongoDbContextProvider<BackOfficeMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<CrawlerAccount>> GetListAsync(
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
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, username, password, twoFactorCode, accountType, accountStatus, email, emailPassword, isActive);
            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? CrawlerAccountConsts.GetDefaultSorting(false) : sorting);
            return await query.As<IMongoQueryable<CrawlerAccount>>()
                .PageBy<CrawlerAccount, IMongoQueryable<CrawlerAccount>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetCountAsync(
           string filterText = null,
           string username = null,
           string password = null,
           string twoFactorCode = null,
           AccountType? accountType = null,
           AccountStatus? accountStatus = null,
           string email = null,
           string emailPassword = null,
           bool? isActive = null,
           CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, username, password, twoFactorCode, accountType, accountStatus, email, emailPassword, isActive);
            return await query.As<IMongoQueryable<CrawlerAccount>>().LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<CrawlerAccount> ApplyFilter(
            IQueryable<CrawlerAccount> query,
            string filterText,
            string username = null,
            string password = null,
            string twoFactorCode = null,
            AccountType? accountType = null,
            AccountStatus? accountStatus = null,
            string email = null,
            string emailPassword = null,
            bool? isActive = null)
        {
            return query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => e.Username.Contains(filterText) || e.Password.Contains(filterText) || e.TwoFactorCode.Contains(filterText) || e.Email.Contains(filterText) || e.EmailPassword.Contains(filterText))
                    .WhereIf(!string.IsNullOrWhiteSpace(username), e => e.Username.Contains(username))
                    .WhereIf(!string.IsNullOrWhiteSpace(password), e => e.Password.Contains(password))
                    .WhereIf(!string.IsNullOrWhiteSpace(twoFactorCode), e => e.TwoFactorCode.Contains(twoFactorCode))
                    .WhereIf(accountType.HasValue, e => e.AccountType == accountType)
                    .WhereIf(accountStatus.HasValue, e => e.AccountStatus == accountStatus)
                    .WhereIf(!string.IsNullOrWhiteSpace(email), e => e.Email.Contains(email))
                    .WhereIf(!string.IsNullOrWhiteSpace(emailPassword), e => e.EmailPassword.Contains(emailPassword))
                    .WhereIf(isActive.HasValue, e => e.IsActive == isActive);
        }
    }
}