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

namespace LC.Crawler.BackOffice.CrawlerCredentials
{
    public class MongoCrawlerCredentialRepository : MongoDbRepository<BackOfficeMongoDbContext, CrawlerCredential, Guid>, ICrawlerCredentialRepository
    {
        public MongoCrawlerCredentialRepository(IMongoDbContextProvider<BackOfficeMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<CrawlerCredentialWithNavigationProperties> GetWithNavigationPropertiesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var crawlerCredential = await (await GetMongoQueryableAsync(cancellationToken))
                .FirstOrDefaultAsync(e => e.Id == id, GetCancellationToken(cancellationToken));

            var crawlerAccount = await (await GetDbContextAsync(cancellationToken)).CrawlerAccounts.AsQueryable().FirstOrDefaultAsync(e => e.Id == crawlerCredential.CrawlerAccountId, cancellationToken: cancellationToken);
            var crawlerProxy = await (await GetDbContextAsync(cancellationToken)).CrawlerProxies.AsQueryable().FirstOrDefaultAsync(e => e.Id == crawlerCredential.CrawlerProxyId, cancellationToken: cancellationToken);

            return new CrawlerCredentialWithNavigationProperties
            {
                CrawlerCredential = crawlerCredential,
                CrawlerAccount = crawlerAccount,
                CrawlerProxy = crawlerProxy,

            };
        }

        public async Task<List<CrawlerCredentialWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            DataSourceType? dataSourceType = null,
            DateTime? crawledAtMin = null,
            DateTime? crawledAtMax = null,
            bool? isAvailable = null,
            Guid? crawlerAccountId = null,
            Guid? crawlerProxyId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, dataSourceType, crawledAtMin, crawledAtMax, isAvailable, crawlerAccountId, crawlerProxyId);
            var crawlerCredentials = await query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? CrawlerCredentialConsts.GetDefaultSorting(false) : sorting.Split('.').Last())
                .As<IMongoQueryable<CrawlerCredential>>()
                .PageBy<CrawlerCredential, IMongoQueryable<CrawlerCredential>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));

            var dbContext = await GetDbContextAsync(cancellationToken);
            return crawlerCredentials.Select(s => new CrawlerCredentialWithNavigationProperties
            {
                CrawlerCredential = s,
                CrawlerAccount = dbContext.CrawlerAccounts.AsQueryable().FirstOrDefault(e => e.Id == s.CrawlerAccountId),
                CrawlerProxy = dbContext.CrawlerProxies.AsQueryable().FirstOrDefault(e => e.Id == s.CrawlerProxyId),

            }).ToList();
        }

        public async Task<List<CrawlerCredential>> GetListAsync(
            string filterText = null,
            DataSourceType? dataSourceType = null,
            DateTime? crawledAtMin = null,
            DateTime? crawledAtMax = null,
            bool? isAvailable = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, dataSourceType, crawledAtMin, crawledAtMax, isAvailable);
            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? CrawlerCredentialConsts.GetDefaultSorting(false) : sorting);
            return await query.As<IMongoQueryable<CrawlerCredential>>()
                .PageBy<CrawlerCredential, IMongoQueryable<CrawlerCredential>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetCountAsync(
           string filterText = null,
           DataSourceType? dataSourceType = null,
           DateTime? crawledAtMin = null,
           DateTime? crawledAtMax = null,
           bool? isAvailable = null,
           Guid? crawlerAccountId = null,
           Guid? crawlerProxyId = null,
           CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, dataSourceType, crawledAtMin, crawledAtMax, isAvailable, crawlerAccountId, crawlerProxyId);
            return await query.As<IMongoQueryable<CrawlerCredential>>().LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<CrawlerCredential> ApplyFilter(
            IQueryable<CrawlerCredential> query,
            string filterText,
            DataSourceType? dataSourceType = null,
            DateTime? crawledAtMin = null,
            DateTime? crawledAtMax = null,
            bool? isAvailable = null,
            Guid? crawlerAccountId = null,
            Guid? crawlerProxyId = null)
        {
            return query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => true)
                    .WhereIf(dataSourceType.HasValue, e => e.DataSourceType == dataSourceType)
                    .WhereIf(crawledAtMin.HasValue, e => e.CrawledAt >= crawledAtMin.Value)
                    .WhereIf(crawledAtMax.HasValue, e => e.CrawledAt <= crawledAtMax.Value)
                    .WhereIf(isAvailable.HasValue, e => e.IsAvailable == isAvailable)
                    .WhereIf(crawlerAccountId != null && crawlerAccountId != Guid.Empty, e => e.CrawlerAccountId == crawlerAccountId)
                    .WhereIf(crawlerProxyId != null && crawlerProxyId != Guid.Empty, e => e.CrawlerProxyId == crawlerProxyId);
        }
    }
}