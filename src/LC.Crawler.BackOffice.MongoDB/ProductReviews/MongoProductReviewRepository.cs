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

namespace LC.Crawler.BackOffice.ProductReviews
{
    public class MongoProductReviewRepository : MongoDbRepository<BackOfficeMongoDbContext, ProductReview, Guid>, IProductReviewRepository
    {
        public MongoProductReviewRepository(IMongoDbContextProvider<BackOfficeMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<ProductReviewWithNavigationProperties> GetWithNavigationPropertiesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var productReview = await (await GetMongoQueryableAsync(cancellationToken))
                .FirstOrDefaultAsync(e => e.Id == id, GetCancellationToken(cancellationToken));

            var product = await (await GetDbContextAsync(cancellationToken)).Products.AsQueryable().FirstOrDefaultAsync(e => e.Id == productReview.ProductId, cancellationToken: cancellationToken);

            return new ProductReviewWithNavigationProperties
            {
                ProductReview = productReview,
                Product = product,

            };
        }

        public async Task<List<ProductReviewWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string name = null,
            string content = null,
            decimal? ratingMin = null,
            decimal? ratingMax = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            int? likesMin = null,
            int? likesMax = null,
            Guid? productId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, content, ratingMin, ratingMax, createdAtMin, createdAtMax, likesMin, likesMax, productId);
            var productReviews = await query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ProductReviewConsts.GetDefaultSorting(false) : sorting.Split('.').Last())
                .As<IMongoQueryable<ProductReview>>()
                .PageBy<ProductReview, IMongoQueryable<ProductReview>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));

            var dbContext = await GetDbContextAsync(cancellationToken);
            return productReviews.Select(s => new ProductReviewWithNavigationProperties
            {
                ProductReview = s,
                Product = dbContext.Products.AsQueryable().FirstOrDefault(e => e.Id == s.ProductId),

            }).ToList();
        }

        public async Task<List<ProductReview>> GetListAsync(
            string filterText = null,
            string name = null,
            string content = null,
            decimal? ratingMin = null,
            decimal? ratingMax = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            int? likesMin = null,
            int? likesMax = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, content, ratingMin, ratingMax, createdAtMin, createdAtMax, likesMin, likesMax);
            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ProductReviewConsts.GetDefaultSorting(false) : sorting);
            return await query.As<IMongoQueryable<ProductReview>>()
                .PageBy<ProductReview, IMongoQueryable<ProductReview>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetCountAsync(
           string filterText = null,
           string name = null,
           string content = null,
           decimal? ratingMin = null,
           decimal? ratingMax = null,
           DateTime? createdAtMin = null,
           DateTime? createdAtMax = null,
           int? likesMin = null,
           int? likesMax = null,
           Guid? productId = null,
           CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, content, ratingMin, ratingMax, createdAtMin, createdAtMax, likesMin, likesMax, productId);
            return await query.As<IMongoQueryable<ProductReview>>().LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<ProductReview> ApplyFilter(
            IQueryable<ProductReview> query,
            string filterText,
            string name = null,
            string content = null,
            decimal? ratingMin = null,
            decimal? ratingMax = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            int? likesMin = null,
            int? likesMax = null,
            Guid? productId = null)
        {
            return query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => e.Name.Contains(filterText) || e.Content.Contains(filterText))
                    .WhereIf(!string.IsNullOrWhiteSpace(name), e => e.Name.Contains(name))
                    .WhereIf(!string.IsNullOrWhiteSpace(content), e => e.Content.Contains(content))
                    .WhereIf(ratingMin.HasValue, e => e.Rating >= ratingMin.Value)
                    .WhereIf(ratingMax.HasValue, e => e.Rating <= ratingMax.Value)
                    .WhereIf(createdAtMin.HasValue, e => e.CreatedAt >= createdAtMin.Value)
                    .WhereIf(createdAtMax.HasValue, e => e.CreatedAt <= createdAtMax.Value)
                    .WhereIf(likesMin.HasValue, e => e.Likes >= likesMin.Value)
                    .WhereIf(likesMax.HasValue, e => e.Likes <= likesMax.Value)
                    .WhereIf(productId != null && productId != Guid.Empty, e => e.ProductId == productId);
        }
    }
}