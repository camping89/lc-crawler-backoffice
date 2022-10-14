using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;
using MongoDB.Driver.Linq;
using MongoDB.Driver;

namespace LC.Crawler.BackOffice.DataSources
{
    public class MongoDataSourceRepository : MongoDbRepository<BackOfficeMongoDbContext, DataSource, Guid>, IDataSourceRepository
    {
        public MongoDataSourceRepository(IMongoDbContextProvider<BackOfficeMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<DataSource>> GetListAsync(
            string filterText = null,
            string url = null,
            bool? isActive = null,
            string postToSite = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, url, isActive, postToSite);
            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? DataSourceConsts.GetDefaultSorting(false) : sorting);
            return await query.As<IMongoQueryable<DataSource>>()
                .PageBy<DataSource, IMongoQueryable<DataSource>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetCountAsync(
           string filterText = null,
           string url = null,
           bool? isActive = null,
           string postToSite = null,
           CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, url, isActive, postToSite);
            return await query.As<IMongoQueryable<DataSource>>().LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<DataSource> ApplyFilter(
            IQueryable<DataSource> query,
            string filterText,
            string url = null,
            bool? isActive = null,
            string postToSite = null)
        {
            return query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => e.Url.Contains(filterText) || e.PostToSite.Contains(filterText))
                    .WhereIf(!string.IsNullOrWhiteSpace(url), e => e.Url.Contains(url))
                    .WhereIf(isActive.HasValue, e => e.IsActive == isActive)
                    .WhereIf(!string.IsNullOrWhiteSpace(postToSite), e => e.PostToSite.Contains(postToSite));
        }
    }
}