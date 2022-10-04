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

namespace LC.Crawler.BackOffice.ProductAttributes
{
    public class MongoProductAttributeRepository : MongoDbRepository<BackOfficeMongoDbContext, ProductAttribute, Guid>, IProductAttributeRepository
    {
        public MongoProductAttributeRepository(IMongoDbContextProvider<BackOfficeMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<ProductAttributeWithNavigationProperties> GetWithNavigationPropertiesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var productAttribute = await (await GetMongoQueryableAsync(cancellationToken))
                .FirstOrDefaultAsync(e => e.Id == id, GetCancellationToken(cancellationToken));

            var product = await (await GetDbContextAsync(cancellationToken)).Products.AsQueryable().FirstOrDefaultAsync(e => e.Id == productAttribute.ProductId, cancellationToken: cancellationToken);

            return new ProductAttributeWithNavigationProperties
            {
                ProductAttribute = productAttribute,
                Product = product,

            };
        }

        public async Task<List<ProductAttributeWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string slug = null,
            string key = null,
            string value = null,
            Guid? productId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, slug, key, value, productId);
            var productAttributes = await query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ProductAttributeConsts.GetDefaultSorting(false) : sorting.Split('.').Last())
                .As<IMongoQueryable<ProductAttribute>>()
                .PageBy<ProductAttribute, IMongoQueryable<ProductAttribute>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));

            var dbContext = await GetDbContextAsync(cancellationToken);
            return productAttributes.Select(s => new ProductAttributeWithNavigationProperties
            {
                ProductAttribute = s,
                Product = dbContext.Products.AsQueryable().FirstOrDefault(e => e.Id == s.ProductId),

            }).ToList();
        }

        public async Task<List<ProductAttribute>> GetListAsync(
            string filterText = null,
            string slug = null,
            string key = null,
            string value = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, slug, key, value);
            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ProductAttributeConsts.GetDefaultSorting(false) : sorting);
            return await query.As<IMongoQueryable<ProductAttribute>>()
                .PageBy<ProductAttribute, IMongoQueryable<ProductAttribute>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetCountAsync(
           string filterText = null,
           string slug = null,
           string key = null,
           string value = null,
           Guid? productId = null,
           CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, slug, key, value, productId);
            return await query.As<IMongoQueryable<ProductAttribute>>().LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<ProductAttribute> ApplyFilter(
            IQueryable<ProductAttribute> query,
            string filterText,
            string slug = null,
            string key = null,
            string value = null,
            Guid? productId = null)
        {
            return query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => e.Slug.Contains(filterText) || e.Key.Contains(filterText) || e.Value.Contains(filterText))
                    .WhereIf(!string.IsNullOrWhiteSpace(slug), e => e.Slug.Contains(slug))
                    .WhereIf(!string.IsNullOrWhiteSpace(key), e => e.Key.Contains(key))
                    .WhereIf(!string.IsNullOrWhiteSpace(value), e => e.Value.Contains(value))
                    .WhereIf(productId != null && productId != Guid.Empty, e => e.ProductId == productId);
        }
    }
}