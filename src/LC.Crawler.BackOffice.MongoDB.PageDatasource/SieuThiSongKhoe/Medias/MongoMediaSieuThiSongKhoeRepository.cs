using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.PageDatasource.SieuThiSongKhoe.MongoDb;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.PageDatasource.SieuThiSongKhoe.Medias;

public class MongoMediaSieuThiSongKhoeRepository : MongoDbRepository<SieuThiSongKhoeMongoDbContext, Media, Guid>, IMediaSieuThiSongKhoeRepository
{
    public MongoMediaSieuThiSongKhoeRepository(IMongoDbContextProvider<SieuThiSongKhoeMongoDbContext> dbContextProvider) : base(dbContextProvider)
    {
            
    }
    public async Task<List<Media>> GetListAsync(
            string filterText = null,
            string name = null,
            string contentType = null,
            string url = null,
            string description = null,
            bool? isDowloaded = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, contentType, url, description, isDowloaded);
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
           string description = null,
           bool? isDowloaded = null,
           CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, contentType, url, description, isDowloaded);
            return await query.As<IMongoQueryable<Media>>().LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<Media> ApplyFilter(
            IQueryable<Media> query,
            string filterText,
            string name = null,
            string contentType = null,
            string url = null,
            string description = null,
            bool? isDowloaded = null)
        {
            return query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => e.Name.Contains(filterText) || e.ContentType.Contains(filterText) || e.Url.Contains(filterText) || e.Description.Contains(filterText))
                    .WhereIf(!string.IsNullOrWhiteSpace(name), e => e.Name.Contains(name))
                    .WhereIf(!string.IsNullOrWhiteSpace(contentType), e => e.ContentType.Contains(contentType))
                    .WhereIf(!string.IsNullOrWhiteSpace(url), e => e.Url.Contains(url))
                    .WhereIf(!string.IsNullOrWhiteSpace(description), e => e.Description.Contains(description))
                    .WhereIf(isDowloaded.HasValue, e => e.IsDowloaded == isDowloaded);
        }
}