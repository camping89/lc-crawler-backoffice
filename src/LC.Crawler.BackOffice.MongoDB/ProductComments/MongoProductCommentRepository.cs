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

namespace LC.Crawler.BackOffice.ProductComments
{
    public class MongoProductCommentRepository : MongoDbRepository<BackOfficeMongoDbContext, ProductComment, Guid>, IProductCommentRepository
    {
        public MongoProductCommentRepository(IMongoDbContextProvider<BackOfficeMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<ProductCommentWithNavigationProperties> GetWithNavigationPropertiesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var productComment = await (await GetMongoQueryableAsync(cancellationToken))
                .FirstOrDefaultAsync(e => e.Id == id, GetCancellationToken(cancellationToken));

            var product = await (await GetDbContextAsync(cancellationToken)).Products.AsQueryable().FirstOrDefaultAsync(e => e.Id == productComment.ProductId, cancellationToken: cancellationToken);

            return new ProductCommentWithNavigationProperties
            {
                ProductComment = productComment,
                Product = product,

            };
        }

        public async Task<List<ProductCommentWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string name = null,
            string content = null,
            int? likesMin = null,
            int? likesMax = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            Guid? productId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, content, likesMin, likesMax, createdAtMin, createdAtMax, productId);
            var productComments = await query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ProductCommentConsts.GetDefaultSorting(false) : sorting.Split('.').Last())
                .As<IMongoQueryable<ProductComment>>()
                .PageBy<ProductComment, IMongoQueryable<ProductComment>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));

            var dbContext = await GetDbContextAsync(cancellationToken);
            return productComments.Select(s => new ProductCommentWithNavigationProperties
            {
                ProductComment = s,
                Product = dbContext.Products.AsQueryable().FirstOrDefault(e => e.Id == s.ProductId),

            }).ToList();
        }

        public async Task<List<ProductComment>> GetListAsync(
            string filterText = null,
            string name = null,
            string content = null,
            int? likesMin = null,
            int? likesMax = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, content, likesMin, likesMax, createdAtMin, createdAtMax);
            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ProductCommentConsts.GetDefaultSorting(false) : sorting);
            return await query.As<IMongoQueryable<ProductComment>>()
                .PageBy<ProductComment, IMongoQueryable<ProductComment>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetCountAsync(
           string filterText = null,
           string name = null,
           string content = null,
           int? likesMin = null,
           int? likesMax = null,
           DateTime? createdAtMin = null,
           DateTime? createdAtMax = null,
           Guid? productId = null,
           CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, content, likesMin, likesMax, createdAtMin, createdAtMax, productId);
            return await query.As<IMongoQueryable<ProductComment>>().LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<ProductComment> ApplyFilter(
            IQueryable<ProductComment> query,
            string filterText,
            string name = null,
            string content = null,
            int? likesMin = null,
            int? likesMax = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            Guid? productId = null)
        {
            return query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => e.Name.Contains(filterText) || e.Content.Contains(filterText))
                    .WhereIf(!string.IsNullOrWhiteSpace(name), e => e.Name.Contains(name))
                    .WhereIf(!string.IsNullOrWhiteSpace(content), e => e.Content.Contains(content))
                    .WhereIf(likesMin.HasValue, e => e.Likes >= likesMin.Value)
                    .WhereIf(likesMax.HasValue, e => e.Likes <= likesMax.Value)
                    .WhereIf(createdAtMin.HasValue, e => e.CreatedAt >= createdAtMin.Value)
                    .WhereIf(createdAtMax.HasValue, e => e.CreatedAt <= createdAtMax.Value)
                    .WhereIf(productId != null && productId != Guid.Empty, e => e.ProductId == productId);
        }
    }
}