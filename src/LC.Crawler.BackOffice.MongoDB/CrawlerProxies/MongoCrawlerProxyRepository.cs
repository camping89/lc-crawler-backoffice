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

namespace LC.Crawler.BackOffice.CrawlerProxies
{
    public class MongoCrawlerProxyRepository : MongoDbRepository<BackOfficeMongoDbContext, CrawlerProxy, Guid>, ICrawlerProxyRepository
    {
        public MongoCrawlerProxyRepository(IMongoDbContextProvider<BackOfficeMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<CrawlerProxy>> GetListAsync(
            string filterText = null,
            string ip = null,
            string port = null,
            string protocol = null,
            string username = null,
            string password = null,
            DateTime? pingedAtMin = null,
            DateTime? pingedAtMax = null,
            bool? isActive = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, ip, port, protocol, username, password, pingedAtMin, pingedAtMax, isActive);
            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? CrawlerProxyConsts.GetDefaultSorting(false) : sorting);
            return await query.As<IMongoQueryable<CrawlerProxy>>()
                .PageBy<CrawlerProxy, IMongoQueryable<CrawlerProxy>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetCountAsync(
           string filterText = null,
           string ip = null,
           string port = null,
           string protocol = null,
           string username = null,
           string password = null,
           DateTime? pingedAtMin = null,
           DateTime? pingedAtMax = null,
           bool? isActive = null,
           CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, ip, port, protocol, username, password, pingedAtMin, pingedAtMax, isActive);
            return await query.As<IMongoQueryable<CrawlerProxy>>().LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<CrawlerProxy> ApplyFilter(
            IQueryable<CrawlerProxy> query,
            string filterText,
            string ip = null,
            string port = null,
            string protocol = null,
            string username = null,
            string password = null,
            DateTime? pingedAtMin = null,
            DateTime? pingedAtMax = null,
            bool? isActive = null)
        {
            return query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => e.Ip.Contains(filterText) || e.Port.Contains(filterText) || e.Protocol.Contains(filterText) || e.Username.Contains(filterText) || e.Password.Contains(filterText))
                    .WhereIf(!string.IsNullOrWhiteSpace(ip), e => e.Ip.Contains(ip))
                    .WhereIf(!string.IsNullOrWhiteSpace(port), e => e.Port.Contains(port))
                    .WhereIf(!string.IsNullOrWhiteSpace(protocol), e => e.Protocol.Contains(protocol))
                    .WhereIf(!string.IsNullOrWhiteSpace(username), e => e.Username.Contains(username))
                    .WhereIf(!string.IsNullOrWhiteSpace(password), e => e.Password.Contains(password))
                    .WhereIf(pingedAtMin.HasValue, e => e.PingedAt >= pingedAtMin.Value)
                    .WhereIf(pingedAtMax.HasValue, e => e.PingedAt <= pingedAtMax.Value)
                    .WhereIf(isActive.HasValue, e => e.IsActive == isActive);
        }
    }
}