using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.LongChau.MongoDb;
using LC.Crawler.BackOffice.Medias;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.LongChau.Medias
{
    public class MongoMediaLongChauRepository : MongoDbRepository<LongChauMongoDbContext, Media, Guid>, IMediaLongChauRepository
    {
        public MongoMediaLongChauRepository(IMongoDbContextProvider<LongChauMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<Media>> GetListAsync(
            string filterText = null,
            string name = null,
            string contentType = null,
            string url = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, contentType, url);
            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? MediaConsts.GetDefaultSorting(false) : sorting);
            return await query.As<IMongoQueryable<Media>>()
                .PageBy<Media, IMongoQueryable<Media>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetCountAsync(
           string filterText = null,
           string name = null,
           string contentType = null,
           string url = null,
           CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, contentType, url);
            return await query.As<IMongoQueryable<Media>>().LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<Media> ApplyFilter(
            IQueryable<Media> query,
            string filterText,
            string name = null,
            string contentType = null,
            string url = null)
        {
            return query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => e.Name.Contains(filterText) || e.ContentType.Contains(filterText) || e.Url.Contains(filterText))
                    .WhereIf(!string.IsNullOrWhiteSpace(name), e => e.Name.Contains(name))
                    .WhereIf(!string.IsNullOrWhiteSpace(contentType), e => e.ContentType.Contains(contentType))
                    .WhereIf(!string.IsNullOrWhiteSpace(url), e => e.Url.Contains(url));
        }
    }
}